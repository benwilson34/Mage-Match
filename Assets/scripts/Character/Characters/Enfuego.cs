using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Enfuego : Character {

    private int _passive_swapsThisTurn = 0;

    public Enfuego(MageMatch mm, int id) : base(mm, Ch.Enfuego, id) { }

    public override void OnEffectControllerLoad() {
        MMLog.Log_Enfuego("Loading PASSIVE...");
        // SwapEffect for incrementing swapsThisTurn
        SwapEffect se = new SwapEffect(_playerId, "EnfPassive_Damage", Effect.Behav.Damage, Passive_OnSwap);
        EffectManager.AddEventEffect(se);

        // TurnEffect for reseting the counter
        TurnEffect te = new TurnEndEffect(_playerId, "EnfPassive_Reset", Effect.Behav.TickDown, Passive_OnTurnEnd);
        EffectManager.AddEventEffect(te);

        // when we have List<Buff>
        //Buff b = new Buff();
        //b.SetAdditional(Enf_Passive, true);
        //mm.GetPlayer(playerID).AddBuff(b);
    }


    #region ---------- PASSIVE ----------

    public IEnumerator Passive_OnSwap(SwapEventArgs args) {
        if (args.id != _playerId)
            yield break;

        const int swapDmg = 5;
        if(_passive_swapsThisTurn > 0)
            DealDamage(_passive_swapsThisTurn * swapDmg);

        _passive_swapsThisTurn++;
        MMLog.Log_Enfuego("Incrementing swaps to " + _passive_swapsThisTurn);
        yield return null;
    }

    public IEnumerator Passive_OnTurnEnd(int id) {
        if (id == _playerId) {
            // reset swaps for the next turn
            _passive_swapsThisTurn = 0;
            MMLog.Log_Enfuego("Reseting swaps.");
        }
        yield return null;
    }
    #endregion


    public IEnumerator DragSample(TileSeq prereq) {
        yield return Targeting.WaitForDragTarget(4);

        foreach (TileBehav tb in Targeting.GetTargetTBs()) {
            _mm.StartCoroutine(Burning.Set(_playerId, tb));
        }
    }


    #region ----------  SPELLS  ----------

    // Fiery Fandango
    protected override IEnumerator MatchSpell(TileSeq seq) {
        AudioController.Trigger(SFX.Enfuego.FieryFandango);

        int burnNum = 2, dmg = 30;
        switch (seq.GetSeqLength()) {
            case 3: // safe to remove?
                break;
            case 4:
                dmg = 50;
                yield return Opponent.Hand._DiscardRandom();
                burnNum = 4;
                break;
            case 5:
                dmg = 80;
                yield return Opponent.Hand._DiscardRandom(2);
                burnNum = 7;
                break;
        }

        if (seq.GetTileAt(0).IsElement(Tile.Element.Fire)) 
            dmg += 20;
        DealDamage(dmg);

        List<TileBehav> tbs = TileFilter.GetTilesByAbleEnch(Enchantment.Type.Burning);

        burnNum = Mathf.Min(burnNum, tbs.Count);
        for (int i = 0; i < burnNum; i++) {
            yield return _mm.syncManager.SyncRand(_playerId, Random.Range(0, tbs.Count));
            int index = _mm.syncManager.GetRand();
            TileBehav ctb = tbs[index];
            tbs.RemoveAt(index);
            MMLog.Log_Enfuego("Setting Burning to " + ctb.PrintCoord());
            _mm.StartCoroutine(Burning.Set(_playerId, ctb)); // yield return?
        }

        yield return null;
    }

    // Baila!
    protected override IEnumerator Spell1(TileSeq prereq) {
        AudioController.Trigger(SFX.Enfuego.Baila);

        yield return ThisPlayer.DrawHexes(2); // my draw
        yield return Opponent.DrawHexes(2); // their draw

        yield return Prompt.WaitForSwap(prereq);
        if(Prompt.WasSuccessful)
            yield return Prompt.ContinueSwap();
    }

    // Incinerate
    protected override IEnumerator Spell2(TileSeq prereq) {
        yield return Targeting.WaitForDragTarget(6, Inc_Filter);

        AudioController.Trigger(SFX.Enfuego.Incinerate);

        List<TileBehav> tbs = Targeting.GetTargetTBs();
        int dmg = tbs.Count * 35;
        foreach (TileBehav tb in tbs) {
            MMLog.Log_Enfuego("Destroying tile at " + tb.PrintCoord());
            HexManager.RemoveTile(tb.tile, false);
            yield return AnimationController.WaitForSeconds(.15f);
        }

        DealDamage(dmg);
    }
    public List<TileBehav> Inc_Filter(List<TileBehav> tbs) {
        List<TileBehav> filterTBs = new List<TileBehav>();
        foreach(TileBehav tb in tbs) {
            if (tb.GetEnchType() == Enchantment.Type.Burning) {
                MMLog.Log_Enfuego("Incinerate adding tile at " + tb.PrintCoord());
                filterTBs.Add(tb);
            }
        }
        return filterTBs;
    }

    // Hot Potatoes
    protected override IEnumerator Spell3(TileSeq prereq) {
        HealthModEffect he = new HealthModEffect(_playerId, "HotPotatoes", HotPot_Debuff, HealthModEffect.Type.ReceivingBonus, true) { turnsLeft = 3 };
        EffectManager.AddHealthMod(he);

        yield return null;
    }
    public float HotPot_Debuff(Player p, int dmg) {
        MMLog.Log_Enfuego("Hot Potato debuff on " + p.Name + ", handcount=" + p.Hand.Count);
        // TODO sound fx
        const int dmgPerHexInHand = 3;
        return p.Hand.Count * dmgPerHexInHand;
    }

    // White-Hot Combo Kick
    protected override IEnumerator SignatureSpell(TileSeq prereq) {
        yield return Targeting.WaitForTileTarget(3);

        List<TileBehav> tbs = Targeting.GetTargetTBs();

        foreach (TileBehav tb in tbs) {
            DealDamage(70);

            if (tb.tile.IsElement(Tile.Element.Fire)) { // spread Burning to 4 nearby
                List<TileBehav> ctbs = HexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
                for (int i = 0; i < ctbs.Count; i++) {
                    TileBehav ctb = ctbs[i];
                    bool remove = false;
                    for (int j = 0; j < tbs.Count; j++) { // don't try to enchant other targets
                        if (ctb.tile.HasSamePos(tbs[j].tile)) {
                            remove = true;
                            break;
                        }
                    }

                    if (remove || !ctb.CanSetEnch(Enchantment.Type.Burning)) {
                        ctbs.RemoveAt(i);
                        i--;
                    }
                }

                int burns = Mathf.Min(4, ctbs.Count);
                for (int i = 0; i < burns; i++) {
                    MMLog.Log_Enfuego("WHCK count=" + burns);
                    yield return _mm.syncManager.SyncRand(_playerId, Random.Range(0, ctbs.Count));
                    int index = _mm.syncManager.GetRand();
                    TileBehav ctb = ctbs[index];
                    ctbs.RemoveAt(index);
                    MMLog.Log_Enfuego("Setting Burning to " + ctb.PrintCoord());
                    _mm.StartCoroutine(Burning.Set(_playerId, ctb)); // yield return?
                }
            } else if (tb.tile.IsElement(Tile.Element.Muscle)) {
                yield return Opponent.Hand._DiscardRandom();
            }

            HexManager.RemoveTile(tb.tile, true);
            AudioController.Trigger(SFX.Enfuego.Sig_WHCK);

            yield return AnimationController.WaitForSeconds(.4f);
        }
    }
    #endregion

    //public IEnumerator Backburner() {
    //    yield return mm.syncManager.SyncRand(playerID, Random.Range(15, 26));
    //    int dmg = mm.syncManager.GetRand();
    //    mm.GetPlayer(playerID).DealDamage(dmg);

    //    mm.EffectController.AddMatchEffect(new MatchEffect(1, Backburner_Match, null, 1), "backb");
    //    yield return null;
    //}
    //IEnumerator Backburner_Match(int id) {
    //    MMLog.Log_Enfuego("Rewarding player " + id + " with 1 AP.");
    //    mm.GetPlayer(id).AP++;
    //    yield return null;
    //}

    //public IEnumerator HotBody() {
    //    mm.EffectController.AddSwapEffect(new SwapEffect(3, HotBody_Swap, null), "hotbd");
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
