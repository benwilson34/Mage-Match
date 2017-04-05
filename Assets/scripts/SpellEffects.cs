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

	public IEnumerator Deal496Dmg(){
		mm.ActiveP().DealDamage(496);
        return null;
	}

    public IEnumerator StoneTest() {
        targeting.WaitForCellTarget(1, StoneTest_Target);
        return null;
    }
    void StoneTest_Target(CellBehav cb) {
        GameObject stone;
        stone = mm.GenerateToken("stone");
        stone.transform.SetParent(GameObject.Find("tilesOnBoard").transform);
        mm.DropTile(cb.col, stone, .08f);
    }

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
				mm.ActiveP ().DealDamage (15);
			}
		}
	}

	public void CaughtYouMirin(){
		mm.ActiveP().ChangeBuff_DmgMult (.5f); // 50% damage multiplier
        TurnEffect t = new TurnEffect(4, CaughtYouMirin_Turn, CaughtYouMirin_End, null);
        t.priority = 3;
        mm.effectCont.AddEndTurnEffect(t, "cym");
	}
	IEnumerator CaughtYouMirin_Turn(int id){ // technically this isn't needed
		mm.GetPlayer(id).ChangeBuff_DmgMult (.5f); // 50% damage multiplier
        yield return null; // for now
	}
	IEnumerator CaughtYouMirin_End(int id){
		mm.GetPlayer(id).ChangeBuff_DmgMult (1f); // reset back to 100% dmg
        yield return null; // for now
    }

	public IEnumerator Cherrybomb(){
        targeting.WaitForTileTarget(1, Cherrybomb_Target);
        return null;
	}
	void Cherrybomb_Target(TileBehav tb){
        Ench_SetCherrybomb(mm.ActiveP().id, tb); // right id?
	}


    // -------------------------------- ENCHANTMENTS --------------------------------------


	public void Ench_SetCherrybomb(int id, TileBehav tb){
		Enchantment ench = new Enchantment (id, null, null, Ench_Cherrybomb_Remove);
        ench.SetTypeTier(Enchantment.EnchType.Cherrybomb, 2);
		tb.SetEnchantment (ench);
		tb.GetComponent<SpriteRenderer> ().color = new Color (.4f, .4f, .4f);
	}
	IEnumerator Ench_Cherrybomb_Remove(int id, TileBehav tb){
		Debug.Log ("SPELLEFFECTS: Resolving Cherrybomb at (" + tb.tile.col + ", " + tb.tile.row + ")");
		mm.GetPlayer(id).DealDamage (200);

		List<TileBehav> tbs = hexGrid.GetSmallAreaTiles (tb.tile.col, tb.tile.row);
		foreach(TileBehav ctb in tbs){
			if (ctb.ableDestroy)
				mm.RemoveTile (ctb.tile.col, ctb.tile.row, true);
		}
        yield return null; // for now
    }

	public void Ench_SetBurning(int id, TileBehav tb){
        // Burning does 3 dmg per tile per end-of-turn for 5 turns. It does double damage on expiration.
        //		Debug.Log("SPELLEFFECTS: Setting burning...");
        Enchantment ench = new Enchantment(id, 5, Ench_Burning_TEffect, Ench_Burning_End, null);
        ench.SetTypeTier(Enchantment.EnchType.Burning, 1);
        ench.priority = 1;
		tb.SetEnchantment (ench);
		tb.GetComponent<SpriteRenderer> ().color = new Color (1f, .4f, .4f);
		mm.effectCont.AddEndTurnEffect(ench, "burn");
	}
	IEnumerator Ench_Burning_TEffect(int id, TileBehav tb){
		mm.GetPlayer(id).DealDamage (3);
        yield return null; // for now
    }
	IEnumerator Ench_Burning_End(int id, TileBehav tb){
		mm.GetPlayer(id).DealDamage (6);
        yield return null; // for now
    }

    public void Ench_SetStoneTok(TileBehav tb) {
        Enchantment ench = new Enchantment(5, Ench_StoneTok_TEffect, Ench_StoneTok_End, null);
        ench.SetTypeTier(Enchantment.EnchType.StoneTok, 3);
        ench.priority = 4;
        tb.SetEnchantment(ench);
        mm.effectCont.AddEndTurnEffect(ench, "stoT");
    }
    IEnumerator Ench_StoneTok_TEffect(int id, TileBehav tb) {
        int c = tb.tile.col, r = tb.tile.row;
        if (hexGrid.CellExists(c, r - 1) && hexGrid.IsSlotFilled(c, r - 1)) {
            mm.RemoveTile(c, r - 1, false);
        }
        yield return null; // for now
    }
    IEnumerator Ench_StoneTok_End(int id, TileBehav tb) {
        mm.RemoveTile(tb.tile, false);
        yield return null; // for now
    }

}