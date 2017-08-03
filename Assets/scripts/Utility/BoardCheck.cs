using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class BoardCheck {

	private List<Spell> checkList; // compare list
    private MageMatch mm;
    private HexGrid hexGrid;
    private List<SkipCheck> skips;

    // check skipping object
    private class SkipCheck{
		public int col, row, dir; 
		public SkipCheck(Tile t, int dir){
			this.col = t.col;
			this.row = t.row;
			this.dir = dir;
		}
	}

	public BoardCheck(MageMatch mm){
        this.mm = mm;
        hexGrid = mm.hexGrid;
		skips = new List<SkipCheck> ();
	}

    //TileSeq GetCoreSeq(Tile t) {
    //    switch (t.element) {
    //        case Tile.Element.Fire:
    //            return new TileSeq("fffff");
    //        case Tile.Element.Water:
    //            return new TileSeq("wwwww");
    //        case Tile.Element.Earth:
    //            return new TileSeq("eeeee");
    //        case Tile.Element.Air:
    //            return new TileSeq("aaaaa");
    //        case Tile.Element.Muscle:
    //            return new TileSeq("mmmmm");
    //        default:
    //            MMLog.Log_BoardCheck("bad element for Core Sequence at " + t.col + ", " + t.row);
    //            return null;
    //    }
    //}

    public int CheckColumn(int c){
		int r = hexGrid.TopOfColumn(c);
		if (hexGrid.IsCellFilled (c, r))
			return -1;
		
		int min = hexGrid.BottomOfColumn (c);
		while (r > min && !hexGrid.IsCellFilled(c, r - 1))
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
        MMLog.Log_BoardCheck("EmptyCheck: totalf = ");
		return ratios;
	}

    // note: the 8th element is the total number of empty cells
    public int[] EmptyCount() {
        int[] counts = new int[8];
        counts[7] = HexGrid.numCells - hexGrid.GetPlacedTiles ().Count;
        for (int i = 0; i < HexGrid.numCols; i++) {
            if (CheckColumn(i) >= 0)
                counts[i] = hexGrid.TopOfColumn(i) - CheckColumn(i) + 1;
            else
                counts[i] = 0;
            //Debug.Log("HEXGRID: counts[" + i + "] = " + counts[i]);
        }
        //Debug.Log("HEXGRID: counts total = " + counts[7]);
        return counts;
    }

	public List<TileSeq>[] CheckBoard(List<Spell> spells){
		skips.Clear();

		List<TileSeq>[] returnList = new List<TileSeq>[5]; // list of all matching seqs to be returned
        for (int i = 0; i < 5; i++) {
            returnList[i] = new List<TileSeq>();
        }

		for(int c = 0; c < HexGrid.numCols; c++){ // for each col
			for(int r = hexGrid.BottomOfColumn(c); r <= hexGrid.TopOfColumn(c); r++){ // for each row

				if (hexGrid.IsCellFilled(c, r)) { // if there's a tile there
					List<TileSeq>[] playList = CheckTile (c, r, spells);

                    if (playList != null) {
                        List<TileSeq> spellSeqs;
                        int total = 0;
                        for (int i = 0; i < playList.Length; i++) {
                            spellSeqs = playList[i];
                            if(spellSeqs.Count > 0)
                                returnList[i].AddRange(spellSeqs);
                            total += spellSeqs.Count;
                        }
                        MMLog.Log_BoardCheck("Total for [" + c + "," + r + "]: " + total, MMLog.LogLevel.Standard);
                    }
				} else
					break; // breaks just inner loop...eventually won't because of floating tiles
			}	
		} // --Ends checking loops

        for (int i = 0; i < 5; i++) {
            MMLog.Log_BoardCheck(spells[i].name + " --> " + PrintSeqList(returnList[i]), MMLog.LogLevel.Standard);
        }

		return returnList;
	}

	List<TileSeq>[] CheckTile(int c, int r, List<Spell> spells){
        // if check matches color of current, keep checking the line
		List<TileSeq>[] returnList = new List<TileSeq>[5];
        for (int i = 0; i < 5; i++) {
            returnList[i] = new List<TileSeq>();
        }

        if (!hexGrid.GetTileBehavAt (c, r).ableMatch) // handle current tile not matchable
			return returnList;

        List<Spell> shortList = new List<Spell>(spells);
        Tile currentTile = hexGrid.GetTileAt(c, r);
        for (int i = 0; i < shortList.Count; i++) { // for each TileSeq in seqList

            // TODO if Spell is CoreSpell, add the right sequence...
            if (shortList[i] is CoreSpell) {
                //MMLog.Log_BoardCheck("Setting " + currentTile.element + " as current elem.", MMLog.LogLevel.Standard);
                ((CoreSpell)shortList[i]).currentElem = currentTile.element;
            } else {
                TileSeq seq = shortList[i].GetTileSeq();
                if (!currentTile.element.Equals(seq.GetElementAt(0))) { // remove any seqs that don't start with current color
                    shortList.RemoveAt(i);
                    i--;
                }
            }
        }

        
        if (shortList.Count == 0)
            return returnList;

        string sps = "";
        foreach (Spell s in shortList) {
            sps += s.GetTileSeq().SeqAsString() + ", ";
        }

        //MMLog.Log_BoardCheck("tiles[" + c + ", " + r + "]: Shortlist: " + sps, MMLog.LogLevel.Standard);

        // for each direction...
        for (int dir = 0; dir < 6; dir++) {
			bool skip = false;
			foreach (SkipCheck s in skips) {
				if (s.col == c && s.row == r && s.dir == dir) {
                    //MMLog.Log_BoardCheck("Skipping (" + s.col + ", " + s.row + ") in dir " + s.dir);
					skip = true;
					break;
				}
			}
			if (skip) // or empty?
				continue;

            checkList = new List<Spell>(shortList);
            List<TileSeq> seqs = new List<TileSeq>(); // updated list of any seqs
            foreach (Spell s in shortList) {
                seqs.Add(new TileSeq(currentTile));
            }

            int dc = 0, dr = 0;
            // for each tile in the direction...
            for (int seqIndex = 1; seqIndex < 5; seqIndex++) { // max seq length is 5
                bool skipCurrentSeq = false;
                if (!hexGrid.HasAdjacentCell(c + dc, r + dr, dir))
                    skipCurrentSeq = true;
                else {
                    hexGrid.GetOffset(dir, out dc, out dr);
                    dc *= seqIndex;
                    dr *= seqIndex;
                    if (!hexGrid.IsCellFilled(c + dc, r + dr))
                        skipCurrentSeq = true;
                }

                TileBehav ctb = hexGrid.GetTileBehavAt(c + dc, r + dr);
                if (skipCurrentSeq || !ctb.ableMatch) {
                    for (int i = 0; i < checkList.Count; i++) {
                        if (checkList[i] is CoreSpell && seqs[i].GetSeqLength() >= 3) {
                            AddMatchSkips(seqs[i], dir);
                            returnList[checkList[i].index].Add(seqs[i]);
                        }
                    }
                    break;
                }

                //MMLog.Log_BoardCheck("About to check (" + (c + dc) + "," + (r + dr) + "), seqs=" + PrintSeqList(seqs), MMLog.LogLevel.Standard);

                // for each spell still in the list...
                for (int i = 0; i < checkList.Count; i++) {
                    Spell s = checkList[i];
                    bool remove = false;

                    Tile.Element nextElem;
                    if (s is CoreSpell) {
                        nextElem = ((CoreSpell)s).currentElem; // could just be local var
                        //MMLog.Log_BoardCheck("Checking core spell with elem " + nextElem, MMLog.LogLevel.Standard);
                    } else
                        nextElem = s.GetTileSeq().GetElementAt(seqIndex);

                    if (ctb.tile.element.Equals(nextElem)) { // if the next tile matches the next in the seq
                        seqs[i].sequence.Add(ctb.tile);
                        //MMLog.Log_BoardCheck("Next tile matches! " + checkList[i].PrintSeq() + " length=" + seqs[i].GetSeqLength(), MMLog.LogLevel.Standard);

                        if (seqs[i].GetSeqLength() == s.GetLength()) { // complete seq!
                            returnList[checkList[i].index].Add(seqs[i]);
                            remove = true;
                        }
                    } else { // it doesn't match...
                        //MMLog.Log_BoardCheck("Seq " + checkList[i].PrintSeq() + " was not found!", MMLog.LogLevel.Standard);
                        if (checkList[i] is CoreSpell && seqs[i].GetSeqLength() >= 3) {
                            AddMatchSkips(seqs[i], dir);
                            returnList[checkList[i].index].Add(seqs[i]);
                        }
                        remove = true;
                    }

                    if (remove) {
                        checkList.RemoveAt(i);
                        seqs.RemoveAt(i);
                        i--;
                    }
                } // end of spell check
            } // end of direction line
		} // end of dir loop

		return returnList;
	}

	void AddMatchSkips(TileSeq seq, int dir){
		MMLog.Log_BoardCheck("Adding skips for " + PrintSeq (seq, true));
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
