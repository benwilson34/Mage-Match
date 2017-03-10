using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellEffects {

	private MageMatch mm;
    private Targeting targeting;
    private HexGrid hexGrid;

	public SpellEffects(){
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
        targeting = mm.targeting;
        hexGrid = mm.hexGrid;
	}

	// -------------------------------------- SPELLS ------------------------------------------

	public void Deal496Dmg(){
		mm.ActiveP ().DealDamage (496, false);
	}

    public void StoneTest() {
        targeting.WaitForCellTarget(1, StoneTest_Target);
    }
    void StoneTest_Target(CellBehav cb) {
        GameObject stone;
        stone = mm.GenerateToken("stone");
        stone.transform.SetParent(GameObject.Find("tilesOnBoard").transform);
        mm.DropTile(cb.col, stone, .08f);
    }

    // ----- Enfuego Martin -----

	public void WhiteHotComboKick(){
		targeting.WaitForTileTarget (3, WHCK_Target);
	}
	void WHCK_Target(TileBehav tb){
		mm.ActiveP ().DealDamage (70, false);
		
		if (tb.tile.element.Equals (Tile.Element.Fire)){
			// TODO spread Burning
			List<TileBehav> tbs =  hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
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
			mm.InactiveP().DiscardRandom(1);
		}
		
		mm.RemoveTile (tb.tile, true);
	}

	// PLACEHOLDER
	public void Baila(){
        targeting.WaitForTileAreaTarget (true, Baila_Target);
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
		mm.ActiveP().SetMatchEffect (new MatchEffect(3, HotBody_Match, HotBody_End));
        mm.eventCont.turnEnd += HotBody_OnTurnChange;
	}
    public void HotBody_OnTurnChange(int id) {
        mm.GetPlayer(id).DealDamage(25, false); // should just be ChangeHealth(-25)?
        mm.GetOpponent(id).DealDamage(25, false);
    }
	void HotBody_Match(int id){
        // TODO still doesn't seem to function properly...seems to be whiffing at least once
        // maybe it's enchanting part of the match??
		List<TileBehav> tbs = hexGrid.GetPlacedTiles();
        for (int i = 0; i < tbs.Count; i++) {
            TileBehav tb = tbs[i];
            if (tb.HasEnchantment()) {
                tbs.Remove(tb);
                i--;
            }
        }
		for (int i = 0; i < 3; i++) {
			int rand = Random.Range (0, tbs.Count);
            Ench_SetBurning(tbs[rand]);
            tbs.RemoveAt(rand);
		}
	}
    void HotBody_End(int id) {
        mm.eventCont.turnEnd -= HotBody_OnTurnChange;
    }

	public void HotAndBothered(){
		mm.InactiveP ().ChangeBuff_DmgExtra (15);
        TurnEffect t = new TurnEffect(5, HAB_Turn, HAB_End, null);
        t.priority = 3;
        mm.effectCont.AddEndTurnEffect(t);
	}
	void HAB_Turn(int id){
		mm.InactiveP ().ChangeBuff_DmgExtra (15); // technically not needed
	}
	void HAB_End(int id){
		mm.InactiveP ().ChangeBuff_DmgExtra (0); // reset
	}

	public void Pivot(){
        TurnEffect t = new TurnEffect(1, null, Pivot_End, null);
        t.priority = 3;
        mm.effectCont.AddEndTurnEffect(t);

        mm.ActiveP().SetMatchEffect(new MatchEffect(1, Pivot_Match, null));
    }
    void Pivot_End(int id) {
        mm.ActiveP().ClearMatchEffect();
    }
	void Pivot_Match(int id){
		mm.ActiveP().AP++;
	}
	
	public void Incinerate(){
		// TODO drag targeting
		int burnCount = mm.InactiveP().hand.Count * 2;
		Debug.Log ("SPELLFX: Incinerate burnCount = " + burnCount);
		mm.InactiveP().DiscardRandom (2);
        targeting.WaitForDragTarget (burnCount, Incinerate_Target);
	}
	void Incinerate_Target(List<TileBehav> tbs){
		foreach(TileBehav tb in tbs)
			Ench_SetBurning (tb);
	}

    // ----- The Gravekeeper -----

    public void ZombieSynergy() {
        int count = 0;
        List<TileBehav> tbs = hexGrid.GetPlacedTiles();
        foreach (TileBehav tb in tbs) {
            if (tb.HasEnchantment() && 
                (tb.GetEnchType() == Enchantment.EnchType.Zombify ||
                tb.GetEnchType() == Enchantment.EnchType.ZombieTok)) {
                List<TileBehav> adjTBs = hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
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
        mm.ActiveP().DealDamage(count * 4, false);
    }

    public void HumanResources() {
        targeting.WaitForTileAreaTarget(false, HumanResources_Target);
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
        targeting.WaitForTileAreaTarget(false, CompanyLuncheon_Target);
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
        targeting.WaitForCellTarget(1, RaiseZombie_Target);
    }
    void RaiseZombie_Target(CellBehav cb) {
        // TODO eventually HexGrid.RaiseColumn()
        // hardset bottom cell
        GameObject zomb = mm.GenerateToken("zombie");
        zomb.transform.SetParent(GameObject.Find("tilesOnBoard").transform); // move to MM.PutTile
        //mm.PutTile(zomb, col, bottomr);
        mm.hexGrid.RaiseTileBehavIntoColumn(zomb.GetComponent<TileBehav>(), cb.col);
    }

    // ----- 

	public void LightningPalm(){
        targeting.WaitForTileTarget(1, LightningPalm_Target);
	}
	void LightningPalm_Target(TileBehav tb){
		List<TileBehav> tileList = hexGrid.GetPlacedTiles ();
		Tile tile;
		for (int i = 0; i < tileList.Count; i++) {
			tile = tileList [i].tile;
			if (tile.element.Equals (tb.tile.element)) {
				mm.RemoveTile(tile, true);
				mm.ActiveP ().DealDamage (15, false);
			}
		}
	}

	public void CaughtYouMirin(){
		mm.ActiveP().ChangeBuff_DmgMult (.5f); // 50% damage multiplier
        TurnEffect t = new TurnEffect(4, CaughtYouMirin_Turn, CaughtYouMirin_End, null);
        t.priority = 3;
        mm.effectCont.AddEndTurnEffect(t);
	}
	void CaughtYouMirin_Turn(int id){ // technically this isn't needed
		mm.GetPlayer(id).ChangeBuff_DmgMult (.5f); // 50% damage multiplier
	}
	void CaughtYouMirin_End(int id){
		mm.GetPlayer(id).ChangeBuff_DmgMult (1f); // reset back to 100% dmg
	}

	public void Cherrybomb(){
        targeting.WaitForTileTarget(1, Cherrybomb_Target);
	}
	void Cherrybomb_Target(TileBehav tb){
		Ench_SetCherrybomb(tb);
	}

	public void Magnitude10(){
        TurnEffect t = new TurnEffect(3, Magnitude10_Turn, Magnitude10_End, null);
        t.priority = 4;
        mm.effectCont.AddEndTurnEffect (t);
	}
	void Magnitude10_Turn(int id){
		int dmg = 0;
		for (int col = 0; col < 7; col++) {
			int row = hexGrid.BottomOfColumn (col);
			if (hexGrid.IsSlotFilled (col, row)) {
				Tile t = hexGrid.GetTileAt (col, row);
				if (!t.element.Equals (Tile.Element.Earth)) {
					mm.RemoveTile (t, true);
					dmg -= 15;
				}
			}
		}
		mm.GetPlayer(id).DealDamage (-dmg, false); // TODO not negative
	}
	void Magnitude10_End(int id){
		Magnitude10_Turn (id);
	}

	public void Sinkhole(){

	}

	public void BoulderBarrage(){

	}

	public void Stalagmite(){
        targeting.WaitForCellTarget(1, Stalagmite_Target);
	}
	void Stalagmite_Target(CellBehav cb){
		int col = cb.col;
		int bottomr = hexGrid.BottomOfColumn (col);
		// hardset bottom three cells of column
		GameObject stone;
		for (int i = 0; i < 3; i++){
			stone = mm.GenerateToken ("stone");
			stone.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
			mm.PutTile (stone, col, bottomr + i);
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
		mm.GetPlayer(id).DealDamage (200, false);

		List<TileBehav> tbs = hexGrid.GetSmallAreaTiles (tb.tile.col, tb.tile.row);
		foreach(TileBehav ctb in tbs){
			if (ctb.ableDestroy)
				mm.RemoveTile (ctb.tile.col, ctb.tile.row, true);
		}
	}

	public void Ench_SetBurning(TileBehav tb){
        // Burning does 3 dmg per tile per end-of-turn for 5 turns. It does double damage on expiration.
        //		Debug.Log("SPELLEFFECTS: Setting burning...");
        Enchantment ench = new Enchantment(5, Ench_Burning_Turn, Ench_Burning_End, null);
        ench.SetTypeTier(Enchantment.EnchType.Burning, 1);
        ench.priority = 1;
		tb.SetEnchantment (ench);
		tb.GetComponent<SpriteRenderer> ().color = new Color (1f, .4f, .4f);
		mm.effectCont.AddEndTurnEffect(ench);
	}
	void Ench_Burning_Turn(int id, TileBehav tb){
		mm.GetPlayer(id).DealDamage (3, false);
	}
	void Ench_Burning_End(int id, TileBehav tb){
		mm.GetPlayer(id).DealDamage (6, false);
	}

    public void Ench_SetZombify(TileBehav tb, bool skip){
        Enchantment ench = new Enchantment(Ench_Zombify_Turn, null, null);
        ench.SetTypeTier(Enchantment.EnchType.Zombify, 1);
        ench.priority = 6;
        if (skip)
            ench.SkipCurrent();
        tb.SetEnchantment(ench);
        tb.GetComponent<SpriteRenderer>().color = new Color(0f, .4f, 0f);
        mm.effectCont.AddEndTurnEffect(ench);
    }
    void Ench_Zombify_Turn(int id, TileBehav tb) {
        // TODO filter list before rand
        List<TileBehav> tbs = hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
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
                        mm.RemoveTile(ctb.tile, true);
                        mm.GetPlayer(id).DealDamage(10, false);
                        mm.GetPlayer(id).ChangeHealth(10, false, false);
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
        ench.priority = 6; // TODO 6.1?
        tb.SetEnchantment(ench);
        mm.effectCont.AddEndTurnEffect(ench);
    }
    void Ench_ZombieTok_Turn(int id, TileBehav tb) {
        Ench_Zombify_Turn(id, tb);
        Ench_Zombify_Turn(id, tb);
    }

    public void Ench_SetStoneTok(TileBehav tb) {
        Enchantment ench = new Enchantment(5, Ench_StoneTok_Turn, Ench_StoneTok_End, null);
        ench.SetTypeTier(Enchantment.EnchType.StoneTok, 3);
        ench.priority = 4;
        tb.SetEnchantment(ench);
        mm.effectCont.AddEndTurnEffect(ench);
    }
    void Ench_StoneTok_Turn(int id, TileBehav tb) {
        int c = tb.tile.col, r = tb.tile.row;
        if (hexGrid.CellExists(c, r - 1) && hexGrid.IsSlotFilled(c, r - 1)) {
            mm.RemoveTile(c, r - 1, false);
        }
    }
    void Ench_StoneTok_End(int id, TileBehav tb) {
        mm.RemoveTile(tb.tile, false);
    }

}