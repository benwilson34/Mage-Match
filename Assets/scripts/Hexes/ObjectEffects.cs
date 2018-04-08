using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

// TODO rename to EnchantEffects - will token stuff go here too?
public class ObjectEffects {

    private MageMatch _mm;
    private HexManager _hexMan;
    private Targeting _targeting;
    private HexGrid _hexGrid;

    public ObjectEffects(MageMatch mm) {
        this._mm = mm;
        _hexMan = mm.hexMan;
        _targeting = mm.targeting;
        _hexGrid = mm.hexGrid;
    }

    // -------------------------------------- SPELLS ------------------------------------------

    //public IEnumerator Deal496Dmg(TileSeq prereq) {
    //    _mm.ActiveP().DealDamage(496);
    //    yield return null;
    //}

    //public IEnumerator StoneTest(int id) {
    //    yield return targeting.WaitForCellTarget(1);
    //    if (targeting.WasCanceled())
    //        yield break;

    //    CellBehav cb = targeting.GetTargetCBs()[0];
    //    HandObject stone = tileMan.GenerateToken(id, "stone");
    //    stone.transform.SetParent(GameObject.Find("tilesOnBoard").transform);
    //    mm.DropTile(cb.col, stone);
    //}

    public IEnumerator LightningPalm(int id) {
        yield return _targeting.WaitForTileTarget(1);
        List<TileBehav> tbs = _targeting.GetTargetTBs();
        if (tbs.Count != 1)
            yield return null;

        Tile.Element elem = tbs[0].tile.element;
        List<TileBehav> tileList = _hexGrid.GetPlacedTiles();
        for (int i = 0; i < tileList.Count; i++) {
            Tile tile = tileList[i].tile;
            if (tile.element.Equals(elem)) {
                _hexMan.RemoveTile(tile, true);
                _mm.GetPC(id).DealDamage(15);
            }
        }
    }

    public IEnumerator Cherrybomb(TileSeq prereq) {
        yield return _targeting.WaitForTileTarget(1);

        List<TileBehav> tbs = _targeting.GetTargetTBs();
        if (tbs.Count != 1)
            yield return null;

        TileBehav tb = _targeting.GetTargetTBs()[0];
        Ench_SetCherrybomb(_mm.ActiveP().id, tb); // right id?
    }


    // -------------------------------- ENCHANTMENTS --------------------------------------

    // TODO IEnum types for animation

    public void Ench_SetCherrybomb(int id, TileBehav tb) {
        Enchantment ench = new Enchantment(id, Enchantment.Type.Cherrybomb, Effect.Type.Destruct, null, null, Ench_Cherrybomb_Remove);
        tb.SetEnchantment(ench);
        tb.GetComponent<SpriteRenderer>().color = new Color(.4f, .4f, .4f);
    }
    IEnumerator Ench_Cherrybomb_Remove(int id, TileBehav tb) {
        MMLog.Log_EnchantFx("Resolving Cherrybomb at " + tb.PrintCoord());
        _mm.GetPC(id).DealDamage(200);

        List<TileBehav> tbs = _hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
        foreach (TileBehav ctb in tbs) {
            //if (ctb.ableDestroy) // shouldn't have to check here...
            _hexMan.RemoveTile(ctb.tile.col, ctb.tile.row, true);
        }
        yield return null; // for now
    }

    public IEnumerator Ench_SetBurning(int id, TileBehav tb) {
        // Burning does 3 dmg per tile per end-of-turn for 5 turns. It does double damage on expiration.
        //		Debug.MMLog.Log_EnchantFx("SPELLEFFECTS: Setting burning...");
        yield return _mm.animCont._Burning(tb);
        _mm.audioCont.Trigger(AudioController.EnfuegoSoundEffect.BurningEnchant);

        Enchantment ench = new Enchantment(id, 5, Enchantment.Type.Burning, Effect.Type.Damage, Ench_Burning_TEffect, Ench_Burning_End);
        ench.TriggerEffectEveryTurn();

        tb.SetEnchantment(ench);
        tb.GetComponent<SpriteRenderer>().color = new Color(1f, .4f, .4f);
        _mm.effectCont.AddEndTurnEffect(ench, "burn");
    }
    IEnumerator Ench_Burning_TEffect(int id, TileBehav tb) {
        MMLog.Log_EnchantFx("Burning TurnEffect at " + tb.PrintCoord());
        yield return _mm.animCont._Burning_Turn(_mm.GetOpponent(id), tb);
        _mm.audioCont.Trigger(AudioController.EnfuegoSoundEffect.BurningDamage);
        _mm.GetPC(id).DealDamage(3);
        //yield return null; // for now
    }
    IEnumerator Ench_Burning_End(int id, TileBehav tb) {
        _mm.GetPC(id).DealDamage(6);
        _mm.audioCont.Trigger(AudioController.EnfuegoSoundEffect.BurningTimeout);
        yield return null; // for now
    }

