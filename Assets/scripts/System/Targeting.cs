using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class Targeting {

    public enum TargetMode { Tile, TileArea, Cell, Drag, Selection };
    public static TargetMode currentTMode = TargetMode.Tile;
    //public bool targetingCanceled = false;

    private static MageMatch _mm;
    private static int _targetsLeft = 0;
    private static List<TileBehav> _targetTBs, _validTBs;
    private static List<CellBehav> _targetCBs, _validCBs;
    private static Vector3 _lastTCenter;
    private static bool _largeAreaMode = false;
    private static TileBehav _lastDragTarget;
    //private List<GameObject> outlines;

    public delegate List<TileBehav> TileFilterFunc(List<TileBehav> tbs);
    public delegate List<CellBehav> CellFilterFunc(List<CellBehav> cbs);

    public static void Init(MageMatch mm) {
        _mm = mm;
    }

    public static bool IsTargetMode() {
        return _targetsLeft > 0;
    }

    static void DecTargets() { // maybe remove?
        MMLog.Log_Targeting("Targets remaining = " + _targetsLeft);
        _targetsLeft--;
        //if (targetsLeft == 0) //?
        //	currentTMode = TargetMode.Tile;
    }

    static List<TileBehav> GetValidTBs(TileFilterFunc filter) {
        List<TileBehav> tbs = HexGrid.GetPlacedTiles();
        if (filter != null)
            tbs = filter(tbs);
        return tbs;
    }

    public static IEnumerator WaitForTileTarget(int count, TileFilterFunc filter = null) {
        yield return WaitForTileTarget(count, GetValidTBs(filter));
    }
    public static IEnumerator WaitForTileTarget(int count, List<TileBehav> tbs) {
        currentTMode = TargetMode.Tile;
        _targetsLeft = count;
        _targetTBs = new List<TileBehav>();
        MMLog.Log_Targeting("targets = " + _targetsLeft);

        _validTBs = tbs;
        yield return TargetingScreen();
    }

    public static IEnumerator WaitForTileAreaTarget(bool largeArea, TileFilterFunc filter = null) {
        yield return WaitForTileAreaTarget(largeArea, GetValidTBs(filter));
    }
    public static IEnumerator WaitForTileAreaTarget(bool largeArea, List<TileBehav> tbs) {
        currentTMode = TargetMode.TileArea;
        _targetsLeft = 1;
        _targetTBs = new List<TileBehav>();
        _largeAreaMode = largeArea;
        MMLog.Log_Targeting("Waiting for TileArea target. Targets = " + _targetsLeft);

        _validTBs = tbs;
        yield return TargetingScreen();
    }

    public static IEnumerator WaitForDragTarget(int count, TileFilterFunc filter = null) {
        yield return WaitForDragTarget(count, GetValidTBs(filter));
    }
    public static IEnumerator WaitForDragTarget(int count, List<TileBehav> tbs) {
        currentTMode = TargetMode.Drag;
        _targetsLeft = count;
        _targetTBs = new List<TileBehav>();
        MMLog.Log_Targeting("Waiting for Drag target. Targets = " + _targetsLeft);

        _validTBs = tbs;
        yield return TargetingScreen();
    }

    public static void OnTBTarget(TileBehav tb) {
        foreach (TileBehav ctb in _targetTBs) // prevent targeting a tile that's already targeted
            if (ctb.EqualsTag(tb.hextag))
                return;

        // TODO these shouldn't be in the valid tiles in the first place
        //foreach (Tile ct in GetSelection().sequence) // prevent targeting prereq
        //    if (ct.HasSamePos(tb.tile))
        //        return;

        bool valid = false;
        for(int i = 0; i < _validTBs.Count; i++) {
            TileBehav ctb = _validTBs[i];
            if (ctb.EqualsTag(tb.hextag)) {
                valid = true;
                _validTBs.RemoveAt(i);
                break;
            }
        }
        if (!valid)
            return;
        // TODO if targetable

        _mm.syncManager.SendTBTarget(tb);
        AudioController.Trigger(AudioController.OtherSoundEffect.ChooseTarget);

        _mm.stats.Report("$ TARGET TILE " + tb.PrintCoord(), false);

        if (currentTMode == TargetMode.Tile) {
            Tile t = tb.tile;
            _mm.uiCont.OutlineTarget(t.col, t.row);
            _targetTBs.Add(tb);
            DecTargets();
            _mm.uiCont.ShowAlertText(_mm.ActiveP().name + ", choose " + _targetsLeft + " more targets.");
            MMLog.Log_Targeting("Targeted tile " + tb.PrintCoord());
        } else if (currentTMode == TargetMode.TileArea) {
            List<TileBehav> tbs;
            Tile t = tb.tile;
            if (_largeAreaMode)
                tbs = HexGrid.GetLargeAreaTiles(t.col, t.row);
            else
                tbs = HexGrid.GetSmallAreaTiles(t.col, t.row);
            tbs.Add(tb);

            foreach (TileBehav ctb in tbs) {
                Tile ct = ctb.tile;
                _mm.uiCont.OutlineTarget(ct.col, ct.row);
                // TODO if targetable
                // TODO remove any prereq overlap!
                _targetTBs.Add(ctb);
            }
            DecTargets();
            MMLog.Log_Targeting("Targeted area centered on tile " + tb.PrintCoord());
        } else if (currentTMode == TargetMode.Drag) {
            if (!IsDragTBValid(tb)) {
                EndDragTarget();
                return;
            }
            _lastDragTarget = tb;

            Tile t = tb.tile;
            _mm.uiCont.OutlineTarget(t.col, t.row);
            _targetTBs.Add(tb);
            DecTargets();
            _mm.uiCont.ShowAlertText(_mm.ActiveP().name + ", choose " + _targetsLeft + " more targets.");
            MMLog.Log_Targeting("Targeted tile " + tb.PrintCoord());
        }
    }

    static bool IsDragTBValid(TileBehav tb) {
        if (_lastDragTarget == null) return true;
        return HexGrid.CellsAreAdjacent(_lastDragTarget.tile, tb.tile);
    }

    public static void EndDragTarget() {
        _mm.syncManager.SendEndDragTarget();
        _targetsLeft = 0;
    }

    // TODO
    public static IEnumerator WaitForCellTarget(int count, List<CellBehav> cbs = null) {
        currentTMode = TargetMode.Cell;
        _targetsLeft = count;
        _targetCBs = new List<CellBehav>();
        //CBtargetEffect = targetEffect;
        //Debug.Log ("targets = " + targetsLeft);

        // TODO filter cells
        if (cbs == null)
            cbs = HexGrid.GetAllCellBehavs();

        _validCBs = cbs;

        yield return TargetingScreen();
    }

    public static void OnCBTarget(CellBehav cb) {
        ////Debug.Log ("Targeted cell (" + cb.col + ", " + cb.row + ")");
        //foreach (CellBehav ccb in _targetCBs) // prevent targeting a tile that's already targeted
        //    if (ccb.HasSamePos(cb))
        //        return;
        //// TODO if targetable
        //// TODO check validCBs

        //_mm.syncManager.SendCBTarget(cb);

        //_mm.stats.Report(string.Format("$ TARGET CELL ({0},{1})", cb.col, cb.row), false);

        //_mm.uiCont.OutlineTarget(cb.col, cb.row);
        //_targetCBs.Add(cb);
        //DecTargets();
        //_mm.uiCont.ShowAlertText(_mm.ActiveP().name + ", choose " + _targetsLeft + " more targets.");
        //MMLog.Log_Targeting("Targeted cell (" + cb.col + ", " + cb.row + ")");

        foreach (CellBehav ccb in _targetCBs) // prevent targeting a cell that's already targeted
            if (cb.EqualsCoord(ccb))
                return;

        bool valid = false;
        for (int i = 0; i < _validCBs.Count; i++) {
            CellBehav ccb = _validCBs[i];
            if (cb.EqualsCoord(ccb)) {
                valid = true;
                _validCBs.RemoveAt(i);
                break;
            }
        }
        if (!valid)
            return;
        // TODO if targetable

        _mm.syncManager.SendCBTarget(cb);
        AudioController.Trigger(AudioController.OtherSoundEffect.ChooseTarget);

        _mm.stats.Report("$ TARGET CELL " + cb.PrintCoord(), false);

        _targetCBs.Add(cb);
        DecTargets();
        _mm.uiCont.ShowAlertText(_mm.ActiveP().name + ", choose " + _targetsLeft + " more targets.");
        MMLog.Log_Targeting("Targeted cell " + cb.PrintCoord());
    }

    static IEnumerator TargetingScreen() {
        //targetingCanceled = false;
        Player p = _mm.ActiveP();
        _mm.EnterState(MageMatch.State.Targeting);
        if (_mm.MyTurn())
            p.hand.FlipAllHexes(true);

        _mm.uiCont.ShowAlertText(p.name + ", choose " + _targetsLeft + " more targets.");

        IList validObjs; // selected targets are removed from this, so the loop breaks if no more valids
        if (currentTMode == TargetMode.Cell) {
            validObjs = _validCBs;
            _mm.uiCont.ActivateTargetingUI(_validCBs);
        } else {
            validObjs = _validTBs;
            _mm.uiCont.ActivateTargetingUI(_validTBs);
        }

        if (_mm.IsReplayMode())
            _mm.replay.GetTargets();

        yield return new WaitUntil(() => _targetsLeft == 0 || validObjs.Count == 0);
        MMLog.Log_Targeting("no more targets.");
        _mm.GetComponent<InputController>().InvalidateClick(); // prevent weirdness from player still dragging
        _lastDragTarget = null;

        _mm.uiCont.ShowAlertText("Here are your targets!");
        yield return _mm.animCont.WaitForSeconds(1f);

        _mm.uiCont.DeactivateTargetingUI();

        currentTMode = TargetMode.Tile; // needed?
        //targetTBs = null?
        if (_mm.MyTurn())
            p.hand.FlipAllHexes(false);
        _mm.ExitState();
    }

    public static List<TileBehav> GetTargetTBs() { return _targetTBs; }

    public static List<CellBehav> GetTargetCBs() { return _targetCBs; }

    //public void ClearTargets() {
    //    mm.syncManager.SendClearTargets();

    //    if (currentTMode == TargetMode.Cell) {
    //        for (int i = 0; i < targetCBs.Count;)
    //            targetCBs.RemoveAt(0); //?
    //    } else {
    //        for (int i = 0; i < targetTBs.Count;)
    //            targetTBs.RemoveAt(0); //?
    //    }

    //    targetsLeft = targets;
    //    mm.uiCont.UpdateMoveText(mm.ActiveP().name + ", choose " + targetsLeft + " more targets.");
    //}

    //public void CancelTargeting() {
    //    mm.syncManager.SendCancelTargeting();

    //    targetingCanceled = true;
    //    targetsLeft = 0;
    //}

    //public bool WasCanceled() { return targetingCanceled; }

    public static bool TargetsRemain() { return _targetsLeft > 0; }


    // -----------------  SPELL SELECTION  -----------------

    public static bool selectionCanceled = false;

    private static List<TileSeq> selections;
    private static bool selectionChosen = false;

    public static IEnumerator SpellSelectScreen(List<TileSeq> seqs) {
        currentTMode = TargetMode.Selection;
        selectionCanceled = false;
        selectionChosen = false;
        selections = new List<TileSeq>(seqs);

        _mm.EnterState(MageMatch.State.Selecting);
        if (_mm.MyTurn())
            _mm.ActiveP().hand.FlipAllHexes(true);

        MMLog.Log_Targeting("seqs=" + BoardCheck.PrintSeqList(seqs));
        _mm.uiCont.ShowSpellSeqs(selections);

        MMLog.Log_Targeting("Starting to show spell select screen, selections=" +         BoardCheck.PrintSeqList(selections));

        yield return new WaitUntil(() => (selections.Count == 1 && selectionChosen) || selectionCanceled);

        MMLog.Log_Targeting("Chose prereq!");

        if (selectionCanceled) {
            selections = null; // or clear? to avoid nullrefs
        }

        _mm.uiCont.HideSpellSeqs();
        if (_mm.MyTurn())
            _mm.ActiveP().hand.FlipAllHexes(false);
        currentTMode = TargetMode.Tile;
        _mm.GetComponent<InputController>().InvalidateClick(); // i don't like this 

        _mm.ExitState();
        yield return null; //?
    }

    public static void OnSelection(TileBehav tb) {
        List<TileSeq> newSelections = new List<TileSeq>(selections);

        for (int i = 0; i < newSelections.Count; i++) {
            TileSeq seq = newSelections[i];
            if (!seq.IncludesTile(tb.tile)) {
                newSelections.RemoveAt(i);
                i--;
            }
        }

        if (newSelections.Count != 0) {
            _mm.syncManager.SendTBSelection(tb);
            selectionChosen = true;
            selections = newSelections;
            _mm.uiCont.HideSpellSeqs();
            _mm.uiCont.ShowSpellSeqs(selections);
        } else
            MMLog.Log_Targeting("Player clicked on an invalid tile: " + tb.PrintCoord());
    }

    public static TileSeq GetSelection() {
        if (selections == null)
            return null;
        else
            return selections[0];
    }

    public static void ClearSelection() { selections = null; }

    public static bool IsSelectionMode() { return currentTMode == TargetMode.Selection; }

    public static void CancelSelection() {
        _mm.syncManager.SendCancelSelection();
        selectionCanceled = true;
    }
}
