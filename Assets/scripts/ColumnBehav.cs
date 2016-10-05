using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ColumnBehav : MonoBehaviour {

	public int col;

	// TODO hovers/arrows?
	void OnMouseDown(){
		if (MageMatch.menu) {
			MageMatch mm = GameObject.Find ("board").GetComponent<MageMatch> ();
			Tile.Element element = Settings.GetClickElement ();
			if (element != Tile.Element.None) {
//				Debug.Log ("Clicked on col " + col + "; menu element is not None.");
				GameObject go = mm.GenerateTile (element);
				go.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
				mm.PlaceTile (col, go, .15f);
			}
		}
	}
}