    public void Ench_SetStone(TileBehav tb) {
        Enchantment ench = new Enchantment(5, Enchantment.Type.StoneTok, Effect.Type.Destruct, Ench_StoneTok_TEffect, Ench_StoneTok_End, null);
        tb.SetEnchantment(ench);
        _mm.effectCont.AddEndTurnEffect(ench, "stoT");
    }
    IEnumerator Ench_StoneTok_TEffect(int id, TileBehav tb) {
        int c = tb.tile.col, r = tb.tile.row;
        if (_hexGrid.CellExists(c, r - 1) && _hexGrid.IsCellFilled(c, r - 1)) {
            _hexMan.RemoveTile(c, r - 1, false);
        }
        yield return null; // for now
    }
    IEnumerator Ench_StoneTok_End(int id, TileBehav tb) {
        _hexMan.RemoveTile(tb.tile, false);
        yield return null; // for now
    }

    // TODO take skip out since Zombie doesn't have a turn effect anymore
    public IEnumerator Ench_SetZombie(int id, TileBehav tb, bool skip = false, bool anim = true) {
        if (anim) {
            yield return _mm.animCont._Zombify(tb);
            _mm.audioCont.Trigger(AudioController.GraveKSoundEffect.ZombieEnchant);
        }

        Enchantment ench = new Enchantment(id, Enchantment.Type.Zombie, Effect.Type.Enchant, Ench_Zombie_TEffect, null);
        if (skip)
            ench.SkipCurrent();

        MMLog.Log("ObjEffects", "orange", "Zombify's enchtype is "+ench.enchType);

        _mm.effectCont.AddEndTurnEffect(ench, "zomb");

        tb.SetEnchantment(ench);
        tb.GetComponent<SpriteRenderer>().color = new Color(0f, .4f, 0f);
        yield return null;
    }
    IEnumerator Ench_Zombie_TEffect(int id, TileBehav tb) {
        //zombify_select = false; // only use if you're sloppy. 
        if (tb == null)
            MMLog.LogError("SPELLFX: >>>>>Zombify called with a null tile!! Maybe it was removed?");
        //MMLog.Log_EnchantFx("----- Zombify at " + tb.PrintCoord() + " starting -----");

        List<TileBehav> tbs = _hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
        //MMLog.Log_EnchantFx("Tile has " + tbs.Count + " tiles around it.");
        for (int i = 0; i < tbs.Count; i++) {
            TileBehav ctb = tbs[i];
            if (!ctb.CanSetEnch(Enchantment.Type.Zombie)) { // other conditions where Zombify wouldn't work?
                tbs.RemoveAt(i);
                i--;
                //MMLog.Log_EnchantFx("Ignoring tile at " + ctb.PrintCoord());
            }
        }
        //MMLog.Log_EnchantFx("Tile now has " + tbs.Count + " available tiles around it.");

        //yield return new WaitUntil(() => zombify_select);
        //zombify_select = false;

        if (tbs.Count == 0) { // no targets
            //MMLog.Log_EnchantFx("Zombify at " + tb.PrintCoord() + " has no targets!");
            //mm.syncManager.SendZombifySelect(id, -1, -1);
            yield break;
        }

        int rand = Random.Range(0, tbs.Count);
        yield return _mm.syncManager.SyncRand(id, rand);
        TileBehav selectTB = tbs[_mm.syncManager.GetRand()];
        MMLog.Log_EnchantFx("Zombify attacking TB at " + selectTB.PrintCoord());

        //mm.syncManager.SendZombifySelect(id, selectTB.tile.col, selectTB.tile.row);

        yield return _mm.animCont._Zombify_Attack(tb.transform, selectTB.transform); // anim 1

        if (selectTB.tile.element == Tile.Element.Muscle) {
            yield return _hexMan._RemoveTile(selectTB, true); // maybe?
            _mm.audioCont.Trigger(AudioController.GraveKSoundEffect.ZombieGulp);

            _mm.GetPC(id).DealDamage(10);
            _mm.GetPC(id).Heal(10);
        } else {
            yield return Ench_SetZombie(id, selectTB, true, false);
            _mm.audioCont.Trigger(AudioController.GraveKSoundEffect.ZombieAttack);
        }

        yield return _mm.animCont._Zombify_Back(tb.transform); // anim 2

        //MMLog.Log_EnchantFx("----- Zombify at " + tb.PrintCoord() + " done -----");
        yield return null; // needed?
    }
}