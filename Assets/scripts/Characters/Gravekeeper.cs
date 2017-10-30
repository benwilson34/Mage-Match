using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Gravekeeper : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

    private Spell altCoreSpell;

    public Gravekeeper(MageMatch mm, int id) : base(mm, Ch.Gravekeeper) {
        playerID = id;
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;
        objFX = mm.hexFX;

        characterName = "The Gravekeeper";
        maxHealth = 1200;
        SetDeckElements(0, 10, 20, 0, 20);

        spells = new Spell[5];
        spells[0] = new SignatureSpell(0, "Tombstone", "EMEE", 1, 20, Tombstone);
        spells[1] = new Spell(1, "The Oogie Boogie", "ME", 1, TheOogieBoogie);
        spells[2] = new Spell(2, "Party Crashers", "MWE", 1, PartyCrashers);
        spells[3] = new Spell(3, "Undead Union", "WEM", 1, UndeadUnion);
        spells[4] = new CoreSpell(4, "Business in the Front", 1, BusinessInTheFront);

        altCoreSpell = new CoreSpell(4, "Party in the Back", 1, PartyInTheBack);

        InitSpells();
    }

    // ----- spells -----

    public IEnumerator BusinessInTheFront(TileSeq seq) {
        int dmg = 0, zombs = 0;
        switch (seq.GetSeqLength()) {
            case 3: dmg = 10;
                zombs = 1;
                break;
            case 4: dmg = 30;
                zombs = 2;
                break;
            case 5: dmg = 60;
                zombs = 3;
                break;
        }
        mm.ActiveP().DealDamage(dmg);

        if (seq.GetElementAt(0) == Tile.Element.Earth) // not safe if there are multi-color tiles
            zombs++;

        List<TileBehav> tbs = hexGrid.GetPlacedTiles(seq);
        for (int i = 0; i < zombs && tbs.Count > 0; i++) {
            yield return mm.syncManager.SyncRand(playerID, Random.Range(0, tbs.Count));
            int rand = mm.syncManager.GetRand();
            yield return objFX.Ench_SetZombify(playerID, tbs[rand], false); // skip?
            tbs.RemoveAt(rand);
        }

        SwitchCoreSpell();

        yield return null;
    }

    public IEnumerator PartyInTheBack(TileSeq seq) {
        int dmg = 0;
        switch (seq.GetSeqLength()) {
            case 3:
                dmg = 20;
                yield return targeting.WaitForTileTarget(1);
                break;
            case 4:
                dmg = 50;
                yield return targeting.WaitForTileAreaTarget(false);
                break;
            case 5:
                dmg = 90;
                yield return targeting.WaitForTileAreaTarget(true);
                break;
        }

        if (targeting.WasCanceled())
            yield return null;

        if (seq.GetElementAt(0) == Tile.Element.Muscle) // not safe if there are multi-color tiles
            dmg += 20;
        mm.ActiveP().DealDamage(dmg);

        List<TileBehav> tbs = targeting.GetTargetTBs();
        foreach (TileBehav tb in tbs) {
            if(tb.GetEnchType() == Enchantment.EnchType.Zombify)
                yield return tb.TriggerEnchantment(); // that easy?
        }

        SwitchCoreSpell();

        yield return null;
    }

    void SwitchCoreSpell() {
        Spell newSpell = altCoreSpell;
        altCoreSpell = spells[4];
        spells[4] = newSpell;
        mm.uiCont.GetButtonCont(4).SpellChanged();
    }

    // TODO handle 0 or 1 zombies on the board
    public IEnumerator TheOogieBoogie() {
        yield return targeting.WaitForTileTarget(2, TOB_Filter);
        if (targeting.WasCanceled())
            yield return null;

        List<TileBehav> tbs = targeting.GetTargetTBs();
        Tile a = tbs[0].tile, b = tbs[1].tile;
        yield return mm._SwapTiles(false, a.col, a.row, b.col, b.row);

        foreach (TileBehav tb in tbs)
            yield return tb.TriggerEnchantment(); // that easy?

        yield return null;
    }

    public List<TileBehav> TOB_Filter(List<TileBehav> tbs) {
        List<TileBehav> filterTBs = new List<TileBehav>();
        foreach (TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.EnchType.Zombify)
                filterTBs.Add(tb);
        }
        return filterTBs;
    }

    // TODO handle no tiles in hand...should be part of WaitForDrop?
    public IEnumerator PartyCrashers() {
        for (int i = 0; i < 2; i++) {
            yield return mm.prompt.WaitForDrop();
            // TODO if canceled
            TileBehav tb = mm.prompt.GetDropTile();
            MMLog.Log_Gravekeeper("Player " + playerID + " dropped " + tb.tag);
            yield return objFX.Ench_SetZombify(playerID, tb, false);
            int col = mm.prompt.GetDropCol();
            int nextTBrow = mm.boardCheck.CheckColumn(col) - 1; // next TB under, if any

            mm.prompt.ContinueDrop();

            if (nextTBrow >= mm.hexGrid.BottomOfColumn(col)) {
                tileMan.RemoveTile(col, nextTBrow, false);
            }

            mm.GetPlayer(playerID).DealDamage(30);
        }
        yield return null;
    }

    //public IEnumerator HumanResources() {
    //    yield return targeting.WaitForTileAreaTarget(false);
    //    if (targeting.WasCanceled())
    //        yield break;

    //    List<TileBehav> tbs = targeting.GetTargetTBs();
    //    //foreach (TileBehav tb in tbs) {
    //    for(int i = 0; i < tbs.Count; i++) { // foreach
    //        TileBehav tb = tbs[i];
    //        if (tb.tile.element == Tile.Element.Muscle) {
    //            if (tb.CanSetEnch(Enchantment.EnchType.Zombify)) {
    //                yield return spellfx.Ench_SetZombify(playerID, tb, false);
    //                continue;
    //            }
    //        }
    //        tbs.RemoveAt(i);
    //        i--;
    //    }

    //    MMLog.Log_Gravekeeper("HumanResources trigger count=" + tbs.Count);
    //    foreach (TileBehav tb in tbs) {
    //        if (tb.GetEnchType() == Enchantment.EnchType.Zombify) {
    //            MMLog.Log_Gravekeeper("Triggering zomb at " + tb.PrintCoord());
    //            yield return tb.TriggerEnchantment();
    //        }
    //    }
    //}

    public IEnumerator UndeadUnion() {
        List<TileBehav> tbs = mm.hexGrid.GetPlacedTiles();
        for (int i = 0; i < tbs.Count; i++) { // filter zombs
            TileBehav tb = tbs[i];
            if (tb.GetEnchType() != Enchantment.EnchType.Zombify) {
                tbs.RemoveAt(i);
                i--;
            }
        }
        MMLog.Log_Gravekeeper(tbs.Count + " zombs on the board...");

        DealAdjZombDmg(tbs);

        yield return null;
    }

    void DealAdjZombDmg(List<TileBehav> tbs) {
        int count = 0;
        foreach (TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.EnchType.Zombify) {
                List<TileBehav> adjTBs = hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
                foreach (TileBehav adjTB in adjTBs) {
                    if (adjTB.GetEnchType() == Enchantment.EnchType.Zombify) {
                        count++;
                    }
                }
            }
        }
        MMLog.Log_Gravekeeper("DealAdjZombDmg has counted " + count + " adjacent zombs");
        if(count > 0)
            mm.ActiveP().DealDamage(count * 5);
    }

    //public IEnumerator CompanyLuncheon() {
    //    targeting.WaitForTileAreaTarget(false, CompanyLuncheon_Target);
    //    yield return null;
    //}
    //void CompanyLuncheon_Target(List<TileBehav> tbs) {
    //    for (int i = 0; i < tbs.Count; i++) {
    //        TileBehav tb = tbs[i];
    //        if (!tb.HasEnchantment() ||
    //            tb.GetEnchType() != Enchantment.EnchType.Zombify ||
    //            tb.GetEnchType() != Enchantment.EnchType.ZombieTok) {
    //            tbs.Remove(tb);
    //            i--;
    //        }
    //    }
    //    foreach (TileBehav tb in tbs)
    //        tb.TriggerEnchantment();
    //}

    // delete
    //public IEnumerator ZombifySpell() {
    //    yield return targeting.WaitForTileTarget(1);
    //    if (targeting.WasCanceled())
    //        yield break;
    //    TileBehav tb = targeting.GetTargetTBs()[0];
    //    yield return spellfx.Ench_SetZombify(playerID, tb, false);
    //}

    //public IEnumerator GatherTheGhouls() {
    //    List<TileBehav> tbs =  hexGrid.GetPlacedTiles();
    //    for (int i = 0; i < tbs.Count; i++) {
    //        TileBehav ctb = tbs[i];
    //        if (!ctb.CanSetEnch(Enchantment.EnchType.Zombify)) {
    //            tbs.RemoveAt(i);
    //            i--;
    //        }
    //    }

    //    int count = Mathf.Min(tbs.Count, 3);

    //    for (int i = 0; i < count; i++) {
    //        // TODO sync array, not in loop...maybe
    //        yield return mm.syncManager.SyncRand(playerID, Random.Range(0, tbs.Count));
    //        int rand = mm.syncManager.GetRand();
    //        yield return spellfx.Ench_SetZombify(playerID, tbs[rand], false);
    //        tbs.RemoveAt(rand);
    //    }
    //}

    //void RaiseZombie_Target(TileBehav tb) {
    //    //GameObject zomb = mm.GenerateToken("zombie");
    //    //zomb.transform.SetParent(GameObject.Find("tilesOnBoard").transform); // move to MM.PutTile
    //    //mm.hexGrid.RaiseTileBehavIntoColumn(zomb.GetComponent<TileBehav>(), cb.col);
    //    Ench_SetZombify(playerID, tb, false);
    //}

    public IEnumerator Tombstone() {
        mm.GetPlayer(playerID).DealDamage(225);

        yield return targeting.WaitForCellTarget(1);
        if (targeting.WasCanceled())
            yield break;

        CellBehav cb = targeting.GetTargetCBs()[0];
        int col = cb.col;
        Hex tomb = tileMan.GenerateToken(playerID, "tombstone");
        tomb.transform.SetParent(GameObject.Find("tilesOnBoard").transform);

        TombstoneToken ttb = (TombstoneToken)tomb;
        mm.effectCont.AddEndTurnEffect(new TurnEffect(5, Effect.Type.Add, ttb.Tombstone_Turn, ttb.Tombstone_TEnd), "tombs");

        // destroy tiles under token
        for (int row = hexGrid.BottomOfColumn(col); row < hexGrid.TopOfColumn(col); row++)
            if (hexGrid.IsCellFilled(col, row))
                tileMan.RemoveTile(col, row, false);
            else
                break; // TODO handle floating tiles

        mm.DropTile(col, ttb); // idk how to animate this one yet
    }
}
