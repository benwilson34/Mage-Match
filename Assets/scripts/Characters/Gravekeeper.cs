﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (loadout == 1)
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

        spells[0] = new SignatureSpell(0, "Tombstone", "EM", 1, 20, Tombstone);
        spells[1] = new Spell(1, "Zombie Synergy", "MEE", 1, ZombieSynergy);
        spells[2] = new Spell(2, "Human Resources", "MEME", 1, HumanResources);
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

    // TODO Undead Union
    public IEnumerator ZombieSynergy() {
        int count = 0;
        List<TileBehav> tbs = hexGrid.GetPlacedTiles();
        foreach (TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.EnchType.Zombify ||
                tb.GetEnchType() == Enchantment.EnchType.ZombieTok) {
                List<TileBehav> adjTBs = hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
                foreach (TileBehav adjTB in adjTBs) {
                    if (adjTB.GetEnchType() == Enchantment.EnchType.Zombify ||
                        adjTB.GetEnchType() == Enchantment.EnchType.ZombieTok) {
                        count++;
                    }
                }
            }
        }
        Debug.Log("SPELLEFFECTS: Zombie Synergy has counted " + count + " adjacent zombs");
        mm.ActiveP().DealDamage(count * 4);

        yield return null;
    }

    public IEnumerator HumanResources() {
        targeting.WaitForTileAreaTarget(false, HumanResources_Target);
        yield return null;
    }
    void HumanResources_Target(List<TileBehav> tbs) {
        foreach (TileBehav tb in tbs) {
            if (tb.tile.element == Tile.Element.Muscle) {
                if (tb.GetEnchType() != Enchantment.EnchType.Zombify &&
                    tb.GetEnchType() != Enchantment.EnchType.ZombieTok)
                    spellfx.Ench_SetZombify(playerID, tb, false);
            }
        }
    }

    public IEnumerator CompanyLuncheon() {
        targeting.WaitForTileAreaTarget(false, CompanyLuncheon_Target);
        yield return null;
    }
    void CompanyLuncheon_Target(List<TileBehav> tbs) {
        for (int i = 0; i < tbs.Count; i++) {
            TileBehav tb = tbs[i];
            if (!tb.HasEnchantment() ||
                tb.GetEnchType() != Enchantment.EnchType.Zombify ||
                tb.GetEnchType() != Enchantment.EnchType.ZombieTok) {
                tbs.Remove(tb);
                i--;
            }
        }
        foreach (TileBehav tb in tbs)
            tb.TriggerEnchantment();
    }

    // sample
    public IEnumerator ZombifySpell() {
        yield return targeting.WaitForTileTarget(1);
        if (targeting.WasCanceled())
            yield break;
        TileBehav tb = targeting.GetTargetTBs()[0];
        spellfx.Ench_SetZombify(playerID, tb, false);
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
            spellfx.Ench_SetZombify(playerID, tbs[rand], false);
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
