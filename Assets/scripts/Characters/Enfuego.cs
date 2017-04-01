using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enfuego : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''
    private bool hotBody_active = false;

    public Enfuego(MageMatch mm, int id, int loadout) {
        spellfx = new SpellEffects(); //?
        this.mm = mm;
        playerID = id;
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;
        characterName = "Enfuego";
        spells = new Spell[4];

        if (loadout == 1)
            EnfuegoA();
        else
            EnfuegoB();
    }

    void EnfuegoA() { // Enfuego A - Supah Hot Fire
        loadoutName = "Supah Hot Fire";
        maxHealth = 1000;

        SetDeckElements(50, 0, 0, 20, 30);

        spells[0] = new Spell(0, "White-Hot Combo Kick", "MFFM", 1, WhiteHotComboKick);
        spells[1] = new Spell(1, "Baila!", "FMF", 1, Baila);
        spells[2] = new Spell(2, "Incinerate", "FAFF", 1, Incinerate);
        spells[3] = new Spell(3, "Phoenix Fire", "AFM", 1, PhoenixFire);
    }

    // FOCUS
    void EnfuegoB() { // Enfuego B - Hot Feet
        loadoutName = "Hot Feet";
        maxHealth = 1100;

        SetDeckElements(50, 0, 15, 0, 35);

        spells[0] = new Spell(0, "White-Hot Combo Kick", "MFFM", 1, WhiteHotComboKick);
        spells[1] = new Spell(1, "Hot Body", "FEFM", 1, HotBody);
        spells[2] = new Spell(2, "Hot and Bothered", "FMF", 1, HotAndBothered);
        spells[3] = new Spell(3, "Pivot", "MEF", 0, Pivot);
    }

    // ----- spells -----

    public IEnumerator WhiteHotComboKick() {
        targeting.WaitForTileTarget(3, WHCK_Target);
        yield return null;
    }
    void WHCK_Target(TileBehav tb) {
        mm.ActiveP().DealDamage(70, false);

        if (tb.tile.element.Equals(Tile.Element.Fire)) {
            // TODO spread Burning
            List<TileBehav> tbs = hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
            int burns = Mathf.Min(3, tbs.Count);
            int tries = 10; // TODO generalize this form of randomization. see Commish spell also.
            for (int i = 0; i < burns; i++) {
                int rand = Random.Range(0, tbs.Count);
                TileBehav ctb = tbs[rand];
                if (ctb.HasEnchantment()) {
                    tries--;
                    i--;
                } else {
                    spellfx.Ench_SetBurning(mm.ActiveP().id, ctb); // right ID?
                    tries = 10;
                }
                if (tries == 0)
                    break;
            }
        } else if (tb.tile.element.Equals(Tile.Element.Muscle)) {
            mm.InactiveP().DiscardRandom(1);
        }

        mm.RemoveTile(tb.tile, true);
    }

    // PLACEHOLDER
    public IEnumerator Baila() {
        targeting.WaitForTileAreaTarget(true, Baila_Target);
        yield return null;
    }
    void Baila_Target(List<TileBehav> tbs) {
        foreach (TileBehav tb in tbs)
            spellfx.Ench_SetBurning(mm.ActiveP().id, tb); // right ID?
    }

    // empty
    public IEnumerator PhoenixFire() {
        yield return null;
    }

    // TODO
    public IEnumerator HotBody() {
        mm.effectCont.AddSwapEffect(new SwapEffect(10, 3, HotBody_Swap, null), "hotbd");
        yield return null;
    }
    IEnumerator HotBody_Swap(int id, int c1, int r1, int c2, int r2) {
        // TODO check ID???

        Debug.Log("ENFUEGO: HotBody's swap effect was called!");
        yield return null;

        //if(id == playerID) {
        //    mm.GetPlayer(id).DealDamage(25, false);

        //    // TODO still doesn't function properly...seems to be whiffing at least once
        //    // maybe it's enchanting part of the match??
        //    List<TileBehav> tbs = hexGrid.GetPlacedTiles();
        //    for (int i = 0; i < tbs.Count; i++) {
        //        TileBehav tb = tbs[i];
        //        if (tb.HasEnchantment()) {
        //            tbs.Remove(tb);
        //            i--;
        //        }
        //    }

        //    Debug.Log("ENFUEGO: Waiting to choose the tile to apply Burning to.");
        //    //yield return new WaitUntil(() => hotBody_active);
        //    int rand = Random.Range(0, tbs.Count);
        //    spellfx.Ench_SetBurning(id, tbs[rand]);
        //    //mm.syncManager
        //} else {
        //    // TODO something about waiting for it...
        //} 
    }
    //void HotBody_Tile(int col, int row) {
    //    // TODO
    //}

    public IEnumerator HotAndBothered() {
        Debug.Log("ENFUEGO: HAB called????");
        mm.InactiveP().ChangeBuff_DmgExtra(15);
        TurnEffect t = new TurnEffect(5, HAB_Turn, HAB_End, null);
        t.priority = 3;
        mm.effectCont.AddEndTurnEffect(t, "handb");
        yield return null;
    }
    IEnumerator HAB_Turn(int id) {
        mm.InactiveP().ChangeBuff_DmgExtra(15); // technically not needed
        yield return null;
    }
    IEnumerator HAB_End(int id) {
        mm.InactiveP().ChangeBuff_DmgExtra(0); // reset
        yield return null;
    }

    public IEnumerator Pivot() {
        TurnEffect t = new TurnEffect(1, null, Pivot_End, null);
        t.priority = 3;
        mm.effectCont.AddEndTurnEffect(t, "pivot");

        mm.effectCont.AddMatchEffect(new MatchEffect(1, 1, Pivot_Match, null), "pivot");
        yield return null;
    }
    IEnumerator Pivot_End(int id) {
        //mm.ActiveP().ClearMatchEffect();
        yield return null;
    }
    IEnumerator Pivot_Match(int id) {
        mm.ActiveP().AP++;
        yield return null;
    }

    public IEnumerator Incinerate() {
        // TODO drag targeting
        int burnCount = mm.InactiveP().hand.Count * 2;
        Debug.Log("SPELLFX: Incinerate burnCount = " + burnCount);
        mm.InactiveP().DiscardRandom(2);
        targeting.WaitForDragTarget(burnCount, Incinerate_Target);
        yield return null;
    }
    void Incinerate_Target(List<TileBehav> tbs) {
        foreach (TileBehav tb in tbs)
            spellfx.Ench_SetBurning(mm.ActiveP().id, tb); // right ID?
    }
}
