using UnityEngine;
using System.Collections;

public class Commish  {

	private int mood = 0, numTiles;
	private MageMatch mm;
    //private bool activeEffects = true;

	public Commish(MageMatch mm) {
        this.mm = mm;
    }

    public void InitEvents() {
        mm.eventCont.commishMatch += OnCommishMatch;
        //mm.eventCont.match += OnMatch;
    }

    public void OnCommishMatch(string[] seqs) {
        int sum = 0;
        string s = "{";
        foreach (string seq in seqs) {
            int i = seq.Length;
            sum += i;
            s += i + ",";
        }
        s = s.Remove(s.Length - 1, 1) + "}";
        Debug.Log("COMMISH: CommishMatch: lens=" + s + ", sum=" + sum);
        numTiles += sum;
    }

    //public void OnMatch(int id, int[] lens) {
    //    // TODO mood stuff etc.
    //}

    public IEnumerator CTurn(){
        //if (activeEffects) {
        //    if (mood == -100)
        //        AngryDamage();
        //    else if (mood == 100)
        //        HappyHealing();
        //    else
        //        ChangeMood(-35);

        //}
        numTiles = 5;
        yield return PlaceTiles();
//		Debug.Log ("CTurn: done placing tiles.");
	}

	public IEnumerator PlaceTiles(){
		int tries = 20;
		float[] ratios;
		yield return ratios = mm.boardCheck.EmptyCheck ();

        bool finalSuccess = false;
		GameObject go = mm.GenerateTile (GetTileElement());

        // TODO better randomization & sync rand instead of this complicated syncing
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
                if (i == numTiles - 1)
                    finalSuccess = true;
            }

            if (finalSuccess) {
                yield return new WaitUntil(() => !mm.IsBoardChecking());
                finalSuccess = false;
            }
		}

		if (tries == 0) {
			Debug.Log ("COMMISH: The board is full. The Commissioner ends his turn early.");
			GameObject.Destroy (go);
		}

        // TODO timing???
	    Debug.Log ("COMMISH: CommishTurnDone.");
        mm.eventCont.CommishTurnDone();
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
        //Debug.Log("COMMISH: GetSemiRandomCol val=" + val);
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
		mood = Mathf.Clamp(mood, -100, 100);
	}
	
	public int GetMood(){
		return mood;
	}
	
	//public void AngryDamage(){
 //       mm.uiCont.UpdateMoveText("The Commissioner is furious! Both players take 50 dmg and discard one tile!");
	//	Player p = mm.InactiveP ();
	//	p.ChangeHealth (-50);
	//	p.DiscardRandom (1);

	//	p = mm.ActiveP();
	//	p.ChangeHealth (-50);
	//	p.DiscardRandom (1);
		
	//	mood = 0;
	//	//mm.uiCont.UpdateCommishMeter ();
	//}
	
	//public void HappyHealing(){
 //       mm.uiCont.UpdateMoveText("The Commissioner is pleased, and has decided to heal both players for 100!");
	//	mm.InactiveP ().ChangeHealth (100);
	//	//mm.InactiveP ().DrawTiles (1); // buggy

	//	mm.ActiveP ().ChangeHealth (100);
	//	//mm.ActiveP ().DrawTiles (1); // buggy
		
	//	mood = 0;
	//	//mm.uiCont.UpdateCommishMeter ();
	//}
}