using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Commish  {

	private int mood = 0, numTiles;
	private MageMatch mm;
    //private bool activeEffects = true;

	public Commish(MageMatch mm) {
        this.mm = mm;
    }

    public void InitEvents() {
        mm.eventCont.commishMatch += OnCommishMatch;
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
        Debug.Log("COMMISH: CommishMatch: lens=" + s + ", sum=" + sum);
        numTiles += sum;
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
        numTiles = 5;
        yield return PlaceTiles();
//		Debug.Log ("CTurn: done placing tiles.");
	}

	IEnumerator PlaceTiles(){
        Debug.Log("   ---------- COMMISH TURN BEGIN ----------");
        int prevCount = 0;
        Queue<int> cols = new Queue<int>();
        Queue<Tile.Element> elems = new Queue<Tile.Element>();
        for (int i = 0; i < numTiles; i++) {
            if (cols.Count == 0) {
                cols = GetRandomCols(numTiles - prevCount); // can be i?
                yield return mm.syncManager.SyncRands(mm.ActiveP().id, cols.ToArray());
                cols = new Queue<int>( mm.syncManager.GetRands(cols.Count) );

                int[] rands = GetRandomInts(cols.Count);
                yield return mm.syncManager.SyncRands(mm.ActiveP().id, rands);
                elems = GetRandomElems( mm.syncManager.GetRands(rands.Length) );
                prevCount = numTiles;
            }
            if (cols.Peek() == -1) {
                Debug.Log ("COMMISH: The board is full. The Commissioner ends his turn early.");
                break;
            }
            if (i != 0) { // wait to drop next tile (anim purposes only)
                yield return new WaitForSeconds(.15f);
            }

            GameObject go = mm.GenerateTile(elems.Dequeue());
            Debug.Log ("COMMISH: Dropping into col " + cols.Peek());

            if (mm.DropTile(cols.Dequeue(), go)) {
                go.transform.SetParent(GameObject.Find("tilesOnBoard").transform);
            } else {
                Debug.LogError("COMMISH: Tried to drop into a full column!");
                break;
            }

            if (i == numTiles - 1) {
                yield return mm.BoardChecking(); // wait to see if there are any matches
            }
        }

        Debug.Log("   ---------- COMMISH TURN END ----------");
        mm.eventCont.CommishTurnDone();
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
                Debug.LogError("COMMISH: Tried to get a bad element!");
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
 //       //Debug.Log("COMMISH: GetSemiRandomCol val=" + val);
 //       float thresh = 0;
	//	for (int i = 0; i < HexGrid.numCols; i++) {
	//		thresh += ratios [i];
	//		if (val < thresh)
	//			return i;
	//	}
	//	Debug.Log ("COMMISH: GetSemiRandomCol: shouldn't get to this point. val = " + val);
	//	return 6;
	//}

    Queue<int> GetRandomCols(int num) {
        int[] counts = mm.boardCheck.EmptyCount();
        Queue<int> cs = new Queue<int>();
        for (int i = 0; i < num; i++) {
            if (counts[7] == 0) {
                cs.Enqueue(-1); // signal no more spots
                break;
            }

            int val = Random.Range(0, counts[7]);
            //Debug.Log("COMMISH: GetSemiRandomCol val=" + val);
            int sum = 0;
            for (int c = 0; c < HexGrid.numCols; c++) {
                sum += counts[c];
                if (val < sum) {
                    counts[c]--; // update column count
                    counts[7]--; // update total count
                    cs.Enqueue(c);
                    break;
                }
            }
            //Debug.Log("COMMISH: GetSemiRandomCol: shouldn't get to this point. val = " + val);
        }
        // syncing could be here?
        return cs;
    }

    public void ChangeMood(int amount){
		mood += amount;
		mood = Mathf.Clamp(mood, -100, 100);
	}
	
	public int GetMood(){
		return mood;
	}
	
	//public void AngryDamage(){
 //       mm.uiCont.UpdateMoveText("The Commissioner is furious! Both players take 50 dmg and discard one tile!");
	//	Player p = mm.InactiveP ();
	//	p.ChangeHealth (-50);
	//	p.DiscardRandom (1);

	//	p = mm.ActiveP();
	//	p.ChangeHealth (-50);
	//	p.DiscardRandom (1);
		
	//	mood = 0;
	//	//mm.uiCont.UpdateCommishMeter ();
	//}
	
	//public void HappyHealing(){
 //       mm.uiCont.UpdateMoveText("The Commissioner is pleased, and has decided to heal both players for 100!");
	//	mm.InactiveP ().ChangeHealth (100);
	//	//mm.InactiveP ().DrawTiles (1); // buggy

	//	mm.ActiveP ().ChangeHealth (100);
	//	//mm.ActiveP ().DrawTiles (1); // buggy
		
	//	mood = 0;
	//	//mm.uiCont.UpdateCommishMeter ();
	//}
}