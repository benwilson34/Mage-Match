using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellEffects {

	private static MageMatch mm;
	private static int targets;
	private delegate void TargetEffect (TileBehav tb);
	private static TargetEffect targetEffect;
	
	public static void Init(){
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
	}

	public static bool IsTargetMode (){
		return targets > 0;
	}

	static void WaitForTargetClick(int count){
		targets = count;
		Debug.Log ("targets = " + targets);
	}

	public static void OnTargetClick(TileBehav tb){
		targets--;
		Debug.Log ("Targeted tile (" + tb.tile.col + ", " + tb.tile.row + ")");
		targetEffect (tb);
	}

	// -------------------------------------- SPELLS ---------------------------------------------

	public void Deal496Dmg(){
		mm.InactivePlayer ().ChangeHealth (-496);
	}

	public void WhiteHotComboKick(){
		targetEffect = WHCK_Target;
		WaitForTargetClick (3);
	}
	void WHCK_Target(TileBehav tb){
		if (tb.tile.element.Equals (Tile.Element.Fire))
			mm.InactivePlayer ().ChangeHealth (-100);
		else if (tb.tile.element.Equals (Tile.Element.Muscle)) { // TODO
//			mm.InactivePlayer.RandomDiscard(1);
			mm.InactivePlayer ().ChangeHealth (-70);
		} else
			mm.InactivePlayer ().ChangeHealth (-70);
		
		mm.RemoveTile (tb.tile, true);
	}

	public void LightningPalm(){ // TODO targeting
		targetEffect = LightningPalm_Target;
		WaitForTargetClick(1);
	}
	void LightningPalm_Target(TileBehav tb){
		List<TileBehav> tileList = BoardCheck.PlacedTileList ();
		Tile tile;
		for (int i = 0; i < tileList.Count; i++) {
			tile = tileList [i].tile;
			if (tile.element.Equals (tb.tile.element)) {
				mm.RemoveTile(tile, true);
				mm.InactivePlayer ().ChangeHealth (-15);
			}
		}
	}

	public void CaughtYouMirin(){
		MageMatch.activep.ChangeBuff_Dmg (.5f); // 50% damage multiplier
		MageMatch.endTurnEffects.Add( new TurnEffect (MageMatch.activep.id, 4, CaughtYouMirin_Turn, CaughtYouMirin_End, null));
	}
	void CaughtYouMirin_Turn(int id){ // technically this isn't needed
		MageMatch.GetPlayer(id).ChangeBuff_Dmg (.5f); // 50% damage multiplier
	}
	void CaughtYouMirin_End(int id){
		MageMatch.GetPlayer(id).ChangeBuff_Dmg (1f); // reset back to 100% dmg
	}

	//PLACEHOLDER EFFECT
	public void HotBody(){
		targetEffect = Hotbody_Target;
		WaitForTargetClick (3);
	}
	public void Hotbody_Target(TileBehav tb){
//		mm.Transmute (tb.tile.col, tb.tile.row, Tile.Element.Fire);
//		mm.DiscardTile(MageMatch.GetOpponent(MageMatch.activep.id));
		Ench_SetBurning(tb);
	}
		
	public void Cherrybomb(){
		targetEffect = Cherrybomb_Target;
		WaitForTargetClick(1);
	}
	void Cherrybomb_Target(TileBehav tb){
//		tb.SetEnchantment (Ench_Cherrybomb);
		Ench_SetCherrybomb(tb);
	}
	 
	public void Incinerate(){
		int burnCount = mm.InactivePlayer().hand.Count * 2;
		mm.DiscardTile (mm.InactivePlayer(), 2);
		targetEffect = Incinerate_Target;
		WaitForTargetClick (burnCount);
	}
	void Incinerate_Target(TileBehav tb){
		Ench_SetBurning (tb);
	}

	public void Magnitude10(){
		MageMatch.endTurnEffects.Add (new TurnEffect (MageMatch.activep.id, 3, Magnitude10_Turn, Magnitude10_End, null));
	}
	void Magnitude10_Turn(int id){
		int dmg = 0;
		for (int col = 0; col < 7; col++) {
			int row = HexGrid.BottomOfColumn (col);
			if (HexGrid.IsSlotFilled (col, row)) {
				Tile t = HexGrid.GetTileAt (col, row);
				if (!t.element.Equals (Tile.Element.Earth)) {
					mm.RemoveTile (t, true);
					dmg -= 15;
				}
			}
		}
		MageMatch.GetOpponent(id).ChangeHealth (dmg);
	}
	void Magnitude10_End(int id){
		Magnitude10_Turn (id);
	}

	public void Sinkhole(){

	}

	public void BoulderBarrage(){

	}

	public void Stalagmite(){

	}

	public void LivingFleshArmor(){

	}

	public void FigureFourLeglock(){

	}

	public static void Comm_Place5RandomTiles(){
		mm.StartAnim(Comm_Anim());
	}

	static IEnumerator Comm_Anim(){
		int numTiles = 5;
		int tries = 20;
		float[] ratios;
		yield return ratios = BoardCheck.EmptyCheck ();

		GameObject go = mm.GenerateTile (MageMatch.commish.loadout.GetTileElement());

		for (int i = 0; i < numTiles && tries > 0; i++) {
			if (tries == 20 && i != 0) {
				yield return new WaitForSeconds (.15f);
				go = mm.GenerateTile (MageMatch.commish.loadout.GetTileElement ());
			}

			int col = GetSemiRandomCol (ratios);
			if (!mm.PlaceTile (col, go, .15f)) { // if col is full
				i--;
				tries--;
			} else {
				go.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
				tries = 20;
			}
		}

		if (tries == 0) {
			Debug.Log ("The board is full. The Commissioner ends his turn early.");
			GameObject.Destroy (go);
		}
	}

	static int GetSemiRandomCol(float[] ratios){
		float val = Random.Range (0f, 1f);
//		Debug.Log ("GetSemiRandomCol: val = " + val);
		float thresh = 0;
		for (int i = 0; i < HexGrid.numCols; i++) {
			thresh += ratios [i];
			if (val < thresh)
				return i;
		}
		Debug.Log ("GetSemiRandomCol: shouldn't get to this point. val = " + val);
		return 6;
	}

	// -------------------------------- ENCHANTMENTS --------------------------------------

	public void Ench_SetCherrybomb(TileBehav tb){
		TurnEffect effect = new TurnEffect (MageMatch.activep.id, 0, null, null, Ench_Cherrybomb_Resolve);
		tb.SetEnchantment (effect);
		tb.GetComponent<SpriteRenderer> ().color = new Color (.4f, .4f, .4f);
	}
	void Ench_Cherrybomb_Resolve(int id, TileBehav tb){
		Tile tile = tb.tile;

		Debug.Log ("CHERRYBOMB tile = (" + tile.col + ", " + tile.row + ")");

		MageMatch.GetOpponent(id).ChangeHealth (-200);

		// Board N
		if (tile.row != HexGrid.TopOfColumn (tile.col)) { // Board N
			if (HexGrid.IsSlotFilled (tile.col, tile.row + 1)){
				mm.RemoveTile(tile.col, tile.row + 1, false);
			}
		}

		// Board NE
		if (tile.row != HexGrid.numRows - 1 && tile.col != HexGrid.numCols - 1) {
			if (HexGrid.IsSlotFilled (tile.col + 1, tile.row + 1))
				mm.RemoveTile(tile.col + 1, tile.row + 1, false);
		}

		// Board SE
		bool bottomcheck = !(tile.col >= 3 && tile.row == HexGrid.BottomOfColumn(tile.col));
		if (tile.col != HexGrid.numCols - 1 && bottomcheck) {
			if (HexGrid.IsSlotFilled (tile.col + 1, tile.row))
				mm.RemoveTile(tile.col + 1, tile.row, false);
		}

		// Board S
		if (tile.row != HexGrid.BottomOfColumn (tile.col)) {
			if (HexGrid.IsSlotFilled (tile.col, tile.row - 1))
				mm.RemoveTile(tile.col, tile.row - 1, false);
		}

		// Board SW
		if (tile.row != 0 && tile.col != 0) {
			if (HexGrid.IsSlotFilled (tile.col - 1, tile.row - 1))
				mm.RemoveTile(tile.col - 1, tile.row - 1, false);
		}

		// Board NW
		bool topcheck = !(tile.col <= 3 && tile.row == HexGrid.TopOfColumn (tile.col));
		if (tile.col != 0 && topcheck) {
			if (HexGrid.IsSlotFilled (tile.col - 1, tile.row))
				mm.RemoveTile(tile.col - 1, tile.row, false);
		}
	}

	public void Ench_SetBurning(TileBehav tb){
		// Burning does 3 dmg per tile per end-of-turn for 5 turns. It does double damage on expiration.
		TurnEffect effect = new TurnEffect (MageMatch.activep.id, 5, Ench_Burning_Turn, Ench_Burning_End, Ench_Burning_Cancel);
		tb.SetEnchantment (effect);
		tb.GetComponent<SpriteRenderer> ().color = new Color (1f, .4f, .4f);
		MageMatch.endTurnEffects.Add(effect);
	}
	void Ench_Burning_Turn(int id){
		MageMatch.GetOpponent(id).ChangeHealth (-3);
	}
	void Ench_Burning_End(int id){
		MageMatch.GetOpponent(id).ChangeHealth (-6);
	}
	void Ench_Burning_Cancel(int id, TileBehav tb){
		
	}
	
}
