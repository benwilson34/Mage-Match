using UnityEngine;
using System.Collections;

public static class EnchantEffects {

	private static MageMatch mm;

	public static void Init(){
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
	}

	public static void Burning(TileBehav tb){
		// TODO??????????
	}

	public static void Cherrybomb(TileBehav tb){
		Tile tile = tb.tile;

		Debug.Log ("CHERRYBOMB tile = (" + tile.col + ", " + tile.row + ")");

		mm.InactivePlayer ().ChangeHealth (-200);

		// Board N
		if (tile.row != HexGrid.TopOfColumn (tile.col)) { // Board N
			if (HexGrid.IsSlotFilled (tile.col, tile.row + 1)){
				mm.RemoveTile(tile.col, tile.row + 1, false);
			}
		}

		// Board NE
		if (tile.row != HexGrid.numRows - 1 && tile.col != HexGrid.numCols - 1) {
			if (HexGrid.IsSlotFilled (tile.col + 1, tile.row + 1))
				mm.RemoveTile(tile.col + 1, tile.row + 1, false);
		}

		// Board SE
		bool bottomcheck = !(tile.col >= 3 && tile.row == HexGrid.BottomOfColumn(tile.col));
		if (tile.col != HexGrid.numCols - 1 && bottomcheck) {
			if (HexGrid.IsSlotFilled (tile.col + 1, tile.row))
				mm.RemoveTile(tile.col + 1, tile.row, false);
		}

		// Board S
		if (tile.row != HexGrid.BottomOfColumn (tile.col)) {
			if (HexGrid.IsSlotFilled (tile.col, tile.row - 1))
				mm.RemoveTile(tile.col, tile.row - 1, false);
		}
		
		// Board SW
		if (tile.row != 0 && tile.col != 0) {
			if (HexGrid.IsSlotFilled (tile.col - 1, tile.row - 1))
				mm.RemoveTile(tile.col - 1, tile.row - 1, false);
		}

		// Board NW
		bool topcheck = !(tile.col <= 3 && tile.row == HexGrid.TopOfColumn (tile.col));
		if (tile.col != 0 && topcheck) {
			if (HexGrid.IsSlotFilled (tile.col - 1, tile.row))
				mm.RemoveTile(tile.col - 1, tile.row, false);
		}
	}
}
