using UnityEngine;
using System.Collections;

public static class HexGrid {

	public const int numCols = 7, numRows = 7;
	public const int numCells = 37;

	private const float horiz = 0.866025f; // sqrt(3) / 2 ... it's the height of an equilateral triangle, used to offset the horiz position on the board
	private static TileBehav[,] tileGrid;

	public static void Init(){ // TODO! change to public static void Init()?
		tileGrid = new TileBehav[numCols, numRows];
	}

	public static void HardSetTileBehavAt(TileBehav tb, int col, int row){
		if (IsSlotFilled (col, row))
			ClearTileBehavAt (col, row);
		SetTileBehavAt (tb, col, row);
	}

	public static void SetTileBehavAt(TileBehav tb, int col, int row){
		tileGrid [col, row] = tb;
		MageMatch.BoardChanged ();
	}

	public static void ClearTileBehavAt(int col, int row){
		tileGrid [col, row] = null;
		MageMatch.BoardChanged ();
	}

	public static TileBehav GetTileBehavAt(int col, int row){
		return tileGrid [col, row];
	}

	public static Tile GetTileAt(int col, int row){
		if (tileGrid [col, row] != null)
			return tileGrid [col, row].tile;
		else
			return null;
	}

	public static int BottomOfColumn(int col){ // 0, 0, 0, 0, 1, 2, 3
		if (col >= 0 && col <= 6)
			return (int)Mathf.Max (0, col - 3);
		else
			return -1;
	}

	public static int TopOfColumn(int col){    // 3, 4, 5, 6, 6, 6, 6
		if (col >= 0 && col <= 6)
			return (int)Mathf.Min (col + 3, 6);
		else
			return -1;
	}

//	public static int HeightOfColumn(int col){
//		Debug.Log ("HeightOfColumn: col = " + col + ", " + TopOfColumn(col) + ", " + BottomOfColumn (col));
//		return TopOfColumn (col) - BottomOfColumn (col) + 1;
//	}

	public static bool IsSlotFilled(int col, int row){
		return tileGrid [col, row] != null;
	}

	public static bool IsGridAtRest(){ // better with a List of placed tiles
		for (int c = 0; c < numCols; c++) {
			for (int r = BottomOfColumn(c); r <= TopOfColumn(c); r++) {
				if (IsSlotFilled(c, r)) {
					if (!tileGrid [c, r].IsInPosition())
						return false;
				} else
					break;
			}
		}
		return true;
	}

	public static Vector2 GridCoordToPos(int col, int row){
		return new Vector2 (GridColToPos (col), GridRowToPos (col, row));
	}

	public static float GridColToPos(int col){
		return -10f + (col * horiz); // TODO! somewhat magic num
	}

	public static float GridRowToPos(int col, int row){
		return (1.5f - (0.5f * col)) + row; // TODO! somewhat magic num
	}

	public static bool Swap(int c1, int r1, int c2, int r2){
//		Debug.Log("Swapping (" + c1 + ", " + r1 + ") to (" + c2 + ", " + r2 + ")");
		if (IsSlotFilled(c2, r2)) { // if there's something in the slot
			if (!tileGrid [c1, r1].ableSwap || !tileGrid [c2, r2].ableSwap)
				return false;
			TileBehav temp = GetTileBehavAt(c2, r2);
			SetTileBehavAt(GetTileBehavAt(c1, r1), c2, r2); // TODO look at TileBehav.ChangePos
			GetTileBehavAt(c2, r2).ChangePos (c2, r2);
			SetTileBehavAt (temp, c1, r1);
			GetTileBehavAt(c1, r1).ChangePos (c1, r1);
			MageMatch.BoardChanged ();
			CheckGrav ();
			return true;
		}
		return false;
	}

	// apply "gravity" to any tiles with empty space under them
	public static void CheckGrav(){ // TODO! eventually replace the grav built into TileBehav??????????
		for (int c = 0; c < numCols; c++) { // for each column
			bool skip = false;
			for (int r = BottomOfColumn(c); r < TopOfColumn(c) && !skip; r++) { // for each row in the col
				if (tileGrid [c, r] == null) { // if there's not something there...
					for (int r2 = r + 1; r2 <= TopOfColumn(c); r2++) { // ...loop thru the cells above it until something is hit
						if (tileGrid [c, r2] != null) {
							tileGrid [c, r] = tileGrid [c, r2];
							tileGrid [c, r2] = null;
							tileGrid [c, r].ChangePos (r2, c, r, .01f);
							break;
						} else if (r2 == TopOfColumn(c)) { // catch empty column
							skip = true;
						}
					}
				}
			}
		}
	}
}
