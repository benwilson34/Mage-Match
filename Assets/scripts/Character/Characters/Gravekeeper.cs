using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Gravekeeper : Character {

    private Spell _altMatchSpell;

    public Gravekeeper(MageMatch mm, int id) : base(mm, Ch.Gravekeeper, id) {
        _altMatchSpell = new MatchSpell(0, "Party in the Back", PartyInTheBack);
        CharacterInfo info = CharacterInfo.GetCharacterInfo(Ch.Gravekeeper);
        _altMatchSpell.info = CharacterInfo.GetSpellInfoString(info.altSpells[0], true);
    }

    #region ---------- SPELLS ----------

    // Business in the Front
    protected override IEnumerator MatchSpell(TileSeq seq) {
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
        DealDamage(dmg);

        if (seq.GetTileAt(0).IsElement(Tile.Element.Earth))
            zombs++;

        List<TileBehav> tbs = TileFilter.GetTilesByAbleEnch(Enchantment.Type.Zombie);

        for (int i = 0; i < zombs && tbs.Count > 0; i++) {
            yield return _mm.syncManager.SyncRand(_playerId, Random.Range(0, tbs.Count));
            int rand = _mm.syncManager.GetRand();
            yield return Zombie.Set(_playerId, tbs[rand]); // skip?
            tbs.RemoveAt(rand);
        }

        SwitchMatchSpell();

        yield return null;
    }

    // alt core spell - Party in the Back
    public IEnumerator PartyInTheBack(TileSeq seq) {
        AudioController.Trigger(SFX.Gravekeeper.PartyInTheBack);

        int dmg = 0;
        switch (seq.GetSeqLength()) {
            case 3:
                dmg = 20;
                yield return Targeting.WaitForTileTarget(1);
                break;
            case 4:
                dmg = 50;
                yield return Targeting.WaitForTileAreaTarget(false);
                break;
            case 5:
                dmg = 90;
                yield return Targeting.WaitForTileAreaTarget(true);
                break;
        }

        if (seq.GetTileAt(0).IsElement(Tile.Element.Earth))
            dmg += 20;
        DealDamage(dmg);

        List<TileBehav> tbs = Targeting.GetTargetTBs();
        foreach (TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.Type.Zombie)
                yield return ((Zombie)tb.GetEnchantment()).Attack();
        }

        SwitchMatchSpell();

        yield return null;
    }

    void SwitchMatchSpell() {
        Spell newSpell = _altMatchSpell;
        _altMatchSpell = _spells[0];
        _spells[0] = newSpell;

        MMLog.Log_Gravekeeper("GRAVEK: Switching core spell..." + _spells[0].info);
        _mm.uiCont.GetButtonCont(_playerId, 0).SpellChanged();
    }

    // The Oogie Boogie
    protected override IEnumerator Spell1(TileSeq prereq) {
        AudioController.Trigger(SFX.Gravekeeper.OogieBoogie);


        var zombs = TileFilter.GetTilesByEnch(Enchantment.Type.Zombie);
        if (zombs.Count < 2) // if not enough Zombies
            yield break; // TODO feedback for whiffs

        yield return Targeting.WaitForTileTarget(2, zombs);

        List<TileBehav> tbs = Targeting.GetTargetTBs();
        if (tbs.Count < 2)
            yield return null;

        Tile a = tbs[0].tile, b = tbs[1].tile;
        yield return _mm._SwapTiles(a.col, a.row, b.col, b.row);

        foreach (TileBehav tb in tbs)
            yield return ((Zombie)tb.GetEnchantment()).Attack();

        yield return null;
    }

    // Party Crashers
    protected override IEnumerator Spell2(TileSeq prereq) {
        AudioController.Trigger(SFX.Gravekeeper.PartyCrashers);

        const int dropCount = 2;
        Prompt.SetDropCount(dropCount);
        for (int i = 0; i < dropCount; i++) {
            yield return Prompt.WaitForDropTile();
            if (!Prompt.WasSuccessful)
                break;

            TileBehav tb = (TileBehav) Prompt.GetDropHex();
            MMLog.Log_Gravekeeper("Player " + _playerId + " dropped " + tb.hextag);
            yield return Zombie.Set(_playerId, tb);
            int col = Prompt.GetDropCol();
            int nextTBrow = BoardCheck.CheckColumn(col) - 1; // next TB under, if any

            yield return Prompt.ContinueDrop();

            if (nextTBrow >= HexGrid.BottomOfColumn(col)) {
                HexManager.RemoveTile(col, nextTBrow, false);
            }

            DealDamage(30);
        }
        yield return null;
    }

    // Undead Union
    protected override IEnumerator Spell3(TileSeq prereq) {
        AudioController.Trigger(SFX.Gravekeeper.UndeadUnion);

        var tbs = TileFilter.GetTilesByEnch(Enchantment.Type.Zombie);
        MMLog.Log_Gravekeeper(tbs.Count + " zombs on the board...");

        int count = 0;
        foreach (TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.Type.Zombie) {
                List<TileBehav> adjTBs = HexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
                foreach (TileBehav adjTB in adjTBs) {
                    if (adjTB.GetEnchType() == Enchantment.Type.Zombie) {
                        count++;
                    }
                }
            }
        }
        MMLog.Log_Gravekeeper("DealAdjZombDmg has counted " + count + " adjacent zombs");

        const int dmgAmount = 5;
        if(count > 0)
            DealDamage(count * dmgAmount);

        yield return null;
    }

    // Tombstone
    protected override IEnumerator SignatureSpell(TileSeq prereq) {
        AudioController.Trigger(SFX.Gravekeeper.Sig_Bell1);
        AudioController.Trigger(SFX.Gravekeeper.Sig_Motorcycle);

        DealDamage(225);

        yield return Targeting.WaitForCellTarget(1);

        CellBehav cb = Targeting.GetTargetCBs()[0];
        int col = cb.col;

        TileBehav tombstone = HexManager.GenerateTile(_playerId, "Tombstone");

        // destroy tiles under token
        var tbs = HexGrid.GetTilesInCol(col);
        // TODO foreach tb in tbs instead
        for (int row = HexGrid.BottomOfColumn(col); row < HexGrid.TopOfColumn(col); row++)
            if (HexGrid.IsCellFilled(col, row))
                HexManager.RemoveTile(col, row, false);

        _mm.DropTile(tombstone, col); // idk how to animate this one yet

        AudioController.Trigger(SFX.Gravekeeper.Sig_TSDrop);
    }
    #endregion

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

    //void RaiseZombie_Target(TileBehav tb) {
    //    //GameObject zomb = mm.GenerateToken("zombie");
    //    //zomb.transform.SetParent(GameObject.Find("tilesOnBoard").transform); // move to MM.PutTile
    //    //mm.hexGrid.RaiseTileBehavIntoColumn(zomb.GetComponent<TileBehav>(), cb.col);
    //    Ench_SetZombify(playerID, tb, false);
    //}
}
