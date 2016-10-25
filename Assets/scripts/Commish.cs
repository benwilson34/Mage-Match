using UnityEngine;
using System.Collections;

public static class Commish  {

	private static MageMatch mm;
	//private static Spell[] spells;

	public static void  Init() {
		
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
		//spells = new Spell[1];
		//spells[0] = new Spell ("Commissioner turn", "", 0, Comm_Place5RandomTiles);

	}

	public static IEnumerator Place_Tiles(){
		int numTiles = 5;
		int tries = 20;
		float[] ratios;
		yield return ratios = BoardCheck.EmptyCheck ();

		GameObject go = mm.GenerateTile (GetTileElement());

		for (int i = 0; i < numTiles && tries > 0; i++) {
			if (tries == 20 && i != 0) {
				yield return new WaitForSeconds (.15f);
				go = mm.GenerateTile (GetTileElement ());
			}

			int col = GetSemiRandomCol (ratios);
			if (!mm.PlaceTile (col, go, .15f)) { // if col is full
				i--;
				tries--;
			} else {
				go.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
				tries = 20;
			}
		}

		if (tries == 0) {
			Debug.Log ("The board is full. The Commissioner ends his turn early.");
			GameObject.Destroy (go);
		}
	}

	static Tile.Element GetTileElement (){
		int rand = Random.Range (0, 100);
		if      (rand < 20)
			return Tile.Element.Fire;
		else if (rand < 40)
			return Tile.Element.Water;
		else if (rand < 60)
			return Tile.Element.Earth;
		else if (rand < 80)
			return Tile.Element.Air;
		else
			return Tile.Element.Muscle;
	}

	static int GetSemiRandomCol(float[] ratios){
		float val = Random.Range (0f, 1f);
		//		Debug.Log ("GetSemiRandomCol: val = " + val);
		float thresh = 0;
		for (int i = 0; i < HexGrid.numCols; i++) {
			thresh += ratios [i];
			if (val < thresh)
				return i;
		}
		Debug.Log ("GetSemiRandomCol: shouldn't get to this point. val = " + val);
		return 6;
	}
		
}