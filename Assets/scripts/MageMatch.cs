using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class MageMatch : MonoBehaviour {

    // how to handle performing action here? also better name for Normal?
    public enum State { Normal, Selecting, Targeting, Menu, TurnSwitching };
    private Stack<State> stateStack;

    public enum Turn { PlayerTurn, CommishTurn }; // MyTurn, OppTurn?
    public Turn currentTurn;
    public MMLog.LogLevel debugLogLevel = MMLog.LogLevel.Standard;
    [HideInInspector]
    public int myID;
    [HideInInspector]
    public bool switchingTurn = false;

    public GameSettings gameSettings;
    public SyncManager syncManager;
    public HexGrid hexGrid;
    public BoardCheck boardCheck;
    public TileManager tileMan;
    public Commish commish;
    public TurnTimer timer;
    public Targeting targeting;
    public Prompt prompt;
    public EffectController effectCont;
    public EventController eventCont;
    public AudioController audioCont;
    public AnimationController animCont;
    public UIController uiCont;
    public Stats stats;
    public ObjectEffects hexFX;
    public InputController inputCont;

    private Player p1, p2, activep;
    private Transform tilesOnBoard;
    private bool endGame = false;
    private int checking = 0, actionsPerforming = 0, matchesResolving = 0;
    private List<TileSeq>[] spellsOnBoard;

    private DebugSettings debugSettings;
    private bool isDebugMode = false;
    public DebugTools debugTools;

    public delegate void LoadEvent();
    public event LoadEvent onEffectContReady;
    public event LoadEvent onEventContReady;

    void Start() {
        MMLog.Init(debugLogLevel);

        GameObject debugObj = GameObject.Find("debugSettings");
        if (debugObj != null) {
            debugSettings = debugObj.GetComponent<DebugSettings>();
            isDebugMode = true;
            debugTools = GameObject.Find("toolsMenu").GetComponent<DebugTools>();
            debugTools.Init(this);

            MMLog.LogWarning("This scene is in debug mode!!");
            PhotonNetwork.offlineMode = true;
            PhotonNetwork.CreateRoom("debug");
            PhotonNetwork.JoinRoom("debug");
        }

        stateStack = new Stack<State>();

        StartCoroutine(Reset());
    }

    public IEnumerator Reset() {
        tilesOnBoard = GameObject.Find("tilesOnBoard").transform;
        gameSettings = GameObject.Find("gameSettings").GetComponent<GameSettings>();
        MMLog.Log_MageMatch("gamesettings: p1="+gameSettings.p1name+",p2="+gameSettings.p2name+",timer="+gameSettings.turnTimerOn);

        uiCont = GameObject.Find("ui").GetComponent<UIController>();
        uiCont.Init();
        timer = gameObject.GetComponent<TurnTimer>();
        
        targeting = new Targeting(this);
        prompt = new Prompt(this);
        audioCont = new AudioController(this);
        animCont = GetComponent<AnimationController>();

        syncManager = GetComponent<SyncManager>();
        syncManager.Init();

        if (IsDebugMode())
            myID = 1;
        else
            myID = PhotonNetwork.player.ID;

        hexGrid = new HexGrid();
        tileMan = new TileManager(this);

        uiCont.GetCellOverlays();
        boardCheck = new BoardCheck(this);
        hexFX = new ObjectEffects(this);

        endGame = false;

        p1 = new Player(1);
        p2 = new Player(2);
        activep = p1;

        commish = new Commish(this);

        effectCont = new EffectController(this);
        EffectContLoaded();
        InitEvents();

        currentTurn = Turn.PlayerTurn;
        uiCont.SetDrawButton(true);
        activep.InitAP();

        if(!IsDebugMode())
            yield return syncManager.Checkpoint(); // idk if this is really doing anything

        // TODO animate beginning of game
        for (int i = 0; i < 4; i++) {
            yield return LocalP().DealTile();
            yield return new WaitForSeconds(.25f);
        }

        if (MyTurn())
            yield return activep.DealTile();

        stats = new Stats(p1, p2);

        timer.InitTimer();
        uiCont.Reset();

        inputCont = GetComponent<InputController>();
        EnterState(State.Normal);

        // TODO init some stuff that would otherwise be BeginTurnEvent()
        yield return null;
    }

    public void InitEvents() {
        eventCont = new EventController(this);
        EventContLoaded();

        eventCont.boardAction += OnBoardAction;
        eventCont.AddDropEvent(OnDrop, EventController.Type.GameAction); // checking
        eventCont.AddSwapEvent(OnSwap, EventController.Type.GameAction); // checking
        eventCont.gameAction += OnGameAction;
        eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.GameAction); // checking
        eventCont.AddTurnEndEvent(OnTurnEnd, EventController.Type.GameAction); // checking

        if (gameSettings.turnTimerOn)
            eventCont.timeout += OnTimeout;

        syncManager.InitEvents(this, eventCont);
        audioCont.InitEvents();
        effectCont.InitEvents();
        commish.InitEvents();
        uiCont.InitEvents();
        p1.InitEvents();
        p2.InitEvents();

        animCont.Init(this);
    }

    public void EffectContLoaded() {
        if(onEffectContReady != null)
            onEffectContReady();
    }

    public void EventContLoaded() {
        if(onEventContReady != null)
            onEventContReady();
    }

    // could have event if there are other things that depend on this
    public void EnterState(State state) {
        stateStack.Push(state);
        inputCont.SwitchContext(state); 
    }

    public void ExitState() {
        State s = stateStack.Pop();
        if (s == State.Menu)
            BoardChanged();
        inputCont.SwitchContext(stateStack.Peek());
    } 

    public State GetState() {
        return stateStack.Peek();
    }

    public bool IsDebugMode() { return isDebugMode; }

    // passthru function...not the best.
    public bool Debug_ApplyAPcost() {
        if (debugSettings != null)
            return debugSettings.applyAPcost;
        else return true;
    }

    // passthru function...not the best.
    public bool Debug_OnePlayerMode() {
        if (debugSettings != null)
            return debugSettings.onePlayerMode;
        else return false;
    }

    #region Event callbacks
    public void OnBoardAction() {
        if (checking == 0) { //?
            StartCoroutine(BoardChecking());
        }
    }

    public IEnumerator OnDrop(int id, bool playerAction, string tag, int col) {
        yield return BoardChecking(); // should go below? if done in a spell, should there be a similar event to this one OnSpellCast that checks the board once the spell resolves?
        if(playerAction)
            eventCont.GameAction(true);
        yield return null;
    }

    public IEnumerator OnSwap(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        yield return BoardChecking();
        if(playerAction)
            eventCont.GameAction(true);
    }

    public void OnGameAction(int id, bool costsAP) { // eventually just pass in int for cost?
        if (!Debug_ApplyAPcost())
            return;

        if (currentTurn != Turn.CommishTurn) { //?
            //Debug.Log("MAGEMATCH: OnGameAction called!");
            if (costsAP)
                activep.AP--;
            if (activep.AP == 0) {
                uiCont.SetDrawButton(false);
                StartCoroutine(TurnSystem());
            }
        }
    }

    public IEnumerator OnTurnBegin(int id) {
        yield return BoardChecking();
    }

    public IEnumerator OnTurnEnd(int id) {
        yield return BoardChecking();
    }

    public void OnTimeout(int id) {
        syncManager.TurnTimeout();
    }
    #endregion

    public void TurnTimeout() {
        Player p = GetPlayer(activep.id);
        MMLog.Log_MageMatch(p.name + "'s turn just timed out! They had " + p.AP + " AP left.");
        uiCont.SetDrawButton(false);
        StartCoroutine(TurnSystem());
    }

    IEnumerator TurnSystem() {
        if (switchingTurn) // could just use state I think
            yield break;

        EnterState(State.TurnSwitching);

        switchingTurn = true;
        timer.Pause();

        yield return new WaitUntil(() => actionsPerforming == 0 && tileMan.removing == 0); //?
        yield return new WaitUntil(() => checking == 0); //?
        MMLog.Log_MageMatch("<b>   ---------- TURNSYSTEM START ----------</b>");
        yield return eventCont.TurnEnd();

        uiCont.DeactivateAllSpellButtons(); //? These should be part of any boardaction...
        uiCont.SetDrawButton(false);

        currentTurn = Turn.CommishTurn;
        yield return commish.CTurn();

        currentTurn = Turn.PlayerTurn;

        if(!Debug_OnePlayerMode())
            activep = InactiveP();

        yield return eventCont.TurnBegin();
        SpellCheck();
        timer.InitTimer();

        MMLog.Log_MageMatch("<b>   ---------- TURNSYSTEM END ----------</b>");
        ExitState();
        switchingTurn = false;
    }

    public IEnumerator BoardChecking() {
        checking++; // prevents overcalling/retriggering
        yield return new WaitUntil(() => tileMan.removing == 0); //?
        MMLog.Log_MageMatch("About to check the board.");

        //cascade = 0;
        //while (true) {
            yield return new WaitUntil(() => !animCont.IsAnimating() && tileMan.removing == 0); //?
            hexGrid.CheckGrav(); // TODO make IEnum
            yield return new WaitUntil(() => hexGrid.IsGridAtRest());
            //List<TileSeq> seqMatches = boardCheck.MatchCheck();
            //if (seqMatches.Count > 0) { // if there's at least one MATCH
            //    MMLog.Log_MageMatch("Resolving matches...");
            //    yield return ResolveMatches(seqMatches);
            //    cascade++;
            //} else {
                if (currentTurn == Turn.PlayerTurn) {
                    //if (cascade > 1) {
                    //    eventCont.Cascade(cascade);
                    //    cascade = 0;
                    //}
                    SpellCheck();
                }
                uiCont.UpdateDebugGrid();
                //break;
            //}
        //}
        MMLog.Log_MageMatch("Done checking.");
        checking--;
    }

    void SpellCheck() { 
        Character c = activep.character;
        List<TileSeq> spellSeqList = c.GetTileSeqList();
        spellsOnBoard = boardCheck.CheckBoard(c.GetSpells());
        
        for (int s = 0; s < spellsOnBoard.Length; s++) {
            Spell sp = c.GetSpell(s);
            //Debug.Log("MAGEMATCH: Checking "+sp.name+"...");
            //bool spellIsOnBoard = false;
            //if (sp is CooldownSpell) {
            //    if (((CooldownSpell)sp).IsReadyToCast()) {
            //        //Debug.Log("MAGEMATCH: " + sp.name + " is core, and there's no effect with tag=" + sp.effectTag);
            //        //spellIsOnBoard = true;
            //    } else {
            //        MMLog.Log_MageMatch(sp.name + " is core, but it's cooling down!");
            //    }                    
            //} else { // could be cleaner.
                if (sp is SignatureSpell && !((SignatureSpell)sp).IsReadyToCast()) {
                    //Log(sp.name + " is signature, but not enough meter!");
                    continue;
                }

                //for (int i = 0; i < spellsOnBoard.Length; i++) {
                    MMLog.Log_MageMatch("spell[" + s + "] count=" + spellsOnBoard[s].Count);
                    if (spellsOnBoard[s].Count > 0)
                        uiCont.ActivateSpellButton(s);
                    else
                        uiCont.DeactivateSpellButton(s); // needed?

                    // TODO boardseq stuff will be handled by the spell selection thing

                    //TileSeq matchSeq = spellsOnBoard[i];
                    //if (matchSeq.MatchesTileSeq(sp.GetTileSeq())) {
                    //    spellIsOnBoard = true;
                    //    c.GetSpell(s).SetBoardSeq(matchSeq);
                    //    break;
                    //}
                //}
            //}                
        }
    }

    //IEnumerator ResolveMatches(List<TileSeq> seqList) {
    //    matchesResolving++;
    //    MMLog.Log_MageMatch("   ---------- MATCH BEGIN ----------");
    //    MMLog.Log_MageMatch("At least one match: " + boardCheck.PrintSeqList(seqList) + " and state="+currentState.ToString());
    //    if (currentState != GameState.CommishTurn) {
    //        MMLog.Log_MageMatch("Match was made by a player!");
    //        string[] seqs = GetTileSeqs(seqList);
    //        RemoveSeqList(seqList); // hopefully putting this first makes it more responsive?
    //        yield return eventCont.Match(seqs); // raise player Match event
    //    } else {
    //        eventCont.CommishMatch(GetTileSeqs(seqList)); // raise CommishMatch event
    //        RemoveSeqList(seqList);
    //    }

    //    yield return BoardChecking();
    //    MMLog.Log_MageMatch("   ---------- MATCH END ----------");
    //    matchesResolving--;
    //}


    #region ----- Game Actions -----

    public void PlayerDrawTile() {
        StartCoroutine(_Draw(activep.id, "", true));
    }
    
    // for spells and such
    // needed? just call mm.GetPlayer(id).DrawTiles()?
    public void DrawTile(int id) {
        StartCoroutine(_Draw(id, "", false));
    }

    public void DrawTile(int id, string genTag) {
        StartCoroutine(_Draw(id, genTag, false));
    }

    public IEnumerator _Draw(int id, string genTag, bool playerAction) {
        MMLog.Log_MageMatch("   ---------- DRAW BEGIN ----------");
        actionsPerforming++;
        Player p = GetPlayer(id);
        if (p.hand.IsFull()) {
            MMLog.Log_MageMatch("Player " + id + "'s hand is full.");
        } else {
            yield return p.DrawTiles(1, genTag, playerAction, false);
        }
        MMLog.Log_MageMatch("   ---------- DRAW END ----------");
        actionsPerforming--;
        yield return null;
    }

    public void PlayerDropTile(int col, Hex hex) {
        activep.hand.Remove(hex); // remove from hand
        StartCoroutine(_Drop(true, col, hex));
    }

    public void DropTile(int col, Hex hex) {
        StartCoroutine(_Drop(false, col, hex));
    }

    IEnumerator _Drop(bool playerAction, int col, Hex hex) {
        MMLog.Log_MageMatch("   ---------- DROP BEGIN ----------");
        actionsPerforming++;

        hex.Reveal();

        GameObject go = hex.gameObject;
        go.transform.SetParent(tilesOnBoard);
        TileBehav tb = hex.GetComponent<TileBehav>();
        tb.SetPlaced();
        tb.ChangePos(hexGrid.TopOfColumn(col) + 1, col, boardCheck.CheckColumn(col), .08f);
        if (currentTurn == Turn.PlayerTurn) { //kinda hacky
            yield return eventCont.Drop(playerAction, hex.tag, col);
        } else if (currentTurn == Turn.CommishTurn)
            eventCont.CommishDrop(tb.tile.element, col);

        MMLog.Log_MageMatch("   ---------- DROP END ----------");
        actionsPerforming--;
        yield return null;
    }

    public void PlayerSwapTiles(int c1, int r1, int c2, int r2) {
        StartCoroutine(_SwapTiles(true, c1, r1, c2, r2));
    }

    // for spells and such
    public void SwapTiles(int c1, int r1, int c2, int r2) {
        StartCoroutine(_SwapTiles(false, c1, r1, c2, r2));
    }

    public IEnumerator _SwapTiles( bool playerAction, int c1, int r1, int c2, int r2) {
        MMLog.Log_MageMatch("   ---------- SWAP BEGIN ----------");
        actionsPerforming++;
        if (hexGrid.Swap(c1, r1, c2, r2)) { // I feel like this check should be in InputCont
            yield return eventCont.Swap(playerAction, c1, r1, c2, r2); 
        }
        MMLog.Log_MageMatch("   ---------- SWAP END ----------");
        actionsPerforming--;
    }

    // TODO somewhere in here, spell prereq selection screen!
    public IEnumerator CastSpell(int spellNum) {
        MMLog.Log_MageMatch("   ---------- CAST SPELL BEGIN ----------");
        actionsPerforming++;
        syncManager.SendSpellCast(spellNum);

        Player p = activep;
        Spell spell = p.character.GetSpell(spellNum);
        if (p.AP >= spell.APcost) {
            targeting.selectionCanceled = false; // maybe not needed here

            MMLog.Log_MageMatch("spell cast spellNum=" + spellNum + ", spell count=" + spellsOnBoard[spellNum].Count);

            yield return targeting.SpellSelectScreen(spellsOnBoard[spellNum]);

            if (!targeting.selectionCanceled) {

                targeting.targetingCanceled = false;
                uiCont.DeactivateAllSpellButtons(); // ?
                p.SetCurrentSpell(spellNum);

                if (spell is CoreSpell)
                    yield return ((CoreSpell)spell).CastCore(targeting.GetSelection());
                else
                    yield return spell.Cast();

                if (!targeting.WasCanceled()) { // should be an event callback?
                    eventCont.SpellCast(spell);
                    StartCoroutine(uiCont.GetButtonCont(spellNum).Transition_MainView());
                    tileMan.RemoveSeq(targeting.GetSelection());
                    p.ApplySpellCosts();
                    yield return BoardChecking(); //?
                }
            }
        } else {
            uiCont.UpdateMoveText("Not enough AP to cast!");
        }
        MMLog.Log_MageMatch("   ---------- CAST SPELL END ----------");
        actionsPerforming--;
    }

    public IEnumerator CancelSpell() {
        targeting.CancelSelection();
        for (int i = 0; i < spellsOnBoard.Length; i++) {
            if (spellsOnBoard[i].Count > 0)
                uiCont.ActivateSpellButton(i);
        }
        yield return null;
    }

    public bool IsPerformingAction() { return actionsPerforming > 0; }

    #endregion


    public void PutTile(TileBehav tb, int col, int row, bool checkGrav = false) {
        if (hexGrid.IsCellFilled(col, row))
            tileMan.RemoveTile(col, row, false);
        tb.HardSetPos(col, row);
        // TODO move to tilesOnBoard obj
        if(checkGrav)
            BoardChanged();
    }

    //public void Transmute(int col, int row, Tile.Element element) {
    //    Destroy(hexGrid.GetTileBehavAt(col, row).gameObject);
    //    hexGrid.ClearTileBehavAt(col, row);
    //    TileBehav tb = GenerateTile(element).GetComponent<TileBehav>();
    //    tb.ChangePos(col, row);
    //}

    public Player ActiveP() {
        return activep;
    }

    public Player InactiveP() {
        if (activep.id == 1)
            return p2;
        else
            return p1;
    }

    public Player GetPlayer(int id) {
        if (id == 1)
            return p1;
        else
            return p2;
    }

    public Player GetOpponent(int id) {
        return GetPlayer(OpponentId(id));
    }

    public int OpponentId(int id) {
        if (id == 1)
            return 2;
        else
            return 1;
    }

    public Player LocalP() { return GetPlayer(myID); }

    public bool MyTurn() { return activep.id == myID; }

    public bool IsMe(int id) { return id == myID; }

    // move to BoardCheck?
    public string[] GetTileSeqs(List<TileSeq> seqs) {
        string[] ss = new string[seqs.Count];
        TileSeq seq = seqs[0];
        for (int i = 0; i < seqs.Count; seq = seqs[i], i++) {
            ss[i] = seq.SeqAsString();
        }
        return ss;
    }

    public void BoardChanged() {
        eventCont.BoardAction();
    }

    public void EndTheGame() {
        endGame = true;
        uiCont.UpdateMoveText("Wow!! " + activep.name + " has won!!");
        uiCont.DeactivateAllSpellButtons();
        eventCont.boardAction -= OnBoardAction; //?
        timer.Pause();
    }

    public bool IsEnded() { return endGame; }

    public bool IsCommishTurn() { return currentTurn == Turn.CommishTurn; }
}
