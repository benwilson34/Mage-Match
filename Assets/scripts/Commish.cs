using UnityEngine;
using System.Collections;

public class Commish  {

	private int mood = 0;
	private MageMatch mm;
    private bool active = true;

	public Commish() {
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
	}

	public IEnumerator CTurn(){
        if (active) {
            if (mood <= -100) {
                AngryDamage();
            } else if (mood >= 100) {
                HappyHealing();
            } else
                ChangeMood(-35);

            yield return PlaceTiles();
//		Debug.Log ("CTurn: done placing tiles.");
        }
	}

	public IEnumerator PlaceTiles(){
		int numTiles = 5;
		int tries = 20;
		float[] ratios;
		yield return ratios = mm.boardCheck.EmptyCheck ();

		GameObject go = mm.GenerateTile (GetTileElement());

		for (int i = 0; i < numTiles && tries > 0; i++) {
			if (tries == 20 && i != 0) {
				yield return new WaitForSeconds (.15f);
				go = mm.GenerateTile (GetTileElement ());
			}

			int col = GetSemiRandomCol (ratios);
			if (!mm.DropTile (col, go, .15f)) { // if col is full
				i--;
				tries--;
			} else {
				go.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
				tries = 20;
			}
		}

		if (tries == 0) {
			Debug.Log ("COMMISH: The board is full. The Commissioner ends his turn early.");
			GameObject.Destroy (go);
		}
	}

	Tile.Element GetTileElement (){
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

	int GetSemiRandomCol(float[] ratios){
		float val = Random.Range (0f, 1f);
		//		Debug.Log ("GetSemiRandomCol: val = " + val);
		float thresh = 0;
		for (int i = 0; i < HexGrid.numCols; i++) {
			thresh += ratios [i];
			if (val < thresh)
				return i;
		}
		Debug.Log ("COMMISH: GetSemiRandomCol: shouldn't get to this point. val = " + val);
		return 6;
	}

	public void ChangeMood(int amount){
		mood += amount;
		Mathf.Clamp(mood, -100, 100);
	}
	
	public int GetMood(){
		return mood;
	}
	
	public void AngryDamage(){
		Debug.Log ("The Commissioner is furious! He deals damage to both players and makes them discard one tile!");
		Player p = mm.InactiveP ();
		p.ChangeHealth (-50);
		p.DiscardRandom (1);

		p = mm.ActiveP();
		p.ChangeHealth (-50);
		p.DiscardRandom (1);
		
		mood = 0;
		mm.uiCont.UpdateCommishMeter ();
	}
	
	public void HappyHealing(){
		Debug.Log ("The Commissioner is pleased, and has decided to heal both players for 100!");
		mm.InactiveP ().ChangeHealth (100);
		mm.InactiveP ().DrawTiles (1);

		mm.ActiveP ().ChangeHealth (100);
		mm.ActiveP ().DrawTiles (1);
		
		mood = 0;
		mm.uiCont.UpdateCommishMeter ();
	}
}