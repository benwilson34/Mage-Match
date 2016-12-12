using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellEffects {

	private static MageMatch mm;

	public static void Init(){
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
	}

	// -------------------------------------- SPELLS ---------------------------------------------

	public void Deal496Dmg(){
		MageMatch.InactiveP ().ChangeHealth (-496);
	}

	public void WhiteHotComboKick(){
		Targeting.WaitForTileTarget (3, WHCK_Target);
	}
	void WHCK_Target(TileBehav tb){
		MageMatch.InactiveP ().ChangeHealth (-70);
		
		if (tb.tile.element.Equals (Tile.Element.Fire)){
			// TODO spread Burning
			List<TileBehav> tbs =  HexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
			int burns = Mathf.Min (3, tbs.Count);
			int tries = 10; // TODO generalize this form of randomization. see Commish spell also.
			for (int i = 0; i < burns; i++) {
				int rand = Random.Range (0, tbs.Count);
				TileBehav ctb = tbs [rand];
				if (ctb.HasEnchantment ()) {
					tries--;
					i--;
				} else {
					Ench_SetBurning (ctb);
					tries = 10;
				}
				if (tries == 0)
					break;
			}
		} else if (tb.tile.element.Equals (Tile.Element.Muscle)) {
			MageMatch.InactiveP().DiscardRandom(1);
		}
		
		mm.RemoveTile (tb.tile, true, true);
	}

	// PLACEHOLDER
	public void Baila(){
		Targeting.WaitForTileAreaTarget (true, Baila_Target);
	}
	void Baila_Target(List<TileBehav> tbs){
        foreach(TileBehav tb in tbs)
		    Ench_SetBurning (tb);
	}

	// empty
	public void PhoenixFire(){
		
	}
	
	// TODO
	public void HotBody(){
		MageMatch.ActiveP().SetMatchEffect (3, Hotbody_Match);
	}
	public void Hotbody_Match(){
		// TODO threshold to prevent infinite loop
		List<TileBehav> tbs = HexGrid.GetPlacedTiles();
		for (int i = 0; i < 3; i++) {
			int rand = Random.Range (0, tbs.Count);
			if (tbs[rand].HasEnchantment()) {
				i--;
			} else {
				Ench_SetBurning (tbs [rand]);
			}
		}
	}

	public void HotAndBothered(){
		MageMatch.InactiveP ().ChangeBuff_DmgExtra (15);
		MageMatch.endTurnEffects.Add( new TurnEffect (5, HAB_Turn, HAB_End, null));
	}
	void HAB_Turn(int id){
		MageMatch.InactiveP ().ChangeBuff_DmgExtra (15); // technically not needed
	}
	void HAB_End(int id){
		MageMatch.InactiveP ().ChangeBuff_DmgExtra (0); // reset
	}

	public void Pivot(){
        MageMatch.endTurnEffects.Add(new TurnEffect(1, null, Pivot_End, null));
        MageMatch.ActiveP().SetMatchEffect(1, Pivot_Match);
    }
    void Pivot_End(int id) {
        MageMatch.ActiveP().ClearMatchEffect();
    }
	void Pivot_Match(){
		MageMatch.ActiveP().AP++;
	}
	
	public void Incinerate(){
		// TODO drag targeting
		int burnCount = MageMatch.InactiveP().hand.Count * 2;
		Debug.Log ("SPELLFX: Incinerate burnCount = " + burnCount);
		MageMatch.InactiveP().DiscardRandom (2);
		Targeting.WaitForDragTarget (burnCount, Incinerate_Target);
	}
	void Incinerate_Target(List<TileBehav> tbs){
		foreach(TileBehav tb in tbs)
			Ench_SetBurning (tb);
	}

    public void ZombieSynergy() {
        int count = 0;
        List<TileBehav> tbs = HexGrid.GetPlacedTiles();
        foreach (TileBehav tb in tbs) {
            if (tb.HasEnchantment() && 
                (tb.GetEnchType() == Enchantment.EnchType.Zombify ||
                tb.GetEnchType() == Enchantment.EnchType.ZombieTok)) {
                List<TileBehav> adjTBs = HexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
                foreach (TileBehav adjTB in adjTBs) {
                    if (adjTB.HasEnchantment() && 
                        (adjTB.GetEnchType() == Enchantment.EnchType.Zombify ||
                        adjTB.GetEnchType() == Enchantment.EnchType.ZombieTok)) {
                        count++;
                    }
                }
            }
        }
        Debug.Log("SPELLEFFECTS: Zombie Synergy has counted " + count + " adjacent zombs");
        MageMatch.ActiveP().DealDamage(count * 4);
    }

    public void HumanResources() {
        Targeting.WaitForTileAreaTarget(false, HumanResources_Target);
    }
    void HumanResources_Target(List<TileBehav> tbs) {
        foreach (TileBehav tb in tbs) {
            if (tb.tile.element == Tile.Element.Muscle) {
                if (tb.GetEnchType() != Enchantment.EnchType.Zombify &&
                    tb.GetEnchType() != Enchantment.EnchType.ZombieTok)
                    Ench_SetZombify(tb, false);
            }
        }
    }

    public void CompanyLuncheon() {
        Targeting.WaitForTileAreaTarget(false, CompanyLuncheon_Target);
    }
    void CompanyLuncheon_Target(List<TileBehav> tbs) {
        for (int i = 0; i < tbs.Count; i++) {
            TileBehav tb = tbs[i];
            if (!tb.HasEnchantment() || 
                tb.GetEnchType() != Enchantment.EnchType.Zombify ||
                tb.GetEnchType() != Enchantment.EnchType.ZombieTok) {
                tbs.Remove(tb);
                i--;
            }
        }
        foreach(TileBehav tb in tbs)
            tb.TriggerEnchantment();
    }

    public void RaiseZombie() {
        Targeting.WaitForCellTarget(1, RaiseZombie_Target);
    }
    void RaiseZombie_Target(CellBehav cb) {
        // TODO eventually HexGrid.RaiseColumn()
        // hardset bottom cell
        int col = cb.col;
        int bottomr = HexGrid.BottomOfColumn(col);
        GameObject zomb;
        zomb = mm.GenerateToken("zombie");
        zomb.transform.SetParent(GameObject.Find("tilesOnBoard").transform); // move to MM.PutTile
        MageMatch.PutTile(zomb, col, bottomr);
    }

	public void LightningPalm(){
		Targeting.WaitForTileTarget(1, LightningPalm_Target);
	}
	void LightningPalm_Target(TileBehav tb){
		List<TileBehav> tileList = HexGrid.GetPlacedTiles ();
		Tile tile;
		for (int i = 0; i < tileList.Count; i++) {
			tile = tileList [i].tile;
			if (tile.element.Equals (tb.tile.element)) {
				mm.RemoveTile(tile, true, true);
				MageMatch.InactiveP ().ChangeHealth (-15);
			}
		}
	}

	public void CaughtYouMirin(){
		MageMatch.ActiveP().ChangeBuff_DmgMult (.5f); // 50% damage multiplier
		MageMatch.endTurnEffects.Add( new TurnEffect (4, CaughtYouMirin_Turn, CaughtYouMirin_End, null));
	}
	void CaughtYouMirin_Turn(int id){ // technically this isn't needed
		MageMatch.GetPlayer(id).ChangeBuff_DmgMult (.5f); // 50% damage multiplier
	}
	void CaughtYouMirin_End(int id){
		MageMatch.GetPlayer(id).ChangeBuff_DmgMult (1f); // reset back to 100% dmg
	}

	public void Cherrybomb(){
		Targeting.WaitForTileTarget(1, Cherrybomb_Target);
	}
	void Cherrybomb_Target(TileBehav tb){
		Ench_SetCherrybomb(tb);
	}

	public void Magnitude10(){
		MageMatch.endTurnEffects.Add (new TurnEffect (3, Magnitude10_Turn, Magnitude10_End, null));
	}
	void Magnitude10_Turn(int id){
		int dmg = 0;
		for (int col = 0; col < 7; col++) {
			int row = HexGrid.BottomOfColumn (col);
			if (HexGrid.IsSlotFilled (col, row)) {
				Tile t = HexGrid.GetTileAt (col, row);
				if (!t.element.Equals (Tile.Element.Earth)) {
					mm.RemoveTile (t, true, true);
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
		Targeting.WaitForCellTarget(1, Stalagmite_Target);
	}
	void Stalagmite_Target(CellBehav cb){
		int col = cb.col;
		int bottomr = HexGrid.BottomOfColumn (col);
		// hardset bottom three cells of column
		GameObject stone;
		for (int i = 0; i < 3; i++){
			stone = mm.GenerateToken ("stone");
			stone.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
			MageMatch.PutTile (stone, col, bottomr + i);
		}
	}

	public void LivingFleshArmor(){

	}

	public void FigureFourLeglock(){

	}

	// -------------------------------- ENCHANTMENTS --------------------------------------

	public void Ench_SetCherrybomb(TileBehav tb){
		Enchantment ench = new Enchantment (null, null, Ench_Cherrybomb_Resolve);
        ench.SetTypeTier(Enchantment.EnchType.Cherrybomb, 2);
		tb.SetEnchantment (ench);
		tb.GetComponent<SpriteRenderer> ().color = new Color (.4f, .4f, .4f);
	}
	void Ench_Cherrybomb_Resolve(int id, TileBehav tb){
		Debug.Log ("SPELLEFFECTS: Resolving Cherrybomb at (" + tb.tile.col + ", " + tb.tile.row + ")");
		MageMatch.GetOpponent(id).ChangeHealth (-200);

		List<TileBehav> tbs = HexGrid.GetSmallAreaTiles (tb.tile.col, tb.tile.row);
		foreach(TileBehav ctb in tbs){
			if (ctb.ableDestroy)
				mm.RemoveTile (ctb.tile.col, ctb.tile.row, false, true);
		}
	}

	public void Ench_SetBurning(TileBehav tb){
        // Burning does 3 dmg per tile per end-of-turn for 5 turns. It does double damage on expiration.
        //		Debug.Log("SPELLEFFECTS: Setting burning...");
        Enchantment ench = new Enchantment(5, Ench_Burning_Turn, Ench_Burning_End, null);
        ench.SetTypeTier(Enchantment.EnchType.Burning, 1);
		tb.SetEnchantment (ench);
		tb.GetComponent<SpriteRenderer> ().color = new Color (1f, .4f, .4f);
		MageMatch.endTurnEffects.Add(ench);
	}
	void Ench_Burning_Turn(int id, TileBehav tb){
		MageMatch.GetOpponent(id).ChangeHealth (-3);
	}
	void Ench_Burning_End(int id, TileBehav tb){
		MageMatch.GetOpponent(id).ChangeHealth (-6);
	}

    public void Ench_SetZombify(TileBehav tb, bool skip){
        Enchantment ench = new Enchantment(Ench_Zombify_Turn, null, null);
        ench.SetTypeTier(Enchantment.EnchType.Zombify, 1);
        if (skip)
            ench.SkipCurrent();
        tb.SetEnchantment(ench);
        tb.GetComponent<SpriteRenderer>().color = new Color(0f, .4f, 0f);
        MageMatch.endTurnEffects.Add(ench);
    }
    void Ench_Zombify_Turn(int id, TileBehav tb) {
        // TODO filter list before rand
        List<TileBehav> tbs = HexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
        if (tbs.Count > 0){
            int tries = 15;
            TileBehav ctb;
            for (int i = 0; i < 1 && tries > 0; i++) {
                int rand = Random.Range(0, tbs.Count);
                ctb = tbs[rand];
                if (ctb.tile.element == Tile.Element.Muscle) {
                    if (!ctb.HasEnchantment() || 
                        (ctb.GetEnchType() != Enchantment.EnchType.Zombify &&
                        ctb.GetEnchType() != Enchantment.EnchType.ZombieTok)) {
                        mm.RemoveTile(ctb.tile, true, true);
                        MageMatch.GetPlayer(id).DealDamage(10);
                        MageMatch.GetPlayer(id).ChangeHealth(10);
                    }
                } else if (ctb.HasEnchantment()) { // TODO TB - ableEnchant
                    i--;
                    tries--;
                } else {
                    Ench_SetZombify(ctb, true);
                }
            }
        }
    }

    public void Ench_SetZombieTok(TileBehav tb) {
        Enchantment ench = new Enchantment(Ench_ZombieTok_Turn, null, null);
        ench.SetTypeTier(Enchantment.EnchType.ZombieTok, 3);
        tb.SetEnchantment(ench);
        MageMatch.endTurnEffects.Add(ench);
    }
    void Ench_ZombieTok_Turn(int id, TileBehav tb) {
        Ench_Zombify_Turn(id, tb);
        Ench_Zombify_Turn(id, tb);
    }

}