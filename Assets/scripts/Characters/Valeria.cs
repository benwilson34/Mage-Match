using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Valeria : Character {

    private HexGrid hexGrid; // eventually these will be static again?
    private Targeting targeting; // ''

    public Valeria(MageMatch mm, int id) : base(mm, Ch.Valeria, id) {
        objFX = mm.hexFX;
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;
    }

    public override void OnEffectContLoad() {
        MMLog.Log("Valeria","cyan","Loading PASSIVE...");
        SwapEffect se = new SwapEffect(playerId, Passive_Swap);
        mm.effectCont.AddSwapEffect(se, "VlSwp");

        // when we have List<Buff>
        //Buff b = new Buff();
        //b.SetAdditional(Enf_Passive, true);
        //mm.GetPlayer(playerID).AddBuff(b);
    }

    public IEnumerator Passive_Swap(int id, int c1, int r1, int c2, int r2) {
        ThisPlayer().Heal(7);
        yield return null;
    }

    //public int Enf_Passive(Player p) {
    //    MMLog.Log_Enfuego("PASSIVE buff being called!");
    //    return swapsThisTurn * 4;
    //}


    // -----  SPELLS  -----

    // Whirlpool Spin
    protected override IEnumerator CoreSpell(TileSeq seq) {
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

        if (seq.GetElementAt(0) == Tile.Element.Water) // not safe for multicolored tiles
            swaps++;

        ThisPlayer().DealDamage(dmg);

        MMLog.Log("Valeria", "magenta", "swaps="+swaps);
        for (int i = 0; i < swaps; i++) {
            MMLog.Log("Valeria", "pink", "Waiting for swap " + (i+1) + " of " + swaps);
            yield return mm.prompt.WaitForSwap(seq);
            if (mm.prompt.WasSuccessful())
                yield return mm.prompt.ContinueSwap();
        }

        yield return null;
    }

    // Mariposa
    protected override IEnumerator Spell1(TileSeq prereq) {
        // deal 30-60 dmg
        yield return mm.syncManager.SyncRand(playerId, Random.Range(30, 61));
        int dmg = mm.syncManager.GetRand();
        ThisPlayer().DealDamage(dmg);

        // get targets
        yield return targeting.WaitForTileTarget(2);
        List<TileBehav> tbs = targeting.GetTargetTBs();
        if (tbs.Count < 2)
            yield break;

        TileBehav first = tbs[0], second = tbs[1];
        int firstCol = first.tile.col, firstRow = first.tile.row;

        // move first to second and destroy second
        int secCol = second.tile.col, secRow = second.tile.row;
        yield return hexMan._RemoveTile(second, false);
        yield return first._ChangePos(secCol, secRow, 0.3f);

        // put new water tile in first's place
        TileBehav waterTile = (TileBehav)hexMan.GenerateTile(playerId, Tile.Element.Water);
        waterTile.SetPlaced(); // this kinda sucks here
        yield return waterTile._ChangePos(firstCol, firstRow);

        yield return null;
    }

    // Rain Dance
    protected override IEnumerator Spell2(TileSeq prereq) {
        TurnEffect te = new TurnEffect(5, Effect.Type.Add, RainDance_T, null);
        mm.effectCont.AddBeginTurnEffect(te, "rainD");
        yield return null;
    }
    public IEnumerator RainDance_T(int id) {
        yield return mm.syncManager.SyncRand(playerId, Random.Range(3, 6));
        int dropCount = mm.syncManager.GetRand();

        // random water drops
        yield return DropWaterIntoRandomCols(dropCount);

        // heal 15-25
        yield return mm.syncManager.SyncRand(playerId, Random.Range(15, 26));
        int healing = mm.syncManager.GetRand();
        ThisPlayer().Heal(healing);

        yield return null;
    }

    IEnumerator DropWaterIntoRandomCols(int count) {
        int[] cols = mm.boardCheck.GetRandomCols(count);
        yield return mm.syncManager.SyncRands(playerId, cols);
        cols = mm.syncManager.GetRands(cols.Length);

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
            TileBehav newWater = (TileBehav)hexMan.GenerateTile(playerId, Tile.Element.Water);
            mm.DropTile(col, newWater);
        }
    }

    // Balanco
    protected override IEnumerator Spell3(TileSeq prereq) {
        DropEffect de = new DropEffect(playerId, Balanco_Drop, 3);
        de.isGlobal = true;
        mm.effectCont.AddDropEffect(de, "balDp");

        SwapEffect se = new SwapEffect(playerId, Balanco_Swap, 3);
        se.isGlobal = true;
        mm.effectCont.AddSwapEffect(se, "balSw");
        yield return null;
    }
    public IEnumerator Balanco_Drop(int id, bool playerAction, string tag, int col) {
        if (Hex.TagType(tag) == "W")
            ThisPlayer().DealDamage(7);
        yield return null;    
    }
    public IEnumerator Balanco_Swap(int id, int c1, int r1, int c2, int r2) {
        Tile.Element elem1 = mm.hexGrid.GetTileAt(c1, r1).element;
        Tile.Element elem2 = mm.hexGrid.GetTileAt(c2, r2).element;
        int dmg = 0;
        if (elem1 == Tile.Element.Water)
            dmg += 7;
        if (elem2 == Tile.Element.Water)
            dmg += 7;

        if (dmg > 0)
            ThisPlayer().DealDamage(dmg);
        yield return null;
    }

    // Hurricane Cutter
    protected override IEnumerator SignatureSpell(TileSeq prereq) {
        yield return HandleMatchesOnBoard();

        const int numSwaps = 4;
        for (int i = 0; i < numSwaps; i++) {
            yield return mm.prompt.WaitForSwap(prereq);
            if (!mm.prompt.WasSuccessful())
                break;

            yield return mm.prompt.ContinueSwap();

            yield return HandleMatchesOnBoard();
        }

        yield return null;
    }

    IEnumerator HandleMatchesOnBoard() {
        var spells = new List<Spell>();
        spells.Add(new CoreSpell(0, "", null));

        List<TileSeq> seqs = mm.boardCheck.CheckBoard(spells)[0];
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

                yield return mm.hexMan._RemoveSeq(seq, false);

                ThisPlayer().DealDamage(dmg);
                yield return DropWaterIntoRandomCols(dropCount);
            }

            // wait for board to update...will this work?
            yield return mm.BoardChecking(false);

            seqs = mm.boardCheck.CheckBoard(spells)[0];
        }
        yield return null;
    }
}
