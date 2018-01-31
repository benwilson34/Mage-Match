﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Enfuego : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

    private int swapsThisTurn = 0;

    public Enfuego(MageMatch mm, int id) : base(mm, Ch.Enfuego, id) {
        objFX = mm.hexFX;
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;
    }

    public override void OnEffectContLoad() {
        MMLog.Log_Enfuego("Loading PASSIVE...");
        // SwapEffect for incrementing swapsThisTurn
        SwapEffect se = new SwapEffect(-1, Passive_Swap, null);
        mm.effectCont.AddSwapEffect(se, "EnSwp");

        // TurnEffect for reseting the counter
        TurnEffect te = new TurnEffect(-1, Effect.Type.None, Passive_ResetSwaps, null); // idk about type here
        mm.effectCont.AddEndTurnEffect(te, "EnEnd");

        // when we have List<Buff>
        //Buff b = new Buff();
        //b.SetAdditional(Enf_Passive, true);
        //mm.GetPlayer(playerID).AddBuff(b);
    }

    public IEnumerator Passive_Swap(int id, int c1, int r1, int c2, int r2) {
        if(swapsThisTurn > 0)
            ThisPlayer().DealDamage(swapsThisTurn * 5);

        swapsThisTurn++;
        MMLog.Log_Enfuego("Incrementing swaps to " + swapsThisTurn);
        yield return null;
    }

    public IEnumerator Passive_ResetSwaps(int id) {
        swapsThisTurn = 0;
        MMLog.Log_Enfuego("Reseting swaps.");
        yield return null;
    }

    //public int Enf_Passive(Player p) {
    //    MMLog.Log_Enfuego("PASSIVE buff being called!");
    //    return swapsThisTurn * 4;
    //}

    public IEnumerator DragSample(TileSeq prereq) {
        yield return targeting.WaitForDragTarget(4);

        foreach (TileBehav tb in targeting.GetTargetTBs()) {
            mm.StartCoroutine(objFX.Ench_SetBurning(playerId, tb));
        }
    }
    // TODO sample filter???


    // -----  SPELLS  -----

    // Fiery Fandango
    protected override IEnumerator CoreSpell(TileSeq seq) {
        int burnNum = 2, dmg = 30;
        switch (seq.GetSeqLength()) {
            case 3: // safe to remove?
                break;
            case 4:
                dmg = 50;
                mm.GetOpponent(playerId).DiscardRandom(1);
                burnNum = 4;
                break;
            case 5:
                dmg = 80;
                mm.GetOpponent(playerId).DiscardRandom(2);
                burnNum = 7;
                break;
        }

        if (seq.GetElementAt(0).Equals(Tile.Element.Fire)) // not safe for multi-element tiles
            dmg += 20;
        ThisPlayer().DealDamage(dmg);

        List<TileBehav> tbs = mm.hexGrid.GetPlacedTiles(seq);
        for (int i = 0; i < tbs.Count; i++) {
            TileBehav tb = tbs[i];
            if (!tb.CanSetEnch(Enchantment.EnchType.Burning)) {
                tbs.RemoveAt(i);
                i--;
            }
        }

        burnNum = Mathf.Min(burnNum, tbs.Count);
        for (int i = 0; i < burnNum; i++) {
            yield return mm.syncManager.SyncRand(playerId, Random.Range(0, tbs.Count));
            int index = mm.syncManager.GetRand();
            TileBehav ctb = tbs[index];
            tbs.RemoveAt(index);
            MMLog.Log_Enfuego("Setting Burning to " + ctb.PrintCoord());
            mm.StartCoroutine(objFX.Ench_SetBurning(playerId, ctb)); // yield return?
        }

        yield return null;
    }

    // Baila!
    protected override IEnumerator Spell1(TileSeq prereq) {
        yield return ThisPlayer().DrawTiles(2, "", false, false); // my draw
        yield return mm.GetOpponent(playerId).DrawTiles(2, "", false, false); // their draw

        yield return mm.prompt.WaitForSwap(prereq);
        if(mm.prompt.WasSuccessful())
            yield return mm.prompt.ContinueSwap();
    }

    // Incinerate
    protected override IEnumerator Spell2(TileSeq prereq) {
        yield return targeting.WaitForDragTarget(6, Inc_Filter);

        List<TileBehav> tbs = targeting.GetTargetTBs();
        int dmg = tbs.Count * 35;
        foreach (TileBehav tb in tbs) {
            MMLog.Log_Enfuego("Destroying tile at " + tb.PrintCoord());
            mm.hexMan.RemoveTile(tb.tile, false);
            yield return new WaitForSeconds(.15f);
        }

        ThisPlayer().DealDamage(dmg);
    }
    public List<TileBehav> Inc_Filter(List<TileBehav> tbs) {
        List<TileBehav> filterTBs = new List<TileBehav>();
        foreach(TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.EnchType.Burning) {
                MMLog.Log_Enfuego("Incinerate adding tile at " + tb.PrintCoord());
                filterTBs.Add(tb);
            }
        }
        return filterTBs;
    }

    // Hot Potatoes
    protected override IEnumerator Spell3(TileSeq prereq) {
        HealthEffect he = new HealthEffect(mm.OpponentId(playerId), 3, HotPot_Buff, true, false);
        mm.effectCont.AddHealthEffect(he, "hotpo");

        yield return null;
    }
    public float HotPot_Buff(Player p, int dmg) {
        MMLog.Log_Enfuego("Hot Potato debuff on " + p.name + ", handcount=" + p.hand.Count());
        return p.hand.Count() * 3;
    }


    // White-Hot Combo Kick
    protected override IEnumerator SignatureSpell(TileSeq prereq) {
        yield return targeting.WaitForTileTarget(3);

        List<TileBehav> tbs = targeting.GetTargetTBs();

        foreach (TileBehav tb in tbs) {
            mm.ActiveP().DealDamage(70);

            if (tb.tile.element.Equals(Tile.Element.Fire)) { // spread Burning to 4 nearby
                List<TileBehav> ctbs = hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
                for (int i = 0; i < ctbs.Count; i++) {
                    TileBehav ctb = ctbs[i];
                    bool remove = false;
                    for (int j = 0; j < tbs.Count; j++) { // don't try to enchant other targets
                        if (ctb.tile.HasSamePos(tbs[j].tile)) {
                            remove = true;
                            break;
                        }
                    }

                    if (remove || !ctb.CanSetEnch(Enchantment.EnchType.Burning)) {
                        ctbs.RemoveAt(i);
                        i--;
                    }
                }

                int burns = Mathf.Min(4, ctbs.Count);
                for (int i = 0; i < burns; i++) {
                    MMLog.Log_Enfuego("WHCK count=" + burns);
                    yield return mm.syncManager.SyncRand(playerId, Random.Range(0, ctbs.Count));
                    int index = mm.syncManager.GetRand();
                    TileBehav ctb = ctbs[index];
                    ctbs.RemoveAt(index);
                    MMLog.Log_Enfuego("Setting Burning to " + ctb.PrintCoord());
                    mm.StartCoroutine(objFX.Ench_SetBurning(playerId, ctb)); // yield return?
                }
            } else if (tb.tile.element.Equals(Tile.Element.Muscle)) {
                yield return mm.InactiveP().DiscardRandom(1);
            }

            mm.hexMan.RemoveTile(tb.tile, true);
        }
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
