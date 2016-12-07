using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class MageMatch : MonoBehaviour {

	public enum GameState { PlayerTurn, TargetMode, BoardChecking, CommishTurn };
	public static GameState currentState;

	public GameObject firePF, waterPF, earthPF, airPF, musclePF;  // tile prefabs
	public GameObject stonePF, emberPF, prereqPF, targetPF;       // token prefabs
	[HideInInspector] public static int turns;                 // number of current turn
	[HideInInspector] public static bool menu = false;         // is the settings menu open?
	[HideInInspector] public static GameObject currentTile;    // current game tile
	[HideInInspector] private static Player p1, p2, activep;
	[HideInInspector] public static List<Effect> beginTurnEffects, endTurnEffects;

	private static bool endGame = false;             	       // is either player dead?
//	private static bool boardChanged = false;                  // has the board changed?
//	private static bool commishTurn = false;                  // has the board changed?
	private static int animating = 0;                     // is something animating?

	void Start () {
		BoardCheck.Init ();

		//Loadout.Init ();
        Character.Init();
		Commish.Init ();

		UIController.Init ();
		Targeting.Init ();
		AudioController.Init ();
		Reset ();
	}

	public void Reset(){ // initialize board/game
		//clear tiles
		GameObject tileFolder = transform.Find ("tilesOnBoard").gameObject;
		if (tileFolder != null) {
			Destroy (tileFolder);
		}
		tileFolder = new GameObject ("tilesOnBoard");
		tileFolder.transform.SetParent (this.transform);
		if(p1 != null){ // empty player1 hand
			while (p1.hand.Count > 0) {
				Destroy (p1.hand [0].gameObject);
				p1.hand.RemoveAt (0);
			}
		}
		if(p2 != null){ // empty player2 hand
			while (p2.hand.Count > 0) {
				Destroy (p2.hand [0].gameObject);
				p2.hand.RemoveAt (0);
			}
		}

		HexGrid.Init(); // init game board
		beginTurnEffects = new List<Effect>(); // init beginning-of-turn effects
		endTurnEffects = new List<Effect>(); // init end-of-turn effects

		turns = 0;
		endGame = false;

		p1 = new Player (1);
		p1.DrawTiles (3);
		p1.AlignHand (.12f, true);

		p2 = new Player (2);
		p2.DrawTiles (3);
		p2.AlignHand (.12f, true);

		currentState = GameState.PlayerTurn;
		activep = p1;
		InactiveP().FlipHand ();
		activep.InitAP();
		activep.DrawTiles (2);

        Stats.Init(p1, p2);

		UIController.UpdateTurnText();
		UIController.UpdateDebugGrid ();
		UIController.UpdateMoveText ("");

		UIController.ShowLoadout (p1);
		UIController.ShowLoadout (p2);
		UIController.DeactivateAllSpellButtons (p1);
		UIController.DeactivateAllSpellButtons (p2);
	}


	// ---------------------------- Update is called once per frame - MAIN GAME LOOP ----------------------------
	void Update () {
		// if there is no winning player and the settings menu is not open
		if (!endGame && !menu && !IsTargetMode() && !IsAnimating()) { 
			switch (currentState) {

			case GameState.BoardChecking:
				HexGrid.CheckGrav(); // TODO! move into v(that)v?
				if (HexGrid.IsGridAtRest ()) { // ...AND all the tiles are in place
					List<TileSeq> seqMatches = BoardCheck.MatchCheck ();
					if (seqMatches.Count > 0) { // if there's at least one MATCH
						ResolveMatchEffects (seqMatches);
					} else {
						currentState = GameState.PlayerTurn;
						SpellCheck();
						UIController.UpdateDebugGrid ();
					}
					UIController.UpdatePlayerInfo(); // try to move out of main loop
				}
				break;

			case GameState.PlayerTurn:
				if (activep.AP == 0) {
					StartCoroutine(TurnSystem());
				}
				UIController.UpdatePlayerInfo(); // ditto here
				UIController.UpdateCommishMeter();
				break;

			case GameState.CommishTurn:
				break;

			}
		}
	}
	// -----------------------------------------------------------------------------------------------------------


	IEnumerator TurnSystem(){
		ResolveEndTurnEffects ();
		turns++;
		UIController.UpdateTurnText ();
		UIController.DeactivateAllSpellButtons (activep);
		
		currentState = GameState.CommishTurn;
		yield return Commish.CTurn(); // place 5 random tiles
//		Debug.Log ("TurnSystem: done placing tiles.");
		yield return new WaitUntil(() => animating == 0);
//		yield return WaitForAnims();
//		Debug.Log("MAGEMATCH: Commish turn done.");

		activep = InactiveP ();
		activep.InitAP ();
		activep.FlipHand ();
		UIController.FlipGradient (); // ugly
		activep.DrawTiles(2);
//		DealPlayerHand (activep, 2); // deal 2 tiles to activep at beginning of turn
		ResolveBeginTurnEffects ();

		SpellCheck ();
		currentState = GameState.BoardChecking;
	}

	void SpellCheck(){ // TODO clean up
        Character c = activep.character;
		List<TileSeq> spells = c.GetTileSeqList ();
		spells.AddRange (InactiveP ().character.GetTileSeqList ()); // probably not needed?
		spells = BoardCheck.SpellCheck (spells);
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
					UIController.ActivateSpellButton (activep, s);
				else
					UIController.DeactivateSpellButton (activep, s);
			}
		} else
			UIController.DeactivateAllSpellButtons (activep);
	}

	void ResolveMatchEffects(List<TileSeq> seqList){
		Debug.Log("MAGEMATCH: At least one match: " + BoardCheck.PrintSeqList(seqList));
		for (int i = 0; i < seqList.Count; i++) {
			if (currentState != GameState.CommishTurn) {
				activep.ResolveMatchEffect (); // match-based effects

				if (seqList [i].GetSeqLength () == 3) {
					InactiveP ().ChangeHealth (Random.Range (-100, -75));
					Commish.ChangeMood(10);
				} else if (seqList [i].GetSeqLength () == 4) {
					InactiveP ().ChangeHealth (Random.Range (-150, -125));
					Commish.ChangeMood(15);
				} else { // TODO critical matches
					InactiveP ().ChangeHealth (Random.Range (-225, -200));
					Commish.ChangeMood(20);
				}
			}
		}
		RemoveSeqList (seqList);
        Stats.IncMatch(activep.id, seqList.Count);
		//activep.matches += seqList.Count;
	}

	void ResolveBeginTurnEffects(){
		Effect effect;
		for (int i = 0; i < beginTurnEffects.Count; i++) {
			effect = beginTurnEffects [i];
			if (effect.ResolveEffect ()) { // if it's the last pass of the effect (turnsLeft == 0)
				beginTurnEffects.Remove (effect);
				if(effect is Enchantment)
					((Enchantment)effect).GetEnchantee ().ClearEnchantment ();
				i--;
			} else {
				Debug.Log ("MAGEMATCH: Beginning-of-turn effect " + i + " has " + effect.TurnsRemaining () + " turns left.");
			}
		}
	}
		
	void ResolveEndTurnEffects(){
		Effect effect;
		for (int i = 0; i < endTurnEffects.Count; i++) {
			effect = endTurnEffects [i];
			if (effect.ResolveEffect ()) { // if it's the last pass of the effect (turnsLeft == 0)
				endTurnEffects.Remove (effect);
				if(effect is Enchantment)
					((Enchantment)effect).GetEnchantee ().ClearEnchantment ();
				i--;
			} else {
				Debug.Log ("MAGEMATCH: End-of-turn effect " + i + " has " + effect.TurnsRemaining () + " turns left.");
			}
		}
	}
		
	public bool DropTile(int col){
		if (DropTile(col, currentTile, .08f)) {
			activep.hand.Remove (currentTile.GetComponent<TileBehav>()); // remove from hand
			//			CheckGrav (); //? eventually once gravity gets moved to this script?
			activep.AP--;
			if (activep.AP == 0) {
				activep.FlipHand ();
			}
			return true;
		} else
			return false;
	}

	public bool DropTile(int col, GameObject go, float dur){
		int row = BoardCheck.CheckColumn (col); // check that column isn't full
		if (row >= 0) { // if the col is not full
			TileBehav currentTileBehav = go.GetComponent<TileBehav> ();
			currentTileBehav.SetPlaced();
			currentTileBehav.ChangePos (HexGrid.TopOfColumn(col) + 1, col, row, dur); // move to appropriate spot in column
			BoardChanged();
			return true;
		}
		return false;
	}

	public void SwapTiles(int c1, int r1, int c2, int r2){
		if (!IsCommishTurn ()) {
			if (HexGrid.Swap (c1, r1, c2, r2)) {
				activep.AP--;
				if (activep.AP == 0)
					activep.FlipHand ();
				AudioController.SwapSound ();
			}
		}
	}

	// TODO
	public static void PutTile(GameObject go, int col, int row){
		TileBehav currentTileBehav = go.GetComponent<TileBehav> ();
		if(HexGrid.IsSlotFilled(col, row))
			Destroy(HexGrid.GetTileBehavAt (col, row).gameObject);
		currentTileBehav.SetPlaced();
		currentTileBehav.HardSetPos (col, row);
		BoardChanged();
	}

	public void Transmute(int col, int row, Tile.Element element){
		Destroy(HexGrid.GetTileBehavAt (col, row).gameObject);
		HexGrid.ClearTileBehavAt (col, row);
		TileBehav tb = GenerateTile (element).GetComponent<TileBehav>();
		tb.ChangePos (col, row);
	}

	public static Player ActiveP(){
		return activep;
	}

	public static Player InactiveP(){
		if (activep.id == 1)
			return p2;
		else
			return p1;
	}

	public static Player GetPlayer(int id){
		if (id == 1)
			return p1;
		else
			return p2;
	}

	public static Player GetOpponent(int id){
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
			Commish.ChangeMood(45);
			UIController.DeactivateAllSpellButtons (activep); // ?
			if(currentState != GameState.TargetMode){ // kinda shitty
				RemoveSeq (p.GetCurrentBoardSeq ());
				p.ApplyAPCost();
				BoardChanged ();
			}
//			UIController.UpdateMoveText (activep.name + " casts " + spell.name + " for " + spell.APcost + " AP!!");
			UIController.UpdatePlayerInfo(); // move to BoardChecking handling??
		} else {
			UIController.UpdateMoveText ("Not enough AP to cast!");
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
		Debug.Log ("MAGEMATCH: RemoveSeq() about to remove " + BoardCheck.PrintSeq(seq, true));
		Tile tile;
		for (int i = 0; i < seq.sequence.Count;) {
			tile = seq.sequence [0];
			if (HexGrid.IsSlotFilled (tile.col, tile.row))
				RemoveTile (tile, false, true);
			else
				Debug.Log ("RemoveSeq(): The tile at (" + tile.col + ", " + tile.row + ") is already gone.");
			seq.sequence.Remove (tile);
		}
	}

	public void RemoveTile(Tile tile, bool checkGrav, bool resolveEnchant){
		RemoveTile(tile.col, tile.row, checkGrav, resolveEnchant);
	}

	public void RemoveTile(int col, int row, bool checkGrav, bool resolveEnchant){
//		Debug.Log ("Removing (" + col + ", " + row + ")");
		TileBehav tb = HexGrid.GetTileBehavAt (col, row);
		if (resolveEnchant && tb.HasEnchantment ()) {
			Debug.Log ("MAGEMATCH: About to resolve enchant on tile (" + col + ", " + row + ")");
			tb.ResolveEnchantment ();
		}
//		if (tb.ResolveEnchantment ()) TODO

		StartCoroutine (Remove_Anim (col, row, tb, true)); // FIXME hardcode
	}

	IEnumerator Remove_Anim(int col, int row, TileBehav tb, bool checkGrav){
		animating++;
		Tween swellTween = tb.transform.DOScale (new Vector3 (1.25f, 1.25f), .15f);
		tb.GetComponent<SpriteRenderer> ().DOColor (new Color (0, 1, 0, 0), .15f);
//		Camera.main.DOShakePosition (.1f, 1.5f, 20, 90, false);
		AudioController.BreakSound ();

		yield return swellTween.WaitForCompletion ();
		animating--;
		Destroy (tb.gameObject);
		HexGrid.ClearTileBehavAt(col, row);

		if(checkGrav){
			BoardChanged ();
		}
	}

	public static void BoardChanged(){
//		boardChanged = true;
		if(currentState != GameState.CommishTurn)
			currentState = GameState.BoardChecking;
	}

	public static void EndTheGame(){
		endGame = true;
		UIController.UpdateMoveText ("Wow!! " + activep.pname + " has won!!");
		UIController.DeactivateAllSpellButtons (p1);
		UIController.DeactivateAllSpellButtons (p2);
	}

	public static bool IsEnded(){
		return endGame;
	}

	public static bool IsCommishTurn(){
//		if (currentState == GameState.CommishTurn)
//			Debug.Log ("IsCommishTurn evaluates to true!");
//		else
//			Debug.Log ("IsCommishTurn evaluates to false!");
		return currentState == GameState.CommishTurn;
	}

	public static bool IsTargetMode(){
		return currentState == GameState.TargetMode;
	}

	public static void IncAnimating(){
		animating++;
	}

	public static void DecAnimating(){
		animating--;
	}

	public static bool IsAnimating(){
		return animating > 0;
	}

	public void StartAnim(IEnumerator anim){
		StartCoroutine (anim);
	}
}
