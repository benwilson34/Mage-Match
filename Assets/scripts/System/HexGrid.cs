using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;
using DG.Tweening;

public static class HexGrid {

	public const int NUM_COLS = 7, NUM_ROWS = 7;
	public const int NUM_CELLS = 37;
    public const float horiz = 0.866025f; // sqrt(3) / 2 ... it's the height of an equilateral triangle, used to offset the horiz position on the board

    public enum Dir { N, NE, SE, S, SW, NW };

    private static MageMatch _mm;
	private static TileBehav[,] _tileGrid;
    private static CellBehav[,] _cells;
	// TODO private List<TileBehav> _tilesOnBoard?

	public static void Init(MageMatch mm){
		_tileGrid = new TileBehav[NUM_COLS, NUM_ROWS];
        _mm = mm;

        _cells = new CellBehav[NUM_COLS, NUM_ROWS];
        for (int c = 0; c < NUM_COLS; c++) {
            for (int r = BottomOfColumn(c); r <= TopOfColumn(c); r++) {
                _cells[c, r] = GameObject.Find("cell" + c + r).GetComponent<CellBehav>(); // maybe slow?
            }
        }
    }


    #region ---------- TILES / TILE BEHAVS ----------

    public static void HardSetTileBehavAt(TileBehav tb, int col, int row){
        MMLog.Log_HexGrid("setting (" + col + ", " + row + ") to " + tb.hextag, MMLog.LogLevel.Standard);
		if (IsCellFilled (col, row))
            _tileGrid[col, row] = null;
        SetTileBehavAt (tb, col, row);
	}

    public static void RaiseTileBehavIntoColumn(TileBehav tb, int col) {
        RaiseTileBehavIntoCell(tb, col, BottomOfColumn(col));
    }

    public static void RaiseTileBehavIntoCell(TileBehav tb, int col, int row) {
        //TODO test
        tb.SetPlaced(); //?
        tb.transform.SetParent(GameObject.Find("tilesOnBoard").transform); //?

        int top = TopOfColumn(col);
        for (int r = top; r >= row; r--) {
            if (IsCellFilled(col, r)) {
                if (r == top) {
                    // handle top of column getting pushed out
                    HexManager.RemoveTile(col, r, false);
                    continue;
                }
                _tileGrid[col, r].ChangePos(col, r + 1);
                _tileGrid[col, r] = null;
            }
        }
        _mm.StartCoroutine(tb._ChangePos(col, row, .15f, "raise")); //startRow param not needed here
        //tb.ChangePos(col, row);
    }

    public static void SetTileBehavAt(TileBehav tb, int col, int row){
		_tileGrid [col, row] = tb;
	}

	public static List<TileBehav> GetPlacedTiles(TileSeq skip = null){
		List<TileBehav> returnList = new List<TileBehav> ();
		for(int c = 0; c < NUM_COLS; c++) { // for each col
			for(int r = BottomOfColumn(c); r <= TopOfColumn(c); r++) { // for each row
				if (IsCellFilledButNotInvoked(c, r)) // if there's a tile there
					returnList.Add(GetTileBehavAt(c, r));
			}	
		}

        // if skip is provided, remove those TBs
        if (skip != null) {
            List<Tile> skips = new List<Tile>(skip.sequence);
            for (int i = 0; i < returnList.Count; i++) {
                for (int s = 0; s < skips.Count; s++) {
                    if (skips[s].HasSamePos(returnList[i].tile)) {
                        returnList.RemoveAt(i);
                        i--;
                        skips.RemoveAt(s);
                        s--;
                        break;
                    }
                }

                if (skips.Count == 0)
                    break;
            }
        }

        MMLog.Log_HexGrid("Total num of tiles on board=" + returnList.Count, MMLog.LogLevel.Standard);
		return returnList;
	}

    public static bool IsBoardFull() { return GetPlacedTiles().Count == NUM_CELLS; }

    public static List<TileBehav> GetTilesInCol(int col) {
        var tbs = new List<TileBehav>();
        for (int r = BottomOfColumn(col); r < TopOfColumn(col); r++) {
            if (IsCellFilledButNotInvoked(col, r))
                tbs.Add(GetTileBehavAt(col, r));
        }
        return tbs;
    }

	public static TileBehav GetTileBehavAt(int col, int row){
		return _tileGrid [col, row];
	}

	public static Tile GetTileAt(int col, int row){
		if (_tileGrid [col, row] != null)
			return _tileGrid [col, row].tile;
		else
			return null;
    }

    public static bool IsCellFilledButNotInvoked(int col, int row) {
        return IsCellFilled(col, row) && !WasTileBehavInvoked(col, row);
    }

    static bool WasTileBehavInvoked(int col, int row) {
        return _tileGrid[col, row].wasInvoked;
    }

