using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Targeting {

	public enum TargetMode { Tile, TileArea, Cell, CellArea, Drag };
	public static TargetMode currentTMode = TargetMode.Tile;

	private static MageMatch mm;
	private static int targetsLeft = 0;
	private static List<TileBehav> targetTBs; // static?
	private static Vector3 lastTCenter;       // static?

	private static List<GameObject> outlines;

	public delegate void TBTargetEffect (TileBehav tb);
	private static TBTargetEffect TBtargetEffect;
	public delegate void CBTargetEffect (CellBehav cb);
	private static CBTargetEffect CBtargetEffect;
	public delegate void TBDragTargetEffect (List<TileBehav> tbs);
	private static TBDragTargetEffect TBdragTargetEffect;

	public static void Init(){
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
	}

	public static bool IsTargetMode(){
		return targetsLeft > 0;
	}

	static void DecTargets(){ // maybe remove?
		Debug.Log ("INPUTCONTROLLER: Targets remaining = " + targetsLeft);
		targetsLeft--;
		if (targetsLeft == 0) //?
			currentTMode = TargetMode.Tile;
	}

	public static void WaitForTileTarget(int count, TBTargetEffect targetEffect){
		// TODO handle fewer tiles on board than count
		// TODO handle targeting same tile more than once
		currentTMode = TargetMode.Tile;
		targetsLeft = count;
		targetTBs = new List<TileBehav> ();
		TBtargetEffect = targetEffect;
		Debug.Log ("TARGETING: targets = " + targetsLeft);

		mm.StartCoroutine(TargetingScreen());
	}

	public static void OnTileTarget(TileBehav tb){ // doesn't need param due to field rn
		foreach (TileBehav ctb in targetTBs) // prevent targeting a tile that's already targeted
			if (ctb.tile.col == tb.tile.col && ctb.tile.row == tb.tile.row)
				return;
		foreach (Tile ct in MageMatch.activep.GetCurrentBoardSeq().sequence) // prevent targeting prereq
			if (ct.col == tb.tile.col && ct.row == tb.tile.row)
				return;

		Tile t = tb.tile;
		OutlineTarget (t.col, t.row);
		targetTBs.Add(tb);
		DecTargets ();
		Debug.Log ("TARGETING: Targeted tile (" + t.col + ", " + t.row + ")");
	}

	// TODO
	public static void WaitForCellTarget(int count, CBTargetEffect targetEffect){
		currentTMode = TargetMode.Cell;
		targetsLeft = count;
		CBtargetEffect = targetEffect;
		Debug.Log ("targets = " + targetsLeft);
	}

	public static void OnCellTarget(CellBehav cb){ // doesn't need param due to field rn
		DecTargets ();
		Debug.Log ("Targeted cell (" + cb.col + ", " + cb.row + ")");
		CBtargetEffect (cb);
	}

	// TODO
	public static void WaitForDragTarget(int count, TBDragTargetEffect targetEffect){
		// TODO
		currentTMode = TargetMode.Drag;
		targetsLeft = count;
		TBdragTargetEffect = targetEffect;
	}

	public static void OnDragTarget (List<TileBehav> tbs){ // should just pass each TB that gets painted?
		// TODO
		TBdragTargetEffect(tbs);
	}

	static IEnumerator TargetingScreen(){
		MageMatch.currentState = MageMatch.GameState.TargetMode;
		// TODO color background
		TileSeq seq = MageMatch.activep.GetCurrentBoardSeq();
		OutlinePrereq(seq);
		// TODO implement cast and cancel mechanics???
		yield return new WaitUntil(() => targetsLeft == 0);
		// if targets is 0, change back to Tile targetmode

		mm.RemoveSeq (seq);
		foreach(TileBehav tb in targetTBs){
			TBtargetEffect (tb);
		}
		MageMatch.BoardChanged ();
		MageMatch.activep.ApplyAPCost ();

		foreach(GameObject go in outlines){
			GameObject.Destroy (go);
		}
		outlines = null; // memory leak?
	}

	static void OutlinePrereq(TileSeq seq){
		outlines = new List<GameObject> (); // move to Init?
		GameObject go;
		foreach (Tile t in seq.sequence) {
			go = mm.GenerateToken ("prereq");
			go.transform.position = HexGrid.GridCoordToPos (t.col, t.row);
			outlines.Add (go);
		}
	}

	static void OutlineTarget(int col, int row){
		GameObject go = mm.GenerateToken ("target");
		go.transform.position = HexGrid.GridCoordToPos (col, row);
		outlines.Add (go);
	}
}
