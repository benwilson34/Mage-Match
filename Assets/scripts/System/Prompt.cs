using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public static class Prompt {

    public enum PromptMode { None, Drop, Swap, QuickdrawDrop };
    public static PromptMode currentMode = PromptMode.None;

    public enum PromptModifier { None, SwapEmpty };
    public static PromptModifier modifier = PromptModifier.None;

    private static MageMatch _mm;
    private static int _count = -1;

    // TODO make/support dropping hexes (i.e. you could drop a consumable)
    // perhaps the Wait func should take a filter like targeting...
    private static Hex _dropHex;
    private static int _dropCol;

    private static int _swapC1, _swapR1, _swapC2, _swapR2;
    private static bool _successful = false;

    private static int _quickdrawDropCol;
    private static bool _quickdrawWentToHand = false;

    public static void Init(MageMatch mm) {
        _mm = mm;
    }

    public static bool WasSuccessful { get {
            MMLog.Log("PROMPT", "blue", "prompt was" + (_successful ? "" : " not") + " successful");
            return _successful;
        }
    }

    static void ResetCount() { _count = -1; }


    #region ---------- DROP ----------

    public static void SetDropCount(int count, List<Hex> ignoredHexes = null) {
        _count = count;
        ToggleDropUI(true, ignoredHexes);
    }

    static void ToggleDropUI(bool on, List<Hex> ignoredHexes) {
        if (ignoredHexes != null && _mm.MyTurn()) {
            // flip unavailable ones
            foreach (Hex h in ignoredHexes)
                h.Flip(on);
            //_mm.inputCont.RestrictInteractableHexes(ignoredHexes);
            //_mm.inputCont.SetAllowHandRearrange(on); // needed?
        }
    }

    public static IEnumerator WaitForDrop() {
        //_successful = false;
        yield return WaitForDrop(null);
    }

    public static IEnumerator WaitForDropTile() {
        //_successful = false;
        var hexes = _mm.ActiveP.Hand.GetAllHexes();

        if (HexGrid.IsBoardFull())
            yield break;

        hexes = TileFilter.FilterByCategory(hexes, Hex.Category.Charm);
        yield return WaitForDrop(hexes);
    }

    public static IEnumerator WaitForDrop(List<Hex> ignoredHexes) {
        MMLog.Log("PROMPT", "blue", "Waiting for DROP...");
        if (_count == -1) {
            MMLog.LogWarning("PROMPT: Waiting for DROP but count wasn't set! Defaulted on 1.");
            SetDropCount(1, ignoredHexes);
        }
        _successful = false;

        int ignCount = 0;
        if (ignoredHexes != null)
            ignCount = ignoredHexes.Count;

        if (_mm.ActiveP.Hand.Count == ignCount) {
            // nothing to drop; prompt whiffs
            MMLog.Log("PROMPT", "blue", ">>>>>> Canceling prompted DROP!!");
            ResetCount();
            yield break;
        }

        _mm.uiCont.ShowAlertText("Drop a hex into the board!");
        currentMode = PromptMode.Drop;

        //if (ignoredHexes != null && _mm.MyTurn()) {
        //    // flip unavailable ones
        //    foreach (Hex h in ignoredHexes)
        //        h.Flip();
        //    //_mm.inputCont.RestrictInteractableHexes(ignoredHexes);
        //}

        if (_mm.IsReplayMode)
            ReplayEngine.GetPrompt();

        yield return new WaitUntil(() => currentMode == PromptMode.None);

        if (!_successful || _count == 0) {
            MMLog.Log("PROMPT", "blue", "DROPs are done.");
            ToggleDropUI(false, ignoredHexes);
            ResetCount();
        }

        //if (ignoredHexes != null && _mm.MyTurn()) {
        //    // flip back
        //    foreach (Hex h in ignoredHexes)
        //        h.Flip();
        //    //_mm.inputCont.EndRestrictInteractableHexes();
        //}
    }

    public static void SetDrop(Hex hex, int col = -1) {
        MMLog.Log("PROMPT", "blue", "DROP is " + hex.hextag);
        _mm.syncManager.SendDropSelection(hex, col);

        string str = "$ PROMPT DROP " + hex.hextag + (col == -1 ? "" : " col" + col);
        Report.ReportLine(str, false);

        _dropHex = hex;
        _dropCol = col;

        currentMode = PromptMode.None;
        _count--;
        _successful = true;
    }

    public static Hex GetDropHex() { return _dropHex; }

    public static int GetDropCol() { return _dropCol; }

    public static IEnumerator ContinueDrop() {
        _mm.ActiveP.Hand.Remove(_dropHex);
        yield return _mm._Drop(_dropHex, _dropCol, EventController.DropState.PromptDrop);
    }
    #endregion


    #region ---------- QUICKDRAW ----------

    public static IEnumerator WaitForQuickdrawAction(Hex hex) {
        _successful = false;

        if (!Hex.IsCharm(hex.hextag) && HexGrid.IsBoardFull()) {
            // can't be dropped in; quickdraw whiffs
            yield break;
        }
        _count = 1;
        Player p = _mm.ActiveP;

        _mm.uiCont.ToggleQuickdrawUI(true, hex);

        _mm.uiCont.ShowLocalAlertText(p.ID, "Choose what to do with the Quickdraw hex!");
        AudioController.Trigger(SFX.Other.Quickdraw_Prompt);
        _quickdrawWentToHand = false;
        currentMode = PromptMode.Drop;

        // ignore every hex except this one
        var ignoredHexes = new List<Hex>();
        if (_mm.MyTurn()) {
            MMLog.Log("PROMPT", "orange", "My quickdraw tile, and my turn. " + hex.hextag);
            foreach (Hex h in p.Hand.GetAllHexes()) {
                if (!h.EqualsTag(hex)) {
                    ignoredHexes.Add(h);
                    h.Flip();
                }
            }
            MMLog.Log("PROMPT", "orange", ignoredHexes.Count + " restricted tiles in hand.");

            //_mm.inputCont.RestrictInteractableHexes(ignoredHexes);
        }

        if (_mm.IsReplayMode)
            ReplayEngine.GetPrompt();

        // the InputController calls SetDrop for this too
        //_mm.inputCont.SetAllowHandRearrange(false);
        yield return new WaitUntil(() => currentMode == PromptMode.None);
        //_mm.inputCont.SetAllowHandRearrange(true);

        if (_mm.MyTurn()) {
            foreach (Hex h in ignoredHexes)
                h.Flip();
            //_mm.inputCont.EndRestrictInteractableHexes();
        }

        if (!_quickdrawWentToHand) { // the player dropped it
            AudioController.Trigger(SFX.Other.Quickdraw_Drop);
            p.Hand.Remove(hex);
            yield return _mm._Drop(hex, _dropCol);
            yield return _mm._Draw(p.ID);
        }

        _mm.uiCont.ToggleQuickdrawUI(false);
        _count = -1;

        yield return null;
    }

    public static void SetQuickdrawHand() {
        _quickdrawWentToHand = true;
        _mm.syncManager.SendKeepQuickdraw();
        currentMode = PromptMode.None;
        Report.ReportLine("$ PROMPT KEEP QUICKDRAW", false);
    }
    #endregion


    #region ---------- SWAP ----------

    public static void SetSwapCount(int count, PromptModifier mod = PromptModifier.None) {
        _count = count;
        if (mod != PromptModifier.None)
            modifier = mod;
        ToggleSwapUI(true);
    }

    static void ToggleSwapUI(bool on) {
        if (_mm.MyTurn())
            _mm.ActiveP.Hand.FlipAllHexes(on);
    }

    public static IEnumerator WaitForSwap() {
        yield return WaitForSwap(new TileSeq()); // is this ok?
    }

    public static IEnumerator WaitForSwap(TileSeq seq) {
        if (_count == -1) {
            MMLog.LogWarning("PROMPT: Waiting for SWAP but count wasn't set! Defaulted on 1.");
            SetSwapCount(1);
        }
        MMLog.Log("PROMPT", "blue", "Waiting for SWAP...");
        _successful = false;

        if (HexGrid.GetPlacedTiles().Count == 0 || // if board is empty, break
            !SwapIsPossible(seq)) {
            MMLog.Log("PROMPT", "blue", ">>>>>> Canceling prompted SWAP!!");
            ResetCount();
            yield break;
        }

        _mm.uiCont.ShowAlertText(_mm.ActiveP.Name + ", swap two adjacent tiles!");
        currentMode = PromptMode.Swap;
        

        if (_mm.IsReplayMode)
            ReplayEngine.GetPrompt();

        //_mm.inputCont.SetAllowHandDragging(false); // might not be needed now
        yield return new WaitUntil(() => currentMode == PromptMode.None);
        //_mm.inputCont.SetAllowHandDragging(true); // might not be needed now

        if (!_successful || _count == 0) {
            ToggleSwapUI(false);
            ResetCount();
            modifier = PromptModifier.None;
        }
    }

    // if needed elsewhere, move to HexGrid
    public static bool SwapIsPossible(TileSeq seq) {
        List<TileBehav> tbs = HexGrid.GetPlacedTiles(seq);
        foreach (TileBehav tb in tbs) {
            if (HexGrid.HasAdjacentNonprereqTile(tb.tile, seq))
                return true;
        }
        return false;
    }

    public static void SetSwaps(int c1, int r1, int c2, int r2) {
        _mm.syncManager.SendSwapSelection(c1, r1, c2, r2);

        Report.ReportLine(string.Format("$ PROMPT SWAP ({0},{1}) ({2},{3})", c1, r1, c2, r2), false);

        _swapC1 = c1;
        _swapR1 = r1;
        _swapC2 = c2;
        _swapR2 = r2;
        //MMLog.Log("PROMPT", "blue", "SWAPS are " + 
            //_swapTiles[0].hextag + " and " + _swapTiles[1].hextag);

        currentMode = PromptMode.None;
        _count--;
        _successful = true;
    }

    public static int[] GetSwapCoords() {
        return new int[4] { _swapC1, _swapR1, _swapC2, _swapR2 };
    }

    public static TileBehav[] GetSwapTBs() {
        var swapTiles = new TileBehav[2];
        swapTiles[0] = HexGrid.GetTileBehavAt(_swapC1, _swapR1);
        if (modifier != PromptModifier.SwapEmpty)
            swapTiles[1] = HexGrid.GetTileBehavAt(_swapC2, _swapR2);
        return swapTiles;
    }

    public static IEnumerator ContinueSwap() {
        yield return _mm._SwapTiles(_swapC1, _swapR1, _swapC2, _swapR2, EventController.SwapState.PromptSwap);
        if (modifier == PromptModifier.SwapEmpty)
            _mm.BoardChanged();
    }
    #endregion

}
