using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Enfuego : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

    public Enfuego(MageMatch mm, int id) : base(mm) {
        playerID = id;
        objFX = mm.objFX;
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;

        characterName = "Enfuego";
        maxHealth = 1000;
        SetDeckElements(20, 0, 0, 15, 15);

        spells = new Spell[5];
        spells[0] = new SignatureSpell(0, "White-Hot Combo Kick", "MFFM", 1, 40, WhiteHotComboKick);
        spells[1] = new Spell(1, "¡Baila!", "FM", 1, Baila);
        spells[2] = new Spell(2, "Incinerate", "FAM", 1, DoNothing);
        spells[3] = new Spell(3, "Hot Potatoes", "FFA", 1, HotPotatoes);
        spells[4] = new CoreSpell(4, "Fiery Fandango", 1, FieryFandango);

        InitSpells();
    }

    public IEnumerator DoNothing() {
        mm.uiCont.UpdateMoveText("This spell does nothing. Thanks!");
        yield return null;
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
                    mm.StartCoroutine(objFX.Ench_SetBurning(playerID, ctb)); // yield return?
                }
            } else if (tb.tile.element.Equals(Tile.Element.Muscle)) {
                yield return mm.InactiveP().DiscardRandom(1);
            }

            mm.tileMan.RemoveTile(tb.tile, true);
        }
    }

    // Core
    public IEnumerator FieryFandango(TileSeq seq) {
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

    public IEnumerator Baila() {
        mm.GetPlayer(playerID).DrawTiles(1, "", false, false); // my draw
        mm.GetOpponent(playerID).DrawTiles(1, "", false, false); // their draw

        yield return mm.prompt.WaitForSwap();
        mm.prompt.ContinueSwap();
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
            yield return objFX.Ench_SetBurning(playerID, tb);
            //yield return new WaitForSeconds(.2f);
        }

        yield return mm.InactiveP().DiscardRandom(2); 
    }

    public List<TileBehav> Inc_Filter(List<TileBehav> tbs) {
        List<TileBehav> filterTBs = new List<TileBehav>();
        foreach(TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.EnchType.Burning)
                filterTBs.Add(tb);
        }
        return filterTBs;
    }

    // PLACEHOLDER
    public IEnumerator HotPotatoes() {
        yield return null;
    }

    //public IEnumerator Backburner() {
    //    yield return mm.syncManager.SyncRand(playerID, Random.Range(15, 26));
    //    int dmg = mm.syncManager.GetRand();
    //    mm.GetPlayer(playerID).DealDamage(dmg);

    //    mm.effectCont.AddMatchEffect(new MatchEffect(1, Backburner_Match, null, 1), "backb");
    //    yield return null;
    //}
    //IEnumerator Backburner_Match(int id) {
    //    MMLog.Log_Enfuego("Rewarding player " + id + " with 1 AP.");
    //    mm.GetPlayer(id).AP++;
    //    yield return null;
    //}

    //public IEnumerator HotBody() {
    //    mm.effectCont.AddSwapEffect(new SwapEffect(3, HotBody_Swap, null), "hotbd");
    //    yield return null;
    //}
    //IEnumerator HotBody_Swap(int id, int c1, int r1, int c2, int r2) {
    //    int rand = Random.Range(10, 16); // 10-15 dmg
    //    yield return mm.syncManager.SyncRand(id, rand);
    //    mm.GetPlayer(id).DealDamage(mm.syncManager.GetRand());

    //    List<TileBehav> tbs = hexGrid.GetPlacedTiles();
    //    for (int i = 0; i < tbs.Count; i++) {
    //        TileBehav tb = tbs[i];
    //        if (!tb.CanSetEnch(Enchantment.EnchType.Burning)) {
    //            MMLog.Log_Enfuego("Removing " + tb.PrintCoord());
    //            tbs.RemoveAt(i);
    //            i--;
    //        }
    //    }

    //    rand = Random.Range(0, tbs.Count);
    //    yield return mm.syncManager.SyncRand(id, rand);
    //    TileBehav tbSelect = tbs[mm.syncManager.GetRand()];

    //    MMLog.Log_Enfuego("About to apply burning to the tb at " + tbSelect.PrintCoord());
    //    yield return spellfx.Ench_SetBurning(id, tbSelect);
    //}
}
