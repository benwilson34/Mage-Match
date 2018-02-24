using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class Commish  {

	private int _mood = 0, _numTiles;
	private MageMatch _mm;
    //private bool activeEffects = true;

	public Commish(MageMatch mm) {
        _mm = mm;
    }

    public void InitEvents() {
        _mm.eventCont.commishMatch += OnCommishMatch;
        //mm.eventCont.match += OnMatch;
    }

    public void OnCommishMatch(string[] seqs) {
        int sum = 0;
        string s = "{";
        foreach (string seq in seqs) {
            int i = seq.Length;
            sum += i;
            s += i + ",";
        }
        s = s.Remove(s.Length - 1, 1) + "}";
        MMLog.Log_Commish("CommishMatch: lens=" + s + ", sum=" + sum);
        _numTiles += sum;
    }

    //public void OnMatch(int id, int[] lens) {
    //    // TODO mood stuff etc.
    //}

    public IEnumerator CTurn(){
        //if (activeEffects) {
        //    if (mood == -100)
        //        AngryDamage();
        //    else if (mood == 100)
        //        HappyHealing();
        //    else
        //        ChangeMood(-35);

        //}
        _numTiles = 5;
        yield return PlaceTiles();
//		Debug.Log ("CTurn: done placing tiles.");
	}

	IEnumerator PlaceTiles(){
        MMLog.Log_Commish("   ---------- COMMISH TURN BEGIN ----------");
        int prevCount = 0;
        Queue<int> colQ = new Queue<int>();
        Queue<Tile.Element> elems = new Queue<Tile.Element>();
        for (int i = 0; i < _numTiles; i++) {
            if (colQ.Count == 0) {
                int[] cols = _mm.boardCheck.GetRandomCols(_numTiles - prevCount); // can be i?
                if (cols.Length == 0) {  // board is full
                    MMLog.Log_Commish("The board is full. The Commissioner ends his turn early.");
                    break;
                }
                yield return _mm.syncManager.SyncRands(_mm.ActiveP().id, cols);
                colQ = new Queue<int>( _mm.syncManager.GetRands(cols.Length) );

                int[] rands = GetRandomInts(colQ.Count);
                yield return _mm.syncManager.SyncRands(_mm.ActiveP().id, rands);
                elems = GetRandomElems( _mm.syncManager.GetRands(rands.Length) );
                prevCount = _numTiles;
            }
            
            if (i != 0) { // wait to drop next tile (anim purposes only)
                yield return new WaitForSeconds(.15f);
            }

            Hex tb = _mm.hexMan.GenerateTile(3, elems.Dequeue()); // should get own func?
            MMLog.Log_Commish("Dropping into col " + colQ.Peek());

            int col = colQ.Dequeue();
            if (_mm.boardCheck.CheckColumn(col) >= 0)
                _mm.DropTile(col, tb);
            else {
                MMLog.LogError("COMMISH: Tried to drop into a full column!");
                break;
            }
        }

        MMLog.Log_Commish("   ---------- COMMISH TURN END ----------");
        _mm.eventCont.CommishTurnDone();
	}

    int[] GetRandomInts(int num) {
        int[] rs = new int[num];
        for (int i = 0; i < num; i++) {
            rs[i] = Random.Range(0, 5);
        }
        return rs;
    }

	Tile.Element GetTileElement (int i){
        switch (i) {
            case 0:
                return Tile.Element.Fire;
            case 1:
                return Tile.Element.Water;
            case 2:
                return Tile.Element.Earth;
            case 3:
                return Tile.Element.Air;
            case 4:
                return Tile.Element.Muscle;
            default:
                MMLog.LogError("COMMISH: Tried to get a bad element!");
                return Tile.Element.None;
        }
	}

    Queue<Tile.Element> GetRandomElems(int[] rands) {
        Queue<Tile.Element> elems = new Queue<Tile.Element>();
        for (int i = 0; i < rands.Length; i++) {
            elems.Enqueue(GetTileElement(rands[i]));
        }
        return elems;
    }

	//int GetSemiRandomCol(float[] ratios){
	//	float val = Random.Range (0f, 1f);
 //       //Debug.MMLog.Log_Commish("COMMISH: GetSemiRandomCol val=" + val);
 //       float thresh = 0;
	//	for (int i = 0; i < HexGrid.numCols; i++) {
	//		thresh += ratios [i];
	//		if (val < thresh)
	//			return i;
	//	}
	//	Debug.Log ("COMMISH: GetSemiRandomCol: shouldn't get to this point. val = " + val);
	//	return 6;
	//}

    

    public void ChangeMood(int amount){
		_mood += amount;
		_mood = Mathf.Clamp(_mood, -100, 100);
	}
	
	public int GetMood(){
		return _mood;
	}
}