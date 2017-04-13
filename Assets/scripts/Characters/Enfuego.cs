using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enfuego : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

    public Enfuego(MageMatch mm, int id, int loadout) : base(mm) {
        playerID = id;
        //this.mm = mm; //?
        spellfx = mm.spellfx;
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;
        characterName = "Enfuego";
        spells = new Spell[4];

        if (loadout == 1)
            EnfuegoA();
        else
            EnfuegoB();
        InitSpells();
    }

    void EnfuegoA() { // Enfuego A - Supah Hot Fire
        loadoutName = "Supah Hot Fire";
        maxHealth = 1000;

        SetDeckElements(50, 0, 0, 20, 30);

        spells[0] = new SignatureSpell(0, "White-Hot Combo Kick", "MFFM", 1, 40, WhiteHotComboKick);
        spells[1] = new Spell(1, "¡Baile!", "FM", 1, Baile);
        spells[2] = new Spell(2, "Incinerate", "FAF", 1, Incinerate);
        spells[3] = new CoreSpell(3, "Fiery Fandango", 3, 1, FieryFandango);
    }

    // FOCUS
    void EnfuegoB() { // Enfuego B - Hot Feet
        loadoutName = "Hot Feet";
        maxHealth = 1100;

        SetDeckElements(50, 0, 15, 0, 35);

        spells[0] = new SignatureSpell(0, "White-Hot Combo Kick", "MFFM", 1, 40, WhiteHotComboKick);
        spells[1] = new Spell(1, "Hot Body", "MEF", 1, HotBody);
        spells[2] = new Spell(2, "Backburner", "MF", 1, Backburner);
        spells[3] = new CoreSpell(3, "Fiery Fandango", 3, 1, FieryFandango);
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

    public IEnumerator FieryFandango() {
        mm.GetPlayer(playerID).ChangeDebuff_DmgExtra(15);
        mm.GetOpponent(playerID).ChangeDebuff_DmgExtra(15);
        TurnEffect te = new TurnEffect(2, null, FF_TEnd, null);
        yield return null;
    }
    IEnumerator FF_TEnd(int id) {
        mm.GetPlayer(id).ChangeDebuff_DmgExtra(0);
        mm.GetOpponent(id).ChangeDebuff_DmgExtra(0);
        yield return null;
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
                    if (!ctb.CanSetEnch(Enchantment.EnchType.Burning)) {
                        ctbs.RemoveAt(i);
                        i--;
                    }
                }

                int burns = Mathf.Min(3, ctbs.Count);
                for (int i = 0; i < burns; i++) {
                    Debug.Log("ENFUEGO: WHCK count=" + burns);
                    int rand = Random.Range(0, ctbs.Count);
                    yield return mm.syncManager.SyncRand(playerID, rand);
                    TileBehav ctb = ctbs[mm.syncManager.GetRand()];
                    Debug.Log("ENFUEGO: Setting Burning to " + ctb.PrintCoord());
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

    public IEnumerator Backburner() {
        yield return mm.syncManager.SyncRand(playerID, Random.Range(15, 26));
        int dmg = mm.syncManager.GetRand();
        mm.GetPlayer(playerID).DealDamage(dmg);

        mm.effectCont.AddMatchEffect(new MatchEffect(1, 1, Backburner_Match, null), "backb");
        yield return null;
    }
    IEnumerator Backburner_Match(int id) {
        Debug.Log("ENFUEGO: Rewarding player " + id + " with 1 AP.");
        mm.GetPlayer(id).AP++;
        yield return null;

    }

    // TODO
    public IEnumerator HotBody() {
        mm.effectCont.AddSwapEffect(new SwapEffect(3, 3, HotBody_Swap, null), "hotbd");
        yield return null;
    }
    IEnumerator HotBody_Swap(int id, int c1, int r1, int c2, int r2) {
        int rand = Random.Range(10, 16); // 10-15 dmg
        yield return mm.syncManager.SyncRand(id, rand);
        mm.GetPlayer(id).DealDamage(mm.syncManager.GetRand());

        List<TileBehav> tbs = hexGrid.GetPlacedTiles();
        for (int i = 0; i < tbs.Count; i++) {
            TileBehav tb = tbs[i];
            if (!tb.CanSetEnch(Enchantment.EnchType.Burning)) {
                Debug.Log("ENFUEGO: Removing " + tb.PrintCoord());
                tbs.RemoveAt(i);
                i--;
            }
        }

        rand = Random.Range(0, tbs.Count);
        yield return mm.syncManager.SyncRand(id, rand);
        TileBehav tbSelect = tbs[mm.syncManager.GetRand()];

        Debug.Log("ENFUEGO: About to apply burning to the tb at " + tbSelect.PrintCoord());
        yield return mm.animCont._Burning(tbSelect);
        spellfx.Ench_SetBurning(id, tbSelect);
    }

    //public IEnumerator HotAndBothered() {
    //    Debug.Log("ENFUEGO: HAB called????");
    //    mm.InactiveP().ChangeBuff_DmgBonus(15);
    //    TurnEffect t = new TurnEffect(5, HAB_Turn, HAB_End, null);
    //    t.priority = 3;
    //    mm.effectCont.AddEndTurnEffect(t, "handb");
    //    yield return null;
    //}
    //IEnumerator HAB_Turn(int id) {
    //    mm.InactiveP().ChangeBuff_DmgBonus(15); // technically not needed
    //    yield return null;
    //}
    //IEnumerator HAB_End(int id) {
    //    mm.InactiveP().ChangeBuff_DmgBonus(0); // reset
    //    yield return null;
    //}

    //public IEnumerator Pivot() {
    //    TurnEffect t = new TurnEffect(1, null, Pivot_End, null);
    //    t.priority = 3;
    //    mm.effectCont.AddEndTurnEffect(t, "pivot");

    //    mm.effectCont.AddMatchEffect(new MatchEffect(1, 1, Pivot_Match, null), "pivot");
    //    yield return null;
    //}
    //IEnumerator Pivot_End(int id) {
    //    //mm.ActiveP().ClearMatchEffect();
    //    yield return null;
    //}
    //IEnumerator Pivot_Match(int id) {
    //    mm.ActiveP().AP++;
    //    yield return null;
    //}

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
