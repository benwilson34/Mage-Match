using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class MageMatch : MonoBehaviour {

    // how to handle performing action here? also better name for Normal?
    public enum State { Normal, BeginningOfGame, Selecting, Targeting, NewsfeedMenu, DebugMenu, TurnSwitching };
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
    public ReplayEngine replay;
    private bool _isDebugMode = false, _isReplayMode = false;

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
            _isDebugMode = true;

            if (debugSettings.replayMode) {
                _isReplayMode = true;
                replay = new ReplayEngine(this);
                replay.Load("2018-4-16_16-33-0");
            }

            MMLog.LogWarning("This scene is in debug mode!!");
            PhotonNetwork.offlineMode = true;
            PhotonNetwork.CreateRoom("debug");
            PhotonNetwork.JoinRoom("debug");
        }

        _stateStack = new Stack<State>();

        StartCoroutine(Reset());
    }

    public IEnumerator Reset() {
        inputCont = GetComponent<InputController>();
        EnterState(State.Normal);
        EnterState(State.BeginningOfGame);

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
        syncManager.Init(this);
        //yield return syncManager.SyncRandomSeed(Random.Range(0, 255));

        hexGrid = new HexGrid();
        hexMan = new HexManager(this);
        TileFilter.Init(hexGrid);

        debugTools = GameObject.Find("Debug").GetComponent<DebugTools>();
        debugTools.Init(this);

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
        //uiCont.SetDrawButton(true);
        //_activep.InitAP();

        if(!IsDebugMode())
            yield return syncManager.Checkpoint(); // idk if this is really doing anything

        uiCont.Reset();
        stats = new Stats(_p1, _p2);

        yield return _p1.deck.Shuffle();
        yield return _p2.deck.Shuffle();

        // TODO animate beginning of game
        for (int i = 0; i < 7; i++) {
            for (int pid = 1; pid <= 2; pid++) {
                //yield return GetPlayer(p).DealHex();
                yield return _Deal(pid);
                yield return animCont.WaitForSeconds(.1f);
            }
        }

        //yield return _Deal(_activep.id);

        timer.StartTimer();

        ExitState(); // end BeginningOfGame state

        yield return eventCont.TurnBegin(); // is this ok?

        if (IsReplayMode()) {
            uiCont.ToggleLoadingText(true);
            inputCont.SetBlocking(true);

            yield return replay.StartReplay();

            _isReplayMode = false;
            uiCont.ToggleLoadingText(false);
            inputCont.SetBlocking(false);
        }
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

        syncManager.InitEvents(eventCont);
        audioCont.InitEvents();
        effectCont.InitEvents();
        commish.InitEvents();
        uiCont.InitEvents();
        debugTools.InitEvents();
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

    public bool IsDebugMode() { return _isDebugMode; }

    public bool IsReplayMode() { return _isReplayMode; }

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

        debugTools.UpdateDebugGrid();

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
            if (sp is SignatureSpell && !((SignatureSpell)sp).IsReadyToCast()) {
                //Log(sp.name + " is signature, but not enough meter!");
                continue;
            }

            //MMLog.Log_MageMatch("spell[" + s + "] count=" + _spellsOnBoard[s].Count);
            if (_spellsOnBoard[s].Count > 0)
                uiCont.ActivateSpellButton(id, s);
            else
                uiCont.DeactivateSpellButton(id, s); // needed?

            // TODO boardseq stuff will be handled by the spell selection thing                
        }
    }


    #region ----- Game Actions -----

    public IEnumerator _Deal(int id) {
        yield return _Draw(id, 1, false, true);
    }

    public void PlayerDrawHex() {
        StartCoroutine(_Draw(_activep.id, 1, true));
    }

    public IEnumerator _Draw(int id, int count = 1, bool playerAction = false, bool dealt = false) {
        MMLog.Log_MageMatch("   ---------- DRAW BEGIN ----------");
        _actionsPerforming++;

        Player p = GetPlayer(id);
        if (p.hand.IsFull()) {
            MMLog.Log_MageMatch("Player " + id + "'s hand is full.");
        } else {
            //MMLog.Log_Player("p" + id + " drawing with genTag=" + genTag);
            for (int i = 0; i < count && !p.hand.IsFull(); i++) {
                yield return p.deck.ReadyNextHextag();
                string hextag = p.deck.GetNextHextag();
                Hex hex = hexMan.GenerateHex(id, hextag);
                hex.putBackIntoDeck = true;

                if (id != myID)
                    hex.Flip();

                //hex.transform.position = Camera.main.ScreenToWorldPoint(mm.uiCont.GetPinfo(id).position);

                yield return eventCont.Draw(EventController.Status.Begin, id, hex.hextag, playerAction, dealt);

                p.hand.Add(hex);
                // I feel like the draw anim should go here

                yield return hex.OnDraw(); // Quickdraw prompting/other effects?

                yield return eventCont.Draw(EventController.Status.End, id, hex.hextag, playerAction, dealt);

                if (playerAction)
                    eventCont.GameAction(true); //?
            }
            MMLog.Log_Player(">>>" + p.hand.NumFullSlots() + " slots filled...");
        }

        MMLog.Log_MageMatch("   ---------- DRAW END ----------");
        _actionsPerforming--;
        yield return null;
    }

    public IEnumerator _Duplicate(int id, string hextag) {
        Player p = GetPlayer(id);
        if (p.hand.IsFull()) {
            MMLog.Log_MageMatch("Player " + id + "'s hand is full. Duplicate failed.");
            yield break;
        }

        Hex hex = hexMan.GenerateHex(id, hextag);

        // TODO animate second hex being duped from first

        if (id != myID)
            hex.Flip();

        p.hand.Add(hex);

        yield return null;
    }



    public void PlayerDropTile(Hex hex, int col) {
        _activep.hand.Remove(hex);
        StartCoroutine(_Drop(hex, col, true));
    }

    public void DropTile(Hex hex, int col) {
        // remove from hand?
        StartCoroutine(_Drop(hex, col));
    }

    public void PlayerDropCharm(Charm charm) {
        _activep.hand.Remove(charm);
        StartCoroutine(_Drop(charm, -1, true));
    }

    public IEnumerator _Drop(Hex hex, int col, bool playerAction = false) {
        MMLog.Log_MageMatch("   ---------- DROP BEGIN ---------- ");
        _actionsPerforming++;

        hex.Reveal();

        if (currentTurn == Turn.PlayerTurn) //kinda hacky
            yield return eventCont.Drop(EventController.Status.Begin, playerAction, hex.hextag, col);

        if (Hex.IsCharm(hex.hextag)) {
            _activep.hand.Remove(hex);
            Charm cons = (Charm)hex;

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
        } 
        //else if (currentTurn == Turn.CommishTurn)
        //    eventCont.CommishDrop(hex.hextag, col);

        //syncManager.CheckHandContents(_activep.id);

        MMLog.Log_MageMatch("   ---------- DROP END ----------");
        _actionsPerforming--;
        yield return null;
    }

    public void CommishDrop(TileBehav tb, int col) {
        MMLog.Log_MageMatch("   ---------- DROP BEGIN ---------- ");
        _actionsPerforming++;

        tb.transform.SetParent(_tilesOnBoard);
        tb.SetPlaced();
        StartCoroutine(tb._ChangePosAndDrop(hexGrid.TopOfColumn(col), col, boardCheck.CheckColumn(col), .08f));

        MMLog.Log_MageMatch("   ---------- DROP END ----------");
        _actionsPerforming--;
    }



    public void PlayerSwapTiles(int c1, int r1, int c2, int r2) {
        StartCoroutine(_SwapTiles(true, c1, r1, c2, r2));
    }

    public IEnumerator _SwapTiles( bool playerAction, int c1, int r1, int c2, int r2) {
        MMLog.Log_MageMatch("   ---------- SWAP BEGIN ----------");
        _actionsPerforming++;

        yield return eventCont.Swap(EventController.Status.Begin, playerAction, c1, r1, c2, r2);

        TileBehav tb1 = hexGrid.GetTileBehavAt(c1, r1);
        TileBehav tb2 = hexGrid.GetTileBehavAt(c2, r2);
        audioCont.Trigger(AudioController.HexSoundEffect.Swap, tb1.GetComponent<AudioSource>());

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
        if (p.AP >= spell.APcost) { // maybe do this check before boardcheck so the button isn't on
            MMLog.Log_MageMatch("spell cast spellNum=" + spellNum + ", spell count=" + _spellsOnBoard[spellNum].Count);

            uiCont.TurnOffSpellButtonsDuringCast(_activep.id, spellNum);

            yield return GetSelectionAndCast(spellNum);

            uiCont.TurnOnSpellButtonsAfterCast(_activep.id, spellNum);
        } else {
            uiCont.ShowAlertText("Not enough AP to cast!");
        }

        uiCont.SetDrawButton(true);
        MMLog.Log_MageMatch("   ---------- CAST SPELL END ----------");
        _actionsPerforming--;
    }

    IEnumerator GetSelectionAndCast(int spellNum) {
        Spell spell = _activep.character.GetSpell(spellNum);

        TileSeq prereq;
        if (IsReplayMode()) {
            prereq = replay.GetSpellSelection();
        } else {
            targeting.selectionCanceled = false;
            yield return targeting.SpellSelectScreen(_spellsOnBoard[spellNum]);
            if (targeting.selectionCanceled)
                yield break;
            prereq = targeting.GetSelection();
        }

        uiCont.DeactivateAllSpellButtons(_activep.id); // ?

        //TileSeq seqCopy = seq.Copy(); //?
        hexMan.SetInvokedSeq(prereq);

        yield return eventCont.SpellCast(EventController.Status.Begin, spell, prereq);

        yield return spell.Cast(prereq);

        yield return eventCont.SpellCast(EventController.Status.End, spell, prereq);

        if (!IsReplayMode()) {
            StartCoroutine(uiCont.GetButtonCont(_activep.id, spellNum).Transition_MainView());
            targeting.ClearSelection();
        }

        _activep.ApplySpellCosts(spell);
        
        hexMan.RemoveInvokedSeq(prereq);
        yield return BoardChecking(); //?
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
        audioCont.Trigger(AudioController.OtherSoundEffect.GameEnd);
        uiCont.ShowAlertText("Wow!! " + _activep.name + " has won!!");
        uiCont.DeactivateAllSpellButtons(1);
        uiCont.DeactivateAllSpellButtons(2);
        eventCont.boardAction -= OnBoardAction; //?
        timer.Pause();
    }

    public bool IsEnded() { return _endGame; }

    public bool IsCommishTurn() { return currentTurn == Turn.CommishTurn; }
}
