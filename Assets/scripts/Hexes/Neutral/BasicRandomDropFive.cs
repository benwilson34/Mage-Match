using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRandomDropFive : Charm {

    public Tile.Element elem;

    private const float ANIM_INTERVAL = .1f;

    public override IEnumerator DropEffect() {
        const int count = 5;

        int[] cols = BoardCheck.GetRandomCols(count);
        yield return _mm.syncManager.SyncRands(PlayerId, cols);
        cols = _mm.syncManager.GetRands(cols.Length);

        string s = "";
        for (int i = 0; i < cols.Length; i++) {
            s += cols[i];
            if (i < cols.Length - 1)
                s += ", ";
        }
        Debug.Log("random columns are [" + s + "]");

        Queue<int> colQ = new Queue<int>(cols);

        int col;
        while (colQ.Count > 0) {
            col = colQ.Dequeue();
            TileBehav newTB = HexManager.GenerateBasicTile(PlayerId, elem);
            _mm.DropTile(newTB, col);
            yield return new WaitForSeconds(ANIM_INTERVAL);
        }
    }
}