    public static void ClearTileBehavAt(int col, int row){
		_tileGrid [col, row] = null;
	}
    #endregion


    #region ----------  GRID POSITION / BOARD BOUNDS ----------

    public static Vector3 GridCoordToPos(Tile t) {
        return GridCoordToPos(t.col, t.row);
    }

	public static Vector3 GridCoordToPos(int col, int row){
        return _cells[col, row].transform.position;
	}

	public static float GridColToPos(int col){
        return _cells[col, 3].transform.position.x;
	}

	public static float GridRowToPos(int col, int row) {
        return _cells[col, row].transform.position.y;
    }

    public static int BottomOfColumn(int col){ // 0, 0, 0, 0, 1, 2, 3
		if (col >= 0 && col <= 6)
			return Mathf.Max (0, col - 3);
		else
			return -1;
	}

	public static int TopOfColumn(int col){    // 3, 4, 5, 6, 6, 6, 6
		if (col >= 0 && col <= 6)
			return Mathf.Min (col + 3, 6);
		else
			return -1;
	}

    public static bool CellExists(int col, int row){
		bool bounds = (col >= 0 && col < NUM_COLS) && (row >= 0 && row < NUM_ROWS);
		bool diag = (row <= TopOfColumn (col)) && (row >= BottomOfColumn(col));
		return bounds && diag;
	}

//	public static int HeightOfColumn(int col){
//		Debug.Log ("HeightOfColumn: col = " + col + ", " + TopOfColumn(col) + ", " + BottomOfColumn (col));
//		return TopOfColumn (col) - BottomOfColumn (col) + 1;
//	}
    #endregion


    #region ---------- CELLS ----------

    public static List<CellBehav> GetAllCellBehavs() {
        List<CellBehav> returnList = new List<CellBehav>();
        for (int c = 0; c < NUM_COLS; c++) { // for each col
            for (int r = BottomOfColumn(c); r <= TopOfColumn(c); r++) { // for each row
                returnList.Add(_cells[c, r]);
            }
        }
        return returnList;
    }

    public static int GetEmptyCellCount() {
        int count = 0;
        for (int c = 0; c < NUM_COLS; c++) { // for each col
            for (int r = BottomOfColumn(c); r <= TopOfColumn(c); r++) { // for each row
                if (IsCellFilled(c, r)) // if there's a tile there
                    count++;
            }
        }
        return NUM_CELLS - count;
    }

    public static CellBehav GetCellBehavAt(int col, int row) {
        return GameObject.Find("cell" + col + row).GetComponent<CellBehav>();
    }

    public static bool IsCellFilled(int col, int row){
		return _tileGrid[col, row] != null;
	}
    #endregion


    public static bool ValidCoord(int c, int r) {
        return c != -1 && r != -1;
    }

    public static bool CanSwap(int c1, int r1, int c2, int r2) {
        //adjacency check
        //if (!ValidCoord(c2, r2))
        //    return false;

        if (IsCellFilledButNotInvoked(c2, r2) &&
            _tileGrid[c1, r1].ableSwap &&
            _tileGrid[c2, r2].ableSwap)
            return true;
        else return false;
    }

	//public static bool Swap(int c1, int r1, int c2, int r2){
 //       Debug.Log("Swapping (" + c1 + ", " + r1 + ") to (" + c2 + ", " + r2 + ")");
 //       // NOTE: this only swaps in the data structure
 //       TileBehav temp = GetTileBehavAt(c2, r2);
	//	SetTileBehavAt(GetTileBehavAt(c1, r1), c2, r2);
 //       if (temp != null) { // handles empty swap
 //           SetTileBehavAt(temp, c1, r1);
 //       } else {
 //           Debug.Log("Clearing (" + c1 + ", " + r1 + ")");
 //           ClearTileBehavAt(c1, r1);
 //       }
	//	CheckGrav();
	//	return true;
	//}
   
	public static void GetOffset(int dir, out int dc, out int dr){
		dc = dr = 0;
		switch (dir) {
		case 0: // N
			dr = 1;
			break;
		case 1: // NE
			dc = 1;
			dr = 1;
			break;
		case 2: // SE
			dc = 1;
			break;
		case 3: // S
			dr = -1; 
			break;
		case 4: // SW
			dc = -1;
			dr = -1;
			break;
		case 5: // NW
			dc = -1;
			break;
		}
	}

	public static bool HasAdjacentCell(int col, int row, int dir){
		int dc, dr;
		GetOffset (dir, out dc, out dr);
		return CellExists (col + dc, row + dr);
	}

    public static void GetAdjacentTile(int c1, int r1, int dir, out int c2, out int r2) {
        c2 = r2 = -1;
        int dc, dr;
        GetOffset(dir, out dc, out dr);
        if(!CellExists(c1 + dc, r1 + dr))
            return;

        c2 = c1 + dc;
        r2 = r1 + dr;
    }

