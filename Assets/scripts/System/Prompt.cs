using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Prompt {

    public enum PromptMode { None, Drop, Swap, QuickdrawDrop };
    public PromptMode currentMode = PromptMode.None;

    private MageMatch _mm;

    // TODO make/support dropping hexes (i.e. you could drop a consumable)
    // perhaps the Wait func should take a filter like targeting...
    private Hex _dropTile;
    private int _dropCol;

    private TileBehav[] _swapTiles;
    private bool _successful = false;

    private int _quickdrawDropCol;
    private bool _quickdrawWentToHand = false;

    public Prompt(MageMatch mm) {
        this._mm = mm;
    }

    public bool WasSuccessful() {
        MMLog.Log("PROMPT", "blue", "prompt was" + (_successful ? "" : " not") + " successful");
        return _successful;
    }


    // ----- DROP -----

    public IEnumerator WaitForDrop() {
        MMLog.Log("PROMPT", "blue", "Waiting for DROP...");
        _successful = false;

        if (_mm.ActiveP().hand.IsEmpty() || _mm.hexGrid.IsBoardFull()) {
            MMLog.Log("PROMPT", "blue", ">>>>>> Canceling prompted DROP!!");
            yield break;
        }

        _mm.uiCont.ShowAlertText("Drop a tile into the board!");
        currentMode = PromptMode.Drop;

        yield return new WaitUntil(() => currentMode == PromptMode.None);
    }

    public void SetDrop(Hex hex, int col = -1) {
        MMLog.Log("PROMPT", "blue", "DROP is " + hex.hextag);
        _mm.syncManager.SendDropSelection(hex, col);

        _dropTile = hex;
        _dropCol = col;

        currentMode = PromptMode.None;
        _successful = true;
    }

    public Hex GetDropTile() { return _dropTile; }

    public int GetDropCol() { return _dropCol; }

    public IEnumerator ContinueDrop() {
        _mm.ActiveP().hand.Remove(_dropTile);
        yield return _mm._Drop(_dropTile, _dropCol);
    }


    // ----- QUICKDRAW -----

    public IEnumerator WaitForQuickdrawAction(Hex hex) {
        _successful = false;
        bool isCharm = Hex.IsCharm(hex.hextag);

        if (!isCharm && _mm.hexGrid.IsBoardFull()) {
            yield break;
        }

        _mm.uiCont.ToggleQuickdrawUI(true, hex);

        _mm.uiCont.ShowAlertText("Choose what to do with the Quickdraw hex!");
        _quickdrawWentToHand = false;
        currentMode = PromptMode.Drop;

        // the InputController calls SetDrop for this too
        yield return new WaitUntil(() => currentMode == PromptMode.None);

        if (!_quickdrawWentToHand) { // the player dropped it
            // TODO sound fx
            yield return _mm._Drop(hex, _dropCol);
            yield return _mm._Draw(_mm.ActiveP().id);
        }

        _mm.uiCont.ToggleQuickdrawUI(false);

        yield return null;
    }

    public void SetQuickdrawHand() {
        _quickdrawWentToHand = true;
        currentMode = PromptMode.None;
    }


    // ----- SWAP -----

    public IEnumerator WaitForSwap() {
        yield return WaitForSwap(new TileSeq()); // is this ok?
    }

    public IEnumerator WaitForSwap(TileSeq seq) {
        MMLog.Log("PROMPT", "blue", "Waiting for SWAP...");
        _successful = false;

        if (_mm.hexGrid.GetPlacedTiles().Count == 0 || // if board is empty, break
            !SwapIsPossible(seq)) {
            MMLog.Log("PROMPT", "blue", ">>>>>> Canceling prompted SWAP!!");
            yield break;
        }

        _mm.uiCont.ShowAlertText(_mm.ActiveP().name + ", swap two adjacent tiles!");
        currentMode = PromptMode.Swap;

        yield return new WaitUntil(() => currentMode == PromptMode.None);
    }

    // if needed elsewhere, move to HexGrid
    public bool SwapIsPossible(TileSeq seq) {
        List<TileBehav> tbs = _mm.hexGrid.GetPlacedTiles(seq);
        foreach (TileBehav tb in tbs) {
            if (_mm.hexGrid.HasAdjacentNonprereqTile(tb.tile, seq))
                return true;
        }
        return false;
    }

    public void SetSwaps(int c1, int r1, int c2, int r2) {
        _mm.syncManager.SendSwapSelection(c1, r1, c2, r2);

        _swapTiles = new TileBehav[2];
        _swapTiles[0] = _mm.hexGrid.GetTileBehavAt(c1, r1);
        _swapTiles[1] = _mm.hexGrid.GetTileBehavAt(c2, r2);
        MMLog.Log("PROMPT", "blue", "SWAPS are " + 
            _swapTiles[0].hextag + " and " + _swapTiles[1].hextag);

        currentMode = PromptMode.None;
        _successful = true;
    }

    public TileBehav[] GetSwapTBs() { return _swapTiles; }

    public IEnumerator ContinueSwap() {
        Tile f = _swapTiles[0].tile, s = _swapTiles[1].tile;
        yield return _mm._SwapTiles(false, f.col, f.row, s.col, s.row);
    }

}
