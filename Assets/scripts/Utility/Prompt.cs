using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Prompt {

    public enum PromptMode { None, Swap, Drop };
    public PromptMode currentMode = PromptMode.None;

    private MageMatch mm;

    // TODO make/support dropping hexes (i.e. you could drop a consumable)
    // perhaps the Wait func should take a filter like targeting...
    private TileBehav dropTile;
    private int dropCol;

    private TileBehav[] swapTiles;
    private bool successful = false;

    public Prompt(MageMatch mm) {
        this.mm = mm;
    }

    //public void Cancel() {
    //    canceled = true;
    //    currentMode = PromptMode.None;
    //}

    //public bool WasCanceled() { return canceled; }

    public bool WasSuccessful() {
        MMLog.Log("PROMPT", "blue", "prompt was" + (successful ? "" : " not") + " successful");
        return successful;
    }

    // ----- DROP -----

    public IEnumerator WaitForDrop() {
        MMLog.Log("PROMPT", "blue", "Waiting for DROP...");
        successful = false;

        if (mm.ActiveP().hand.Count() == 0 ||                      // if player hand is empty, or
            mm.hexGrid.GetPlacedTiles().Count == HexGrid.numCells) { // the board is full
            MMLog.Log("PROMPT", "blue", ">>>>>> Canceling prompted DROP!!");
            yield break;
        }

        mm.uiCont.SendSlidingText("Drop a tile into the board!");
        currentMode = PromptMode.Drop;

        yield return new WaitUntil(() => currentMode == PromptMode.None);
    }

    public void SetDrop(int col, TileBehav tb) {
        MMLog.Log("PROMPT", "blue", "DROP is " + tb.tag);
        mm.syncManager.SendDropSelection(col, tb);

        dropTile = tb;
        dropCol = col;

        currentMode = PromptMode.None;
        successful = true;
    }

    public TileBehav GetDropTile() { return dropTile; }

    public int GetDropCol() { return dropCol; }

    public IEnumerator ContinueDrop() {
        mm.ActiveP().hand.Remove(dropTile);
        yield return mm._Drop(false, dropCol, dropTile);
    }

    // ----- SWAP -----

    public IEnumerator WaitForSwap(TileSeq seq) {
        MMLog.Log("PROMPT", "blue", "Waiting for SWAP...");
        successful = false;

        if (mm.hexGrid.GetPlacedTiles().Count == 0 || // if board is empty, break
            !SwapIsPossible(seq)) {
            MMLog.Log("PROMPT", "blue", ">>>>>> Canceling prompted SWAP!!");
            yield break;
        }

        mm.uiCont.SendSlidingText("Swap two tiles on the board!");
        currentMode = PromptMode.Swap;
        mm.uiCont.UpdateMoveText(mm.ActiveP().name + ", swap two adjacent tiles!");

        yield return new WaitUntil(() => currentMode == PromptMode.None);
    }

    // if needed elsewhere, move to HexGrid
    public bool SwapIsPossible(TileSeq seq) {
        List<TileBehav> tbs = mm.hexGrid.GetPlacedTiles(seq);
        foreach (TileBehav tb in tbs) {
            if (mm.hexGrid.HasAdjacentNonprereqTile(tb.tile, seq))
                return true;
        }
        return false;
    }

    public void SetSwaps(int c1, int r1, int c2, int r2) {
        mm.syncManager.SendSwapSelection(c1, r1, c2, r2);

        swapTiles = new TileBehav[2];
        swapTiles[0] = mm.hexGrid.GetTileBehavAt(c1, r1);
        swapTiles[1] = mm.hexGrid.GetTileBehavAt(c2, r2);
        MMDebug.MMLog.Log("PROMPT", "blue", "SWAPS are " + 
            swapTiles[0].tag + " and " + swapTiles[1].tag);

        currentMode = PromptMode.None;
        successful = true;
    }

    public TileBehav[] GetSwapTBs() { return swapTiles; }

    public IEnumerator ContinueSwap() {
        Tile f = swapTiles[0].tile, s = swapTiles[1].tile;
        yield return mm._SwapTiles(false, f.col, f.row, s.col, s.row);
    }
}
