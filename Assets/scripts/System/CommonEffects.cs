//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DG.Tweening;

class CommonEffects {

    private static MageMatch _mm;

    public static void Init(MageMatch mm) {
        _mm = mm;
    }

    public static void PutTile(TileBehav tb, int col, int row, bool checkGrav = false) {
        if (HexGrid.IsCellFilled(col, row))
            HexManager.RemoveTile(col, row, false);
        tb.HardSetPos(col, row);
        // TODO move to tilesOnBoard obj
        //if (checkGrav)
        //    BoardChanged();
    }

    //public static void Transmute(int col, int row, Tile.Element element) {
    //    Destroy(hexGrid.GetTileBehavAt(col, row).gameObject);
    //    hexGrid.ClearTileBehavAt(col, row);
    //    TileBehav tb = GenerateTile(element).GetComponent<TileBehav>();
    //    tb.ChangePos(col, row);
    //}

    public static int[] GetRandomInds(int listCount, int pickCount) {
        int[] inds = new int[pickCount];
        for (int i = 0; i < pickCount; i++) {
            inds[i] = Random.Range(0, listCount - i);
        }
        return inds;
    }

    public static IEnumerator BounceToHand(TileBehav tb, Player p) {
        HexGrid.ClearTileBehavAt(tb.tile.col, tb.tile.row);
        tb.ClearEnchantment();
        tb.ClearTileEffect();

        yield return tb.transform.DOMove(p.Hand.GetEmptyHandSlotPos(), .3f).WaitForCompletion();

        p.Hand.Add(tb);
        yield return null;
    }

    public static IEnumerator ShootIntoAirAndRearrange(List<TileBehav> tbs) {
        if (tbs.Count == 0)
            yield break;

        Tween t = null;
        foreach (var tb in tbs) {
            HexGrid.ClearTileBehavAt(tb.tile.col, tb.tile.row);
            t = tb.transform.DOMoveY(15, .2f);
            yield return new WaitForSeconds(.1f);
        }

        yield return t.WaitForCompletion();

        // TODO shuffle tbs?
        int[] cols = BoardCheck.GetRandomCols(tbs.Count);
        yield return _mm.syncManager.SyncRands(_mm.ActiveP.ID, cols);
        cols = _mm.syncManager.GetRands(cols.Length);

        IEnumerator tileAnim = null;
        for (int i = 0; i < tbs.Count; i++) {
            int col = cols[i];
            tileAnim = tbs[i]._ChangePosAndDrop(HexGrid.TopOfColumn(col), col, BoardCheck.CheckColumn(col), .08f);
            if (i != tbs.Count - 1) {
                _mm.StartCoroutine(tileAnim);
                yield return new WaitForSeconds(.1f);
            }
        }

        yield return tileAnim;
    }

    public static IEnumerator DropBasicsIntoRandomCols(int id, Tile.Element elem, int count) {
        List<TileBehav> tbs = new List<TileBehav>();
        for (int i = 0; i < count; i++) {
            tbs.Add(HexManager.GenerateBasicTile(id, elem));
        }

        yield return DropIntoRandomCols(id, tbs, count);
    }

    public static IEnumerator DropIntoRandomCols(int id, List<TileBehav> tbs, int count) {
        int[] cols = BoardCheck.GetRandomCols(count);
        yield return _mm.syncManager.SyncRands(id, cols);
        cols = _mm.syncManager.GetRands(cols.Length);

        //string s = "";
        //for (int i = 0; i < cols.Length; i++) {
        //    s += cols[i];
        //    if (i < cols.Length - 1)
        //        s += ", ";
        //}
        //MMLog.Log("Valeria", "magenta", "random water columns are [" + s + "]");

        Queue<int> colQ = new Queue<int>(cols);

        for (int i = 0; i < cols.Length; i++) {
            int col = colQ.Dequeue();
            if (i == cols.Length - 1) {
                yield return _mm._Drop(tbs[i], col);
            } else {
                _mm.StartCoroutine(_mm._Drop(tbs[i], col));
                yield return new WaitForSeconds(.1f);
            }
        }

        yield return null;
    }
}
