using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class MageMatch : MonoBehaviour {

	public enum GameState { PlayerTurn, BoardChecking, CommishTurn };
	public static GameState currentState;

	public GameObject firePF, waterPF, earthPF, airPF, musclePF;  // tile prefabs
	[HideInInspector] public static int turns;                 // number of current turn
	[HideInInspector] public static bool menu = false;         // is the settings menu open?
	[HideInInspector] public static GameObject currentTile;    // current game tile
	[HideInInspector] public static Player p1, p2, activep;
	[HideInInspector] public static List<TurnEffect> beginTurnEffects, endTurnEffects;

	private static bool endGame = false;             	       // is either player dead?
//	private static bool boardChanged = false;                  // has the board changed?
//	private static bool commishTurn = false;                  // has the board changed?
	private static int animating = 0;                     // is something animating?

	void Start () {
		BoardCheck.Init ();
		BoardCheck.debugLogOn = false;

		Loadout.Init ();
		Commish.Init ();
		EnchantEffects.Init ();

		UIController.Init ();
//		InputController.Init ();
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
		beginTurnEffects = new List<TurnEffect>(); // init beginning-of-turn effects
		endTurnEffects = new List<TurnEffect>(); // init end-of-turn effects

		turns = 0;
		endGame = false;

		p1 = new Player (1);
		DealPlayerHand (p1, 3);
		p1.AlignHand (.12f, true);

		p2 = new Player (2);
		DealPlayerHand (p2, 3);
		p2.AlignHand (.12f, true);

		currentState = GameState.PlayerTurn;
		activep = p1;
		InactivePlayer().FlipHand ();
		activep.InitAP();
		DealPlayerHand (activep, 2);

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
		if (!endGame && !menu && !SpellEffects.IsTargetMode() && !IsAnimating()) { 
			switch (currentState) {

			case GameState.BoardChecking:
				HexGrid.CheckGrav(); // TODO! move into v(that)v?
				if (HexGrid.IsGridAtRest ()) { // ...AND all the tiles are in place
					List<TileSeq> seqMatches = BoardCheck.MatchCheck ();
					if (seqMatches.Count > 0) { // if there's at least one MATCH
						Debug.Log("At least one match: " + BoardCheck.PrintSeqList(seqMatches));
						ResolveMatchEffects (seqMatches);
					} else {
//						boardChanged = false;
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
				break;

			case GameState.CommishTurn:
				break;

			}
//			if (boardChanged) { // if the board changed (place, swap, etc)...
//				HexGrid.CheckGrav(); // TODO! move into v(that)v?
//				if (HexGrid.IsGridAtRest ()) { // ...AND all the tiles are in place
//					List<TileSeq> seqMatches = BoardCheck.MatchCheck ();
//					if (seqMatches.Count > 0) { // if there's at least one MATCH
//						Debug.Log("At least one match: " + BoardCheck.PrintSeqList(seqMatches));
//						ResolveMatchEffects (seqMatches);
//					} else {
//						boardChanged = false;
//						SpellCheck();
//						UIController.UpdateDebugGrid ();
//					}
//					UIController.UpdatePlayerInfo(); // try to move out of main loop
//				}
//			} else {
//				TurnSystem ();
//				UIController.UpdatePlayerInfo(); // ditto here
//			}
		}
	}
	// -----------------------------------------------------------------------------------------------------------


	IEnumerator TurnSystem(){ // TODO! needs work - should be a private class?
//		if (activep.AP == 0) { // if active player has just used their last AP
			ResolveEndTurnEffects ();
			turns++;
			UIController.UpdateTurnText ();
			UIController.DeactivateAllSpellButtons (activep);
			
//			commishTurn = true;
			currentState = GameState.CommishTurn;
			yield return Commish.Place_Tiles(); // place 5 random tiles
			Debug.Log("Commish turn done.");
			activep = InactivePlayer ();
			activep.InitAP ();
//		} else if (commishTurn) { // if the commissioner just had his turn
//			commishTurn = false;
			activep.FlipHand ();
			UIController.FlipGradient (); // ugly
			DealPlayerHand (activep, 2); // deal 2 tiles to activep at beginning of turn
			ResolveBeginTurnEffects ();

			SpellCheck ();
		currentState = GameState.PlayerTurn;
//		}
	}

	void SpellCheck(){ // TODO clean up
		List<TileSeq> spells = activep.loadout.GetTileSeqList ();
		spells.AddRange (InactivePlayer ().loadout.GetTileSeqList ()); //?
		spells = BoardCheck.SpellCheck (spells);
		if (spells.Count > 0) {
			List<TileSeq> spellList = activep.loadout.GetTileSeqList ();
			for (int s = 0; s < spellList.Count; s++) {
				TileSeq spellSeq = spellList [s];
				bool spellIsOnBoard = false;
				for (int i = 0; i < spells.Count; i++) {
					TileSeq matchSeq = spells [i];
					if (matchSeq.MatchesTileSeq (spellSeq)) {
						spellIsOnBoard = true;
						activep.loadout.GetSpell (s).SetBoardSeq (matchSeq);
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
		for (int i = 0; i < seqList.Count; i++) {
			// TODO handle enchantments, bigger sequences, critical matches...
//			if (!commishTurn) {
			if (currentState != GameState.CommishTurn) {
				if(seqList[i].GetSeqLength() == 3)
					InactivePlayer ().ChangeHealth (Random.Range (-100, -75));
				else if (seqList[i].GetSeqLength() == 4)
					InactivePlayer ().ChangeHealth (Random.Range (-150, -125));
				else // TODO critical matches
					InactivePlayer ().ChangeHealth (Random.Range (-225, -200));
			}
		}
		RemoveSeqList (seqList);
		activep.matches += seqList.Count;
	}

	void ResolveBeginTurnEffects(){
		TurnEffect turnEffect;
		for (int i = 0; i < beginTurnEffects.Count; i++) {
			turnEffect = beginTurnEffects [i];
			if (turnEffect.ResolveEffect ()) { // if it's the last pass of the effect (turnsLeft == 0)
				beginTurnEffects.Remove (turnEffect);
				turnEffect.GetEnchantee ().ClearEnchantment ();
				i--;
			} else {
				Debug.Log ("Beginning-of-turn effect has " + turnEffect.TurnsRemaining () + " turns left.");
			}
		}
	}
		
	void ResolveEndTurnEffects(){
		TurnEffect turnEffect;
		for (int i = 0; i < endTurnEffects.Count; i++) {
			turnEffect = endTurnEffects [i];
			if (turnEffect.ResolveEffect ()) { // if it's the last pass of the effect (turnsLeft == 0)
				endTurnEffects.Remove (turnEffect);
				turnEffect.GetEnchantee ().ClearEnchantment ();
				i--;
			} else {
				Debug.Log ("End-of-turn effect has " + turnEffect.TurnsRemaining () + " turns left.");
			}
		}
	}
		
	public bool PlaceTile(int col){
		if (PlaceTile(col, currentTile, .08f)) {
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

	public bool PlaceTile(int col, GameObject go, float dur){
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
		if (HexGrid.Swap (c1, r1, c2, r2)) {
			activep.AP--;
			if (activep.AP == 0)
				activep.FlipHand ();
			AudioController.SwapSound ();
		}
	}

	public Player InactivePlayer(){
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
		
	void DealPlayerHand(Player player, int numTiles){
		for (int i = 0; i < numTiles && player.hand.Count < player.handSize; i++) {
			GameObject go = GenerateTile (player.loadout.GetTileElement());
			if (player.id == 1)
				go.transform.position = new Vector3 (-5, 2);
			else if (player.id == 2)
				go.transform.position = new Vector3 (5, 2);
				
			go.transform.SetParent (GameObject.Find ("handslot" + player.id).transform, false);

			TileBehav tb = go.GetComponent<TileBehav> ();
			player.hand.Add (tb);
		}
		AudioController.PickupSound (this.GetComponent<AudioSource> ());
		player.AlignHand (.12f, true);
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

	public void DiscardTile(Player player, int num){
		int tilesInHand = player.hand.Count;
		for(int i = 0; i < num; i++){
			if (tilesInHand > 0) {
				int randomRoll = Random.Range (0, tilesInHand);
				GameObject go = player.hand[randomRoll].gameObject;
				player.hand.RemoveAt (randomRoll);
				Destroy(go);
			}
		}
	}

	public void CastSpell(int spellNum){ // TODO move player stuff (AP) to player.Cast()
		Spell spell = activep.loadout.GetSpell (spellNum);
		if (activep.CastSpell(spellNum)) {
			UIController.DeactivateAllSpellButtons (activep); // ?
			RemoveSeq (spell.GetBoardSeq ());
			BoardChanged ();
			HexGrid.CheckGrav (); //?
//			UIController.UpdateMoveText (activep.name + " casts " + spell.name + " for " + spell.APcost + " AP!!");
			UIController.UpdatePlayerInfo();
		} else {
			UIController.UpdateMoveText ("Not enough AP! That spell costs " + spell.APcost + " and you only have " + activep.AP);
		}
	}
		
	void RemoveSeqList(List<TileSeq> seqList){
		TileSeq seq;
		for(int seqInd = 0; seqInd < seqList.Count; seqInd++){
			seq = seqList[seqInd];
			RemoveSeq (seq);
		}
	}

	void RemoveSeq(TileSeq seq){ // TODO messy stuff
		Debug.Log ("RemoveSeq(): About to remove " + BoardCheck.PrintSeq(seq, true));
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
		Debug.Log ("Removing (" + col + ", " + row + ")");
		TileBehav tb = HexGrid.GetTileBehavAt (col, row);
		if (resolveEnchant && tb.HasEnchantment ()) {
			Debug.Log ("About to resolve enchant on tile (" + col + ", " + row + ")");
			tb.ResolveEnchantment ();
		}
//		if (tb.ResolveEnchantment ()) TODO

		StartCoroutine (Remove_Anim (col, row, tb, true)); // FIXME hardcode
	}

	public void Transmute(int col, int row, Tile.Element element){

		Destroy(HexGrid.GetTileBehavAt (col, row).gameObject);
		HexGrid.ClearTileBehavAt (col, row);
		TileBehav tb = GenerateTile (element).GetComponent<TileBehav>();
		tb.ChangePos (col, row);

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
		currentState = GameState.BoardChecking;
	}

	public static void EndTheGame(){
		endGame = true;
		UIController.UpdateMoveText ("Wow!! " + activep.name + " has won!!");
		UIController.DeactivateAllSpellButtons (p1);
		UIController.DeactivateAllSpellButtons (p2);
	}

	public static bool IsEnded(){
		return endGame;
	}

	public static bool IsCommishTurn(){
		return currentState == GameState.CommishTurn;
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