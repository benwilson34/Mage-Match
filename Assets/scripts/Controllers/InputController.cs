using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO eventually handle mobile tap input instead of clicking
public class InputController : MonoBehaviour {

	private MageMatch mm;

	private Vector3 dragClick;
	private Transform parentT;
	private bool dragged = false;

	private bool lastClick = false, nowClick = false;
	private TileBehav clickTB; // needed?
	private CellBehav clickCB; // needed?
	private bool isClickTB = false;

	void Awake () {
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
	}

	void Update(){ // polling input
		if (Input.GetMouseButton (0)) { // if left mouse is down
			if (Targeting.currentTMode == Targeting.TargetMode.Drag){
//				HandleDrag ();
			} else 
				HandleMouse ();
			nowClick = true; // move up?
		} else if (Input.GetMouseButtonUp (0)) { // if left mouse was JUST released
			nowClick = false;
			HandleMouse ();
		}
	}

	TileBehav GetMouseTile(RaycastHit2D[] hits){
		foreach (RaycastHit2D hit in hits) {
			TileBehav tb = hit.collider.GetComponent<TileBehav> ();
			if (tb != null) {
//				clickTB = tb;
//				isClickTB = true;
				return tb;
			}
		}
		return null;
	}

	CellBehav GetMouseCell(RaycastHit2D[] hits){
		foreach (RaycastHit2D hit in hits) {
			CellBehav cb = hit.collider.GetComponent<CellBehav> ();
			if (cb != null) {
//				clickTB = tb;
//				isClickTB = true;
				return cb;
			}
		}
		return null;
	}

	void HandleMouse(){
		if (!lastClick) { // first frame of click i.e. MouseDown
			if (!GetClick ()) {
//				nowClick = false;
				return;
			}
		}

		if (!lastClick && nowClick) { // MouseDown
			if (isClickTB) {
				TBMouseDown(clickTB);
			} else {
				CBMouseDown(clickCB);
			}
			lastClick = true;
		} else if (lastClick && nowClick) { // MouseDrag
			if (isClickTB) {
				TBMouseDrag(clickTB);
			} else {
//				CBMouseDrag(clickCB);
			}
		} else if (lastClick && !nowClick) { // MouseUp
			if (isClickTB) {
				TBMouseUp(clickTB);
			} else {
//				CBMouseUp(clickCB);
			}
			lastClick = false;
		}
	}

	bool GetClick(){
		isClickTB = false;
		Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition);

		if (Targeting.currentTMode != Targeting.TargetMode.Cell) {
			TileBehav tb = GetMouseTile (hits);
			if (tb != null) {
				clickTB = tb;
				isClickTB = true;
				return true; // void?
			}
		}
		CellBehav cb = GetMouseCell (hits);
		if (cb != null) {
			clickCB = cb;
			return true; // void?
		}

