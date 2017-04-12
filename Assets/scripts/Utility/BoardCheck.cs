using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BoardCheck {

	private bool debugLogOn = false;
	private List<TileSeq> matchSeqList, checkList; // dictionary, compare list??
    private HexGrid hexGrid;

	// check skipping object
	private class SkipCheck{
		public int col, row, dir; 
		public SkipCheck(Tile t, int dir){
			this.col = t.col;
			this.row = t.row;
			this.dir = dir;
		}
	}
	private List<SkipCheck> skips;

	public BoardCheck(HexGrid hg){
        hexGrid = hg;

        matchSeqList = new List<TileSeq> ();
		skips = new List<SkipCheck> ();
		// initialize list of possible basic matches
		matchSeqList.Add (new TileSeq ("fffff"));
		matchSeqList.Add (new TileSeq ("wwwww"));
		matchSeqList.Add (new TileSeq ("eeeee"));
		matchSeqList.Add (new TileSeq ("aaaaa"));
		matchSeqList.Add (new TileSeq ("mmmmm"));

//		Debug.Log (PrintSeqList (matchSeqList));
	}

	public int CheckColumn(int c){
		int r = hexGrid.TopOfColumn(c);
		if (hexGrid.IsSlotFilled (c, r))
			return -1;
		
		int min = hexGrid.BottomOfColumn (c);
		while (r > min && !hexGrid.IsSlotFilled(c, r - 1))
			r--;
		return r;
	}

	public float[] EmptyCheck(){
		float[] ratios = new float[7];
        int[] counts = EmptyCount();
        int total = counts[7];
		for (int i = 0; i < HexGrid.numCols; i++) {
			ratios [i] = (float)counts [i] / total;
//			Debug.Log("     ratios[" + i + "] = " + ratios[i] + ": " + counts[i] + "/" + total);
		}

		float totalf = 0;
		foreach(float f in ratios) totalf += f;
		if(debugLogOn)
			Debug.Log ("EmptyCheck: totalf = " + totalf);
		return ratios;
	}

    // note: the 8th element is the total empty cells...
    public int[] EmptyCount() {
        int[] counts = new int[8];
        counts[7] = HexGrid.numCells - hexGrid.GetPlacedTiles ().Count;
        for (int i = 0; i < HexGrid.numCols; i++) {
            if (CheckColumn(i) >= 0)
                counts[i] = hexGrid.TopOfColumn(i) - CheckColumn(i) + 1;
            else
                counts[i] = 0;
            Debug.Log("HEXGRID: counts[" + i + "] = " + counts[i]);
        }
        Debug.Log("HEXGRID: counts total = " + counts[7]);
        return counts;
    }

	public List<TileSeq> MatchCheck(){
		return CheckBoard (matchSeqList, true);
	}

	public List<TileSeq> SpellCheck(List<TileSeq> spells){ // TODO
		return CheckBoard (spells, false);
	}

	List<TileSeq> CheckBoard(List<TileSeq> list, bool matchMode){
		skips.Clear();

		List<TileSeq> shortList; // for seqs that have matching first colors
		List<TileSeq> returnList = new List<TileSeq>(); // list of all matching seqs to be returned
		for(int c = 0; c < HexGrid.numCols; c++){ // for each col
			for(int r = hexGrid.BottomOfColumn(c); r <= hexGrid.TopOfColumn(c); r++){ // for each row
				if (hexGrid.IsSlotFilled(c, r)) { // if there's a tile there
					shortList = new List<TileSeq>(list); // copies???? just same reference?????
					for(int i = 0; i < shortList.Count; i++){ // for each TileSeq in seqList
						TileSeq seq = shortList [i];
						if (!hexGrid.GetTileAt(c, r).element.Equals(seq.GetElementAt (0))) { // remove any seqs that don't start with current color
							shortList.Remove(seq);
							i--;
						}
					}

					if (debugLogOn)
						Debug.Log ("tiles[" + c + ", " + r + "]: Shortlist: " + PrintSeqList(shortList));
						 // copies?? - TODO
					//Debug.Log("Trying dir = " + d + " with shortlist: " + PrintSeqList(shortList));
					List<TileSeq> playList = CheckTile (c, r, shortList, matchMode);
					if (playList != null)
						returnList.AddRange(playList);
				} else
					break; // breaks just inner loop
			}	
		} // --Ends checking loops

		return returnList;
	}

	List<TileSeq> CheckTile(int c, int r, List<TileSeq> shortList, bool matchMode){
		// if check matches color of current, keep checking the line
		List<TileSeq> returnList = new List<TileSeq> ();
		if (!hexGrid.GetTileBehavAt (c, r).ableMatch) // handle current tile not matchable
			return returnList;

		// direction loop
		for (int dir = 0; dir < 6; dir++) {
			bool skip = false;
			foreach (SkipCheck s in skips) {
				if (s.col == c && s.row == r && s.dir == dir) {
					if(debugLogOn)
						Debug.Log ("Skipping (" + s.col + ", " + s.row + ") in dir " + s.dir);
					skip = true;
					break;
				}
			}
			if (skip)
				continue;
			
//			Debug.Log("dir = " + dir);
			checkList = new List<TileSeq>(shortList); // local?

			for (int i = 0; i < checkList.Count; i++) { // replaced a foreach-loop
				TileSeq checkSeq = checkList [i]; // the sequence being compared to

				TileSeq outSeq = new TileSeq (hexGrid.GetTileAt(c, r)); // the seq being added to to be added to returnList
				int dc = 0, dr = 0; // difference from current tile pos
				for (int seqIndex = 1; seqIndex < checkSeq.sequence.Count; seqIndex++) {
					bool skipCurrentSeq = false;
					if (!hexGrid.HasAdjacentCell (c + dc, r + dr, dir))
						skipCurrentSeq = true;
					else {
						hexGrid.GetOffset (dir, out dc, out dr);
						dc *= seqIndex;
						dr *= seqIndex;
					}

					if (debugLogOn)
						Debug.Log ("Checking tiles[" + (c + dc) + ", " + (r + dr) + "] for seq: " + PrintSeq (checkSeq, false));

					if (!skipCurrentSeq && 
						hexGrid.IsSlotFilled(c + dc, r + dr) && // if there's something there...
						hexGrid.GetTileBehavAt(c + dc, r + dr).ableMatch && // and it can be matched...
						hexGrid.GetTileAt(c + dc, r + dr).element.Equals(checkSeq.GetElementAt (seqIndex))) { // ...and the next tile matches the next in the seq
						outSeq.sequence.Add (hexGrid.GetTileAt(c + dc, r + dr));
					} else {
						if (matchMode && outSeq.GetSeqLength () >= 3) {
							AddMatchSkips (outSeq, dir);
							returnList.Add (outSeq);
						}
						break;
					}

					if (seqIndex == checkSeq.sequence.Count - 1) {
						if (matchMode && outSeq.GetSeqLength () == 5) {
							Debug.Log ("Wow!! 5 in a row!!!!!!");
							AddMatchSkips (outSeq, dir);
						}
						returnList.Add (outSeq);
					}
				} // --End of seq loop
			} // --End of checkList loop
		} // --End of dir loop

		return returnList;
	}

	void AddMatchSkips(TileSeq seq, int dir){
		if(debugLogOn)
			Debug.Log ("Adding skips for " + PrintSeq (seq, true));
		switch (seq.GetSeqLength ()) {
		case 3:
			skips.Add (new SkipCheck (seq.sequence [2], OppDir(dir))); // 3
			break;
		case 4:
			skips.Add (new SkipCheck (seq.sequence [3], OppDir(dir))); // 4
			skips.Add (new SkipCheck (seq.sequence [2], OppDir(dir))); // 3
			skips.Add (new SkipCheck (seq.sequence [1], dir));         // 3
			break;
		case 5:
			skips.Add (new SkipCheck (seq.sequence [4], OppDir(dir))); // 5
			skips.Add (new SkipCheck (seq.sequence [3], OppDir(dir))); // 4
			skips.Add (new SkipCheck (seq.sequence [1], dir));         // 4
			skips.Add (new SkipCheck (seq.sequence [2], OppDir(dir))); // 3
			skips.Add (new SkipCheck (seq.sequence [2], dir));         // 3
			break;
		}
	}

	int OppDir(int dir){
		return (dir + 3) % 6;
	}

	public string PrintSeq(TileSeq seq, bool showPos){ // debug
		string str = "";
		if (showPos) {
			foreach (Tile t in seq.sequence) {
				str += "(" + t.col + "," + t.row + ") ";
			}
			str = "TileSeq " + seq.SeqAsString () + " at " + str;
		} else {
			str = "TileSeq " + seq.SeqAsString ();
		}
		return str;
	}

	public string PrintSeqList(List<TileSeq> seqList){ // debug
		string str = "";
		foreach (TileSeq seq in seqList) {
			str += PrintSeq(seq, true);
		}
		return str;
	}
}
