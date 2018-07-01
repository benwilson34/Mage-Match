using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class MageMatch : MonoBehaviour {

    // how to handle performing action here? also better name for Normal?
    public enum State { Normal, BeginningOfGame, EndOfGame, Selecting, Targeting, NewsfeedMenu, DebugMenu, TurnSwitching };
    private Stack<State> _stateStack;

    public enum Turn { PlayerTurn, CommishTurn }; // MyTurn, OppTurn?
    public Turn currentTurn;
    public MMLog.LogLevel debugLogLevel = MMLog.LogLevel.Standard;
    [HideInInspector]
    public int myID;
    [HideInInspector]
    public bool switchingTurn = false;

    public GameSettings gameSettings;
    //public AnimationController animCont;
    public SyncManager syncManager;
    public TurnTimer timer;
    public UIController uiCont;
    public InputController inputCont;

    public int ActiveID { get {
            return currentTurn == Turn.PlayerTurn ? _activep.ID : Commish.COMMISH_ID;
        } }
    public Player ActiveP { get { return _activep; } }
    

    private Player _p1, _p2, _activep;
    private Transform _tilesOnBoard;
    private bool _endGame = false;
    private int _checking = 0, _actionsPerforming = 0;
    private List<TileSeq>[] _spellsOnBoard;

    public enum GameMode { Multiplayer, TrainingSingleChar, TrainingTwoChars };
    public GameMode gameMode = GameMode.Multiplayer;
    public DebugSettings debugSettings;
    public DebugTools debugTools;
    public bool IsDebugMode { get { return _isDebugMode; } }
    public bool IsReplayMode { get { return _isReplayMode; } }
    //public bool ControllingOneChar { get { return IsDebugMode ? debugSettings.IsOneCharMode : true; } }
    private bool _isDebugMode = false, _isReplayMode = false;

    public delegate void LoadEvent();
    private List<LoadEvent> _onEffectContLoaded;
    private List<LoadEvent> _onEventContLoaded;
    private List<LoadEvent> _onPlayersLoaded;


    #region ---------- INIT ----------
    void Start() {
        StartCoroutine(InitGame());
    }

    public IEnumerator InitGame() {
        _onEffectContLoaded = new List<LoadEvent>();
        _onEventContLoaded = new List<LoadEvent>();
        _onPlayersLoaded = new List<LoadEvent>();
        MMLog.Init(debugLogLevel);
        UserData.Init();
        //CharacterInfo.Init();

        gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        // set game to debug (single-client) mode if appropriate
        GameObject debugObj = GameObject.Find("DebugSettings");
        if (debugObj != null) {
            debugSettings = debugObj.GetComponent<DebugSettings>();
            _isDebugMode = true;
            MMLog.LogWarning("This scene is in debug mode!!");

            if (debugSettings.replayMode) {
                _isReplayMode = true;
                ReplayEngine.Init(this);
            }

            gameMode = debugSettings.IsOneCharMode ? GameMode.TrainingSingleChar : GameMode.TrainingTwoChars;

            PhotonNetwork.offlineMode = true;
        }

        myID = IsDebugMode ? 1 : PhotonNetwork.player.ID;

        inputCont = GetComponent<InputController>();
        inputCont.Init(this);
        uiCont = GameObject.Find("world ui").GetComponent<UIController>();
        uiCont.Init(this);

        _stateStack = new Stack<State>();
        EnterState(State.Normal);
        EnterState(State.BeginningOfGame);

        _tilesOnBoard = GameObject.Find("tilesOnBoard").transform;
        MMLog.Log_MageMatch(gameSettings.SettingsToString());

        RuneInfoLoader.InitInGameRuneInfo(gameSettings);

        timer = gameObject.GetComponent<TurnTimer>();
        
        Targeting.Init(this);
        Prompt.Init(this);
        AudioController.Init(this);
        //animCont = GetComponent<AnimationController>();
        AnimationController.Init(this);

        EventController.Init(this);
        EventContLoaded();
        InitEvents();

        syncManager = GetComponent<SyncManager>();
        syncManager.Init(this);

        Report.Init(this); // depends on Players, EventCont

        HexGrid.Init(this);
        HexManager.Init(this);
        //TileFilter.Init(); // depends on HexGrid

        debugTools = GameObject.Find("Debug").GetComponent<DebugTools>();
        debugTools.Init(this);

        //uiCont.GetCellOverlays(); // depends on HexGrid
        BoardCheck.Init(this);

        _endGame = false;

        Commish.Init(this);
        _p1 = new Player(this, 1);
        _p2 = new Player(this, 2);
        PlayersLoaded();

        Effect.Init(this);
        CommonEffects.Init(this);
        EffectManager.Init(this);
        EffectContLoaded();

        currentTurn = Turn.PlayerTurn;
        //uiCont.SetDrawButton(true);
        //_activep.InitAP();

        if(!IsDebugMode)
            yield return syncManager.Checkpoint(); // idk if this is really doing anything

        yield return _p1.Deck.Shuffle();
        yield return _p2.Deck.Shuffle();



        //yield return new WaitForSeconds(5);
        yield return uiCont.AnimateBeginningOfGame();

        const int initHandDraw = 4;
        for (int i = 0; i < initHandDraw; i++) {
            for (int pid = 1; pid <= 2; pid++) {
                //yield return GetPlayer(p).DealHex();
                if (i == initHandDraw - 1 && pid == 2)
                    yield return _Deal(pid);
                else
                    StartCoroutine(_Deal(pid));
                yield return new WaitForSeconds(.05f);
            }
        }

        if (!IsDebugMode)
            yield return CoinFlip();
        else
            _activep = _p1;

        timer.StartTimer();

        ExitState(); // end BeginningOfGame state

        yield return EventController.TurnBegin(); // is this ok?
        yield return _activep.OnTurnBegin();

        if (gameMode == GameMode.TrainingSingleChar)
            _activep.IncreaseAP(6);

        if (IsReplayMode) {
            uiCont.ToggleLoadingText(true);
            inputCont.SetBlocking(true);

            yield return ReplayEngine.StartReplay();

            _isReplayMode = false;
            uiCont.ToggleLoadingText(false);
            inputCont.SetBlocking(false);
        }
    }

    public void InitEvents() {
        EventController.boardAction += OnBoardAction;
        EventController.AddHandChangeEvent(OnHandChange, MMEvent.Behav.GameAction, MMEvent.Moment.End);
        EventController.AddDropEvent(OnDrop, MMEvent.Behav.GameAction, MMEvent.Moment.End); // checking
        EventController.AddSwapEvent(OnSwap, MMEvent.Behav.GameAction, MMEvent.Moment.End); // checking
        //EventController.gameAction += OnGameAction;
        EventController.AddTurnBeginEvent(OnTurnBegin, MMEvent.Behav.GameAction); // checking
        EventController.AddTurnEndEvent(OnTurnEnd, MMEvent.Behav.GameAction); // checking

        if (gameSettings.turnTimerOn)
            EventController.timeout += OnTimeout;
    }

    public void AddEffectContLoadEvent(LoadEvent ev) {
        AddLoadEvent(ref _onEffectContLoaded, ev);
    }
    void EffectContLoaded() {
        OnLoadEvent(ref _onEffectContLoaded);
    }

    public void AddEventContLoadEvent(LoadEvent ev) {
        AddLoadEvent(ref _onEventContLoaded, ev);
    }
    void EventContLoaded() {
        OnLoadEvent(ref _onEventContLoaded);
    }

    public void AddPlayersLoadEvent(LoadEvent ev) {
        AddLoadEvent(ref _onPlayersLoaded, ev);
    }
    void PlayersLoaded() {
        OnLoadEvent(ref _onPlayersLoaded);
    }

    void AddLoadEvent(ref List<LoadEvent> evList, LoadEvent ev) {
        if (evList == null) // null when the Load in question has already happened; call ev immediately
            ev();
        else                // otherwise, add the ev to the callback list and it'll get called on Load
            evList.Add(ev);
    }

    void OnLoadEvent(ref List<LoadEvent> evList) {
        foreach (LoadEvent ev in evList)
            ev();
        evList = null;
    }

    IEnumerator CoinFlip() {
        yield return syncManager.SyncRand(1, Random.Range(1, 3));
        int firstTurnId = syncManager.GetRand();

        _activep = GetPlayer(firstTurnId);

        yield return uiCont.AnimateCoinFlip();
    }
    #endregion


    #region ---------- GAME STATE ----------

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

    public bool IsCommishTurn() { return currentTurn == Turn.CommishTurn; }
    #endregion


    #region ---------- EVENT CALLBACKS ----------
    public void OnBoardAction() {
        if (_checking == 0) { //?
            StartCoroutine(BoardChecking());
        }
    }

    IEnumerator OnHandChange(HandChangeEventArgs args) {
        if (args.state == EventController.HandChangeState.PlayerDraw) {
            ApplyCost(_activep.ID, 1);
        }
        yield return null;
    }

    public IEnumerator OnDrop(DropEventArgs args) {
        yield return BoardChecking(); // should go below? if done in a spell, should there be a similar event to this one OnSpellCast that checks the board once the spell resolves?
        if (args.state == EventController.DropState.PlayerDrop) {
            int cost = 1;
            if (args.hex.Cat != Hex.Category.BasicTile)
                cost = RuneInfoLoader.GetPlayerRuneInfo(args.id, args.hex.Title).cost;
            ApplyCost(_activep.ID, cost);
        }
        yield return null;
    }

    public IEnumerator OnSwap(SwapEventArgs args) {
        yield return BoardChecking();
        if(args.state == EventController.SwapState.PlayerSwap)
            ApplyCost(_activep.ID, 1);
    }

    public void ApplyCost(int id, int cost) {
        if (gameMode == GameMode.TrainingSingleChar) // don't apply AP cost in one-char mode (infinite AP)
            return;

        //Debug.Log("MAGEMATCH: OnGameAction called!");
        if (cost > 0)
            _activep.DecreaseAP(cost);

        if (_activep.IsOutOfAP) {
            uiCont.SetDrawButton(_activep.ID, false);
            StartCoroutine(TurnSystem());
        }
    }

    public IEnumerator OnTurnBegin(int id) {
        yield return BoardChecking();
    }

    public IEnumerator OnTurnEnd(int id) {
        yield return BoardChecking();
    }

    public void OnTimeout(int id) {
        if (id == myID) {
            syncManager.TurnTimeout();
            TurnTimeout();
        }
    }
    #endregion


    public void TurnTimeout() {
        Player p = GetPlayer(_activep.ID);
        MMLog.Log_MageMatch(p.Name + "'s turn just timed out!");
        uiCont.SetDrawButton(_activep.ID, false);
        StartCoroutine(TurnSystem());
    }

    IEnumerator TurnSystem() {
        if (switchingTurn) // could just use state I think
            yield break;

        EnterState(State.TurnSwitching);

        switchingTurn = true;
        timer.Pause();

        yield return new WaitUntil(() => _actionsPerforming == 0 && HexManager.Removing == 0); //?
        yield return new WaitUntil(() => _checking == 0); //?
        MMLog.Log_MageMatch("<b>   ---------- TURNSYSTEM START ----------</b>");
        yield return EventController.TurnEnd();

        uiCont.DeactivateAllSpellButtons(_activep.ID); //? These should be part of any boardaction...
        uiCont.SetDrawButton(_activep.ID, false);
        yield return uiCont.CenterScreen();
        //yield return new WaitForSeconds(.15f);

        currentTurn = Turn.CommishTurn;
        yield return Commish.DropRandomTiles();
        yield return new WaitForSeconds(.5f);

        currentTurn = Turn.PlayerTurn;

        _activep = InactiveP();
        if (gameMode == GameMode.TrainingTwoChars)
            myID = _activep.ID;
        yield return uiCont.ShiftToActivePlayerSide();

        yield return EventController.TurnBegin();
        SpellCheck();
        timer.StartTimer();
        ExitState();
        yield return _activep.OnTurnBegin();

        MMLog.Log_MageMatch("<b>   ---------- TURNSYSTEM END ----------</b>");
        switchingTurn = false;
    }

    public IEnumerator BoardChecking(bool spellCheck = true) {
        _checking++; // prevents overcalling/retriggering
        yield return new WaitUntil(() => HexManager.Removing == 0); //?
        MMLog.Log_MageMatch("About to check the board.");

        yield return new WaitUntil(() => !AnimationController.IsAnimating && 
                                          HexManager.Removing == 0); //?
        HexGrid.CheckGrav(); // TODO make IEnum
        yield return new WaitUntil(() => HexGrid.IsGridAtRest());

        if (currentTurn == Turn.PlayerTurn && spellCheck)
            SpellCheck();

        debugTools.UpdateDebugGrid();

        MMLog.Log_MageMatch("Done checking.");
        _checking--;
    }

    void SpellCheck() { 
        Character c = _activep.Character;
        _spellsOnBoard = BoardCheck.CheckBoard(c.GetSpells());

        for (int s = 0; s < _spellsOnBoard.Length; s++) {
            Spell sp = _activep.Character.GetSpell(s);
            if (sp is SignatureSpell && !((SignatureSpell)sp).IsReadyToCast) {
                //Log(sp.name + " is signature, but not enough meter!");
                continue;
            }

            MMLog.Log_MageMatch("spell[" + s + "] count=" + _spellsOnBoard[s].Count);
            if (_spellsOnBoard[s].Count > 0)
                uiCont.ActivateSpellButton(_activep.ID, s);
            else
                uiCont.DeactivateSpellButton(_activep.ID, s); // needed?
        }
    }

    //void SetSpellButtons(int id, bool on) {
    //    for (int s = 0; s < _spellsOnBoard.Length; s++) {
    //        Spell sp = _activep.Character.GetSpell(s);
    //        if (sp is SignatureSpell && !((SignatureSpell)sp).IsReadyToCast) {
    //            //Log(sp.name + " is signature, but not enough meter!");
    //            continue;
    //        }

    //        //MMLog.Log_MageMatch("spell[" + s + "] count=" + _spellsOnBoard[s].Count);
    //        if (_spellsOnBoard[s].Count > 0)
    //            uiCont.ActivateSpellButton(id, s);
    //        else
    //            uiCont.DeactivateSpellButton(id, s); // needed?
    //    }
    //}


    #region ---------- DEAL/DRAW ----------

    public IEnumerator _Deal(int id) {
        yield return _Draw(id, 1, EventController.HandChangeState.TurnBeginDeal);
    }

    public void PlayerDrawHex() {
        StartCoroutine(_Draw(_activep.ID, 1, EventController.HandChangeState.PlayerDraw));
    }

    public IEnumerator _Draw(int id, int count = 1, EventController.HandChangeState state = EventController.HandChangeState.DrawFromEffect) {
        MMLog.Log_MageMatch("   ---------- DRAW BEGIN ----------");
        _actionsPerforming++;

        Player p = GetPlayer(id);
        if (p.Hand.IsFull) {
            MMLog.Log_MageMatch("Player " + id + "'s hand is full.");
        } else {
            //MMLog.Log_Player("p" + id + " drawing with genTag=" + genTag);
            for (int i = 0; i < count && !p.Hand.IsFull; i++) {
                //yield return p.deck.ReadyNextHextag();
                string hextag = p.Deck.GetNextHextag();
                Hex hex = HexManager.GenerateHex(id, hextag);
                hex.putBackIntoDeck = true;

                //hex.transform.position = Camera.main.ScreenToWorldPoint(mm.uiCont.GetPinfo(id).position);

                yield return EventController.HandChange(MMEvent.Moment.Begin, id, hex.hextag, state);

                p.Hand.Add(hex);
                // I feel like the draw anim should go here

                yield return hex.OnDraw(); // Quickdraw prompting/other effects?

                yield return EventController.HandChange(MMEvent.Moment.End, id, hex.hextag, state);
            }
            MMLog.Log_Player(">>>" + p.Hand.NumFullSlots() + " slots filled...");
        }

        MMLog.Log_MageMatch("   ---------- DRAW END ----------");
        _actionsPerforming--;
        yield return null;
    }

    public IEnumerator _Duplicate(int id, string hextag) {
        Player p = GetPlayer(id);
        if (p.Hand.IsFull) {
            MMLog.Log_MageMatch("Player " + id + "'s hand is full. Duplicate failed.");
            yield break;
        }

        Hex hex = HexManager.GenerateHex(id, hextag);

        // TODO animate second hex being duped from first

        p.Hand.Add(hex);

        yield return null;
    }
    #endregion


    #region ---------- DROP ----------

    public void PlayerDropTile(Hex hex, int col) {
        _activep.Hand.Remove(hex);
        StartCoroutine(_Drop(hex, col, EventController.DropState.PlayerDrop));
    }

    public void DropTile(Hex hex, int col) {
        // remove from hand?
        StartCoroutine(_Drop(hex, col));
    }

    public void CommishDropTile(Hex hex, int col) {
        // remove from hand?
        StartCoroutine(_Drop(hex, col, EventController.DropState.CommishDrop));
    }

    public void PlayerDropCharm(Charm charm) {
        _activep.Hand.Remove(charm);
        StartCoroutine(_Drop(charm, -1, EventController.DropState.PlayerDrop));
    }

    public IEnumerator _Drop(Hex hex, int col, EventController.DropState state = EventController.DropState.DropFromEffect) {
        MMLog.Log_MageMatch("   ---------- DROP BEGIN ---------- ");
        _actionsPerforming++;

        hex.Reveal();

        yield return EventController.Drop(MMEvent.Moment.Begin, hex, col, state);

        if (Hex.IsCharm(hex.hextag)) {
            _activep.Hand.Remove(hex);
            Charm cons = (Charm)hex;

            // TODO animate? what should it look like when you use it?
            yield return cons.DropEffect(); // could be popped out above

            HexManager.RemoveHex(cons);
        } else {
            hex.transform.SetParent(_tilesOnBoard);
            TileBehav tb = hex.GetComponent<TileBehav>();

            yield return tb.OnDrop(col); // could be popped out above

            tb.SetPlaced();
            yield return tb._ChangePosAndDrop(HexGrid.TopOfColumn(col), col, BoardCheck.CheckColumn(col), .08f);
        }

        yield return EventController.Drop(MMEvent.Moment.End, hex, col, state);

        //else if (currentTurn == Turn.CommishTurn)
        //    eventCont.CommishDrop(hex.hextag, col);

        //syncManager.CheckHandContents(_activep.id);

        MMLog.Log_MageMatch("   ---------- DROP END ----------");
        _actionsPerforming--;
        yield return null;
    }
    #endregion


    #region ---------- SWAP ----------

    public void PlayerSwapTiles(int c1, int r1, int c2, int r2) {
        StartCoroutine(_SwapTiles(c1, r1, c2, r2, EventController.SwapState.PlayerSwap));
    }

    public IEnumerator _SwapTiles(int c1, int r1, int c2, int r2, EventController.SwapState state = EventController.SwapState.SwapFromEffect) {
        MMLog.Log_MageMatch("   ---------- SWAP BEGIN ----------");
        _actionsPerforming++;

        yield return EventController.Swap(MMEvent.Moment.Begin, c1, r1, c2, r2, state);

        TileBehav tb1 = HexGrid.GetTileBehavAt(c1, r1);
        TileBehav tb2 = HexGrid.GetTileBehavAt(c2, r2);
        AudioController.Trigger(SFX.Hex.Swap);

        HexGrid.Swap(c1, r1, c2, r2);
        tb1.ChangePos(c2, r2);
        yield return tb2._ChangePos(c1, r1);

        yield return EventController.Swap(MMEvent.Moment.End, c1, r1, c2, r2, state);
         
        MMLog.Log_MageMatch("   ---------- SWAP END ----------");
        _actionsPerforming--;
    }
    #endregion


    #region ---------- SPELLS ----------

    public IEnumerator _CastSpell(int spellNum) {
        MMLog.Log_MageMatch("   ---------- CAST SPELL BEGIN ----------");
        _actionsPerforming++;
        syncManager.SendSpellCast(spellNum);

        Player p = _activep;
        uiCont.SetDrawButton(_activep.ID, false);
        Spell spell = p.Character.GetSpell(spellNum);
        if (p.AP >= spell.APcost) { // maybe do this check before boardcheck so the button isn't on
            MMLog.Log_MageMatch("spell cast spellNum=" + spellNum + ", spell count=" + _spellsOnBoard[spellNum].Count);

            uiCont.TurnOffSpellButtonsDuringCast(_activep.ID, spellNum);

            yield return GetSelectionAndCast(spellNum);

            uiCont.TurnOnSpellButtonsAfterCast(_activep.ID, spellNum);
        } else {
            uiCont.ShowAlertText("Not enough AP to cast!");
        }

        uiCont.SetDrawButton(_activep.ID, true);
        MMLog.Log_MageMatch("   ---------- CAST SPELL END ----------");
        _actionsPerforming--;
    }

    IEnumerator GetSelectionAndCast(int spellNum) {
        Spell spell = _activep.Character.GetSpell(spellNum);

        TileSeq prereq;
        if (IsReplayMode) {
            prereq = ReplayEngine.GetSpellSelection();
        } else {
            Targeting.selectionCanceled = false;
            yield return Targeting.SpellSelectScreen(_spellsOnBoard[spellNum]);
            if (Targeting.selectionCanceled)
                yield break;
            prereq = Targeting.GetSelection();
        }

        uiCont.DeactivateAllSpellButtons(_activep.ID); // ?

        //TileSeq seqCopy = seq.Copy(); //?
        HexManager.SetInvokedSeq(prereq);

        yield return EventController.SpellCast(MMEvent.Moment.Begin, spell, prereq);

        yield return spell.Cast(prereq);

        yield return EventController.SpellCast(MMEvent.Moment.End, spell, prereq);

        if (!IsReplayMode) {
            StartCoroutine(uiCont.GetButtonCont(_activep.ID, spellNum).Transition_MainView());
            Targeting.ClearSelection();
        }

        ApplySpellCosts(spell);

        HexManager.RemoveInvokedSeq(prereq);
        yield return BoardChecking(); //?
    }

    void ApplySpellCosts(Spell spell) {
        ApplyCost(_activep.ID, spell.APcost);

        if (spell is SignatureSpell) {
            int meterCost = ((SignatureSpell)spell).meterCost;
            MMLog.Log_Player("Applying meter cost...which is " + meterCost);
            _activep.Character.ChangeMeter(-meterCost);
        }
    }

    public IEnumerator _CancelSpell() {
        Targeting.CancelSelection();
        for (int i = 0; i < _spellsOnBoard.Length; i++) {
            if (_spellsOnBoard[i].Count > 0)
                uiCont.ActivateSpellButton(_activep.ID, i);
        }
        yield return null;
    }

    public bool IsPerformingAction() { return _actionsPerforming > 0; }

    #endregion


    #region ---------- PLAYERS ----------

    Player InactiveP() {
        if (_activep.ID == 1)
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

    public Character GetPC(int id) { return GetPlayer(id).Character; }

    //public Character GetOpponentPC(int id) { return GetOpponent(id).character; }

    public int OpponentId(int id) {
        if (id == 1)
            return 2;
        else
            return 1;
    }

    public Player LocalP() { return GetPlayer(myID); }

    public bool MyTurn() { return _activep.ID == myID; }

    public bool IsMe(int id) { return id == myID; }
    #endregion


    public void BoardChanged() {
        EventController.BoardAction();
    }

    public void EndTheGame(int losingPlayerId) {
        _endGame = true;
        timer.Pause();
        AudioController.Trigger(SFX.Other.GameEnd);
        EnterState(State.EndOfGame);
        uiCont.TriggerEndOfMatchScreens(losingPlayerId);

        //uiCont.ShowAlertText("Wow!! " + _activep.name + " has won!!");
        //uiCont.DeactivateAllSpellButtons(1);
        //uiCont.DeactivateAllSpellButtons(2);
        //eventCont.boardAction -= OnBoardAction; //?
    }

    public bool IsEnded() { return _endGame; }

    public void QuitGame() {
        // PUN leave room? Idk if it will complain

        // clear settings objs
        Destroy(GameObject.Find("GameSettings"));
        var go = GameObject.Find("DebugSettings");
        if (go != null)
            Destroy(go);

        SceneManager.LoadScene("Menu");
    }





    public void DEBUG_ShiftScreen() {
        _activep = GetOpponent(_activep.ID);
        StartCoroutine(uiCont.ShiftToActivePlayerSide());
    }

    public void DEBUG_EndGame() {
        EndTheGame(2);
    }

    public void DEBUG_ActivateSpell3() {
        uiCont.ActivateSpellButton(_p1.ID, 2);
        uiCont.ActivateSpellButton(_p2.ID, 2);
    }

    private bool _isGlowing = false;

    public void DEBUG_ToggleGlow() {
        var tbs = HexGrid.GetPlacedTiles();
        int count = Random.Range(0, tbs.Count);
        for (int i = 0; i < count; i++) {
            int rand = Random.Range(0, tbs.Count);
            tbs.RemoveAt(rand);
        }

        _isGlowing = !_isGlowing;
        if (_isGlowing) {
            TileGFX.SetGlowingTiles(tbs, TileGFX.GFXState.PrereqGlowing);
        } else {
            TileGFX.ClearGlowingTiles();
        }
    }
}
