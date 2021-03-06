﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public static class BoardCheck {

	private static List<Spell> _checkList; // compare list
    //private MageMatch mm;
    private static List<SkipCheck> _skips;

    // check skipping object
    private class SkipCheck{
		public int col, row, dir; 
		public SkipCheck(Tile t, int dir){
			this.col = t.col;
			this.row = t.row;
			this.dir = dir;
		}
	}

	public static void Init(MageMatch mm){
        //this.mm = mm;
		_skips = new List<SkipCheck> ();
	}

    public static int CheckColumn(int c){
		int r = HexGrid.TopOfColumn(c);
		if (HexGrid.IsCellFilled (c, r))
			return -1;
		
		int min = HexGrid.BottomOfColumn (c);
		while (r > min && !HexGrid.IsCellFilled(c, r - 1))
			r--;

        MMLog.Log_BoardCheck("Checking col " + c + " got r=" + r, MMLog.LogLevel.Standard);
        return r;
	}

    // note: the 8th element is the total number of empty cells
    static int[] EmptyCount() {
        int[] counts = new int[8];
        counts[7] = HexGrid.GetEmptyCellCount();
        for (int i = 0; i < HexGrid.NUM_COLS; i++) {
            if (CheckColumn(i) >= 0)
                counts[i] = HexGrid.TopOfColumn(i) - CheckColumn(i) + 1;
            else
                counts[i] = 0;
            MMLog.Log_BoardCheck("counts[" + i + "] = " + counts[i], MMLog.LogLevel.Standard);
        }
        MMLog.Log_BoardCheck("counts total = " + counts[7], MMLog.LogLevel.Standard);
        return counts;
    }

    static public int[] GetRandomCols(int num) {
        int[] emptyCounts = EmptyCount();
        List<int> cs = new List<int>();
        for (int i = 0; i < num; i++) {
            if (emptyCounts[7] == 0)
                break;

            int val = Random.Range(0, emptyCounts[7]);
            //Debug.MMLog.Log_Commish("COMMISH: GetSemiRandomCol val=" + val);
            int sum = 0;
            for (int c = 0; c < HexGrid.NUM_COLS; c++) {
                sum += emptyCounts[c];
                if (val < sum) {
                    emptyCounts[c]--; // update column count
                    emptyCounts[7]--; // update total count
                    cs.Add(c);
                    break;
                }
            }
        }
        // syncing could be here?
        return cs.ToArray();
    }

    public static List<TileSeq>[] CheckBoard(List<Spell> spells){
		_skips.Clear();

		List<TileSeq>[] returnList = new List<TileSeq>[5]; // list of all matching seqs to be returned
        for (int i = 0; i < 5; i++) {
            returnList[i] = new List<TileSeq>();
        }

		for(int c = 0; c < HexGrid.NUM_COLS; c++){ // for each col
			for(int r = HexGrid.BottomOfColumn(c); r <= HexGrid.TopOfColumn(c); r++){ // for each row

				if (HexGrid.IsCellFilledButNotInvoked(c, r)) { // if there's a tile there
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
                        //MMLog.Log_BoardCheck("Total for [" + c + "," + r + "]: " + total, MMLog.LogLevel.Standard);
                    }
				}
			}	
		} // --Ends checking loops

        for (int i = 0; i < spells.Count; i++) {
            if(returnList[i].Count > 0)
                MMLog.Log_BoardCheck(spells[i].name + " --> " + PrintSeqList(returnList[i]), MMLog.LogLevel.Standard);
        }

		return returnList;
	}

    static List<TileSeq>[] CheckTile(int c, int r, List<Spell> spells){
        // if check matches color of current, keep checking the line
		List<TileSeq>[] returnList = new List<TileSeq>[5];
        for (int i = 0; i < 5; i++) {
            returnList[i] = new List<TileSeq>();
        }

        if (!HexGrid.GetTileBehavAt (c, r).ableInvoke) // handle current tile not invokable
			return returnList;

        List<Spell> shortList = new List<Spell>(spells);
        Tile currentTile = HexGrid.GetTileAt(c, r);
        for (int i = 0; i < shortList.Count; i++) { // for each TileSeq in seqList

            // TODO if Spell is CoreSpell, add the right sequence...
            if (shortList[i] is MatchSpell) {
                //MMLog.Log_BoardCheck("Setting " + currentTile.element + " as current elem.", MMLog.LogLevel.Standard);
                MatchSpell coreSpell = (MatchSpell)shortList[i];
                foreach (Tile.Element elem in currentTile.elements) {
                    MatchSpell copySpell = new MatchSpell(coreSpell);
                    copySpell.currentElem = elem;
                    shortList.Insert(i, copySpell);
                    i++;
                }
                //((CoreSpell)shortList[i]).currentElem = currentTile.element;
            } else {
                TileSeq seq = shortList[i].GetTileSeq();
                if (!currentTile.IsElement(seq.GetElementAt(0))) { // remove any seqs that don't start with current color
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
			foreach (SkipCheck s in _skips) {
				if (s.col == c && s.row == r && s.dir == dir) {
                    //MMLog.Log_BoardCheck("Skipping (" + s.col + ", " + s.row + ") in dir " + s.dir, MMLog.LogLevel.Standard);
					skip = true;
					break;
				}
			}
			if (skip) // or empty?
				continue;

            _checkList = new List<Spell>(shortList);
            List<TileSeq> seqs = new List<TileSeq>(); // updated list of any seqs
            foreach (Spell s in shortList) {
                seqs.Add(new TileSeq(currentTile));
            }

            int dc = 0, dr = 0;
            // for each tile in the direction...
            for (int seqIndex = 1; seqIndex < 5; seqIndex++) { // max seq length is 5
                bool skipCurrentSeq = false;
                if (!HexGrid.HasAdjacentCell(c + dc, r + dr, dir))
                    skipCurrentSeq = true;
                else {
                    HexGrid.GetOffset(dir, out dc, out dr);
                    dc *= seqIndex;
                    dr *= seqIndex;
                    if (!HexGrid.IsCellFilledButNotInvoked(c + dc, r + dr))
                        skipCurrentSeq = true;
                }

                TileBehav ctb = HexGrid.GetTileBehavAt(c + dc, r + dr);
                if (skipCurrentSeq || !ctb.ableInvoke) {
                    for (int i = 0; i < _checkList.Count; i++) {
                        if (_checkList[i] is MatchSpell && seqs[i].GetSeqLength() >= 3) {
                            AddCoreSkips(seqs[i], dir);
                            returnList[_checkList[i].index].Add(seqs[i]);
                        }
                    }
                    break;
                }

                //MMLog.Log_BoardCheck("About to check (" + (c + dc) + "," + (r + dr) + "), seqs=" + PrintSeqList(seqs), MMLog.LogLevel.Standard);

                // for each spell still in the list...
                for (int i = 0; i < _checkList.Count; i++) {
                    Spell s = _checkList[i];
                    bool remove = false;

                    Tile.Element nextElem;
                    if (s is MatchSpell) {
                        nextElem = ((MatchSpell)s).currentElem; // could just be local var
                        //MMLog.Log_BoardCheck("Checking core spell with elem " + nextElem, MMLog.LogLevel.Standard);
                    } else
                        nextElem = s.GetTileSeq().GetElementAt(seqIndex);

                    if (ctb.tile.IsElement(nextElem)) { // if the next tile matches the next in the seq
                        seqs[i].sequence.Add(ctb.tile);
                        //MMLog.Log_BoardCheck("Next tile matches! " + checkList[i].PrintSeq() + " length=" + seqs[i].GetSeqLength(), MMLog.LogLevel.Standard);

                        if (seqs[i].GetSeqLength() == s.GetLength()) { // complete seq!
                            returnList[s.index].Add(seqs[i]);
                            if (s is MatchSpell) // core of length 5
                                AddCoreSkips(seqs[i], dir);
                            else if (s.isSymmetric)
                                AddSymmetricSkips(seqs[i], dir);
                            remove = true;
                        }
                    } else { // it doesn't match...
                        //MMLog.Log_BoardCheck("Seq " + checkList[i].PrintSeq() + " was not found!", MMLog.LogLevel.Standard);
                        if (s is MatchSpell && seqs[i].GetSeqLength() >= 3) {
                            AddCoreSkips(seqs[i], dir);
                            returnList[s.index].Add(seqs[i]);
                        }
                        remove = true;
                    }

                    if (remove) {
                        _checkList.RemoveAt(i);
                        seqs.RemoveAt(i);
                        i--;
                    }
                } // end of spell check
            } // end of direction line
		} // end of dir loop

		return returnList;
	}

    static void AddCoreSkips(TileSeq seq, int dir){
		//MMLog.Log_BoardCheck("Adding skips for " + PrintSeq (seq, true), MMLog.LogLevel.Standard);
		switch (seq.GetSeqLength ()) {
		case 3:
			_skips.Add (new SkipCheck (seq.sequence [2], OppDir(dir))); // 3
			break;
		case 4:
			_skips.Add (new SkipCheck (seq.sequence [3], OppDir(dir))); // 4
			_skips.Add (new SkipCheck (seq.sequence [2], OppDir(dir))); // 3
			_skips.Add (new SkipCheck (seq.sequence [1], dir));         // 3
			break;
		case 5:
			_skips.Add (new SkipCheck (seq.sequence [4], OppDir(dir))); // 5
			_skips.Add (new SkipCheck (seq.sequence [3], OppDir(dir))); // 4
			_skips.Add (new SkipCheck (seq.sequence [1], dir));         // 4
			_skips.Add (new SkipCheck (seq.sequence [2], OppDir(dir))); // 3
			_skips.Add (new SkipCheck (seq.sequence [2], dir));         // 3
			break;
		}
	}

    static void AddSymmetricSkips(TileSeq seq, int dir) {
        Tile lastT = seq.sequence[seq.GetSeqLength() - 1];
		MMLog.Log_BoardCheck("Adding skip at (" + lastT.col + ", " + lastT.row + ") in dir "+dir, MMLog.LogLevel.Standard);
        _skips.Add (new SkipCheck (lastT, OppDir(dir)));
    }

    static int OppDir(int dir){
		return (dir + 3) % 6;
	}

    static public string PrintSeq(TileSeq seq, bool showPos){ // debug
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

    static public string PrintSeqList(List<TileSeq> seqList){ // debug
		string str = "";
		foreach (TileSeq seq in seqList) {
			str += PrintSeq(seq, true);
		}
		return str;
	}
}
