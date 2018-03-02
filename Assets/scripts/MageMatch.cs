﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class MageMatch : MonoBehaviour {

    // how to handle performing action here? also better name for Normal?
    public enum State { Normal, Selecting, Targeting, NewsfeedMenu, DebugMenu, TurnSwitching };
    private Stack<State> _stateStack;

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
    public HexManager hexMan;
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

    private Player _p1, _p2, _activep;
    private Transform _tilesOnBoard;
    private bool _endGame = false;
    private int _checking = 0, _actionsPerforming = 0;
    private List<TileSeq>[] _spellsOnBoard;

    public DebugSettings debugSettings;
    public DebugTools debugTools;
    private bool isDebugMode = false;

    public delegate void LoadEvent();
    public event LoadEvent onEffectContReady;
    public event LoadEvent onEventContReady;

    void Start() {
        MMLog.Init(debugLogLevel);
        //CharacterInfo.Init();

        // set game to debug (single-client) mode if appropriate
        GameObject debugObj = GameObject.Find("debugSettings");
        if (debugObj != null) {
            debugSettings = debugObj.GetComponent<DebugSettings>();
            isDebugMode = true;

            debugTools = GameObject.Find("ToolsMenu").GetComponent<DebugTools>();
            debugTools.Init(this);

            MMLog.LogWarning("This scene is in debug mode!!");
            PhotonNetwork.offlineMode = true;
            PhotonNetwork.CreateRoom("debug");
            PhotonNetwork.JoinRoom("debug");
        }

        _stateStack = new Stack<State>();

        StartCoroutine(Reset());
    }

    public IEnumerator Reset() {
        _tilesOnBoard = GameObject.Find("tilesOnBoard").transform;
        gameSettings = GameObject.Find("gameSettings").GetComponent<GameSettings>();
        MMLog.Log_MageMatch("gamesettings: p1="+gameSettings.p1name + ",p1 char=" + gameSettings.p1char + ",p2="+gameSettings.p2name+",p2 char=" + gameSettings.p2char+",timer=" +gameSettings.turnTimerOn);

        if (IsDebugMode())
            myID = 1;
        else
            myID = PhotonNetwork.player.ID;

        uiCont = GameObject.Find("world ui").GetComponent<UIController>();
        uiCont.Init();
        timer = gameObject.GetComponent<TurnTimer>();
        
        targeting = new Targeting(this);
        prompt = new Prompt(this);
        audioCont = new AudioController(this);
        animCont = GetComponent<AnimationController>();

        syncManager = GetComponent<SyncManager>();
        syncManager.Init();
        yield return syncManager.SyncRandomSeed(Random.Range(0, 255));

        hexGrid = new HexGrid();
        hexMan = new HexManager(this);

        uiCont.GetCellOverlays();
        boardCheck = new BoardCheck(this);
        hexFX = new ObjectEffects(this);

        _endGame = false;

        _p1 = new Player(1);
        _p2 = new Player(2);
        _activep = _p1;

        commish = new Commish(this);

        effectCont = new EffectController(this);
        EffectContLoaded();
        InitEvents();

        currentTurn = Turn.PlayerTurn;
        uiCont.SetDrawButton(true);
        _activep.InitAP();

        if(!IsDebugMode())
            yield return syncManager.Checkpoint(); // idk if this is really doing anything

        uiCont.Reset();

        // TODO animate beginning of game
        for (int i = 0; i < 4; i++) {
            for (int p = 1; p <= 2; p++) {
                yield return GetPlayer(p).DealTile();
                yield return new WaitForSeconds(.1f);
            }
        }

        yield return _activep.DealTile();

        stats = new Stats(_p1, _p2);

        timer.StartTimer();

        inputCont = GetComponent<InputController>();
        EnterState(State.Normal);

        // TODO init some stuff that would otherwise be BeginTurnEvent()
        yield return null;
    }

    public void InitEvents() {
        eventCont = new EventController(this);
        EventContLoaded();

        eventCont.boardAction += OnBoardAction;
        eventCont.AddDropEvent(OnDrop, EventController.Type.GameAction, EventController.Status.End); // checking
        eventCont.AddSwapEvent(OnSwap, EventController.Type.GameAction, EventController.Status.End); // checking
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
        _p1.InitEvents();
        _p2.InitEvents();

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

    

    // GAME STATE

    // could have event if there are other things that depend on this
    public void EnterState(State state) {
        _stateStack.Push(state);
        inputCont.SwitchContext(state); 
    }

    public void ExitState() {
        State s = _stateStack.Pop();
        if (s == State.DebugMenu)
            BoardChanged();
        inputCont.SwitchContext(_stateStack.Peek());
    } 

    public State GetState() {
        return _stateStack.Peek();
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
        if (_checking == 0) { //?
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
                _activep.AP--;
            if (_activep.AP == 0) {
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
        Player p = GetPlayer(_activep.id);
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

        yield return new WaitUntil(() => _actionsPerforming == 0 && hexMan.removing == 0); //?
        yield return new WaitUntil(() => _checking == 0); //?
        MMLog.Log_MageMatch("<b>   ---------- TURNSYSTEM START ----------</b>");
        yield return eventCont.TurnEnd();

        uiCont.DeactivateAllSpellButtons(_activep.id); //? These should be part of any boardaction...
        uiCont.SetDrawButton(false);

        currentTurn = Turn.CommishTurn;
        yield return commish.CTurn();

        currentTurn = Turn.PlayerTurn;

        if (!Debug_OnePlayerMode()) {
            _activep = InactiveP();
            yield return uiCont.ShiftScreen();
        }

        yield return eventCont.TurnBegin();
        SpellCheck();
        timer.StartTimer();

        MMLog.Log_MageMatch("<b>   ---------- TURNSYSTEM END ----------</b>");
        ExitState();
        switchingTurn = false;
    }

    public IEnumerator BoardChecking(bool spellCheck = true) {
        _checking++; // prevents overcalling/retriggering
        yield return new WaitUntil(() => hexMan.removing == 0); //?
        MMLog.Log_MageMatch("About to check the board.");

        yield return new WaitUntil(() => !animCont.IsAnimating() && hexMan.removing == 0); //?
        hexGrid.CheckGrav(); // TODO make IEnum
        yield return new WaitUntil(() => hexGrid.IsGridAtRest());

        if (currentTurn == Turn.PlayerTurn && spellCheck)
            SpellCheck();

        uiCont.UpdateDebugGrid();

        MMLog.Log_MageMatch("Done checking.");
        _checking--;
    }

    void SpellCheck() { 
        Character c = _activep.character;
        int id = _activep.id;
        //List<TileSeq> spellSeqList = c.GetTileSeqList();
        _spellsOnBoard = boardCheck.CheckBoard(c.GetSpells());
        
        for (int s = 0; s < _spellsOnBoard.Length; s++) {
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
                    MMLog.Log_MageMatch("spell[" + s + "] count=" + _spellsOnBoard[s].Count);
                    if (_spellsOnBoard[s].Count > 0)
                        uiCont.ActivateSpellButton(id, s);
                    else
                        uiCont.DeactivateSpellButton(id, s); // needed?

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

    public void PlayerDrawHex() {
        StartCoroutine(_Draw(_activep.id, "", true));
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
        _actionsPerforming++;

        Player p = GetPlayer(id);
        if (p.hand.IsFull()) {
            MMLog.Log_MageMatch("Player " + id + "'s hand is full.");
        } else {
            yield return p.DrawTiles(1, genTag, playerAction, false);
        }

        MMLog.Log_MageMatch("   ---------- DRAW END ----------");
        _actionsPerforming--;
        yield return null;
    }

    public void PlayerDropTile(int col, Hex hex) {
        _activep.hand.Remove(hex);
        StartCoroutine(_Drop(true, col, hex));
    }

    public void DropTile(int col, Hex hex) {
        // remove from hand?
        StartCoroutine(_Drop(false, col, hex));
    }

    public void PlayerDropConsumable(Consumable cons) {
        _activep.hand.Remove(cons);
        StartCoroutine(_Drop(true, -1, cons));
    }

    public IEnumerator _Drop(bool playerAction, int col, Hex hex) {
        MMLog.Log_MageMatch("   ---------- DROP BEGIN ----------");
        _actionsPerforming++;

        hex.Reveal();

        if (currentTurn == Turn.PlayerTurn) //kinda hacky
            yield return eventCont.Drop(EventController.Status.Begin, playerAction, hex.hextag, col);

        if (Hex.IsConsumable(hex.hextag)) {
            _activep.hand.Remove(hex);
            Consumable cons = (Consumable)hex;

            // TODO animate? what should it look like when you use it?
            yield return cons.DropEffect();

            hexMan.RemoveHex(cons);
        } else {
            hex.transform.SetParent(_tilesOnBoard);
            TileBehav tb = hex.GetComponent<TileBehav>();
            tb.SetPlaced();
            yield return tb._ChangePosAndDrop(hexGrid.TopOfColumn(col), col, boardCheck.CheckColumn(col), .08f);
        }

        if (currentTurn == Turn.PlayerTurn) { //kinda hacky
            yield return eventCont.Drop(EventController.Status.End, playerAction, hex.hextag, col);
        } else if (currentTurn == Turn.CommishTurn)
            eventCont.CommishDrop(hex.hextag, col);

        syncManager.CheckHandContents(_activep.id);

        MMLog.Log_MageMatch("   ---------- DROP END ----------");
        _actionsPerforming--;
        yield return null;
    }

    //public void DropConsumable(Consumable cons) { StartCoroutine(_DropConsumable(cons)); }

    //IEnumerator _DropConsumable(Consumable cons) {
    //    MMLog.Log_MageMatch("   ---------- DROP CONSUMABLE BEGIN ----------");
    //    _actionsPerforming++;

    //    _activep.hand.Remove(cons);

    //    cons.Reveal();
    //    // TODO animate? what should it look like when you use it?


    //    // TODO eventCont begin
    //    yield return syncManager.OnDropLocal(_activep.id, true, cons.hextag, 0);
    //    yield return cons.DropEffect();
    //    // TODO eventCont end


    //    hexMan.RemoveHex(cons);


    //    //GameObject go = hex.gameObject;
    //    //go.transform.SetParent(_tilesOnBoard);
    //    //TileBehav tb = hex.GetComponent<TileBehav>();
    //    //tb.SetPlaced();

    //    //if (currentTurn == Turn.PlayerTurn) //kinda hacky
    //    //    yield return eventCont.Drop(EventController.Status.Begin, playerAction, hex.hextag, col);

    //    //yield return tb._ChangePosAndDrop(hexGrid.TopOfColumn(col), col, boardCheck.CheckColumn(col), .08f);

    //    //if (currentTurn == Turn.PlayerTurn) { //kinda hacky
    //    //    yield return eventCont.Drop(EventController.Status.End, playerAction, hex.hextag, col);
    //    //} else if (currentTurn == Turn.CommishTurn)
    //    //    eventCont.CommishDrop(tb.tile.element, col);

    //    //syncManager.CheckHandContents(_activep.id);

    //    MMLog.Log_MageMatch("   ---------- DROP CONSUMABLE END ----------");
    //    _actionsPerforming--;
    //    yield return null;
    //}

    public void PlayerSwapTiles(int c1, int r1, int c2, int r2) {
        StartCoroutine(_SwapTiles(true, c1, r1, c2, r2));
    }

    public IEnumerator _SwapTiles( bool playerAction, int c1, int r1, int c2, int r2) {
        MMLog.Log_MageMatch("   ---------- SWAP BEGIN ----------");
        _actionsPerforming++;

        yield return eventCont.Swap(EventController.Status.Begin, playerAction, c1, r1, c2, r2);

        TileBehav tb1 = hexGrid.GetTileBehavAt(c1, r1);
        TileBehav tb2 = hexGrid.GetTileBehavAt(c2, r2);
        audioCont.TileSwap(tb1.GetComponent<AudioSource>());

        hexGrid.Swap(c1, r1, c2, r2);
        tb1.ChangePos(c2, r2);
        yield return tb2._ChangePos(c1, r1);

        yield return eventCont.Swap(EventController.Status.End, playerAction, c1, r1, c2, r2);
         
        MMLog.Log_MageMatch("   ---------- SWAP END ----------");
        _actionsPerforming--;
    }

    public IEnumerator _CastSpell(int spellNum) {
        MMLog.Log_MageMatch("   ---------- CAST SPELL BEGIN ----------");
        _actionsPerforming++;
        syncManager.SendSpellCast(spellNum);

        Player p = _activep;
        uiCont.SetDrawButton(false);
        Spell spell = p.character.GetSpell(spellNum);
        if (p.AP >= spell.APcost) {
            targeting.selectionCanceled = false; // maybe not needed here

            MMLog.Log_MageMatch("spell cast spellNum=" + spellNum + ", spell count=" + _spellsOnBoard[spellNum].Count);

            uiCont.TurnOffSpellButtonsDuringCast(_activep.id, spellNum);

            yield return targeting.SpellSelectScreen(_spellsOnBoard[spellNum]);

            if (!targeting.selectionCanceled) {
                uiCont.DeactivateAllSpellButtons(_activep.id); // ?

                TileSeq prereq = targeting.GetSelection();
                //TileSeq seqCopy = seq.Copy(); //?
                hexMan.SetInvokedSeq(prereq);

                yield return eventCont.SpellCast(EventController.Status.Begin, spell, prereq);

                yield return spell.Cast(prereq);

                yield return eventCont.SpellCast(EventController.Status.End, spell, prereq);

                StartCoroutine(uiCont.GetButtonCont(_activep.id, spellNum).Transition_MainView());

                p.ApplySpellCosts(spell);
                hexMan.RemoveInvokedSeq(prereq);
                yield return BoardChecking(); //?
            }

            uiCont.TurnOnSpellButtonsAfterCast(_activep.id, spellNum);
        } else {
            uiCont.UpdateMoveText("Not enough AP to cast!");
        }

        uiCont.SetDrawButton(true);
        MMLog.Log_MageMatch("   ---------- CAST SPELL END ----------");
        _actionsPerforming--;
    }

    public IEnumerator _CancelSpell() {
        targeting.CancelSelection();
        for (int i = 0; i < _spellsOnBoard.Length; i++) {
            if (_spellsOnBoard[i].Count > 0)
                uiCont.ActivateSpellButton(_activep.id, i);
        }
        yield return null;
    }

    public bool IsPerformingAction() { return _actionsPerforming > 0; }

    #endregion


    public void PutTile(TileBehav tb, int col, int row, bool checkGrav = false) {
        if (hexGrid.IsCellFilled(col, row))
            hexMan.RemoveTile(col, row, false);
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
        return _activep;
    }

    public Player InactiveP() {
        if (_activep.id == 1)
            return _p2;
        else
            return _p1;
    }

    public Player GetPlayer(int id) {
        if (id == 1)
            return _p1;
        else
            return _p2;
    }

    public Player GetOpponent(int id) {
        return GetPlayer(OpponentId(id));
    }

    public Character GetPC(int id) { return GetPlayer(id).character; }

    //public Character GetOpponentPC(int id) { return GetOpponent(id).character; }

    public int OpponentId(int id) {
        if (id == 1)
            return 2;
        else
            return 1;
    }

    public Player LocalP() { return GetPlayer(myID); }

    public bool MyTurn() { return _activep.id == myID; }

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
        _endGame = true;
        audioCont.GameEnd();
        uiCont.UpdateMoveText("Wow!! " + _activep.name + " has won!!");
        uiCont.DeactivateAllSpellButtons(1);
        uiCont.DeactivateAllSpellButtons(2);
        eventCont.boardAction -= OnBoardAction; //?
        timer.Pause();
    }

    public bool IsEnded() { return _endGame; }

    public bool IsCommishTurn() { return currentTurn == Turn.CommishTurn; }
}
