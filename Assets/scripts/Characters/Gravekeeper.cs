﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravekeeper : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

    public Gravekeeper(MageMatch mm, int id, int loadout) {
        this.mm = mm;
        playerID = id;
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;
        spellfx = new SpellEffects();
        spells = new Spell[4];
        characterName = "The Gravekeeper";
        if (loadout == 1)
            GravekeeperA();
        else
            GravekeeperB();
    }

    // FOCUS
    void GravekeeperA() { // The Gravekeeper A - Business in the Front
        loadoutName = "Business in the Front";
        maxHealth = 1150;

        SetDeckElements(0, 20, 40, 0, 40);

        spells[0] = new Spell(0, "Target Zombie", "EM", 1, ZombifySpell);
        spells[1] = new Spell(1, "Zombie Synergy", "MEE", 1, ZombieSynergy);
        spells[2] = new Spell(2, "Human Resources", "MEME", 1, HumanResources);
        spells[3] = new Spell(3, "Company Luncheon", "EMWM", 1, CompanyLuncheon);
    }

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

    public IEnumerator ZombieSynergy() {
        int count = 0;
        List<TileBehav> tbs = hexGrid.GetPlacedTiles();
        foreach (TileBehav tb in tbs) {
            if (tb.HasEnchantment() &&
                (tb.GetEnchType() == Enchantment.EnchType.Zombify ||
                tb.GetEnchType() == Enchantment.EnchType.ZombieTok)) {
                List<TileBehav> adjTBs = hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
                foreach (TileBehav adjTB in adjTBs) {
                    if (adjTB.HasEnchantment() &&
                        (adjTB.GetEnchType() == Enchantment.EnchType.Zombify ||
                        adjTB.GetEnchType() == Enchantment.EnchType.ZombieTok)) {
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
                    Ench_SetZombify(playerID, tb, false);
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

    public IEnumerator ZombifySpell() {
        yield return targeting.WaitForTileTarget(1);
        if (targeting.WasCanceled())
            yield break;
        TileBehav tb = targeting.GetTargetTBs()[0];
        Ench_SetZombify(playerID, tb, false);
    }
    //void RaiseZombie_Target(TileBehav tb) {
    //    //GameObject zomb = mm.GenerateToken("zombie");
    //    //zomb.transform.SetParent(GameObject.Find("tilesOnBoard").transform); // move to MM.PutTile
    //    //mm.hexGrid.RaiseTileBehavIntoColumn(zomb.GetComponent<TileBehav>(), cb.col);
    //    Ench_SetZombify(playerID, tb, false);
    //}

    // ----- enchant -----

    //private int zombify_col, zombify_row;
    //public bool zombify_select;

    public void Ench_SetZombify(int id, TileBehav tb, bool skip) {
        Enchantment ench = new Enchantment(id, Ench_Zombify_TEffect, null, null);
        ench.SetTypeTier(Enchantment.EnchType.Zombify, 1);
        ench.priority = 6;
        if (skip)
            ench.SkipCurrent();
        tb.SetEnchantment(ench);
        tb.GetComponent<SpriteRenderer>().color = new Color(0f, .4f, 0f);
        mm.effectCont.AddEndTurnEffect(ench, "zomb");
    }
    IEnumerator Ench_Zombify_TEffect(int id, TileBehav tb) {
        //zombify_select = false; // only use if you're sloppy. 
        if (tb == null)
            Debug.LogError("GRAVEKEEPER: >>>>>Zombify called with a null tile!! Maybe it was removed?");
        Debug.Log("GRAVEKEEPER: ----- Zombify at " + tb.PrintCoord() + " starting -----");

        List<TileBehav> tbs = hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
        Debug.Log("GRAVEKEEPER: Tile has " + tbs.Count + " tiles around it.");
        for (int i = 0; i < tbs.Count; i++) {
            TileBehav ctb = tbs[i];
            if (//ctb.GetEnchTier() > 1 || TODO??????
                ctb.GetEnchType() == Enchantment.EnchType.Zombify) { // other conditions where Zombify wouldn't work?
                tbs.RemoveAt(i);
                i--;
                Debug.Log("GRAVEKEEPER: Ignoring tile at " + ctb.PrintCoord());
            }
        }
        Debug.Log("GRAVEKEEPER: Tile now has " + tbs.Count + " available tiles around it.");

        //yield return new WaitUntil(() => zombify_select);
        //zombify_select = false;

        if (tbs.Count == 0) { // no targets
            Debug.Log("GRAVEKEEPER: Zombify at "+tb.PrintCoord()+" has no targets!");
            //mm.syncManager.SendZombifySelect(id, -1, -1);
            yield break;
        }

        int rand = Random.Range(0, tbs.Count);
        yield return mm.syncManager.SyncRand(playerID, rand);
        TileBehav selectTB = tbs[mm.syncManager.rand];
        Debug.Log("GRAVEKEEPER: Zombify attacking TB at " + selectTB.PrintCoord());

        //mm.syncManager.SendZombifySelect(id, selectTB.tile.col, selectTB.tile.row);

        yield return mm.animCont._Zombify_Attack(tb.transform, selectTB.transform); //anim 1

        if (selectTB.tile.element == Tile.Element.Muscle) {
            Tile t = selectTB.tile;
            yield return mm._RemoveTile(t.col, t.row, true); // maybe?

            mm.GetPlayer(id).DealDamage(10);
            mm.GetPlayer(id).Heal(10);
        } else {
            Ench_SetZombify(id, selectTB, true);
        }

        yield return mm.animCont._Zombify_Back(tb.transform); // anim 2

        Debug.Log("GRAVEKEEPER: ----- Zombify at " + tb.PrintCoord() + " done -----");
        yield return null; // needed?
    }

    //public void SetZombifySelect(int col, int row) {
    //    zombify_col = col;
    //    zombify_row = row;
    //    zombify_select = false;
    //}

    //public void Ench_SetZombieTok(int id, TileBehav tb) {
    //    Enchantment ench = new Enchantment(id, Ench_ZombieTok_TEffect, null, null);
    //    ench.SetTypeTier(Enchantment.EnchType.ZombieTok, 3);
    //    ench.priority = 6; // TODO 6.1?
    //    tb.SetEnchantment(ench);
    //    mm.effectCont.AddEndTurnEffect(ench, "zomT");
    //}
    //IEnumerator Ench_ZombieTok_TEffect(int id, TileBehav tb) {
    //    Debug.Log("SPELL-FX: About to resolve ZombTok's effect!");
    //    yield return Ench_Zombify_TEffect(id, tb);
    //    yield return Ench_Zombify_TEffect(id, tb);
    //    //yield return null; // ?
    //}
}
