using UnityEngine;
using System.Collections;
using DG.Tweening;

public class BoardFeature : MonoBehaviour {

	public Tile tile; // consider boardalts that take up more than one cell

	private MageMatch mm; // i think this can go if no mouse events
	private bool inPos = true; //?

	void Awake(){
		tile = new Tile (Tile.Element.None);
		mm = GameObject.Find ("board").GetComponent<MageMatch> (); //?
	}

	public void ChangePos(int col, int row){
		ChangePos(row, col, row);
	}

	public void ChangePos(int startrow, int col, int row){
		ChangePos(startrow, col, row, .15f);
	}

	public void ChangePos(int startrow, int col, int row, float duration){
//		inPos = false;
//		pos = HexGrid.GridCoordToPos (col, row);
//		Debug.Log ("ChangePos: pos set to = (" + pos [0] + ", " + pos [1] + ")");
		//		transform.position = new Vector3 (pos[0], HexGrid.GridRowToPos(col, startrow));
		tile.SetPos(col, row);
		HexGrid.SetTileBehavAt (this, col, row);

		inPos = false;
		StartCoroutine(ChangePos_Anim(col, startrow, duration));
	}

	IEnumerator ChangePos_Anim(int col, int row, float duration){
		MageMatch.IncAnimating ();
//		inPos = false;
		Tween moveTween = transform.DOMove(new Vector3(HexGrid.GridColToPos(col), HexGrid.GridRowToPos(col, row)),
			duration, false);

		yield return moveTween.WaitForCompletion ();
//		MageMatch.DecAnimating ();
		MageMatch.BoardChanged (); //?
//		boardChanged = true;
		StartCoroutine(Grav_Anim());
	}

	// TODO TODO
	public IEnumerator Grav_Anim(){
		float[] newPos = HexGrid.GridCoordToPos (tile.col, tile.row);
		float height = transform.position.y - newPos [1];
		Tween tween = transform.DOMove (new Vector3 (newPos [0], newPos [1]), .08f * height, false);
		tween.SetEase (Ease.InQuad);
//		tween.SetEase (Bounce);

		yield return tween.WaitForCompletion ();
//		Debug.Log (transform.name + " is in position: (" + tile.col + ", " + tile.row + ")");
//		if(vy < -.1f) // swapping sound fix - kinda sloppy
			AudioController.DropSound (this.GetComponent<AudioSource>());
		inPos = true;
		MageMatch.DecAnimating ();
	}

	// TODO
	float Bounce(float time, float duration, float unusedOvershootOrAmplitude, float unusedPeriod) {
		float thresh = .8f;
		float a = 1f / (thresh * thresh);
//		if ((time /= duration) < (1 / 2.75f)) {
		if ((time /= duration) < (thresh)) {
			return (a * time * time);
		}
//		if (time < (2 / 2.75f)) {
//			return (7.5625f * (time -= (1.5f / 2.75f)) * time + 0.75f);
//		}
//		if (time < (2.5f / 2.75f)) {
//			return (7.5625f * (time -= (2.25f / 2.75f)) * time + 0.9375f);
//		}
		float b = 1f / (.1f * .1f);
		Debug.Log(((b * (time - thresh) * (time - thresh)) - 3));
		return (b * (time -= thresh) * time) - 3;
//		return (b * time * time) - 24;
	}

	public void SetOutOfPosition(){
		inPos = false;
	}

	public bool isInPosition(){
		return inPos;
	}
}