		return false;
	}

	void TBMouseDown(TileBehav tb){
		if (!MageMatch.IsEnded () && !MageMatch.IsCommishTurn()) { // if the game isn't done
			if (!MageMatch.menu) { // if the menu isn't open
				switch(tb.currentState){
				case TileBehav.TileState.Hand:
					if (!Targeting.IsTargetMode ()) {
						parentT = tb.transform.parent;
						tb.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
						// TODO
						//tb.gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");

						MageMatch.currentTile = tb.gameObject;
						AudioController.PickupSound (tb.gameObject.GetComponent<AudioSource> ());
					}
					break;
				case TileBehav.TileState.Placed:
//					Debug.Log ("INPUTCONTROLLER: TBMouseDown called!");
					if (Targeting.IsTargetMode ()
					    && Targeting.currentTMode == Targeting.TargetMode.Tile) {
//						Debug.Log ("INPUTCONTROLLER: TBMouseDown called and tile is placed.");
						Targeting.OnTileTarget (tb);
//					} else if (IsTargetMode () && currentTMode == TargetMode.Drag){
////						OnDragTarget (tbs); // TODO
					} else { // disable during targeting screen?
						dragClick = Camera.main.WorldToScreenPoint (tb.transform.position);
						dragged = true;
					}
					break;
				}
			} else { // menu mode
				Settings.GetClickEffect(tb);
			}
		}
	}

	void TBMouseDrag(TileBehav tb){
//		Debug.Log ("TBMouseDrag called.");
		if (!MageMatch.IsEnded () && !MageMatch.menu 
			&& !MageMatch.IsCommishTurn() && !Targeting.IsTargetMode()) {
			switch (tb.currentState) {
			case TileBehav.TileState.Hand:
				Vector3 cursor = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				cursor.z = 0;
				tb.transform.position = cursor;
				break;
			case TileBehav.TileState.Placed:
				SwapCheck (tb);
				break;
			}
		}
	}

	void TBMouseUp(TileBehav tb){
//		Debug.Log ("TBMouseUp called.");
		if (!MageMatch.IsEnded () && !MageMatch.menu 
			&& !MageMatch.IsCommishTurn() && !Targeting.IsTargetMode()) {
			switch (tb.currentState) {
			case TileBehav.TileState.Hand:
				Vector3 mouse = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				RaycastHit2D[] hits = Physics2D.LinecastAll (mouse, mouse);

				CellBehav cb = GetMouseCell (hits);
				if (cb != null) {
					mm.DropTile (cb.col);
//					return; // void?
				} else {
//				foreach (RaycastHit2D hit in hits){
//					CellBehav cb = hit.collider.GetComponent<CellBehav> ();
//					if (cb != null) {
//						mm.DropTile(cb.col);
//						return;
//					}
//				}
					tb.transform.SetParent (parentT);
					parentT = null;
					MageMatch.activep.AlignHand (.12f, false);
				}
				break;
			}
		}
	}

	void SwapCheck(TileBehav tb){
		Tile tile = tb.tile;
		Vector3 mouse = Input.mousePosition;
		if(Vector3.Distance(mouse, dragClick) > 50 && dragged){ // if dragged more than 50 px away
			mouse -= dragClick;
			mouse.z = 0;
			float angle = Vector3.Angle(mouse, Vector3.right);
			if (mouse.y < 0)
				angle = 360 - angle;
			//				Debug.Log("mouse = " + mouse.ToString() + "; angle = " + angle);
			dragged = false; // TODO move into cases below for continuous dragging
			if (angle < 60) {         // NE cell - board NE
				//					Debug.Log("Drag NE");
				if (tile.row != HexGrid.numRows - 1 && tile.col != HexGrid.numCols - 1)
					mm.SwapTiles(tile.col, tile.row, tile.col + 1, tile.row + 1);
			} else if (angle < 120) { // N cell  - board N
				//					Debug.Log("Drag N");
				if (tile.row != HexGrid.TopOfColumn(tile.col))
					mm.SwapTiles(tile.col, tile.row, tile.col, tile.row + 1);
			} else if (angle < 180) { // W cell  - board NW
				//					Debug.Log("Drag NW");
				bool topcheck = !(tile.col <= 3 && tile.row == HexGrid.TopOfColumn (tile.col));
				if(tile.col != 0 && topcheck)
					mm.SwapTiles(tile.col, tile.row, tile.col - 1, tile.row);
			} else if (angle < 240) { // SW cell - board SW
				//					Debug.Log("Drag SW");
				if (tile.row != 0 && tile.col != 0)
					mm.SwapTiles(tile.col, tile.row, tile.col - 1, tile.row - 1);
			} else if (angle < 300) { // S cell  - board S
				//					Debug.Log("Drag S");
				if (tile.row != HexGrid.BottomOfColumn(tile.col))
					mm.SwapTiles(tile.col, tile.row, tile.col, tile.row - 1);
			} else {                  // E cell  - board SE
				//					Debug.Log("Drag SE");
				bool bottomcheck = !(tile.col >= 3 && tile.row == HexGrid.BottomOfColumn(tile.col));
				if(tile.col != HexGrid.numCols - 1 && bottomcheck)
					mm.SwapTiles(tile.col, tile.row, tile.col + 1, tile.row);
			}
		}
	}

	void CBMouseDown(CellBehav cb){
//		Debug.Log ("OnMouseDown hit on column " + cb.col);
		if (MageMatch.menu) {
			MageMatch mm = GameObject.Find ("board").GetComponent<MageMatch> ();
			Tile.Element element = Settings.GetClickElement ();
			if (element != Tile.Element.None) {
//				Debug.Log ("Clicked on col " + col + "; menu element is not None.");
				GameObject go = mm.GenerateTile (element);
				go.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
				mm.DropTile (cb.col, go, .15f);
			}
		} else if(Targeting.IsTargetMode() && Targeting.currentTMode == Targeting.TargetMode.Cell) {
			Targeting.OnCellTarget (cb);
			//Put target return here!
		}
	}
}