using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Valeria : Character {

    public Valeria(MageMatch mm, int id) : base(mm, Ch.Valeria, id) { }

    public override void OnEffectControllerLoad() {
        MMLog.Log("Valeria","cyan","Loading PASSIVE...");
        SwapEffect se = new SwapEffect(_playerId, "ValeriaPassive_Heal", Effect.Behav.Healing, Passive_Swap);
        EffectManager.AddEventEffect(se);

        // when we have List<Buff>
        //Buff b = new Buff();
        //b.SetAdditional(Enf_Passive, true);
        //mm.GetPlayer(playerID).AddBuff(b);
    }

    public IEnumerator Passive_Swap(SwapEventArgs args) {
        if (args.id == _playerId) {
            const int healAmt = 7;
            Heal(healAmt);
            AudioController.Trigger(SFX.Valeria.Healing);
        }
        yield return null;
    }


    #region ---------- SPELLS ----------

    // Whirlpool Spin
    protected override IEnumerator MatchSpell(TileSeq seq) {
        AudioController.Trigger(SFX.Valeria.SwirlingWater);

        int dmg = 0, swaps = 0;
        switch (seq.GetSeqLength()) {
            case 3:
                dmg = 20;
                break;
            case 4:
                dmg = 40;
                swaps = 1;
                break;
            case 5:
                dmg = 70;
                swaps = 2;
                break;
        }

        if (seq.GetTileAt(0).IsElement(Tile.Element.Water))
            swaps++;

        DealDamage(dmg);

        MMLog.Log("Valeria", "magenta", "swaps="+swaps);
        Prompt.SetSwapCount(swaps);
        for (int i = 0; i < swaps; i++) {
            MMLog.Log("Valeria", "pink", "Waiting for swap " + (i+1) + " of " + swaps);
            yield return Prompt.WaitForSwap(seq);
            if (Prompt.WasSuccessful) {
                yield return Prompt.ContinueSwap();
                AudioController.Trigger(SFX.Valeria.Bubbles2);
            }
        }

        yield return null;
    }

    // Mariposa
    protected override IEnumerator Spell1(TileSeq prereq) {
        // deal 30-60 dmg
        yield return _mm.syncManager.SyncRand(_playerId, Random.Range(30, 61));
        int dmg = _mm.syncManager.GetRand();
        DealDamage(dmg);

        // get targets
        yield return Targeting.WaitForTileTarget(2);
        List<TileBehav> tbs = Targeting.GetTargetTBs();
        if (tbs.Count < 2)
            yield break;

        TileBehav first = tbs[0], second = tbs[1];
        int firstCol = first.tile.col, firstRow = first.tile.row;

        // move first to second and destroy second
        int secCol = second.tile.col, secRow = second.tile.row;
        yield return HexManager._RemoveTile(second, false);
        AudioController.Trigger(SFX.Valeria.Mariposa);
        yield return first._ChangePos(secCol, secRow, 0.3f);

        // put new water tile in first's place
        TileBehav waterTile = HexManager.GenerateBasicTile(_playerId, Tile.Element.Water);
        waterTile.SetPlaced(); // this kinda sucks here
        yield return waterTile._ChangePos(firstCol, firstRow);

        yield return null;
    }

    // Rain Dance
    protected override IEnumerator Spell2(TileSeq prereq) {
        AudioController.Trigger(SFX.Valeria.Rain);
        AudioController.Trigger(SFX.Valeria.ThunderFar);

        TurnEffect te = new TurnBeginEffect(_playerId, "RainDance", Effect.Behav.Add, RainDance_T) { turnsLeft = 5 };
        EffectManager.AddEventEffect(te);
        yield return null;
    }
    public IEnumerator RainDance_T(int id) {
        AudioController.Trigger(SFX.Valeria.RainDance);

        yield return _mm.syncManager.SyncRand(_playerId, Random.Range(3, 6));
        int dropCount = _mm.syncManager.GetRand();

        // random water drops
        yield return CommonEffects.DropBasicsIntoRandomCols(_playerId, Tile.Element.Water, dropCount);

        // heal 15-25
        yield return _mm.syncManager.SyncRand(_playerId, Random.Range(15, 26));
        int healing = _mm.syncManager.GetRand();
        Heal(healing);
        AudioController.Trigger(SFX.Valeria.Healing);

        yield return null;
    }

    // Balanco
    protected override IEnumerator Spell3(TileSeq prereq) {
        AudioController.Trigger(SFX.Valeria.Balanco);

        const int numTurns = 3;

        DropEffect de = new DropEffect(_playerId, "Balanco_Dmg", Effect.Behav.Damage, Balanco_Drop) { turnsLeft = numTurns };
        EffectManager.AddEventEffect(de);

        SwapEffect se = new SwapEffect(_playerId, "Balanco_Dmg", Effect.Behav.Damage, Balanco_Swap) { turnsLeft = numTurns };
        EffectManager.AddEventEffect(se);
        yield return null;
    }
    public IEnumerator Balanco_Drop(DropEventArgs args) {
        if (args.hex is TileBehav && ((TileBehav)args.hex).tile.IsElement(Tile.Element.Water))
            DealDamage(7);
        yield return null;    
    }
    public IEnumerator Balanco_Swap(SwapEventArgs args) {
        Tile tile1 = HexGrid.GetTileAt(args.c1, args.r1);
        Tile tile2 = HexGrid.GetTileAt(args.c2, args.r2);
        int dmg = 0;
        if (tile1.IsElement(Tile.Element.Water))
            dmg += 7;
        if (tile2.IsElement(Tile.Element.Water))
            dmg += 7;

        if (dmg > 0)
            DealDamage(dmg);
        yield return null;
    }

    // Hurricane Cutter
    protected override IEnumerator SignatureSpell(TileSeq prereq) {
        AudioController.Trigger(SFX.Valeria.ThunderClose);
        AudioController.Trigger(SFX.Valeria.Rain);

        yield return HandleMatchesOnBoard();

        const int numSwaps = 4;
        Prompt.SetSwapCount(numSwaps);
        for (int i = 0; i < numSwaps; i++) {
            yield return Prompt.WaitForSwap(prereq);
            if (!Prompt.WasSuccessful)
                break;

            yield return Prompt.ContinueSwap();
            AudioController.Trigger(SFX.Valeria.Sig_Cut);

            yield return HandleMatchesOnBoard();
        }

        yield return null;
    }

    IEnumerator HandleMatchesOnBoard() {
        var spells = new List<Spell>();
        spells.Add(new MatchSpell(0, "", null));

        List<TileSeq> seqs = BoardCheck.CheckBoard(spells)[0];
        while (seqs.Count > 0) {  // this is needed to handle cascades
            for (int s = 0; s < seqs.Count; s++) {
                TileSeq seq = seqs[s];
                int dmg = 0, dropCount = 0;
                switch (seq.GetSeqLength()) {
                    case 3:
                        dmg = 30;
                        dropCount = 2;
                        break;
                    case 4:
                        dmg = 60;
                        dropCount = 3;
                        break;
                    case 5:
                        dmg = 100;
                        dropCount = 4;
                        break;
                }

                yield return HexManager._RemoveSeq(seq, false);

                DealDamage(dmg);
                yield return CommonEffects.DropBasicsIntoRandomCols(_playerId, Tile.Element.Water, dropCount);
            }
            AudioController.Trigger(SFX.Valeria.Sig_WaveCrash);

            // wait for board to update...will this work?
            yield return _mm.BoardChecking(false);

            seqs = BoardCheck.CheckBoard(spells)[0];
        }
        yield return null;
    }
    #endregion
}
