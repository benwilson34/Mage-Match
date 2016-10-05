using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public static class BoardCheck {

	public static bool debugLogOn = false;

	private static List<TileSeq> matchSeqList, checkList; // dictionary, compare list??

	// check skipping object
	private class SkipCheck{
		public int col, row, dir; 
		public SkipCheck(Tile t, int dir){
			this.col = t.col;
			this.row = t.row;
			this.dir = dir;
		}
	}
	private static List<SkipCheck> skips;

	public static void Init(){
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

	public static int CheckColumn(int c){
		int r = HexGrid.TopOfColumn(c);
		if (HexGrid.IsSlotFilled (c, r))
			return -1;
		
		int min = HexGrid.BottomOfColumn (c);
		while (r > min && !HexGrid.IsSlotFilled(c, r - 1))
			r--;
		return r;
	}

	public static float[] EmptyCheck(){
		float[] ratios = new float[7];
		int[] counts = new int[7];
		int total = HexGrid.numCells - PlacedTileList ().Count;
		for (int i = 0; i < HexGrid.numCols; i++) {
			if (CheckColumn (i) >= 0)
				counts [i] = HexGrid.TopOfColumn (i) - CheckColumn (i) + 1;
			else
				counts [i] = 0;
//			Debug.Log("counts[" + i + "] = " + counts[i]);
			ratios [i] = (float)counts [i] / (float)total;
//			Debug.Log("     ratios[" + i + "] = " + ratios[i] + ": " + counts[i] + "/" + total);
		}

		float totalf = 0;
		foreach(float f in ratios) totalf += f;
		Debug.Log ("EmptyCheck: totalf = " + totalf);
		return ratios;
	}

	public static List<TileSeq> MatchCheck(){
		return CheckBoard (matchSeqList, true);
	}

	public static List<TileSeq> SpellCheck(List<TileSeq> spells){ // TODO
		return CheckBoard (spells, false);
	}

	static List<TileSeq> CheckBoard(List<TileSeq> list, bool matchMode){
		skips.Clear();

		List<TileSeq> shortList; // for seqs that have matching first colors
		List<TileSeq> returnList = new List<TileSeq>(); // list of all matching seqs to be returned
		for(int c = 0; c < HexGrid.numCols; c++){ // for each col
			for(int r = HexGrid.BottomOfColumn(c); r <= HexGrid.TopOfColumn(c); r++){ // for each row
				if (HexGrid.IsSlotFilled(c, r)) { // if there's a tile there
					shortList = new List<TileSeq>(list); // copies???? just same reference?????
					for(int i = 0; i < shortList.Count; i++){ // for each TileSeq in seqList
						TileSeq seq = shortList [i];
						if (!HexGrid.GetTileAt(c, r).element.Equals(seq.GetElementAt (0))) { // remove any seqs that don't start with current color
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

	static List<TileSeq> CheckTile(int c, int r, List<TileSeq> shortList, bool matchMode){
		// if check matches color of current, keep checking the line
		List<TileSeq> returnList = new List<TileSeq> ();

		// direction loop
		for (int dir = 0; dir < 6; dir++) {
			bool skip = false;
			foreach (SkipCheck s in skips) {
				if (s.col == c && s.row == r && s.dir == dir) {
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

				TileSeq outSeq = new TileSeq (HexGrid.GetTileAt(c, r)); // the seq being added to to be added to returnList
				int dc = 0, dr = 0; // difference from current tile pos
				for (int seqIndex = 1; seqIndex < checkSeq.sequence.Count; seqIndex++) {
					bool skipCurrentSeq = false;
					switch (dir) { // TODO change edge cases to those used in TileBehav swapping
					case 0: // N cell - board N
						if (r + dr != HexGrid.numRows - 1) {
							dr = 1 * seqIndex;
						} else
							skipCurrentSeq = true;
						break;
					case 1: // NE cell - board NE
						if (c + dc != HexGrid.numCols - 1 && r + dr != HexGrid.numRows - 1) {
							dc = 1 * seqIndex;
							dr = 1 * seqIndex;
						} else
							skipCurrentSeq = true;
						break;
					case 2: // E cell - board SE
						if (c + dc != HexGrid.numCols - 1) {
							dc = 1 * seqIndex;
						} else
							skipCurrentSeq = true;
						break;
					case 3: // S cell - board S
						if (r  + dr != 0) {
							dr = -1 * seqIndex; 
						} else
							skipCurrentSeq = true;
						break;
					case 4: // SW cell - board SW
						if (r + dr != 0 && c + dc != 0) {
							dc = -1 * seqIndex;
							dr = -1 * seqIndex;
						} else
							skipCurrentSeq = true;
						break;
					case 5: // W cell - board NW
						if (c + dc != 0) {
							dc = -1 * seqIndex;
						} else
							skipCurrentSeq = true;
						break;
					}

					if (debugLogOn)
						Debug.Log ("Checking tiles[" + (c + dc) + ", " + (r + dr) + "] for seq: " + PrintSeq (checkSeq, false));

					if (!skipCurrentSeq && 
						HexGrid.IsSlotFilled(c + dc, r + dr) && // if there's something there...
						HexGrid.GetTileAt(c + dc, r + dr).element.Equals(checkSeq.GetElementAt (seqIndex))) { // ...and the next tile matches the next in the seq
						outSeq.sequence.Add (HexGrid.GetTileAt(c + dc, r + dr));
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

	static void AddMatchSkips(TileSeq seq, int dir){
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

	static int OppDir(int dir){
		return (dir + 3) % 6;
	}

	public static List<TileBehav> PlacedTileList(){
		List<TileBehav> returnList = new List<TileBehav> ();
		for(int c = 0; c < HexGrid.numCols; c++){ // for each col
			for(int r = HexGrid.BottomOfColumn(c); r <= HexGrid.TopOfColumn(c); r++){ // for each row
				if (HexGrid.IsSlotFilled(c, r)) { // if there's a tile there
					returnList.Add(HexGrid.GetTileBehavAt(c, r));
				} else
					break; // breaks just inner loop
			}	
		}
		return returnList;
	}

	public static string PrintSeq(TileSeq seq, bool showPos){ // debug
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

	public static string PrintSeqList(List<TileSeq> seqList){ // debug
		string str = "";
		foreach (TileSeq seq in seqList) {
			str += PrintSeq(seq, true);
		}
		return str;
	}
}
