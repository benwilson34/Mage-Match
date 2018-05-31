using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Valeria : Character {

    private HexGrid _hexGrid; // eventually these will be static again?
    private Targeting _targeting; // ''

    public Valeria(MageMatch mm, int id) : base(mm, Ch.Valeria, id) {
        _objFX = mm.hexFX;
        _hexGrid = mm.hexGrid;
        _targeting = mm.targeting;
    }

    public override void OnEffectContLoad() {
        MMLog.Log("Valeria","cyan","Loading PASSIVE...");
        SwapEffect se = new SwapEffect(_playerId, Passive_Swap);
        _mm.effectCont.AddSwapEffect(se, "VlSwp");

        // when we have List<Buff>
        //Buff b = new Buff();
        //b.SetAdditional(Enf_Passive, true);
        //mm.GetPlayer(playerID).AddBuff(b);
    }

    public IEnumerator Passive_Swap(int id, int c1, int r1, int c2, int r2) {
        Heal(7);
        AudioController.Trigger(AudioController.ValeriaSFX.Healing);
        yield return null;
    }

    //public int Enf_Passive(Player p) {
    //    MMLog.Log_Enfuego("PASSIVE buff being called!");
    //    return swapsThisTurn * 4;
    //}


    // -----  SPELLS  -----

    // Whirlpool Spin
    protected override IEnumerator CoreSpell(TileSeq seq) {
        AudioController.Trigger(AudioController.ValeriaSFX.SwirlingWater);

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
        for (int i = 0; i < swaps; i++) {
            MMLog.Log("Valeria", "pink", "Waiting for swap " + (i+1) + " of " + swaps);
            yield return _mm.prompt.WaitForSwap(seq);
            if (_mm.prompt.WasSuccessful()) {
                yield return _mm.prompt.ContinueSwap();
                AudioController.Trigger(AudioController.ValeriaSFX.Bubbles2);
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
        yield return _targeting.WaitForTileTarget(2);
        List<TileBehav> tbs = _targeting.GetTargetTBs();
        if (tbs.Count < 2)
            yield break;

        TileBehav first = tbs[0], second = tbs[1];
        int firstCol = first.tile.col, firstRow = first.tile.row;

        // move first to second and destroy second
        int secCol = second.tile.col, secRow = second.tile.row;
        yield return _hexMan._RemoveTile(second, false);
        AudioController.Trigger(AudioController.ValeriaSFX.Mariposa);
        yield return first._ChangePos(secCol, secRow, 0.3f);

        // put new water tile in first's place
        TileBehav waterTile = _hexMan.GenerateBasicTile(_playerId, Tile.Element.Water);
        waterTile.SetPlaced(); // this kinda sucks here
        yield return waterTile._ChangePos(firstCol, firstRow);

        yield return null;
    }

    // Rain Dance
    protected override IEnumerator Spell2(TileSeq prereq) {
        AudioController.Trigger(AudioController.ValeriaSFX.Rain);
        AudioController.Trigger(AudioController.ValeriaSFX.ThunderFar);

        TurnEffect te = new TurnEffect(_playerId, Effect.Type.Add, RainDance_T, null, 5);
        _mm.effectCont.AddBeginTurnEffect(te, "rainD");
        yield return null;
    }
    public IEnumerator RainDance_T(int id) {
        AudioController.Trigger(AudioController.ValeriaSFX.RainDance);

        yield return _mm.syncManager.SyncRand(_playerId, Random.Range(3, 6));
        int dropCount = _mm.syncManager.GetRand();

        // random water drops
        yield return DropWaterIntoRandomCols(dropCount);

        // heal 15-25
        yield return _mm.syncManager.SyncRand(_playerId, Random.Range(15, 26));
        int healing = _mm.syncManager.GetRand();
        Heal(healing);
        AudioController.Trigger(AudioController.ValeriaSFX.Healing);

        yield return null;
    }

    public IEnumerator DropWaterIntoRandomCols(int count) {
        int[] cols = _mm.boardCheck.GetRandomCols(count);
        yield return _mm.syncManager.SyncRands(_playerId, cols);
        cols = _mm.syncManager.GetRands(cols.Length);

        string s = "";
        for (int i = 0; i < cols.Length; i++) {
            s += cols[i];
            if(i < cols.Length - 1)
                s += ", ";
        }
        MMLog.Log("Valeria", "magenta", "random water columns are [" + s + "]");

        Queue<int> colQ = new Queue<int>(cols);

        int col;
        while (colQ.Count > 0) {
            col = colQ.Dequeue();
            TileBehav newWater = _hexMan.GenerateBasicTile(_playerId, Tile.Element.Water);
            _mm.DropTile(newWater, col);
        }
    }

    // Balanco
    protected override IEnumerator Spell3(TileSeq prereq) {
        AudioController.Trigger(AudioController.ValeriaSFX.Balanco);

        DropEffect de = new DropEffect(_playerId, Balanco_Drop, 3);
        de.isGlobal = true;
        _mm.effectCont.AddDropEffect(de, "balDp");

        SwapEffect se = new SwapEffect(_playerId, Balanco_Swap, 3);
        se.isGlobal = true;
        _mm.effectCont.AddSwapEffect(se, "balSw");
        yield return null;
    }
    public IEnumerator Balanco_Drop(int id, bool playerAction, string tag, int col) {
        if (Hex.TagTitle(tag) == "W")
            DealDamage(7);
        yield return null;    
    }
    public IEnumerator Balanco_Swap(int id, int c1, int r1, int c2, int r2) {
        Tile tile1 = _mm.hexGrid.GetTileAt(c1, r1);
        Tile tile2 = _mm.hexGrid.GetTileAt(c2, r2);
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
        AudioController.Trigger(AudioController.ValeriaSFX.ThunderClose);
        AudioController.Trigger(AudioController.ValeriaSFX.Rain);

        yield return HandleMatchesOnBoard();

        const int numSwaps = 4;
        for (int i = 0; i < numSwaps; i++) {
            yield return _mm.prompt.WaitForSwap(prereq);
            if (!_mm.prompt.WasSuccessful())
                break;

            yield return _mm.prompt.ContinueSwap();
            AudioController.Trigger(AudioController.ValeriaSFX.SigCut);

            yield return HandleMatchesOnBoard();
        }

        yield return null;
    }

    IEnumerator HandleMatchesOnBoard() {
        var spells = new List<Spell>();
        spells.Add(new CoreSpell(0, "", null));

        List<TileSeq> seqs = _mm.boardCheck.CheckBoard(spells)[0];
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

                yield return _mm.hexMan._RemoveSeq(seq, false);

                DealDamage(dmg);
                yield return DropWaterIntoRandomCols(dropCount);
            }
            AudioController.Trigger(AudioController.ValeriaSFX.SigWaveCrash);

            // wait for board to update...will this work?
            yield return _mm.BoardChecking(false);

            seqs = _mm.boardCheck.CheckBoard(spells)[0];
        }
        yield return null;
    }
}
