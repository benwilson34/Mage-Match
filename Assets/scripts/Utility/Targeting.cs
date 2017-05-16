using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Targeting {

    public enum TargetMode { Tile, TileArea, Cell, CellArea, Drag };
    public TargetMode currentTMode = TargetMode.Tile;
    public bool canceled = false;

    private MageMatch mm;
    private int targets, targetsLeft = 0;
    private List<TileBehav> targetTBs;
    private List<CellBehav> targetCBs;
    private Vector3 lastTCenter;
    private bool largeAreaMode = false;

    private List<GameObject> outlines;

    //public delegate void TBTargetEffect(TileBehav tb);
    //public delegate void TBMultiTargetEffect(List<TileBehav> tbs);
    public delegate void CBTargetEffect(CellBehav cb);
    //private TBTargetEffect TBtargetEffect;
    //private TBMultiTargetEffect TBmultiTargetEffect;
    private CBTargetEffect CBtargetEffect;

    public Targeting() {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
    }

    public bool IsTargetMode() {
        return targetsLeft > 0;
    }

    void DecTargets() { // maybe remove?
        Debug.Log("INPUTCONTROLLER: Targets remaining = " + targetsLeft);
        targetsLeft--;
        //if (targetsLeft == 0) //?
        //	currentTMode = TargetMode.Tile;
    }

    public IEnumerator WaitForTileTarget(int count) {
        // TODO handle fewer tiles on board than count
        currentTMode = TargetMode.Tile;
        targets = targetsLeft = count;
        targetTBs = new List<TileBehav>();
        Debug.Log("TARGETING: targets = " + targetsLeft);

        yield return TargetingScreen();
    }

    public IEnumerator WaitForTileAreaTarget(bool largeArea) {
        currentTMode = TargetMode.TileArea;
        targets = targetsLeft = 1;
        targetTBs = new List<TileBehav>();
        largeAreaMode = largeArea;
        Debug.Log("TARGETING: Waiting for TileArea target. Targets = " + targetsLeft);

        yield return TargetingScreen();
    }

    public void OnTBTarget(TileBehav tb) {
        foreach (TileBehav ctb in targetTBs) // prevent targeting a tile that's already targeted
            if (ctb.tile.HasSamePos(tb.tile))
                return;
        foreach (Tile ct in mm.ActiveP().GetCurrentBoardSeq().sequence) // prevent targeting prereq
            if (ct.HasSamePos(tb.tile))
                return;
        // TODO if targetable

        mm.syncManager.SendTBTarget(tb);
        if (currentTMode == TargetMode.Tile) {
            Tile t = tb.tile;
            OutlineTarget(t.col, t.row);
            targetTBs.Add(tb);
            DecTargets();
            mm.uiCont.UpdateMoveText(mm.ActiveP().name + ", choose " + targetsLeft + " more targets.");
            Debug.Log("TARGETING: Targeted tile (" + t.col + ", " + t.row + ")");
        } else if (currentTMode == TargetMode.TileArea) {
            List<TileBehav> tbs;
            Tile t = tb.tile;
            if (largeAreaMode)
                tbs = mm.hexGrid.GetLargeAreaTiles(t.col, t.row);
            else
                tbs = mm.hexGrid.GetSmallAreaTiles(t.col, t.row);
            tbs.Add(tb);

            foreach (TileBehav ctb in tbs) {
                Tile ct = ctb.tile;
                OutlineTarget(ct.col, ct.row);
                // TODO if targetable
                targetTBs.Add(ctb);
            }
            DecTargets();
            Debug.Log("TARGETING: Targeted area centered on tile (" + tb.tile.col + ", " + tb.tile.row + ")");
        }
    }

    // TODO
    public IEnumerator WaitForCellTarget(int count) {
        currentTMode = TargetMode.Cell;
        targets = targetsLeft = count;
        targetCBs = new List<CellBehav>();
        //CBtargetEffect = targetEffect;
        //Debug.Log ("targets = " + targetsLeft);

        yield return TargetingScreen();
    }

    // TODO
    public IEnumerator WaitForCellAreaTarget(bool largeArea, CBTargetEffect targetEffect) {
        currentTMode = TargetMode.CellArea;
        yield return null;
    }

    public void OnCBTarget(CellBehav cb) {
        //DecTargets ();
        //Debug.Log ("Targeted cell (" + cb.col + ", " + cb.row + ")");

        foreach (CellBehav ccb in targetCBs) // prevent targeting a tile that's already targeted
            if (ccb.HasSamePos(cb))
                return;
        //foreach (Tile ct in MageMatch.ActiveP().GetCurrentBoardSeq().sequence) // prevent targeting prereq
        //    if (ct.HasSamePos(tb.tile))
        //        return;
        // TODO if targetable

        mm.syncManager.SendCBTarget(cb);
        if (currentTMode == TargetMode.Cell) {
            OutlineTarget(cb.col, cb.row);
            targetCBs.Add(cb);
            DecTargets();
            mm.uiCont.UpdateMoveText(mm.ActiveP().name + ", choose " + targetsLeft + " more targets.");
            Debug.Log("TARGETING: Targeted tile (" + cb.col + ", " + cb.row + ")");
        } else if (currentTMode == TargetMode.CellArea) {
            //List<TileBehav> tbs;
            //Tile t = tb.tile;
            //if (largeAreaMode)
            //    tbs = HexGrid.GetLargeAreaTiles(t.col, t.row);
            //else
            //    tbs = HexGrid.GetSmallAreaTiles(t.col, t.row);
            //tbs.Add(tb);

            //foreach (TileBehav ctb in tbs) {
            //    Tile ct = ctb.tile;
            //    OutlineTarget(ct.col, ct.row);
            //    // TODO if targetable
            //    targetTBs.Add(ctb);
            //}
            //DecTargets();
            //Debug.Log("TARGETING: Targeted area centered on tile (" + tb.tile.col + ", " + tb.tile.row + ")");
        }
    }

    //public void WaitForDragTarget(int count, TBMultiTargetEffect targetEffect) {
    //    // TODO
    //    currentTMode = TargetMode.Drag;
    //    targetsLeft = count;
    //    TBmultiTargetEffect = targetEffect;
    //}

    //public void OnDragTarget(List<TileBehav> tbs) { // should just pass each TB that gets painted?
    //    // TODO
    //    TBmultiTargetEffect(tbs);
    //}

    IEnumerator TargetingScreen() {
        canceled = false;
        Player p = mm.ActiveP();
        mm.currentState = MageMatch.GameState.TargetMode;

        mm.uiCont.UpdateMoveText(p.name + ", choose " + targetsLeft + " more targets.");
        mm.uiCont.ToggleTargetingUI();
        TileSeq seq = p.GetCurrentBoardSeq();
        OutlinePrereq(seq);

        yield return new WaitUntil(() => targetsLeft == 0);
        Debug.Log("TARGETING: no more targets.");

        mm.uiCont.UpdateMoveText("Here are your targets!");
        yield return new WaitForSeconds(1f);

        //if (!canceled) { // shouldn't be here anymore
        //    mm.RemoveSeq(seq);
        //    //HandleTargets();
        //    mm.BoardChanged();
        //    p.ApplyAPCost();
        //}

        foreach (GameObject go in outlines) {
            GameObject.Destroy(go);
        }
        mm.uiCont.ToggleTargetingUI();
        mm.uiCont.UpdateMoveText("");

        currentTMode = TargetMode.Tile; // needed?
        outlines = null; // memory leak?
        //targetTBs = null?

        mm.currentState = MageMatch.GameState.PlayerTurn;
    }

    public List<TileBehav> GetTargetTBs() { return targetTBs; }

    public List<CellBehav> GetTargetCBs() { return targetCBs; }

    void OutlinePrereq(TileSeq seq) {
        outlines = new List<GameObject>(); // move to Init?
        GameObject go;
        foreach (Tile t in seq.sequence) {
            go = mm.GenerateToken("prereq");
            go.transform.position = mm.hexGrid.GridCoordToPos(t.col, t.row);
            outlines.Add(go);
        }
    }

    void OutlineTarget(int col, int row) {
        GameObject go = mm.GenerateToken("target");
        go.transform.position = mm.hexGrid.GridCoordToPos(col, row);
        outlines.Add(go);
    }

    public void ClearTargets() {
        mm.syncManager.SendClearTargets();

        Player p = mm.ActiveP();
        int prereqs = p.GetCurrentBoardSeq().sequence.Count;
        for (int i = 0; i < outlines.Count - prereqs;) { // clear just the target outlines
            GameObject go = outlines[prereqs];
            GameObject.Destroy(go);
            outlines.Remove(go);
        }

        if (currentTMode == TargetMode.Cell || currentTMode == TargetMode.CellArea) {
            for (int i = 0; i < targetCBs.Count;)
                targetCBs.RemoveAt(0); //?
        } else {
            for (int i = 0; i < targetTBs.Count;)
                targetTBs.RemoveAt(0); //?
        }

        targetsLeft = targets;
        mm.uiCont.UpdateMoveText(p.name + ", choose " + targetsLeft + " more targets.");
    }

    public void CancelTargeting() {
        mm.syncManager.SendCancelTargeting();

        canceled = true;
        targetsLeft = 0;
    }

    public bool WasCanceled() { return canceled; }
}
