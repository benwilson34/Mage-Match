using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;
using DG.Tweening;

public class HexGrid {

	public const int NUM_COLS = 7, NUM_ROWS = 7;
	public const int NUM_CELLS = 37;
    public const float horiz = 0.866025f; // sqrt(3) / 2 ... it's the height of an equilateral triangle, used to offset the horiz position on the board

    private MageMatch mm;
	private TileBehav[,] tileGrid;
    private Vector3[,] cellPositions;
    private float bbLeft, bbRight, bbBottom, bbBottomLeft, bbTop;
	// TODO public static List<TileBehav> tilesOnBoard?

	public HexGrid(){
		tileGrid = new TileBehav[NUM_COLS, NUM_ROWS];
        mm = GameObject.Find("board").GetComponent<MageMatch>();

        cellPositions = new Vector3[NUM_COLS, NUM_ROWS];
        for (int c = 0; c < NUM_COLS; c++) {
            for (int r = BottomOfColumn(c); r <= TopOfColumn(c); r++) {
                cellPositions[c, r] = GameObject.Find("cell" + c + r).transform.position; // maybe slow?
            }
        }
    }

    public void HardSetTileBehavAt(TileBehav tb, int col, int row){
        MMLog.Log_HexGrid("setting (" + col + ", " + row + ") to " + tb.hextag, MMLog.LogLevel.Standard);
		if (IsCellFilled (col, row))
            tileGrid[col, row] = null;
        SetTileBehavAt (tb, col, row);
	}

    public void RaiseTileBehavIntoColumn(TileBehav tb, int col) {
        RaiseTileBehavIntoCell(tb, col, BottomOfColumn(col));
    }

    public void RaiseTileBehavIntoCell(TileBehav tb, int col, int row) {
        //TODO test
        tb.SetPlaced(); //?
        tb.transform.SetParent(GameObject.Find("tilesOnBoard").transform); //?

        int top = TopOfColumn(col);
        for (int r = top; r >= row; r--) {
            if (IsCellFilled(col, r)) {
                if (r == top) {
                    // handle top of column getting pushed out
                    mm.hexMan.RemoveTile(col, r, false);
                    continue;
                }
                tileGrid[col, r].ChangePos(col, r + 1);
                tileGrid[col, r] = null;
            }
        }
        mm.StartCoroutine(tb._ChangePos(col, row, .15f, "raise")); //startRow param not needed here
        //tb.ChangePos(col, row);
    }

    public void SetTileBehavAt(TileBehav tb, int col, int row){
		tileGrid [col, row] = tb;
	}

	public void ClearTileBehavAt(int col, int row){
		tileGrid [col, row] = null;
	}

	public TileBehav GetTileBehavAt(int col, int row){
		return tileGrid [col, row];
	}

	public Tile GetTileAt(int col, int row){
		if (tileGrid [col, row] != null)
			return tileGrid [col, row].tile;
		else
			return null;
	}

    public CellBehav GetCellBehavAt(int col, int row) {
        return GameObject.Find("cell" + col + row).GetComponent<CellBehav>();
    }

	public int BottomOfColumn(int col){ // 0, 0, 0, 0, 1, 2, 3
		if (col >= 0 && col <= 6)
			return Mathf.Max (0, col - 3);
		else
			return -1;
	}

	public int TopOfColumn(int col){    // 3, 4, 5, 6, 6, 6, 6
		if (col >= 0 && col <= 6)
			return Mathf.Min (col + 3, 6);
		else
			return -1;
	}

//	public static int HeightOfColumn(int col){
//		Debug.Log ("HeightOfColumn: col = " + col + ", " + TopOfColumn(col) + ", " + BottomOfColumn (col));
//		return TopOfColumn (col) - BottomOfColumn (col) + 1;
//	}

    public bool WasTileBehavInvoked(int col, int row) {
        return tileGrid[col, row].wasInvoked;
    }

	public bool IsCellFilled(int col, int row){
		return tileGrid[col, row] != null;
	}

    public bool IsCellFilledButNotInvoked(int col, int row) {
        return IsCellFilled(col, row) && !WasTileBehavInvoked(col, row);
    }

	public bool IsGridAtRest(){ // better with a List of placed tiles
		for (int c = 0; c < NUM_COLS; c++) {
			for (int r = BottomOfColumn(c); r <= TopOfColumn(c); r++) {
				if (IsCellFilled(c, r)) {
					if (!tileGrid [c, r].IsInPosition())
						return false;
				} else
					break;
			}
		}
		return true;
	}

    public Vector3 GridCoordToPos(Tile t) {
        return GridCoordToPos(t.col, t.row);
    }

	public Vector3 GridCoordToPos(int col, int row){
        return cellPositions[col, row];
	}

	public float GridColToPos(int col){
        return cellPositions[col, 3].x;
	}

	public float GridRowToPos(int col, int row) {
        return cellPositions[col, row].y;
    }

    public bool CanSwap(int c1, int r1, int c2, int r2) {
        //adjacency check
        if (c2 == -1 || r2 == -1)
            return false;

        if (IsCellFilledButNotInvoked(c2, r2) &&
            tileGrid[c1, r1].ableSwap &&
            tileGrid[c2, r2].ableSwap)
            return true;
        else return false;
    }

