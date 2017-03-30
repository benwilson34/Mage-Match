using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MageMatch : MonoBehaviour {

    public enum GameState { PlayerTurn, TargetMode, CommishTurn };
    public GameState currentState;
    [HideInInspector]
    public bool menu = false; // is the settings menu open? ->move to UICont
    [HideInInspector]
    public int myID;
    [HideInInspector]
    public bool commishTurn = false; // maybe make getters/setters?

    public Commish commish;
    public BoardCheck boardCheck;
    public HexGrid hexGrid;
    public UIController uiCont;
    public EffectController effectCont;
    public AudioController audioCont;
    public Targeting targeting;
    public Stats stats;
    public EventController eventCont;
    public TurnTimer timer;
    public MyTurnManager turnManager;
    public AnimationController animCont;
    // should SpellEffects instance be here?

    private GameObject firePF, waterPF, earthPF, airPF, muscPF;      // tile prefabs
    private GameObject stonePF, emberPF, zombiePF, prereqPF, targetPF; // token prefabs
    private Player p1, p2, activep;
    private bool endGame = false;     // is either player dead?
    private bool checking = false;

    void Start() {
        Random.InitState(1337);

        uiCont = GameObject.Find("ui").GetComponent<UIController>();
        uiCont.Init();
        timer = gameObject.GetComponent<TurnTimer>();
        effectCont = new EffectController();
        targeting = new Targeting();
        audioCont = new AudioController(this);
        LoadPrefabs();
        turnManager = GetComponent<MyTurnManager>();
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
            p1.EmptyHand();
        if (p2 != null)
            p2.EmptyHand();

        hexGrid = new HexGrid();
        boardCheck = new BoardCheck(hexGrid);

        endGame = false;

        p1 = new Player(1);
        p2 = new Player(2);
        activep = p1;

        commish = new Commish(this);

        // TODO make its own method
        eventCont = new EventController(this);
        eventCont.boardAction += OnBoardAction;
        eventCont.gameAction += OnGameAction;
        turnManager.InitEvents(this, eventCont);
        effectCont.InitEvents(eventCont);
        commish.InitEvents();
        uiCont.InitEvents();
        audioCont.InitEvents();
        p1.InitEvents();
        p2.InitEvents();

        for (int i = 0; i < 4; i++)
            LocalP().DealTile();

        currentState = GameState.PlayerTurn;
        uiCont.SetDrawButton(activep, true);
        activep.InitAP();
        if (MyTurn())
            activep.DealTile();
        uiCont.UpdatePlayerInfo();

        stats = new Stats(p1, p2);

        timer.InitTimer();
        uiCont.Reset(p1, p2);
    }

    public void LoadPrefabs() {
        firePF = Resources.Load("prefabs/tile_fire") as GameObject;
        waterPF = Resources.Load("prefabs/tile_water") as GameObject;
        earthPF = Resources.Load("prefabs/tile_earth") as GameObject;
        airPF = Resources.Load("prefabs/tile_air") as GameObject;
        muscPF = Resources.Load("prefabs/tile_muscle") as GameObject;

        stonePF = Resources.Load("prefabs/token_stone") as GameObject;
        emberPF = Resources.Load("prefabs/token_ember") as GameObject;
        zombiePF = Resources.Load("prefabs/token_zombie") as GameObject;
        prereqPF = Resources.Load("prefabs/outline_prereq") as GameObject;
        targetPF = Resources.Load("prefabs/outline_target") as GameObject;
    }

    #region EventCont calls
    public void OnBoardAction() {
        if (!checking) {
            Debug.Log("MAGEMATCH: About to check the board.");
            StartCoroutine(BoardChecking());
        }
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
            uiCont.UpdatePlayerInfo();
        }
    }
    #endregion

    IEnumerator TurnSystem() {
        Debug.Log("MAGEMATCH: Starting TurnSystem.");
        timer.Pause();
        yield return new WaitUntil(() => !checking);
        eventCont.TurnEnd();
        BoardChanged(); // why doesn't this happen when resolving turn effects?
        yield return new WaitUntil(() => !checking);
        uiCont.DeactivateAllSpellButtons(activep);
        uiCont.SetDrawButton(activep, false);

        currentState = GameState.CommishTurn;

        //Debug.Log("MAGEMATCH: TurnSystem: About to check for MyTurn...currentState="+currentState.ToString());
        if (MyTurn()) {
            //Debug.Log("MAGEMATCH: Turnsystem: My turn; waiting for the go-ahead from the other player.");
            yield return new WaitUntil(() => commishTurn);
            Debug.Log("MAGEMATCH: TurnSystem: My turn, about to start the Commish's turn.");
            yield return commish.CTurn(); // place 5 random tiles
        } else {
            turnManager.CommishTurnStart();
            commishTurn = true; //?
            Debug.Log("MAGEMATCH: Turnsystem: Not my turn, but the Commish's turn just started.");
            yield return new WaitUntil(() => !commishTurn);
        }

        yield return new WaitUntil(() => !checking); // needed anymore?

        activep = InactiveP();
        activep.InitAP();
        eventCont.TurnBegin();

        if (MyTurn()) {
            activep.DealTile();
        }

        SpellCheck();
        yield return new WaitUntil(() => !checking); // fixes Commish match dmg bug...for now...
        currentState = GameState.PlayerTurn;
        timer.InitTimer();
    }

    public IEnumerator BoardChecking() {
        checking = true; // prevents overcalling/retriggering
        int cascade = 0;
        while (true) {
            yield return new WaitUntil(() => !menu && !IsTargetMode() && !animCont.IsAnimating());
            hexGrid.CheckGrav(); // TODO! move into v(that)v?
            yield return new WaitUntil(() => hexGrid.IsGridAtRest());
            List<TileSeq> seqMatches = boardCheck.MatchCheck();
            if (seqMatches.Count > 0) { // if there's at least one MATCH
                ResolveMatchEffects(seqMatches);
                cascade++;
            } else {
                if (currentState == GameState.PlayerTurn) {
                    if (cascade > 1)
                        eventCont.Cascade(cascade);
                    SpellCheck();
                }
                uiCont.UpdateDebugGrid();
                break;
            }
            uiCont.UpdatePlayerInfo(); // try to move out of main loop
        }
        checking = false;
    }

    void SpellCheck() { // TODO clean up
        Character c = activep.character;
        List<TileSeq> spells = c.GetTileSeqList();
        spells.AddRange(InactiveP().character.GetTileSeqList()); // probably not needed?
        spells = boardCheck.SpellCheck(spells);
        if (spells.Count > 0) {
            List<TileSeq> spellList = c.GetTileSeqList();
            for (int s = 0; s < spellList.Count; s++) {
                TileSeq spellSeq = spellList[s];
                bool spellIsOnBoard = false;
                for (int i = 0; i < spells.Count; i++) {
                    TileSeq matchSeq = spells[i];
                    if (matchSeq.MatchesTileSeq(spellSeq)) {
                        spellIsOnBoard = true;
                        c.GetSpell(s).SetBoardSeq(matchSeq);
                        break;
                    }
                }

                if (spellIsOnBoard)
                    uiCont.ActivateSpellButton(activep, s);
                else
                    uiCont.DeactivateSpellButton(activep, s);
            }
        } else
            uiCont.DeactivateAllSpellButtons(activep);
    }

    void ResolveMatchEffects(List<TileSeq> seqList) {
        Debug.Log("MAGEMATCH: At least one match: " + boardCheck.PrintSeqList(seqList) + " and state="+currentState.ToString());
        if (currentState != GameState.CommishTurn) {
            Debug.Log("MAGEMATCH: Match was made by a player!!");
            eventCont.Match(GetTileSeqsLens(seqList)); // raise player Match event
        } else {
            eventCont.CommishMatch(GetTileSeqsLens(seqList)); // raise CommishMatch event
        }
        RemoveSeqList(seqList);
    }


    #region ----- Game Actions -----

    public bool DrawTile() {
        if (activep.IsHandFull()) {
            uiCont.UpdateMoveText("Your hand is full!");
            return false;
        } else {
            Tile.Element[] tileElem = activep.DrawTiles(1, Tile.Element.None, false, false);
            //eventCont.Draw(tileElem[0], false);
            //eventCont.GameAction();
            return true;
        }
    }

    public bool DropTile(int col, GameObject go) {
        if (DropTile(col, go, .08f)) {
            activep.hand.Remove(go.GetComponent<TileBehav>()); // remove from hand
            eventCont.GameAction(true); // could get moved into case below...
            return true;
        } else
            return false;
    }

    public bool DropTile(int col, GameObject go, float dur) {
        int row = boardCheck.CheckColumn(col); // check that column isn't full
        if (row >= 0) { // if the col is not full
            TileBehav currentTileBehav = go.GetComponent<TileBehav>();
            currentTileBehav.SetPlaced();
            currentTileBehav.ChangePos(hexGrid.TopOfColumn(col) + 1, col, row, dur); // move to appropriate spot in column
            if (currentState == GameState.PlayerTurn) //kinda hacky
                eventCont.Drop(currentTileBehav.tile.element, col); // etc...
            else if (currentState == GameState.CommishTurn)
                eventCont.CommishDrop(currentTileBehav.tile.element, col);
            return true;
        }
        return false;
    }

    public void SwapTiles(int c1, int r1, int c2, int r2) {
        if (!IsCommishTurn()) {
            if (hexGrid.Swap(c1, r1, c2, r2)) {
                eventCont.Swap(c1, r1, c2, r2);
                if (!menu) { // move to stats?
                    eventCont.GameAction(true);
                }
            }
        }
    }

    public void CastSpell(int spellNum) { // IEnumerator?
        Debug.Log("MAGEMATCH: CastSpell called...");
        Player p = activep;
        if (p.CastSpell(spellNum)) {
            commish.ChangeMood(45);
            uiCont.DeactivateAllSpellButtons(activep); // ?
            if (currentState != GameState.TargetMode) { // kinda shitty
                RemoveSeq(p.GetCurrentBoardSeq());
                p.ApplyAPCost();
            }
            //			UIController.UpdateMoveText (activep.name + " casts " + spell.name + " for " + spell.APcost + " AP!!");
            uiCont.UpdatePlayerInfo(); // move to BoardChecking handling??
        } else {
            uiCont.UpdateMoveText("Not enough AP to cast!");
        }
    }

    #endregion


    // TODO
    public void PutTile(GameObject go, int col, int row) {
        TileBehav currentTileBehav = go.GetComponent<TileBehav>();
        if (hexGrid.IsSlotFilled(col, row))
            Destroy(hexGrid.GetTileBehavAt(col, row).gameObject);
        currentTileBehav.SetPlaced();
        currentTileBehav.HardSetPos(col, row);
        BoardChanged();
    }

    public void Transmute(int col, int row, Tile.Element element) {
        Destroy(hexGrid.GetTileBehavAt(col, row).gameObject);
        hexGrid.ClearTileBehavAt(col, row);
        TileBehav tb = GenerateTile(element).GetComponent<TileBehav>();
        tb.ChangePos(col, row);
    }

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
            case "zombie":
                go = Instantiate(zombiePF);
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

    public void RemoveSeq(TileSeq seq) { // TODO messy stuff
                                         //Debug.Log ("MAGEMATCH: RemoveSeq() about to remove " + boardCheck.PrintSeq(seq, true));
        Tile tile;
        for (int i = 0; i < seq.sequence.Count;) {
            tile = seq.sequence[0];
            if (hexGrid.IsSlotFilled(tile.col, tile.row))
                RemoveTile(tile, true);
            else
                Debug.Log("RemoveSeq(): The tile at (" + tile.col + ", " + tile.row + ") is already gone.");
            seq.sequence.Remove(tile);
        }
    }

    public void RemoveTile(Tile tile, bool resolveEnchant) {
        StartCoroutine(_RemoveTile(tile.col, tile.row, resolveEnchant));
    }

    public void RemoveTile(int col, int row, bool resolveEnchant) {
        StartCoroutine(_RemoveTile(col, row, resolveEnchant));
    }

    IEnumerator _RemoveTile(int col, int row, bool resolveEnchant) {
        //		Debug.Log ("Removing (" + col + ", " + row + ")");
        TileBehav tb = hexGrid.GetTileBehavAt(col, row);
        if (tb.HasEnchantment()) {
            if (resolveEnchant) {
                Debug.Log("MAGEMATCH: About to resolve enchant on tile (" + col + ", " + row + ")");
                tb.ResolveEnchantment();
            }
            tb.ClearEnchantment(); // TODO
        }

        yield return animCont._RemoveTile(tb);

        Destroy(tb.gameObject);
        hexGrid.ClearTileBehavAt(col, row);
        eventCont.TileRemove(tb); //? not needed for checking but idk
        BoardChanged();
    }

    // move to BoardCheck?
    public int[] GetTileSeqsLens(List<TileSeq> seqs) {
        int[] lens = new int[seqs.Count];
        int i = 0;
        foreach (TileSeq seq in seqs) {
            lens[i] = seq.GetSeqLength();
            i++;
        }
        return lens;
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
    }

    public bool IsEnded() { return endGame; }

    public bool IsCommishTurn() { return currentState == GameState.CommishTurn; }

    public bool IsTargetMode() { return currentState == GameState.TargetMode; }

    public bool IsBoardChecking() { return checking; }

}
