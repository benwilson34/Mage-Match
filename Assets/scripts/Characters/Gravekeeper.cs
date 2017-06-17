using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Gravekeeper : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

    public Gravekeeper(MageMatch mm, int id, int loadout) : base(mm) {
        playerID = id;
        //this.mm = mm; //?
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;
        spellfx = mm.spellfx;
        spells = new Spell[4];
        characterName = "The Gravekeeper";
        if (loadout == 0)
            GravekeeperA();
        else
            GravekeeperB();
        InitSpells();
    }

    // FOCUS
    void GravekeeperA() { // The Gravekeeper A - Business in the Front
        loadoutName = "Business in the Front";
        maxHealth = 1150;

        SetDeckElements(0, 20, 40, 0, 40);

        spells[0] = new SignatureSpell(0, "Tombstone", "EMME", 1, 20, Tombstone);
        spells[1] = new Spell(1, "Human Resources", "MEW", 1, HumanResources);
        spells[2] = new Spell(2, "Undead Union", "EMM", 1, UndeadUnion);
        spells[3] = new CoreSpell(3, "Gather the Ghouls", 3, 1, GatherTheGhouls);
    }

    // Needs work, ignore for now
    void GravekeeperB() { // The Gravekeeper B - Party in the Back
        loadoutName = "Party in the Back";
        maxHealth = 1050;

        SetDeckElements(25, 0, 35, 0, 40);

        spells[0] = new Spell(0, "Raise Zombie", "EMME", 1, ZombifySpell);
        spells[1] = new Spell(1, "R.S.V.Z.", "MEM", 1, spellfx.Deal496Dmg); //
        spells[2] = new Spell(2, "The Oogie Boogie", "MFE", 1, spellfx.Deal496Dmg); //
        spells[3] = new Spell(3, "Bottle Rocket Mishap", "EMFM", 1, spellfx.Deal496Dmg); //
    }

    // ----- spells -----

    public IEnumerator HumanResources() {
        yield return targeting.WaitForTileAreaTarget(false);
        if (targeting.WasCanceled())
            yield break;

        List<TileBehav> tbs = targeting.GetTargetTBs();
        //foreach (TileBehav tb in tbs) {
        for(int i = 0; i < tbs.Count; i++) { // foreach
            TileBehav tb = tbs[i];
            if (tb.tile.element == Tile.Element.Muscle) {
                if (tb.CanSetEnch(Enchantment.EnchType.Zombify)) {
                    yield return spellfx.Ench_SetZombify(playerID, tb, false);
                    continue;
                }
            }
            tbs.RemoveAt(i);
            i--;
        }

        MMLog.Log_Gravekeeper("HumanResources trigger count=" + tbs.Count);
        foreach (TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.EnchType.Zombify) {
                MMLog.Log_Gravekeeper("Triggering zomb at " + tb.PrintCoord());
                yield return tb.TriggerEnchantment();
            }
        }
    }

    public IEnumerator UndeadUnion() {
        yield return targeting.WaitForTileAreaTarget(false);
        if (targeting.WasCanceled())
            yield break;

        List<TileBehav> tbs = targeting.GetTargetTBs();
        List<TileBehav> indestructTbs = new List<TileBehav>(tbs);
        for (int i = 0; i < tbs.Count; i++) { // filter zombs
            TileBehav tb = tbs[i];
            if (tb.GetEnchType() == Enchantment.EnchType.Zombify) {
                if (!tb.ableDestroy)
                    indestructTbs.RemoveAt(i);
            } else {
                tbs.RemoveAt(i);
                indestructTbs.RemoveAt(i);
                i--;
            }
        }
        MMLog.Log_Gravekeeper("Undead Union target has " + tbs.Count + " zombs in area");

        foreach (TileBehav tb in indestructTbs) {
            tb.ableDestroy = false;
            TileEffect e = new TileEffect(playerID, 2, Effect.Type.Enchant, null, UndeadUnion_TEnd); // dunno about these settings
            tb.AddTileEffect(e);
            e.SetEnchantee(tb);
            mm.effectCont.AddEndTurnEffect(e, "union"); 
        }

        DealAdjZombDmg(tbs);

        yield return null;
    }
    IEnumerator UndeadUnion_TEnd(int id, TileBehav tb) {
        MMLog.Log_Gravekeeper(">>>>>>>>>>>> UndeadUnion at " + tb.PrintCoord() + " is done!");
        tb.ableDestroy = true;
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
    public IEnumerator ZombifySpell() {
        yield return targeting.WaitForTileTarget(1);
        if (targeting.WasCanceled())
            yield break;
        TileBehav tb = targeting.GetTargetTBs()[0];
        yield return spellfx.Ench_SetZombify(playerID, tb, false);
    }

    public IEnumerator GatherTheGhouls() {
        List<TileBehav> tbs =  hexGrid.GetPlacedTiles();
        for (int i = 0; i < tbs.Count; i++) {
            TileBehav ctb = tbs[i];
            if (!ctb.CanSetEnch(Enchantment.EnchType.Zombify)) {
                tbs.RemoveAt(i);
                i--;
            }
        }

        int count = Mathf.Min(tbs.Count, 3);

        for (int i = 0; i < count; i++) {
            // TODO sync array, not in loop...maybe
            yield return mm.syncManager.SyncRand(playerID, Random.Range(0, tbs.Count));
            int rand = mm.syncManager.GetRand();
            yield return spellfx.Ench_SetZombify(playerID, tbs[rand], false);
            tbs.RemoveAt(rand);
        }
    }

    //void RaiseZombie_Target(TileBehav tb) {
    //    //GameObject zomb = mm.GenerateToken("zombie");
    //    //zomb.transform.SetParent(GameObject.Find("tilesOnBoard").transform); // move to MM.PutTile
    //    //mm.hexGrid.RaiseTileBehavIntoColumn(zomb.GetComponent<TileBehav>(), cb.col);
    //    Ench_SetZombify(playerID, tb, false);
    //}

    public IEnumerator Tombstone() {
        yield return mm.syncManager.SyncRand(playerID, Random.Range(95, 126));
        mm.GetPlayer(playerID).DealDamage(mm.syncManager.GetRand());

        yield return targeting.WaitForCellTarget(1);
        if (targeting.WasCanceled())
            yield break;

        CellBehav cb = targeting.GetTargetCBs()[0];
        int col = cb.col;
        GameObject tomb = mm.GenerateToken("tombstone");
        tomb.transform.SetParent(GameObject.Find("tilesOnBoard").transform);

        ZombieToken ttb = tomb.GetComponent<ZombieToken>();
        mm.effectCont.AddEndTurnEffect(new TurnEffect(5, Effect.Type.Add, ttb.Tombstone_Turn, ttb.Tombstone_TEnd), "tombs");

        // destroy tiles under token
        for (int row = hexGrid.BottomOfColumn(col); row < hexGrid.TopOfColumn(col); row++)
            if (hexGrid.IsCellFilled(col, row))
                mm.RemoveTile(col, row, false);
            else
                break; // TODO handle floating tiles

        mm.DropTile(col, tomb);
    }
}
