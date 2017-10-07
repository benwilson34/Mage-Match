using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prompt {

    public enum PromptMode { None, Swap, Drop };
    public PromptMode currentMode = PromptMode.None;

    private MageMatch mm;
    private TileBehav dropTile;
    private int dropCol;
    private TileBehav[] swapTiles;
    private bool canceled = false;

    public Prompt(MageMatch mm) {
        this.mm = mm;
    }

    public void Cancel() {
        canceled = true;
        currentMode = PromptMode.None;
    }

    public bool WasCanceled() { return canceled; }

    // ----- DROP -----

    public IEnumerator WaitForDrop() {
        MMDebug.MMLog.Log("PROMPT", "blue", "Waiting for DROP...");
        mm.uiCont.SendSlidingText("Drop a tile into the board!");
        if (mm.ActiveP().hand.Count() == 0) // if player hand is empty, break
            yield return null;

        canceled = false;
        currentMode = PromptMode.Drop;

        yield return new WaitUntil(() => currentMode == PromptMode.None);
    }

    public void SetDrop(int col, TileBehav tb) {
        MMDebug.MMLog.Log("PROMPT", "blue", "DROP is " + tb.tag);
        mm.syncManager.SendDropSelection(col, tb);

        dropTile = tb;
        dropCol = col;

        currentMode = PromptMode.None;
    }

    public TileBehav GetDropTile() { return dropTile; }

    public int GetDropCol() { return dropCol; }

    public void ContinueDrop() {
        mm.DropTile(dropCol, dropTile);
    }

    // ----- SWAP -----

    public IEnumerator WaitForSwap() {
        MMDebug.MMLog.Log("PROMPT", "blue", "Waiting for SWAP...");
        mm.uiCont.SendSlidingText("Swap two tiles on the board!");
        if (mm.hexGrid.GetPlacedTiles().Count == 0) // if board is empty, break
            yield return null;

        canceled = false;
        currentMode = PromptMode.Swap;
        mm.uiCont.UpdateMoveText(mm.ActiveP().name + ", swap two adjacent tiles!");

        yield return new WaitUntil(() => currentMode == PromptMode.None);
    }

    public void SetSwaps(int c1, int r1, int c2, int r2) {
        mm.syncManager.SendSwapSelection(c1, r1, c2, r2);

        swapTiles = new TileBehav[2];
        swapTiles[0] = mm.hexGrid.GetTileBehavAt(c1, r1);
        swapTiles[1] = mm.hexGrid.GetTileBehavAt(c2, r2);
        MMDebug.MMLog.Log("PROMPT", "blue", "SWAPS are " + 
            swapTiles[0].tag + " and " + swapTiles[1].tag);

        currentMode = PromptMode.None;
    }

    public TileBehav[] GetSwapTBs() { return swapTiles; }

    public void ContinueSwap() {
        Tile f = swapTiles[0].tile, s = swapTiles[1].tile;
        mm.SwapTiles(f.col, f.row, s.col, s.row);
    }
}