    public static bool CellsAreAdjacent(Tile t1, Tile t2) {
        if (System.Math.Abs(t1.col - t2.col) > 1) return false;
        if (System.Math.Abs(t1.row - t2.row) > 1) return false;
        if (t1.col - t2.col == 1 && t2.row - t1.row == 1) return false;
        if (t2.col - t1.col == 1 && t1.row - t2.row == 1) return false;
        return true;
    }

    public static bool HasAdjacentNonprereqTile(Tile t, TileSeq prereq) {
        List<TileBehav> tbs = GetSmallAreaTiles(t.col, t.row);
        foreach (TileBehav tb in tbs) {
            bool isPrereqAdj = false;
            foreach (Tile prereqT in prereq.sequence) {
                if (tb.tile.HasSamePos(prereqT)) {
                    isPrereqAdj = true;
                    break;
                }
            }

            if (!isPrereqAdj) {
                return true;
            }
        }
        return false;
    }

    public static Dir GetDirection(TileSeq seq) {
        Tile first = seq.sequence[0];
        Tile sec = seq.sequence[1];
        int colDiff = sec.col - first.col;
        int rowDiff = sec.row - first.row;
        if (colDiff == 0)
            return rowDiff == 1 ? Dir.N : Dir.S;
        else {
            if (rowDiff == 0)
                return colDiff == 1 ? Dir.SE : Dir.NW;
            else
                return colDiff == 1 ? Dir.NE : Dir.SW;
        }
    }

    // perimeter; doesn't add center tile
	public static List<TileBehav> GetSmallAreaTiles(int col, int row){
		List<TileBehav> tbs = new List<TileBehav> ();
		int dc, dr;
		for (int dir = 0; dir < 6; dir++) {
			GetOffset (dir, out dc, out dr);
			if (CellExists (col + dc, row + dr)) {
				if (IsCellFilled (col + dc, row + dr))
					tbs.Add (_tileGrid [col + dc, row + dr]);
			}
		}
        //Debug.Log("HEXGRID: " + tbs.Count + " tiles around (" + col + "," + row + ")");
		return tbs;
	}

	public static List<TileBehav> GetLargeAreaTiles(int col, int row){
		List<TileBehav> tbs = GetSmallAreaTiles(col, row);
		for(int dir = 0; dir < 6; dir++){
			int dc, dr;
			GetOffset (dir, out dc, out dr);
			dc *= 2;
			dr *= 2;
			
			if (CellExists(col + dc, row + dr))
			if (IsCellFilled (col + dc, row + dr))
				tbs.Add (_tileGrid [col + dc, row + dr]);
			
			if (HasAdjacentCell (col + dc, row + dr, (dir + 2) % 6)) {
				int dc2, dr2;
				GetOffset ((dir + 2) % 6, out dc2, out dr2);
				//				Debug.Log ("Dir = " + dir + ", new dir = " + ((dir+2)%6) + ", dc2 = " + dc2 + ", dr2 = " + dr2);
				if (IsCellFilled (col + dc + dc2, row + dr + dr2))
					tbs.Add (_tileGrid [col + dc + dc2, row + dr + dr2]);
			}
		}
		return tbs;
	}

	public static bool IsGridAtRest(){ // better with a List of placed tiles
		for (int c = 0; c < NUM_COLS; c++) {
			for (int r = BottomOfColumn(c); r <= TopOfColumn(c); r++) {
				if (IsCellFilled(c, r)) {
					if (!_tileGrid [c, r].IsInPosition())
						return false;
				} else
					break;
			}
		}
		return true;
	}

	// apply "gravity" to any tiles with empty space under them
	public static void CheckGrav(){ // TODO! eventually replace the grav built into TileBehav??????????
        MMLog.Log_BoardCheck("Checking the board...", MMLog.LogLevel.Standard);
		for (int c = 0; c < NUM_COLS; c++) { // for each column
			bool skip = false;
			for (int r = BottomOfColumn(c); r < TopOfColumn(c) && !skip; r++) { // for each cell
				if (_tileGrid [c, r] == null) { // if there's not something there...
					for (int r2 = r + 1; r2 <= TopOfColumn(c); r2++) { // ...loop thru the cells above it until something is hit
                        if (_tileGrid[c, r2] != null) {
                            if (_tileGrid[c, r2].ableGrav) {
                                _tileGrid[c, r] = _tileGrid[c, r2];
                                _tileGrid[c, r2] = null;
                                _tileGrid[c, r].ChangePosAndDrop(r2, c, r, .01f);
                                break;
                            } else { // TODO test with a floating tile
                                r = r2;
                                break;
                            }
						} else if (r2 == TopOfColumn(c)) { // catch empty column
							skip = true;
						}
					}
				}
			}
		}
	}
}
