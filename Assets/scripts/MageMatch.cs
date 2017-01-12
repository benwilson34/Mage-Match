using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class MageMatch : MonoBehaviour {

	public enum GameState { PlayerTurn, TargetMode, CommishTurn };
	public GameState currentState;

	public GameObject firePF, waterPF, earthPF, airPF, musclePF;      // tile prefabs
	public GameObject stonePF, emberPF, prereqPF, targetPF, zombiePF; // token prefabs
	public bool menu = false;         // is the settings menu open? ->move to UICont
	public GameObject currentTile;    // current game tile

	private Player p1, p2, activep;
    public Commish commish;
	private bool endGame = false;     // is either player dead?
	private int animating = 0;        // is something animating?
    private bool checking = false;
    bool turnSwitching = false;

    public BoardCheck boardCheck;
    public HexGrid hexGrid;
    public UIController uiCont;
    public EffectController effectCont;
    public AudioController audioCont;
    public Targeting targeting;
    public Stats stats;
    public EventController eventCont;
    // should SpellEffects instance be here?

	void Start () {
        uiCont = GameObject.Find("ui").GetComponent<UIController>();
        uiCont.Init();
        effectCont = new EffectController();
        targeting = new Targeting();
        audioCont = new AudioController();
		Reset ();
	}

	public void Reset(){ // initialize board/game
		//clear tiles
		GameObject tileFolder = transform.Find ("tilesOnBoard").gameObject;
		if (tileFolder != null)
			Destroy (tileFolder);
		tileFolder = new GameObject ("tilesOnBoard");
		tileFolder.transform.SetParent (this.transform);

		if(p1 != null)
            p1.EmptyHand();
		if(p2 != null)
            p2.EmptyHand();

        //HexGrid.Init(); // init game board
        hexGrid = new HexGrid();
        boardCheck = new BoardCheck(hexGrid);

		endGame = false;

		p1 = new Player (1);
		p1.DrawTiles (4);

		p2 = new Player (2);
		p2.DrawTiles (4);

        commish = new Commish();

        eventCont = new EventController();
        eventCont.boardAction += OnBoardAction;

        currentState = GameState.PlayerTurn;
		activep = p1;
		InactiveP().FlipHand ();
        uiCont.SetDrawButton(InactiveP(), false); // eh
		activep.InitAP();
		activep.DrawTiles (1);

        stats = new Stats(p1, p2);

        uiCont.Reset(p1, p2);
	}


	// ----------------- Update is called once per frame - MAIN GAME LOOP -----------------------
	void Update () {
		// if there is no winning player and the settings menu is not open
		if (!endGame && !menu && !IsTargetMode() && !IsAnimating()) { 
			switch (currentState) {

			//case GameState.BoardChecking:
			//	hexGrid.CheckGrav(); // TODO! move into v(that)v?
			//	if (hexGrid.IsGridAtRest ()) { // ...AND all the tiles are in place
			//		List<TileSeq> seqMatches = boardCheck.MatchCheck ();
			//		if (seqMatches.Count > 0) { // if there's at least one MATCH
			//			ResolveMatchEffects (seqMatches);
			//		} else {
			//			currentState = GameState.PlayerTurn;
			//			SpellCheck();
			//			uiCont.UpdateDebugGrid ();
			//		}
			//		uiCont.UpdatePlayerInfo(); // try to move out of main loop
			//	}
			//	break;

			case GameState.PlayerTurn:
				if (activep.AP == 0 && !turnSwitching) {
					StartCoroutine(TurnSystem());
				}
				uiCont.UpdatePlayerInfo(); // TODO don't call every frame!!!!!!
				uiCont.UpdateCommishMeter(); // same here kinda
				break;

			case GameState.CommishTurn:
				break;

			}
		}
	}
    // ------------------------------------------------------------------------------------------


    public void OnBoardAction() {
        if (!checking) {
            Debug.Log("MAGEMATCH: About to check the board.");
            StartCoroutine(BoardChecking());
        }
    }

    public IEnumerator BoardChecking() {
        checking = true; // prevents overcalling/retriggering
        while (true) {
            Debug.Log("boardcheck point 1");
            yield return new WaitUntil(() => !menu && !IsTargetMode() && !IsAnimating());
            Debug.Log("boardcheck point 2");
            hexGrid.CheckGrav(); // TODO! move into v(that)v?
            yield return new WaitUntil(() => hexGrid.IsGridAtRest());
            Debug.Log("boardcheck point 3");
            List<TileSeq> seqMatches = boardCheck.MatchCheck();
            if (seqMatches.Count > 0) { // if there's at least one MATCH
                ResolveMatchEffects(seqMatches);
            } else {
                if (currentState == GameState.PlayerTurn)
                    SpellCheck();
                uiCont.UpdateDebugGrid();
                break;
            }
            uiCont.UpdatePlayerInfo(); // try to move out of main loop
        }
        checking = false;
    }

	IEnumerator TurnSystem(){
        turnSwitching = true;
        yield return new WaitUntil(() => !checking);
		effectCont.ResolveEndTurnEffects ();
        eventCont.TurnChange(activep.id); //?
		uiCont.UpdateMoveText ("Completed turns: " + stats.turns);
		uiCont.DeactivateAllSpellButtons (activep);
        uiCont.SetDrawButton(activep, false);
		
		currentState = GameState.CommishTurn;
		yield return commish.CTurn(); // place 5 random tiles
		yield return new WaitUntil(() => animating == 0); // needed anymore?
//		Debug.Log("MAGEMATCH: Commish turn done.");

		activep = InactiveP ();
		activep.InitAP ();
		activep.FlipHand ();
        uiCont.SetDrawButton(activep, true);
        uiCont.FlipGradient (); // ugly
		activep.DrawTiles(1, false);
		effectCont.ResolveBeginTurnEffects ();

		SpellCheck ();
        yield return new WaitUntil(() => !checking); // fixes Commish match dmg bug...for now...
		currentState = GameState.PlayerTurn;
        turnSwitching = false;
	}

	void SpellCheck(){ // TODO clean up
        Character c = activep.character;
		List<TileSeq> spells = c.GetTileSeqList ();
		spells.AddRange (InactiveP ().character.GetTileSeqList ()); // probably not needed?
		spells = boardCheck.SpellCheck (spells);
		if (spells.Count > 0) {
			List<TileSeq> spellList = c.GetTileSeqList ();
			for (int s = 0; s < spellList.Count; s++) {
				TileSeq spellSeq = spellList [s];
				bool spellIsOnBoard = false;
				for (int i = 0; i < spells.Count; i++) {
					TileSeq matchSeq = spells [i];
					if (matchSeq.MatchesTileSeq (spellSeq)) {
						spellIsOnBoard = true;
						c.GetSpell (s).SetBoardSeq (matchSeq);
						break;
					}
				}

				if (spellIsOnBoard)
					uiCont.ActivateSpellButton (activep, s);
				else
					uiCont.DeactivateSpellButton (activep, s);
			}
		} else
			uiCont.DeactivateAllSpellButtons (activep);
	}

	void ResolveMatchEffects(List<TileSeq> seqList){
		Debug.Log("MAGEMATCH: At least one match: " + boardCheck.PrintSeqList(seqList));
		for (int i = 0; i < seqList.Count; i++) {
            if (currentState != GameState.CommishTurn) {
                eventCont.Match(activep.id, seqList.Count); // raise player Match event
		        activep.ResolveMatchEffect (); // match-based effects ->EventCont

                if (seqList[i].GetSeqLength() == 3) {
                    ActiveP().DealDamage(Random.Range(30, 50)); // diff 20
                    commish.ChangeMood(10);
                } else if (seqList[i].GetSeqLength() == 4) {
                    ActiveP().DealDamage(Random.Range(60, 85)); // diff 25
                    commish.ChangeMood(15);
                } else { // TODO critical matches
                    ActiveP().DealDamage(Random.Range(95, 125)); // diff 30
                    commish.ChangeMood(20);
                }
            } else {
                eventCont.CommishMatch(seqList.Count); // raise CommishMatch event
            }
		}
		RemoveSeqList (seqList);
	}

    public bool DrawTile() {
        if (activep.IsHandFull()) {
            uiCont.UpdateMoveText("Your hand is full!");
            return false;
        } else {
            activep.DrawTiles(1, false);
            eventCont.Draw(activep.id);
            activep.AP--;
            if (activep.AP == 0) {
                activep.FlipHand();
                uiCont.SetDrawButton(activep, false);
            }
            return true;
        }
    }

	public bool DropTile(int col){
		if (DropTile(col, currentTile, .08f)) {
			activep.hand.Remove (currentTile.GetComponent<TileBehav>()); // remove from hand
			//			CheckGrav (); //? eventually once gravity gets moved to this script?
			activep.AP--;
			if (activep.AP == 0) {
				activep.FlipHand ();
                uiCont.SetDrawButton(activep, false);
			}
			return true;
		} else
			return false;
	}

	public bool DropTile(int col, GameObject go, float dur){
		int row = boardCheck.CheckColumn (col); // check that column isn't full
		if (row >= 0) { // if the col is not full
			TileBehav currentTileBehav = go.GetComponent<TileBehav> ();
			currentTileBehav.SetPlaced();
			currentTileBehav.ChangePos (hexGrid.TopOfColumn(col) + 1, col, row, dur); // move to appropriate spot in column
			//BoardChanged(); //?
            eventCont.Drop(col); // etc...
			return true;
		}
		return false;
	}

	public void SwapTiles(int c1, int r1, int c2, int r2){
		if (!IsCommishTurn ()) {
			if (hexGrid.Swap (c1, r1, c2, r2)) {
				activep.AP--;
                if (activep.AP == 0) {
                    activep.FlipHand();
                    uiCont.SetDrawButton(activep, false); 
                }
				audioCont.SwapSound ();
			}
		}
	}

	// TODO
	public void PutTile(GameObject go, int col, int row){
		TileBehav currentTileBehav = go.GetComponent<TileBehav> ();
		if(hexGrid.IsSlotFilled(col, row))
			Destroy(hexGrid.GetTileBehavAt (col, row).gameObject);
		currentTileBehav.SetPlaced();
		currentTileBehav.HardSetPos (col, row);
		BoardChanged();
	}

    public void Transmute(int col, int row, Tile.Element element){
		Destroy(hexGrid.GetTileBehavAt (col, row).gameObject);
		hexGrid.ClearTileBehavAt (col, row);
		TileBehav tb = GenerateTile (element).GetComponent<TileBehav>();
		tb.ChangePos (col, row);
	}

	public Player ActiveP(){
		return activep;
	}

	public Player InactiveP(){
		if (activep.id == 1)
			return p2;
		else
			return p1;
	}

	public Player GetPlayer(int id){
		if (id == 1)
			return p1;
		else
			return p2;
	}

	public Player GetOpponent(int id){
		if (id == 1)
			return p2;
		else
			return p1;
	}

	public GameObject GenerateTile(Tile.Element element){
		return GenerateTile (element, GameObject.Find ("tileSpawn").transform.position);
	}

	public GameObject GenerateTile(Tile.Element element, Vector3 position){
		GameObject go;
		switch (element) {
		case Tile.Element.Fire:
			go = Instantiate (firePF);
			break;
		case Tile.Element.Water:
			go = Instantiate (waterPF);
			break;
		case Tile.Element.Earth:
			go = Instantiate (earthPF);
			break;
		case Tile.Element.Air:
			go = Instantiate (airPF);
			break;
		case Tile.Element.Muscle:
			go = Instantiate (musclePF);
			break;
		default:
			return null;
		}

		go.transform.position = position;

		return go;
	}

	public GameObject GenerateToken(string name){
		switch (name) {
		case "stone":
			return Instantiate (stonePF);
		case "ember":
			return Instantiate (emberPF);
        case "zombie":
            return Instantiate (zombiePF);
		case "prereq":
			return Instantiate (prereqPF);
		case "target":
			return Instantiate (targetPF);
		default:
			return null;
		}
	}

	public void CastSpell(int spellNum){ // IEnumerator?
		Player p = activep;
//		Spell spell = activep.loadout.GetSpell (spellNum);
		if (p.CastSpell(spellNum)) {
			commish.ChangeMood(45);
			uiCont.DeactivateAllSpellButtons (activep); // ?
			if(currentState != GameState.TargetMode){ // kinda shitty
				RemoveSeq (p.GetCurrentBoardSeq ());
				p.ApplyAPCost();
				//BoardChanged ();
			}
//			UIController.UpdateMoveText (activep.name + " casts " + spell.name + " for " + spell.APcost + " AP!!");
			uiCont.UpdatePlayerInfo(); // move to BoardChecking handling??
		} else {
			uiCont.UpdateMoveText ("Not enough AP to cast!");
		}
	}
		
	void RemoveSeqList(List<TileSeq> seqList){
		TileSeq seq;
		for(int seqInd = 0; seqInd < seqList.Count; seqInd++){
			seq = seqList[seqInd];
			RemoveSeq (seq);
		}
	}

	public void RemoveSeq(TileSeq seq){ // TODO messy stuff
		//Debug.Log ("MAGEMATCH: RemoveSeq() about to remove " + boardCheck.PrintSeq(seq, true));
		Tile tile;
		for (int i = 0; i < seq.sequence.Count;) {
			tile = seq.sequence [0];
			if (hexGrid.IsSlotFilled (tile.col, tile.row))
				RemoveTile (tile, true);
			else
				Debug.Log ("RemoveSeq(): The tile at (" + tile.col + ", " + tile.row + ") is already gone.");
			seq.sequence.Remove (tile);
		}
	}

	public void RemoveTile(Tile tile, bool resolveEnchant){
		RemoveTile(tile.col, tile.row, resolveEnchant);
	}

    public void RemoveTile(int col, int row, bool resolveEnchant) {
        //		Debug.Log ("Removing (" + col + ", " + row + ")");
        TileBehav tb = hexGrid.GetTileBehavAt(col, row);
        if (tb.HasEnchantment()) {
            if (resolveEnchant ) {
                Debug.Log("MAGEMATCH: About to resolve enchant on tile (" + col + ", " + row + ")");
                tb.ResolveEnchantment();
            }
            tb.ClearEnchantment(); // TODO
        }

		StartCoroutine (Remove_Anim (col, row, tb)); // FIXME hardcode
	}

	IEnumerator Remove_Anim(int col, int row, TileBehav tb){
		animating++;
		Tween swellTween = tb.transform.DOScale (new Vector3 (1.25f, 1.25f), .15f);
		tb.GetComponent<SpriteRenderer> ().DOColor (new Color (0, 1, 0, 0), .15f);
//		Camera.main.DOShakePosition (.1f, 1.5f, 20, 90, false);
		audioCont.BreakSound ();

		yield return swellTween.WaitForCompletion ();
		animating--;
		Destroy (tb.gameObject);
		hexGrid.ClearTileBehavAt(col, row);

        eventCont.TileRemove(activep.id, tb); //? not needed for checking but idk
        
		BoardChanged ();
	}

	public void BoardChanged(){
        //if(currentState != GameState.CommishTurn)
        //	currentState = GameState.BoardChecking;
        eventCont.BoardAction();
	}

	public void EndTheGame(){
		endGame = true;
		uiCont.UpdateMoveText ("Wow!! " + activep.name + " has won!!");
        uiCont.DeactivateAllSpellButtons (p1);
        uiCont.DeactivateAllSpellButtons (p2);
        eventCont.boardAction -= OnBoardAction; //?
	}

	public bool IsEnded(){
		return endGame;
	}

	public bool IsCommishTurn(){
//		if (currentState == GameState.CommishTurn)
//			Debug.Log ("IsCommishTurn evaluates to true!");
//		else
//			Debug.Log ("IsCommishTurn evaluates to false!");
		return currentState == GameState.CommishTurn;
	}

	public bool IsTargetMode(){
		return currentState == GameState.TargetMode;
	}

	public void IncAnimating(){
		animating++;
	}

	public void DecAnimating(){
		animating--;
	}

	public bool IsAnimating(){
		return animating > 0;
	}

	public void StartAnim(IEnumerator anim){
		StartCoroutine (anim);
	}
}