	public bool Swap(int c1, int r1, int c2, int r2){
//		Debug.Log("Swapping (" + c1 + ", " + r1 + ") to (" + c2 + ", " + r2 + ")");
        // NOTE: this only swaps in the data structure!! the TBs animating happens in MageMatch
		TileBehav temp = GetTileBehavAt(c2, r2);
		SetTileBehavAt(GetTileBehavAt(c1, r1), c2, r2);
		//GetTileBehavAt(c2, r2).ChangePos(c2, r2);
		SetTileBehavAt (temp, c1, r1);
		//GetTileBehavAt(c1, r1).ChangePos(c1, r1);
		//mm.BoardChanged ();
		CheckGrav ();
		return true;
	}

	public List<TileBehav> GetPlacedTiles(TileSeq skip = null){
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

    public int GetEmptyCellCount() {
        int count = 0;
        for (int c = 0; c < NUM_COLS; c++) { // for each col
            for (int r = BottomOfColumn(c); r <= TopOfColumn(c); r++) { // for each row
                if (IsCellFilled(c, r)) // if there's a tile there
                    count++;
            }
        }
        return NUM_CELLS - count;
    }
		
	public bool CellExists(int col, int row){
		bool bounds = (col >= 0 && col < NUM_COLS) && (row >= 0 && row < NUM_ROWS);
		bool diag = (row <= TopOfColumn (col)) && (row >= BottomOfColumn(col));
		return bounds && diag;
	}

	public void GetOffset(int dir, out int dc, out int dr){
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

	public bool HasAdjacentCell(int col, int row, int dir){
		int dc, dr;
		GetOffset (dir, out dc, out dr);
		return CellExists (col + dc, row + dr);
	}

    public void GetAdjacentTile(int c1, int r1, int dir, out int c2, out int r2) {
        c2 = r2 = -1;
        int dc, dr;
        GetOffset(dir, out dc, out dr);
        if(!CellExists(c1 + dc, r1 + dr))
            return;

        c2 = c1 + dc;
        r2 = r1 + dr;
    }

    public bool CellsAreAdjacent(Tile t1, Tile t2) {
        if (System.Math.Abs(t1.col - t2.col) > 1) return false;
        if (System.Math.Abs(t1.row - t2.row) > 1) return false;
        if (t1.col - t2.col == 1 && t2.row - t1.row == 1) return false;
        if (t2.col - t1.col == 1 && t1.row - t2.row == 1) return false;
        return true;
    }

    public bool HasAdjacentNonprereqTile(Tile t, TileSeq prereq) {
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

    public int GetDirection(TileSeq seq) {
        Tile first = seq.sequence[0];
        Tile sec = seq.sequence[1];
        int colDiff = sec.col - first.col;
        int rowDiff = sec.row - first.row;
        if (colDiff == 0) {
            if (rowDiff == 1)
                return 0;
            else return 3;
        } else {
            if (rowDiff == 0) {
                if (colDiff == 1)
                    return 2;
                else return 5;
            } else {
                if (colDiff == 1)
                    return 1;
                else return 4;
            }
        }
    }

    // perimeter; doesn't add center tile
	public List<TileBehav> GetSmallAreaTiles(int col, int row){
		List<TileBehav> tbs = new List<TileBehav> ();
		int dc, dr;
		for (int dir = 0; dir < 6; dir++) {
			GetOffset (dir, out dc, out dr);
			if (CellExists (col + dc, row + dr)) {
				if (IsCellFilled (col + dc, row + dr))
					tbs.Add (tileGrid [col + dc, row + dr]);
			}
		}
        //Debug.Log("HEXGRID: " + tbs.Count + " tiles around (" + col + "," + row + ")");
		return tbs;
	}

	public List<TileBehav> GetLargeAreaTiles(int col, int row){
		List<TileBehav> tbs = GetSmallAreaTiles(col, row);
		for(int dir = 0; dir < 6; dir++){
			int dc, dr;
			GetOffset (dir, out dc, out dr);
			dc *= 2;
			dr *= 2;
			
			if (CellExists(col + dc, row + dr))
			if (IsCellFilled (col + dc, row + dr))
				tbs.Add (tileGrid [col + dc, row + dr]);
			
			if (HasAdjacentCell (col + dc, row + dr, (dir + 2) % 6)) {
				int dc2, dr2;
				GetOffset ((dir + 2) % 6, out dc2, out dr2);
				//				Debug.Log ("Dir = " + dir + ", new dir = " + ((dir+2)%6) + ", dc2 = " + dc2 + ", dr2 = " + dr2);
				if (IsCellFilled (col + dc + dc2, row + dr + dr2))
					tbs.Add (tileGrid [col + dc + dc2, row + dr + dr2]);
			}
		}
		return tbs;
	}

	// apply "gravity" to any tiles with empty space under them
	public void CheckGrav(){ // TODO! eventually replace the grav built into TileBehav??????????
        MMLog.Log_BoardCheck("Checking the board...", MMLog.LogLevel.Standard);
		for (int c = 0; c < NUM_COLS; c++) { // for each column
			bool skip = false;
			for (int r = BottomOfColumn(c); r < TopOfColumn(c) && !skip; r++) { // for each cell
				if (tileGrid [c, r] == null) { // if there's not something there...
					for (int r2 = r + 1; r2 <= TopOfColumn(c); r2++) { // ...loop thru the cells above it until something is hit
                        if (tileGrid[c, r2] != null) {
                            if (tileGrid[c, r2].ableGrav) {
                                tileGrid[c, r] = tileGrid[c, r2];
                                tileGrid[c, r2] = null;
                                tileGrid[c, r].ChangePosAndDrop(r2, c, r, .01f);
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
