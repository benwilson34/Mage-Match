using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enfuego : Character {

    public int hotBody_selects = 0;
    private int hotBody_col, hotBody_row;

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

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
        spells[1] = new Spell(1, "¡Baile!", "FMF", 1, Baile);
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
        spells[3] = new Spell(3, "Fiery Fandango", 1, Burning); // sample for now
    }

    // ----- spells -----

    // sample
    public IEnumerator Burning() {
        yield return targeting.WaitForTileTarget(1);
        if (targeting.WasCanceled())
            yield break;

        TileBehav tb = targeting.GetTargetTBs()[0];
        yield return mm.animCont._Burning(tb);
        spellfx.Ench_SetBurning(playerID, tb);
    }

    public IEnumerator WhiteHotComboKick() {
        yield return targeting.WaitForTileTarget(3);
        if (targeting.WasCanceled())
            yield break;

        List<TileBehav> tTBs = targeting.GetTargetTBs();

        foreach (TileBehav tb in tTBs) { // maybe for instead?
            mm.ActiveP().DealDamage(70);

            if (tb.tile.element.Equals(Tile.Element.Fire)) { // spread Burning to 3 nearby
                List<TileBehav> ctbs = hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
                for (int i = 0; i < ctbs.Count; i++) {
                    TileBehav ctb = ctbs[i];
                    if (ctb.HasEnchantment() && ctb.GetEnchTier() > 1) {
                        ctbs.RemoveAt(i);
                        i--;
                    }
                }

                int burns = Mathf.Min(3, ctbs.Count);
                for (int i = 0; i < burns; i++) {
                    int rand = Random.Range(0, ctbs.Count);
                    yield return mm.syncManager.SyncRand(playerID, rand);
                    TileBehav ctb = ctbs[mm.syncManager.rand];
                    spellfx.Ench_SetBurning(playerID, ctb);
                }
            } else if (tb.tile.element.Equals(Tile.Element.Muscle)) {
                mm.InactiveP().DiscardRandom(1);
            }

            mm.RemoveTile(tb.tile, true);
        }
    }

    // PLACEHOLDER
    public IEnumerator Baile() {
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
        mm.effectCont.AddSwapEffect(new SwapEffect(3, 3, HotBody_Swap, null), "hotbd");
        yield return null;
    }
    IEnumerator HotBody_Swap(int id, int c1, int r1, int c2, int r2) {
        Debug.Log("ENFUEGO: HotBody's swap effect was called...selects="+hotBody_selects);
        //yield return new WaitForSeconds(3f);

        TileBehav tbSelect = null;
        int rand = Random.Range(10, 16);
        yield return mm.syncManager.SyncRand(id, rand);
        mm.GetPlayer(id).DealDamage(rand);

        // TODO can be done with mm.syncManager.SyncRand()!!!
        //if (id == mm.myID) {
            Debug.Log("ENFUEGO: This effect is mine, player " + playerID + "!");
            List<TileBehav> tbs = hexGrid.GetPlacedTiles();
            for (int i = 0; i < tbs.Count; i++) {
                TileBehav tb = tbs[i];
                if (tb.HasEnchantment() && tb.GetEnchTier() > 1) {
                    tbs.RemoveAt(i);
                    i--;
                }
            }

            //Debug.Log("ENFUEGO: Waiting to choose the tile to apply Burning to.");
            //yield return new WaitUntil(() => hotBody_selects > 0);

            rand = Random.Range(0, tbs.Count);
            yield return mm.syncManager.SyncRand(id, rand);
            tbSelect = tbs[rand];

        //    hotBody_selects--;
        //    mm.syncManager.SendHotBodySelect(id, tbSelect.tile.col, tbSelect.tile.row);
        //} else {
        //    mm.syncManager.StartHotBody(id);

        //    yield return new WaitUntil(() => hotBody_selects == 0);
        //    tbSelect = hexGrid.GetTileBehavAt(hotBody_col, hotBody_row);
        //}

        Debug.Log("About to apply burning to the tb at " + tbSelect.PrintCoord());
        yield return mm.animCont._Burning(tbSelect);
        spellfx.Ench_SetBurning(id, tbSelect);
    }

    public void SetHotBodySelect(int col, int row) {
        hotBody_col = col;
        hotBody_row = row;
        hotBody_selects--;
    }

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
