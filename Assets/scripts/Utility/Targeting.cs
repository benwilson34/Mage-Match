using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Targeting {

    public enum TargetMode { Tile, TileArea, Cell, CellArea, Drag };
    public TargetMode currentTMode = TargetMode.Tile;

    private MageMatch mm;
    private int targets, targetsLeft = 0;
    private List<TileBehav> targetTBs;
    private List<CellBehav> targetCBs;
    private Vector3 lastTCenter;
    private bool largeAreaMode = false, canceled = false;

    private List<GameObject> outlines;

    public delegate void TBTargetEffect(TileBehav tb);
    public delegate void TBMultiTargetEffect(List<TileBehav> tbs);
    public delegate void CBTargetEffect(CellBehav cb);
    private TBTargetEffect TBtargetEffect;
    private TBMultiTargetEffect TBmultiTargetEffect;
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

    public void WaitForTileTarget(int count, TBTargetEffect targetEffect) {
        // TODO handle fewer tiles on board than count
        currentTMode = TargetMode.Tile;
        targets = targetsLeft = count;
        targetTBs = new List<TileBehav>();
        TBtargetEffect = targetEffect;
        Debug.Log("TARGETING: targets = " + targetsLeft);

        mm.StartCoroutine(TargetingScreen());
    }

    public void WaitForTileAreaTarget(bool largeArea, TBMultiTargetEffect targetEffect) {
        currentTMode = TargetMode.TileArea;
        targets = targetsLeft = 1;
        targetTBs = new List<TileBehav>();
        largeAreaMode = largeArea;
        TBmultiTargetEffect = targetEffect;
        Debug.Log("TARGETING: Waiting for TileArea target. Targets = " + targetsLeft);

        mm.StartCoroutine(TargetingScreen());
    }

    public void OnTBTarget(TileBehav tb) {
        foreach (TileBehav ctb in targetTBs) // prevent targeting a tile that's already targeted
            if (ctb.tile.HasSamePos(tb.tile))
                return;
        foreach (Tile ct in mm.ActiveP().GetCurrentBoardSeq().sequence) // prevent targeting prereq
            if (ct.HasSamePos(tb.tile))
                return;
        // TODO if targetable

        mm.turnManager.SendTBTarget(tb);
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
    public void WaitForCellTarget(int count, CBTargetEffect targetEffect) {
        currentTMode = TargetMode.Cell;
        targets = targetsLeft = count;
        targetCBs = new List<CellBehav>();
        CBtargetEffect = targetEffect;
        //Debug.Log ("targets = " + targetsLeft);

        mm.StartCoroutine(TargetingScreen());
    }

    // TODO
    public void WaitForCellAreaTarget(bool largeArea, CBTargetEffect targetEffect) { }

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

        mm.turnManager.SendCBTarget(cb);
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

    // TODO
    public void WaitForDragTarget(int count, TBMultiTargetEffect targetEffect) {
        // TODO
        currentTMode = TargetMode.Drag;
        targetsLeft = count;
        TBmultiTargetEffect = targetEffect;
    }

    public void OnDragTarget(List<TileBehav> tbs) { // should just pass each TB that gets painted?
                                                    // TODO
        TBmultiTargetEffect(tbs);
    }

    IEnumerator TargetingScreen() {
        canceled = false;
        Player p = mm.ActiveP();
        mm.currentState = MageMatch.GameState.TargetMode;

        mm.uiCont.UpdateMoveText(p.name + ", choose " + targetsLeft + " more targets.");
        mm.uiCont.ToggleTargetingUI();
        TileSeq seq = p.GetCurrentBoardSeq();
        OutlinePrereq(seq);
        // TODO implement cast and cancel mechanics???
        yield return new WaitUntil(() => targetsLeft == 0);
        Debug.Log("TARGETING: no more targets.");

        if (!canceled) {
            mm.RemoveSeq(seq);
            HandleTargets();
            mm.BoardChanged();
            p.ApplyAPCost();
        }

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

    void HandleTargets() {
        switch (currentTMode) {
            case TargetMode.Tile:
                foreach (TileBehav tb in targetTBs)
                    TBtargetEffect(tb);
                break;
            case TargetMode.TileArea:
                Debug.Log("TARGETING: Handling TileArea effect...");
                TBmultiTargetEffect(targetTBs);
                break;
            case TargetMode.Cell:
                foreach (CellBehav cb in targetCBs) {
                    CBtargetEffect(cb);
                }
                break;
            case TargetMode.CellArea:
                break;
            case TargetMode.Drag:
                break;
        }
    }

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
        Player p = mm.ActiveP();
        int prereqs = p.GetCurrentBoardSeq().sequence.Count;
        for (int i = 0; i < outlines.Count - prereqs;) {
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
        canceled = true;
        targetsLeft = 0;
    }
}
