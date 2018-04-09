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
    private Hex _dropHex;
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
        _successful = false;
        yield return WaitForDrop(null);
    }

    public IEnumerator WaitForDropTile() {
        _successful = false;
        var hexes = _mm.ActiveP().hand.GetAllHexes();

        if (_mm.hexGrid.IsBoardFull())
            yield break;

        hexes = TileFilter.FilterByCategory(hexes, Hex.Category.Charm);
        yield return WaitForDrop(hexes);
    }

    public IEnumerator WaitForDrop(List<Hex> ignoredHexes) {
        MMLog.Log("PROMPT", "blue", "Waiting for DROP...");
        //_successful = false;

        int ignCount = 0;
        if (ignoredHexes != null)
            ignCount = ignoredHexes.Count;

        if (_mm.ActiveP().hand.Count() == ignCount) {
            MMLog.Log("PROMPT", "blue", ">>>>>> Canceling prompted DROP!!");
            yield break;
        }

        _mm.uiCont.ShowAlertText("Drop a hex into the board!");
        currentMode = PromptMode.Drop;

        if (ignoredHexes != null && _mm.MyTurn()) {
            // flip unavailable ones
            foreach (Hex h in ignoredHexes)
                h.Flip();
            _mm.inputCont.RestrictInteractableHexes(ignoredHexes);
        }

        _mm.inputCont.SetAllowHandRearrange(false);
        yield return new WaitUntil(() => currentMode == PromptMode.None);
        _mm.inputCont.SetAllowHandRearrange(true);

        if (ignoredHexes != null && _mm.MyTurn()) {
            // flip back
            foreach (Hex h in ignoredHexes)
                h.Flip();
            _mm.inputCont.EndRestrictInteractableHexes();
        }
    }

    public void SetDrop(Hex hex, int col = -1) {
        MMLog.Log("PROMPT", "blue", "DROP is " + hex.hextag);
        _mm.syncManager.SendDropSelection(hex, col);

        string str = "$ PROMPT DROP " + hex.hextag + (col == -1 ? "" : " col" + col);
        _mm.stats.Report(str, false);

        _dropHex = hex;
        _dropCol = col;

        currentMode = PromptMode.None;
        _successful = true;
    }

    public Hex GetDropHex() { return _dropHex; }

    public int GetDropCol() { return _dropCol; }

    public IEnumerator ContinueDrop() {
        _mm.ActiveP().hand.Remove(_dropHex);
        yield return _mm._Drop(_dropHex, _dropCol);
    }


    // ----- QUICKDRAW -----

    public IEnumerator WaitForQuickdrawAction(Hex hex) {
        _successful = false;
        bool isCharm = Hex.IsCharm(hex.hextag);

        if (!isCharm && _mm.hexGrid.IsBoardFull()) {
            yield break;
        }
        Player p = _mm.ActiveP();

        _mm.uiCont.ToggleQuickdrawUI(true, hex);

        _mm.uiCont.ShowLocalAlertText(p.id, "Choose what to do with the Quickdraw hex!");
        _quickdrawWentToHand = false;
        currentMode = PromptMode.Drop;

        // ignore every hex except this one
        var ignoredHexes = new List<Hex>();
        if (_mm.MyTurn()) {
            foreach (Hex h in p.hand.GetAllHexes())
                if (!h.EqualsTag(hex)) {
                    ignoredHexes.Add(h);
                    h.Flip();
                }
            _mm.inputCont.RestrictInteractableHexes(ignoredHexes);
        }

        // the InputController calls SetDrop for this too
        _mm.inputCont.SetAllowHandRearrange(false);
        yield return new WaitUntil(() => currentMode == PromptMode.None);
        _mm.inputCont.SetAllowHandRearrange(true);

        if (_mm.MyTurn()) {
            foreach (Hex h in ignoredHexes)
                h.Flip();
            _mm.inputCont.EndRestrictInteractableHexes();
        }

        if (!_quickdrawWentToHand) { // the player dropped it
            // TODO sound fx
            p.hand.Remove(hex);
            yield return _mm._Drop(hex, _dropCol);
            yield return _mm._Draw(p.id);
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

        _mm.inputCont.SetAllowHandDragging(false);
        yield return new WaitUntil(() => currentMode == PromptMode.None);
        _mm.inputCont.SetAllowHandDragging(true);
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

        _mm.stats.Report(string.Format("$ PROMPT SWAP ({0},{1})({2},{3})", c1, r1, c2, r2), false);

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
