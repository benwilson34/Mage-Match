using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Gravekeeper : Character {

    private HexGrid _hexGrid; // eventually these will be static again?
    private Targeting _targeting; // ''

    private Spell _altCoreSpell;

    public Gravekeeper(MageMatch mm, int id) : base(mm, Ch.Gravekeeper, id) {
        _hexGrid = mm.hexGrid;
        _targeting = mm.targeting;
        _objFX = mm.hexFX;

        _altCoreSpell = new CoreSpell(0, "Party in the Back", PartyInTheBack);
        CharacterInfo info = CharacterInfo.GetCharacterInfo(Ch.Gravekeeper);
        _altCoreSpell.info = CharacterInfo.GetSpellInfoString(info.altSpell, true);
    }

    // ----- spells -----

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
        DealDamage(dmg);

        if (seq.GetElementAt(0) == Tile.Element.Earth) // not safe if there are multi-color tiles
            zombs++;

        List<TileBehav> tbs = TileFilter.GetTilesByAbleEnch(Enchantment.Type.Zombie);

        for (int i = 0; i < zombs && tbs.Count > 0; i++) {
            yield return _mm.syncManager.SyncRand(_playerId, Random.Range(0, tbs.Count));
            int rand = _mm.syncManager.GetRand();
            yield return _objFX.Ench_SetZombie(_playerId, tbs[rand], false); // skip?
            tbs.RemoveAt(rand);
        }

        SwitchCoreSpell();

        yield return null;
    }

    // alt core spell - Party in the Back
    public IEnumerator PartyInTheBack(TileSeq seq) {
        _mm.audioCont.Trigger(AudioController.GravekeeperSFX.PartyInTheBack);

        int dmg = 0;
        switch (seq.GetSeqLength()) {
            case 3:
                dmg = 20;
                yield return _targeting.WaitForTileTarget(1);
                break;
            case 4:
                dmg = 50;
                yield return _targeting.WaitForTileAreaTarget(false);
                break;
            case 5:
                dmg = 90;
                yield return _targeting.WaitForTileAreaTarget(true);
                break;
        }

        if (seq.GetElementAt(0) == Tile.Element.Muscle) // not safe if there are multi-color tiles
            dmg += 20;
        DealDamage(dmg);

        List<TileBehav> tbs = _targeting.GetTargetTBs();
        foreach (TileBehav tb in tbs) {
            if(tb.GetEnchType() == Enchantment.Type.Zombie)
                yield return tb.TriggerEnchantment();
        }

        SwitchCoreSpell();

        yield return null;
    }

    void SwitchCoreSpell() {
        Spell newSpell = _altCoreSpell;
        _altCoreSpell = _spells[0];
        _spells[0] = newSpell;

        // keep in mind here that only the LOCAL player needs this
        if (_mm.myID == _playerId) {
            MMLog.Log_Gravekeeper("GRAVEK: Switching core spell..." + _spells[0].info);
            _mm.uiCont.GetButtonCont(_playerId, 0).SpellChanged();
        }
    }

    // The Oogie Boogie
    protected override IEnumerator Spell1(TileSeq prereq) {
        _mm.audioCont.Trigger(AudioController.GravekeeperSFX.OogieBoogie);


        var zombs = TileFilter.GetTilesByEnch(Enchantment.Type.Zombie);
        if (zombs.Count < 2) // if not enough Zombies
            yield break; // TODO feedback for whiffs

        yield return _targeting.WaitForTileTarget(2, zombs);

        List<TileBehav> tbs = _targeting.GetTargetTBs();
        if (tbs.Count < 2)
            yield return null;

        Tile a = tbs[0].tile, b = tbs[1].tile;
        yield return _mm._SwapTiles(false, a.col, a.row, b.col, b.row);

        foreach (TileBehav tb in tbs)
            yield return tb.TriggerEnchantment(); // that easy?

        yield return null;
    }

    // Party Crashers
    protected override IEnumerator Spell2(TileSeq prereq) {
        _mm.audioCont.Trigger(AudioController.GravekeeperSFX.PartyCrashers);

        for (int i = 0; i < 2; i++) {
            yield return _mm.prompt.WaitForDropTile();
            if (!_mm.prompt.WasSuccessful())
                break;

            TileBehav tb = (TileBehav) _mm.prompt.GetDropHex();
            MMLog.Log_Gravekeeper("Player " + _playerId + " dropped " + tb.hextag);
            yield return _objFX.Ench_SetZombie(_playerId, tb, false);
            int col = _mm.prompt.GetDropCol();
            int nextTBrow = _mm.boardCheck.CheckColumn(col) - 1; // next TB under, if any

            yield return _mm.prompt.ContinueDrop();

            if (nextTBrow >= _mm.hexGrid.BottomOfColumn(col)) {
                _hexMan.RemoveTile(col, nextTBrow, false);
            }

            DealDamage(30);
        }
        yield return null;
    }

    // Undead Union
    protected override IEnumerator Spell3(TileSeq prereq) {
        _mm.audioCont.Trigger(AudioController.GravekeeperSFX.UndeadUnion);

        var tbs = TileFilter.GetTilesByEnch(Enchantment.Type.Zombie);
        MMLog.Log_Gravekeeper(tbs.Count + " zombs on the board...");

        int count = 0;
        foreach (TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.Type.Zombie) {
                List<TileBehav> adjTBs = _hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
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
        _mm.audioCont.Trigger(AudioController.GravekeeperSFX.SigBell1);
        _mm.audioCont.Trigger(AudioController.GravekeeperSFX.Motorcycle);

        DealDamage(225);

        yield return _targeting.WaitForCellTarget(1);

        CellBehav cb = _targeting.GetTargetCBs()[0];
        int col = cb.col;

        TombstoneTile tomb = (TombstoneTile)_hexMan.GenerateTile(_playerId, "Tombstone");
        tomb.transform.SetParent(GameObject.Find("tilesOnBoard").transform);

        _mm.effectCont.AddEndTurnEffect(new TurnEffect(_playerId, 5, Effect.Type.Add, tomb.Tombstone_Turn, tomb.Tombstone_TEnd), "tombs");

        // destroy tiles under token
        for (int row = _hexGrid.BottomOfColumn(col); row < _hexGrid.TopOfColumn(col); row++)
            if (_hexGrid.IsCellFilled(col, row))
                _hexMan.RemoveTile(col, row, false);
            else
                break; // TODO handle floating tiles

        _mm.DropTile(tomb, col); // idk how to animate this one yet

        _mm.audioCont.Trigger(AudioController.GravekeeperSFX.SigDrop);
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
