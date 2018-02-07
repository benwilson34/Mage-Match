using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Gravekeeper : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

    private Spell altCoreSpell;

    public Gravekeeper(MageMatch mm, int id) : base(mm, Ch.Gravekeeper, id) {
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;
        objFX = mm.hexFX;

        altCoreSpell = new CoreSpell(0, "Party in the Back", PartyInTheBack);
        CharacterInfo info = CharacterInfo.GetCharacterInfoObj(Ch.Gravekeeper);
        altCoreSpell.info = CharacterInfo.GetSpellInfo(info.altSpell, true);
    }

    // ----- spells -----

    List<TileBehav> Filter_Zombs(List<TileBehav> tbs) {
        List<TileBehav> filterTBs = new List<TileBehav>();
        foreach (TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.EnchType.Zombify)
                filterTBs.Add(tb);
        }
        return filterTBs;
    }

    // Business in the Front
    protected override IEnumerator CoreSpell(TileSeq seq) {
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
        ThisPlayer().DealDamage(dmg);

        if (seq.GetElementAt(0) == Tile.Element.Earth) // not safe if there are multi-color tiles
            zombs++;

        List<TileBehav> tbs = hexGrid.GetPlacedTiles(seq);
        for (int i = 0; i < tbs.Count; i++) {
            if (tbs[i].GetEnchType() == Enchantment.EnchType.Zombify) {
                tbs.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < zombs && tbs.Count > 0; i++) {
            yield return mm.syncManager.SyncRand(playerId, Random.Range(0, tbs.Count));
            int rand = mm.syncManager.GetRand();
            yield return objFX.Ench_SetZombify(playerId, tbs[rand], false); // skip?
            tbs.RemoveAt(rand);
        }

        SwitchCoreSpell();

        yield return null;
    }

    // alt core spell - Party in the Back
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
        altCoreSpell = spells[0];
        spells[0] = newSpell;

        // keep in mind here that only the LOCAL player needs this
        if (mm.myID == playerId) {
            MMLog.Log_Gravekeeper("GRAVEK: Switching core spell..." + spells[0].info);
            mm.uiCont.GetButtonCont(playerId, 0).SpellChanged();
        }
    }

    // The Oogie Boogie
    // TODO handle 0 or 1 zombies on the board
    protected override IEnumerator Spell1(TileSeq prereq) {
        if (Filter_Zombs(hexGrid.GetPlacedTiles()).Count < 2) // if not enough Zombies
            yield break; // TODO feedback for whiffs

        yield return targeting.WaitForTileTarget(2, Filter_Zombs);

        List<TileBehav> tbs = targeting.GetTargetTBs();
        if (tbs.Count < 2)
            yield return null;

        Tile a = tbs[0].tile, b = tbs[1].tile;
        yield return mm._SwapTiles(false, a.col, a.row, b.col, b.row);

        foreach (TileBehav tb in tbs)
            yield return tb.TriggerEnchantment(); // that easy?

        yield return null;
    }

    // Party Crashers
    protected override IEnumerator Spell2(TileSeq prereq) {
        for (int i = 0; i < 2; i++) {
            yield return mm.prompt.WaitForDrop();
            if (!mm.prompt.WasSuccessful())
                break;

            TileBehav tb = mm.prompt.GetDropTile();
            MMLog.Log_Gravekeeper("Player " + playerId + " dropped " + tb.hextag);
            yield return objFX.Ench_SetZombify(playerId, tb, false);
            int col = mm.prompt.GetDropCol();
            int nextTBrow = mm.boardCheck.CheckColumn(col) - 1; // next TB under, if any

            yield return mm.prompt.ContinueDrop();

            if (nextTBrow >= mm.hexGrid.BottomOfColumn(col)) {
                hexMan.RemoveTile(col, nextTBrow, false);
            }

            ThisPlayer().DealDamage(30);
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

    // Undead Union
    protected override IEnumerator Spell3(TileSeq prereq) {
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

    // Tombstone
    protected override IEnumerator SignatureSpell(TileSeq prereq) {
        ThisPlayer().DealDamage(225);

        yield return targeting.WaitForCellTarget(1);

        CellBehav cb = targeting.GetTargetCBs()[0];
        int col = cb.col;
        Hex tomb = hexMan.GenerateToken(playerId, "tombstone");
        tomb.transform.SetParent(GameObject.Find("tilesOnBoard").transform);

        TombstoneToken ttb = (TombstoneToken)tomb;
        mm.effectCont.AddEndTurnEffect(new TurnEffect(5, Effect.Type.Add, ttb.Tombstone_Turn, ttb.Tombstone_TEnd), "tombs");

        // destroy tiles under token
        for (int row = hexGrid.BottomOfColumn(col); row < hexGrid.TopOfColumn(col); row++)
            if (hexGrid.IsCellFilled(col, row))
                hexMan.RemoveTile(col, row, false);
            else
                break; // TODO handle floating tiles

        mm.DropTile(col, ttb); // idk how to animate this one yet
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
}
