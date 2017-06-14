using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Enfuego : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

    public Enfuego(MageMatch mm, int id, int loadout) : base(mm) {
        playerID = id;
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

    void EnfuegoA() { // Enfuego A - Fiery Flamenco
        loadoutName = "Fiery Flamenco";
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
        spells[2] = new Spell(2, "Target only fire", "MF", 1, TargetOnlyFire); // change back to Backburner
        spells[3] = new CoreSpell(3, "Fiery Fandango", 3, 1, FieryFandango);
    }

    public IEnumerator TargetOnlyFire() {
        yield return targeting.WaitForTileTarget(2, TOF_Filter);
        if (targeting.WasCanceled())
            yield return null;

        List<TileBehav> tbs = targeting.GetTargetTBs();
        foreach (TileBehav tb in tbs) {
            yield return spellfx.Ench_SetBurning(playerID, tb);
        }
    }

    public List<TileBehav> TOF_Filter(List<TileBehav> tbs) {
        List<TileBehav> filts = new List<TileBehav>();
        foreach (TileBehav tb in tbs) {
            if (tb.tile.element == Tile.Element.Fire)
                filts.Add(tb);
        }
        return filts;
    }

    // ----- spells -----

    // sample
    //public IEnumerator Burning() {
    //    yield return targeting.WaitForTileTarget(1);
    //    if (targeting.WasCanceled())
    //        yield break;

    //    TileBehav tb = targeting.GetTargetTBs()[0];
    //    yield return mm.animCont._Burning(tb);
    //    spellfx.Ench_SetBurning(playerID, tb);
    //}

    // Signature
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
                    MMLog.Log_Enfuego("WHCK count=" + burns);
                    yield return mm.syncManager.SyncRand(playerID, Random.Range(0, ctbs.Count));
                    TileBehav ctb = ctbs[mm.syncManager.GetRand()];
                    MMLog.Log_Enfuego("Setting Burning to " + ctb.PrintCoord());
                    mm.StartCoroutine(spellfx.Ench_SetBurning(playerID, ctb)); // yield return?
                }
            } else if (tb.tile.element.Equals(Tile.Element.Muscle)) {
                yield return mm.InactiveP().DiscardRandom(1);
            }

            mm.RemoveTile(tb.tile, true);
        }
    }

    // Core
    public IEnumerator FieryFandango() {
        mm.GetPlayer(playerID).ChangeDebuff_DmgExtra(15);
        mm.GetOpponent(playerID).ChangeDebuff_DmgExtra(15);
        mm.effectCont.AddEndTurnEffect(new TurnEffect(2, Effect.Type.Buff, null, FF_TEnd), "fiery");
        yield return null;
    }
    IEnumerator FF_TEnd(int id) {
        mm.GetPlayer(id).ChangeDebuff_DmgExtra(0);
        mm.GetOpponent(id).ChangeDebuff_DmgExtra(0);
        yield return null;
    }

    // PLACEHOLDER
    public IEnumerator Baile() {
        yield return targeting.WaitForTileAreaTarget(true);

        List<TileBehav> tbs = targeting.GetTargetTBs();
        foreach (TileBehav tb in tbs)
            yield return spellfx.Ench_SetBurning(mm.ActiveP().id, tb); // right ID?
    }

    public IEnumerator Incinerate() {
        int burnCount = mm.InactiveP().hand.Count() * 2;
        MMLog.Log_Enfuego("Incinerate burnCount = " + burnCount);

        yield return targeting.WaitForDragTarget(burnCount);
        if (targeting.WasCanceled())
            yield return null;

        List<TileBehav> tbs = targeting.GetTargetTBs();
        foreach (TileBehav tb in tbs) {
            MMLog.Log_Enfuego("Enchanting tile at " + tb.PrintCoord());
            yield return spellfx.Ench_SetBurning(playerID, tb); // right ID?
            //yield return new WaitForSeconds(.2f);
        }

        yield return mm.InactiveP().DiscardRandom(2); 
    }

    public IEnumerator Backburner() {
        yield return mm.syncManager.SyncRand(playerID, Random.Range(15, 26));
        int dmg = mm.syncManager.GetRand();
        mm.GetPlayer(playerID).DealDamage(dmg);

        mm.effectCont.AddMatchEffect(new MatchEffect(1, Backburner_Match, null, 1), "backb");
        yield return null;
    }
    IEnumerator Backburner_Match(int id) {
        MMLog.Log_Enfuego("Rewarding player " + id + " with 1 AP.");
        mm.GetPlayer(id).AP++;
        yield return null;
    }

    public IEnumerator HotBody() {
        mm.effectCont.AddSwapEffect(new SwapEffect(3, HotBody_Swap, null), "hotbd");
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
                MMLog.Log_Enfuego("Removing " + tb.PrintCoord());
                tbs.RemoveAt(i);
                i--;
            }
        }

        rand = Random.Range(0, tbs.Count);
        yield return mm.syncManager.SyncRand(id, rand);
        TileBehav tbSelect = tbs[mm.syncManager.GetRand()];

        MMLog.Log_Enfuego("About to apply burning to the tb at " + tbSelect.PrintCoord());
        yield return spellfx.Ench_SetBurning(id, tbSelect);
    }
}
