﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class MageMatch : MonoBehaviour {

    public enum GameState { PlayerTurn, TargetMode, CommishTurn };
    public GameState currentState;
    public MMLog.LogLevel debugLogLevel = MMLog.LogLevel.Standard;
    [HideInInspector]
    public int myID;
    [HideInInspector]
    public bool switchingTurn = false;

    public GameSettings gameSettings;
    public SyncManager syncManager;
    public HexGrid hexGrid;
    public BoardCheck boardCheck;
    public Commish commish;
    public TurnTimer timer;
    public Targeting targeting;
    public EffectController effectCont;
    public EventController eventCont;
    public AudioController audioCont;
    public AnimationController animCont;
    public UIController uiCont;
    public Stats stats;
    public SpellEffects spellfx;
    //public MMLog mmdebug;

    private GameObject firePF, waterPF, earthPF, airPF, muscPF;      // tile prefabs
    private GameObject stonePF, emberPF, tombstonePF, prereqPF, targetPF; // token prefabs
    private Player p1, p2, activep;
    private Transform tilesOnBoard;
    private bool endGame = false;
    private int checking = 0, removing = 0, actionsPerforming = 0, matchesResolving = 0;
    private int cascade = 0; // should this be how this is handled?

    void Start() {
        //Random.InitState(1337420);
        MMLog.Init(debugLogLevel);

        tilesOnBoard = GameObject.Find("tilesOnBoard").transform;
        gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        MMLog.Log_MageMatch("gamesettings: p1="+gameSettings.p1name+",p2="+gameSettings.p2name+",timer="+gameSettings.turnTimerOn + ",localLeft=" + gameSettings.localPlayerOnLeft + ",hideOppHand=" + gameSettings.hideOpponentHand);

        uiCont = GameObject.Find("ui").GetComponent<UIController>();
        uiCont.Init();
        timer = gameObject.GetComponent<TurnTimer>();
        effectCont = new EffectController(this); //?
        targeting = new Targeting();
        audioCont = new AudioController(this);
        LoadPrefabs();
        syncManager = GetComponent<SyncManager>();
        animCont = GetComponent<AnimationController>();
        myID = PhotonNetwork.player.ID;

        Reset();
    }

    public void Reset() { // initialize board/game
        //clear tiles
        GameObject tileFolder = transform.Find("tilesOnBoard").gameObject;
        if (tileFolder != null)
            Destroy(tileFolder);
        tileFolder = new GameObject("tilesOnBoard");
        tileFolder.transform.SetParent(this.transform);

        if (p1 != null)
            p1.hand.Empty();
        if (p2 != null)
            p2.hand.Empty();

        hexGrid = new HexGrid();
        boardCheck = new BoardCheck(this);
        spellfx = new SpellEffects(this);

        endGame = false;

        p1 = new Player(1);
        p2 = new Player(2);
        activep = p1;

        commish = new Commish(this);

        InitEvents();

        for (int i = 0; i < 4; i++)
            LocalP().DealTile();

        currentState = GameState.PlayerTurn;
        uiCont.SetDrawButton(activep, true);
        activep.InitAP();
        if (MyTurn())
            activep.DealTile();

        stats = new Stats(p1, p2);

        timer.InitTimer();
        uiCont.Reset();

        // TODO init some stuff that would otherwise be BeginTurnEvent()
    }

    public void LoadPrefabs() {
        firePF = Resources.Load("prefabs/tile_fire") as GameObject;
        waterPF = Resources.Load("prefabs/tile_water") as GameObject;
        earthPF = Resources.Load("prefabs/tile_earth") as GameObject;
        airPF = Resources.Load("prefabs/tile_air") as GameObject;
        muscPF = Resources.Load("prefabs/tile_muscle") as GameObject;

        stonePF = Resources.Load("prefabs/token_stone") as GameObject;
        emberPF = Resources.Load("prefabs/token_ember") as GameObject;
        tombstonePF = Resources.Load("prefabs/token_tombstone") as GameObject;
        prereqPF = Resources.Load("prefabs/outline_prereq") as GameObject;
        targetPF = Resources.Load("prefabs/outline_target") as GameObject;
    }

    public void InitEvents() {
        eventCont = new EventController(this);
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

    #region EventCont calls
    public void OnBoardAction() {
        if (checking == 0) { //?
            StartCoroutine(BoardChecking());
        }
    }

    public IEnumerator OnDrop(int id, bool playerAction, Tile.Element elem, int col) {
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
        if (currentState != GameState.CommishTurn) { //?
            //Debug.Log("MAGEMATCH: OnGameAction called!");
            if (costsAP)
                activep.AP--;
            if (activep.AP == 0) {
                uiCont.SetDrawButton(activep, false);
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
        uiCont.SetDrawButton(activep, false);
        StartCoroutine(TurnSystem());
    }

    IEnumerator TurnSystem() {
        if (switchingTurn)
            yield break;

        switchingTurn = true;
        timer.Pause();

        yield return new WaitUntil(() => actionsPerforming == 0 && removing == 0); //?
        yield return new WaitUntil(() => checking == 0); //?
        MMLog.Log_MageMatch("<b>   ---------- TURNSYSTEM START ----------</b>");
        yield return eventCont.TurnEnd();

        uiCont.DeactivateAllSpellButtons(activep); //? These should be part of any boardaction...
        uiCont.SetDrawButton(activep, false);

        currentState = GameState.CommishTurn;
        yield return commish.CTurn();

        currentState = GameState.PlayerTurn;
        activep = InactiveP();
        yield return eventCont.TurnBegin();
        SpellCheck();
        timer.InitTimer();

        MMLog.Log_MageMatch("<b>   ---------- TURNSYSTEM END ----------</b>");
        switchingTurn = false;
    }

    public IEnumerator BoardChecking() {
        checking++; // prevents overcalling/retriggering
        yield return new WaitUntil(() => removing == 0); //?
        MMLog.Log_MageMatch("About to check the board.");

        cascade = 0;
        while (true) {
            yield return new WaitUntil(() => !animCont.IsAnimating() && removing == 0); //?
            hexGrid.CheckGrav(); // TODO make IEnum
            yield return new WaitUntil(() => hexGrid.IsGridAtRest());
            List<TileSeq> seqMatches = boardCheck.MatchCheck();
            if (seqMatches.Count > 0) { // if there's at least one MATCH
                MMLog.Log_MageMatch("Resolving matches...");
                yield return ResolveMatches(seqMatches);
                cascade++;
            } else {
                if (currentState == GameState.PlayerTurn) {
                    if (cascade > 1) {
                        eventCont.Cascade(cascade);
                        cascade = 0;
                    }
                    SpellCheck();
                }
                uiCont.UpdateDebugGrid();
                break;
            }
        }
        MMLog.Log_MageMatch("Done checking.");
        checking--;
    }

    void SpellCheck() { 
        Character c = activep.character;
        List<TileSeq> spellSeqList = c.GetTileSeqList();
        List<TileSeq> spellsOnBoard = boardCheck.SpellCheck(spellSeqList);
        
        for (int s = 0; s < spellSeqList.Count; s++) {
            Spell sp = c.GetSpell(s);
            //Debug.Log("MAGEMATCH: Checking "+sp.name+"...");
            bool spellIsOnBoard = false;
            if (sp is CoreSpell) {
                if (((CoreSpell)sp).IsReadyToCast()) {
                    //Debug.Log("MAGEMATCH: " + sp.name + " is core, and there's no effect with tag=" + sp.effectTag);
                    spellIsOnBoard = true;
                } else {
                    MMLog.Log_MageMatch(sp.name + " is core, but it's cooling down!");
                }                    
            } else { // could be cleaner.
                if (sp is SignatureSpell && !((SignatureSpell)sp).IsReadyToCast()) {
                    //Log(sp.name + " is signature, but not enough meter!");
                    continue;
                }
                for (int i = 0; i < spellsOnBoard.Count; i++) {
                    TileSeq matchSeq = spellsOnBoard[i];
                    if (matchSeq.MatchesTileSeq(sp.GetTileSeq())) {
                        spellIsOnBoard = true;
                        c.GetSpell(s).SetBoardSeq(matchSeq);
                        break;
                    }
                }
            }

            if (spellIsOnBoard)
                uiCont.ActivateSpellButton(activep, s);
            else
                uiCont.DeactivateSpellButton(activep, s); // needed?
        }
    }

    IEnumerator ResolveMatches(List<TileSeq> seqList) {
        matchesResolving++;
        MMLog.Log_MageMatch("   ---------- MATCH BEGIN ----------");
        MMLog.Log_MageMatch("At least one match: " + boardCheck.PrintSeqList(seqList) + " and state="+currentState.ToString());
        if (currentState != GameState.CommishTurn) {
            MMLog.Log_MageMatch("Match was made by a player!");
            string[] seqs = GetTileSeqs(seqList);
            RemoveSeqList(seqList); // hopefully putting this first makes it more responsive?
            yield return eventCont.Match(seqs); // raise player Match event
        } else {
            eventCont.CommishMatch(GetTileSeqs(seqList)); // raise CommishMatch event
            RemoveSeqList(seqList);
        }

        yield return BoardChecking();
        MMLog.Log_MageMatch("   ---------- MATCH END ----------");
        matchesResolving--;
    }


    #region ----- Game Actions -----

    public void PlayerDrawTile() {
        StartCoroutine(_Draw(activep.id, true));
    }
    
    // for spells and such
    // needed? just call mm.GetPlayer(id).DrawTiles()?
    public void DrawTile() {
        StartCoroutine(_Draw(activep.id, false));
    }

    public IEnumerator _Draw(int id, bool playerAction) {
        MMLog.Log_MageMatch("   ---------- DRAW BEGIN ----------");
        actionsPerforming++;
        Player p = GetPlayer(id);
        if (p.hand.IsFull()) {
            MMLog.Log_MageMatch("Player " + id + "'s hand is full.");
        } else {
            p.DrawTiles(1, Tile.Element.None, true, false, false);
        }
        MMLog.Log_MageMatch("   ---------- DRAW END ----------");
        actionsPerforming--;
        yield return null;
    }

    public bool PlayerDropTile(int col, GameObject go) {
        int row = boardCheck.CheckColumn(col); // check that column isn't full
        if (row >= 0) {
            activep.hand.Remove(go.GetComponent<TileBehav>()); // remove from hand
            StartCoroutine(_Drop(true, col, go));
            return true;
        }
        return false;
    }

    public bool DropTile(int col, GameObject go) {
        int row = boardCheck.CheckColumn(col); // check that column isn't full
        if (row >= 0) { // if the col is not full
            StartCoroutine(_Drop(false, col, go));
            return true;
        }
        return false;
    }

    IEnumerator _Drop(bool playerAction, int col, GameObject go) {
        MMLog.Log_MageMatch("   ---------- DROP BEGIN ----------");
        actionsPerforming++;
        go.transform.SetParent(tilesOnBoard);
        TileBehav tb = go.GetComponent<TileBehav>();
        tb.SetPlaced();
        tb.ChangePos(hexGrid.TopOfColumn(col) + 1, col, boardCheck.CheckColumn(col), .08f);
        if (currentState == GameState.PlayerTurn) { //kinda hacky
            yield return eventCont.Drop(playerAction, tb.tile.element, col);
        } else if (currentState == GameState.CommishTurn)
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

    public IEnumerator CastSpell(int spellNum) {
        MMLog.Log_MageMatch("   ---------- CAST SPELL BEGIN ----------");
        actionsPerforming++;
        syncManager.SendSpellCast(spellNum);

        Player p = activep;
        Spell spell = p.character.GetSpell(spellNum);
        if (p.AP >= spell.APcost) {
            targeting.canceled = false;
            uiCont.DeactivateAllSpellButtons(activep); // ?
            p.SetCurrentSpell(spellNum);

            yield return spell.Cast();

            if (!targeting.WasCanceled()) { // should be an event callback?
                eventCont.SpellCast(spell);
                RemoveSeq(p.GetCurrentBoardSeq());
                p.ApplySpellCosts();
                yield return BoardChecking(); //?
            }
//			UIController.UpdateMoveText (activep.name + " casts " + spell.name + " for " + spell.APcost + " AP!!");
        } else {
            uiCont.UpdateMoveText("Not enough AP to cast!");
        }
        MMLog.Log_MageMatch("   ---------- CAST SPELL END ----------");
        actionsPerforming--;
    }

    public bool IsPerformingAction() { return actionsPerforming > 0; }

    #endregion


    // TODO
    public void PutTile(GameObject go, int col, int row) {
        TileBehav currentTileBehav = go.GetComponent<TileBehav>();
        if (hexGrid.IsCellFilled(col, row))
            Destroy(hexGrid.GetTileBehavAt(col, row).gameObject);
        currentTileBehav.SetPlaced();
        currentTileBehav.HardSetPos(col, row);
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
        if (id == 1)
            return p2;
        else
            return p1;
    }

    public Player LocalP() { return GetPlayer(myID); }

    public bool MyTurn() { return activep.id == myID; }

    public bool IsMe(int id) { return id == myID; }

    public GameObject GenerateTile(Tile.Element element) {
        return GenerateTile(element, GameObject.Find("tileSpawn").transform.position);
    }

    public GameObject GenerateTile(Tile.Element element, Vector3 position) {
        GameObject go;
        switch (element) {
            case Tile.Element.Fire:
                go = Instantiate(firePF, position, Quaternion.identity);
                break;
            case Tile.Element.Water:
                go = Instantiate(waterPF, position, Quaternion.identity);
                break;
            case Tile.Element.Earth:
                go = Instantiate(earthPF, position, Quaternion.identity);
                break;
            case Tile.Element.Air:
                go = Instantiate(airPF, position, Quaternion.identity);
                break;
            case Tile.Element.Muscle:
                go = Instantiate(muscPF, position, Quaternion.identity);
                break;
            default:
                return null;
        }

        return go;
    }

    public GameObject GenerateToken(string name) {
        GameObject go;
        switch (name) {
            case "stone":
                go = Instantiate(stonePF);
                break;
            case "ember":
                go = Instantiate(emberPF);
                break;
            case "tombstone":
                go = Instantiate(tombstonePF);
                break;
            case "prereq":
                go = Instantiate(prereqPF);
                break;
            case "target":
                go = Instantiate(targetPF);
                break;
            default:
                return null;
        }

        return go;
    }

    void RemoveSeqList(List<TileSeq> seqList) {
        TileSeq seq;
        for (int seqInd = 0; seqInd < seqList.Count; seqInd++) {
            seq = seqList[seqInd];
            RemoveSeq(seq);
        }
    }

    void RemoveSeq(TileSeq seq) { // TODO messy stuff
        //Debug.Log ("MAGEMATCH: RemoveSeq() about to remove " + boardCheck.PrintSeq(seq, true));
        Tile tile;
        for (int i = 0; i < seq.sequence.Count;) {
            tile = seq.sequence[0];
            if (hexGrid.IsCellFilled(tile.col, tile.row))
                RemoveTile(tile, true);
            else
                Debug.Log("RemoveSeq(): The tile at (" + tile.col + ", " + tile.row + ") is already gone.");
            seq.sequence.Remove(tile);
        }
    }

    // TODO should be IEnums? Then just start the anim at the end?
    public void RemoveTile(Tile tile, bool resolveEnchant) {
        StartCoroutine(_RemoveTile(tile.col, tile.row, resolveEnchant));
    }

    public void RemoveTile(int col, int row, bool resolveEnchant) {
        StartCoroutine(_RemoveTile(col, row, resolveEnchant));
    }

    public IEnumerator _RemoveTile(int col, int row, bool resolveEnchant) {
        //		Debug.Log ("Removing (" + col + ", " + row + ")");
        removing++;

        TileBehav tb = hexGrid.GetTileBehavAt(col, row);

        if (tb == null) {
            MMLog.LogError("MAGEMATCH: RemoveTile tried to access a tile that's gone!");
            removing--;
            yield break;
        }
        if (!tb.ableDestroy) {
            MMLog.LogWarning("MAGEMATCH: RemoveTile tried to remove an indestructable tile!");
            removing--;
            yield break;
        }

        if (tb.HasEnchantment()) {
            if (resolveEnchant) {
                MMLog.Log_MageMatch("About to resolve enchant on tile " + tb.PrintCoord());
                tb.ResolveEnchantment();
            }
            tb.ClearEnchantment(); // TODO
            tb.ClearTileEffects(); //?
        }
        hexGrid.ClearTileBehavAt(col, row); // move up?

        yield return animCont._RemoveTile(tb); // just start it, don't yield?
        Destroy(tb.gameObject);
        eventCont.TileRemove(tb); //? not needed for checking but idk

        removing--;
    }

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
        uiCont.DeactivateAllSpellButtons(p1);
        uiCont.DeactivateAllSpellButtons(p2);
        eventCont.boardAction -= OnBoardAction; //?
        timer.Pause();
    }

    public bool IsEnded() { return endGame; }

    public bool IsCommishTurn() { return currentState == GameState.CommishTurn; }
}
