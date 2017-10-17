using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Enfuego : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

    private int swapsThisTurn = 0;

    public Enfuego(MageMatch mm, int id) : base(mm) {
        playerID = id;
        objFX = mm.hexFX;
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;

        characterName = "Enfuego";
        maxHealth = 1000;
        SetDeckElements(20, 0, 0, 15, 15);

        spells = new Spell[5];
        spells[0] = new SignatureSpell(0, "White-Hot Combo Kick", "MFFM", 1, 40, WhiteHotComboKick);
        spells[1] = new Spell(1, "¡Baila!", "FM", 1, Baila);
        spells[2] = new Spell(2, "DRAG SAMPLE", "FM", 1, DragSample);
        spells[3] = new Spell(3, "Hot Potatoes", "FFA", 1, HotPotatoes);
        spells[4] = new CoreSpell(4, "Fiery Fandango", 1, FieryFandango);

        InitSpells();
    }

    public override void OnEffectContLoad() {
        MMLog.Log_Enfuego("Loading PASSIVE...");
        // SwapEffect for incrementing swapsThisTurn
        SwapEffect se = new SwapEffect(-1, Enf_Swap, null);
        mm.effectCont.AddSwapEffect(se, "EnSwp");

        // TurnEffect for reseting the counter
        TurnEffect te = new TurnEffect(-1, Effect.Type.None, Enf_ResetSwaps, null); // idk about type here
        mm.effectCont.AddEndTurnEffect(te, "EnEnd");

        // when we have List<Buff>
        //Buff b = new Buff();
        //b.SetAdditional(Enf_Passive, true);
        //mm.GetPlayer(playerID).AddBuff(b);
    }

    public IEnumerator Enf_Swap(int id, int c1, int r1, int c2, int r2) {
        if(swapsThisTurn > 0)
            mm.GetPlayer(playerID).DealDamage(swapsThisTurn * 4);

        swapsThisTurn++;
        MMLog.Log_Enfuego("Incrementing swaps to " + swapsThisTurn);
        yield return null;
    }

    public IEnumerator Enf_ResetSwaps(int id) {
        swapsThisTurn = 0;
        MMLog.Log_Enfuego("Reseting swaps.");
        yield return null;
    }

    //public int Enf_Passive(Player p) {
    //    MMLog.Log_Enfuego("PASSIVE buff being called!");
    //    return swapsThisTurn * 4;
    //}

    public IEnumerator DoNothing() {
        mm.uiCont.SendSlidingText("This spell does nothing. Thanks!");
        yield return null;
    }

    public IEnumerator DragSample() {
        yield return targeting.WaitForDragTarget(4);
        if (targeting.WasCanceled())
            yield return null;

        foreach (TileBehav tb in targeting.GetTargetTBs()) {
            mm.StartCoroutine(objFX.Ench_SetBurning(playerID, tb));
        }
    }
    // TODO sample filter???

    // ----- spells -----

    // Signature
    public IEnumerator WhiteHotComboKick() {
        yield return targeting.WaitForTileTarget(3);
        if (targeting.WasCanceled())
            yield break;

        List<TileBehav> tTBs = targeting.GetTargetTBs();

        foreach (TileBehav tb in tTBs) { // maybe for instead?
            mm.ActiveP().DealDamage(70);

            if (tb.tile.element.Equals(Tile.Element.Fire)) { // spread Burning to 4 nearby
                List<TileBehav> ctbs = hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
                for (int i = 0; i < ctbs.Count; i++) {
                    TileBehav ctb = ctbs[i];
                    if (!ctb.CanSetEnch(Enchantment.EnchType.Burning)) {
                        ctbs.RemoveAt(i);
                        i--;
                    }
                }

                int burns = Mathf.Min(4, ctbs.Count);
                for (int i = 0; i < burns; i++) {
                    MMLog.Log_Enfuego("WHCK count=" + burns);
                    yield return mm.syncManager.SyncRand(playerID, Random.Range(0, ctbs.Count));
                    int index = mm.syncManager.GetRand();
                    TileBehav ctb = ctbs[index];
                    ctbs.RemoveAt(index);
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
        int burnNum = 2, dmg = 30;
        switch (seq.GetSeqLength()) {
            case 3: // safe to remove?
                break;
            case 4:
                dmg = 50;
                mm.GetOpponent(playerID).DiscardRandom(1);
                burnNum = 4;
                break;
            case 5:
                dmg = 80;
                mm.GetOpponent(playerID).DiscardRandom(2);
                burnNum = 7;
                break;
        }

        if (seq.GetElementAt(0).Equals(Tile.Element.Fire)) // not safe for multi-element tiles
            dmg += 20;
        mm.GetPlayer(playerID).DealDamage(dmg);

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
            yield return mm.syncManager.SyncRand(playerID, Random.Range(0, tbs.Count));
            int index = mm.syncManager.GetRand();
            TileBehav ctb = tbs[index];
            tbs.RemoveAt(index);
            MMLog.Log_Enfuego("Setting Burning to " + ctb.PrintCoord());
            mm.StartCoroutine(objFX.Ench_SetBurning(playerID, ctb)); // yield return?
        }

        yield return null;
    }

    public IEnumerator Baila() {
        yield return mm.GetPlayer(playerID).DrawTiles(2, "", false, false); // my draw
        yield return mm.GetOpponent(playerID).DrawTiles(2, "", false, false); // their draw

        yield return mm.prompt.WaitForSwap();
        mm.prompt.ContinueSwap();
    }

    public IEnumerator Incinerate() {
        yield return targeting.WaitForDragTarget(6);
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
            if (tb.GetEnchType() == Enchantment.EnchType.Burning) {
                MMLog.Log_Enfuego("Incinerate adding tile at " + tb.PrintCoord());
                filterTBs.Add(tb);
            }
        }
        return filterTBs;
    }

    public IEnumerator HotPotatoes() {
        HealthEffect he = new HealthEffect(mm.OpponentId(playerID), 3, HotPot_Buff, true, false);
        mm.effectCont.AddHealthEffect(he, "hotpo");

        yield return null;
    }
    public float HotPot_Buff(Player p, int dmg) {
        MMLog.Log_Enfuego("Hot Potato debuff on " + p.name + ", handcount=" + p.hand.Count());
        return p.hand.Count() * 3;
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
