using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class Commish  {

    public const int COMMISH_ID = 0;

	private static MageMatch _mm;

	public static void Init(MageMatch mm) {
        _mm = mm;
    }

	public static IEnumerator DropRandomTiles(){
        MMLog.Log_Commish("   ---------- COMMISH TURN BEGIN ----------");

        const int numDrops = 5;

        var cols = BoardCheck.GetRandomCols(numDrops);
        yield return _mm.syncManager.SyncRands(_mm.ActiveP.ID, cols);
        Queue<int> colQ = new Queue<int>( _mm.syncManager.GetRands(cols.Length) );

        var elems = GetRandomInts(numDrops);
        yield return _mm.syncManager.SyncRands(_mm.ActiveP.ID, elems);
        Queue<Tile.Element> elemQ = GetElemQueue( _mm.syncManager.GetRands(cols.Length) );

        for (int i = 0; i < numDrops && colQ.Count > 0; i++) {            
            if (i != 0) { // wait to drop next tile (anim purposes only)
                yield return AnimationController.WaitForSeconds(.15f);
            }

            TileBehav tb = HexManager.GenerateBasicTile(COMMISH_ID, elemQ.Dequeue());
            MMLog.Log_Commish("Dropping into col " + colQ.Peek());

            int col = colQ.Dequeue();
            if (BoardCheck.CheckColumn(col) >= 0) {
                _mm.CommishDropTile(tb, col);
                Report.ReportLine("  # C-DROP " + tb.hextag + " col" + col, false);
            } else {
                MMLog.LogError("COMMISH: Tried to drop into a full column!");
                break;
            }
        }

        MMLog.Log_Commish("   ---------- COMMISH TURN END ----------");
        //EventController.CommishTurnDone();
	}

    static int[] GetRandomInts(int num) {
        int[] rs = new int[num];
        for (int i = 0; i < num; i++) {
            rs[i] = Random.Range(1, 6);
        }
        return rs;
    }

    static Queue<Tile.Element> GetElemQueue(int[] rands) {
        Queue<Tile.Element> elems = new Queue<Tile.Element>();
        for (int i = 0; i < rands.Length; i++) {
            elems.Enqueue((Tile.Element)rands[i]);
        }
        return elems;
    }
}