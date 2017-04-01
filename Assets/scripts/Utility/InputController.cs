using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO eventually handle mobile tap input instead of clicking
public class InputController : MonoBehaviour {

	private MageMatch mm;
    private Targeting targeting;
    private UIController uiCont;
    private GameObject dropTile;
    private bool dropping = false;

	private Vector3 dragClick;
	private Transform parentT;
	private bool dragged = false;

	private bool lastClick = false, nowClick = false;
	private TileBehav clickTB; // needed?
	private CellBehav clickCB; // needed?
	private bool isClickTB = false;

	void Start () {
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
        targeting = mm.targeting;
        //settings = GameObject.Find("ui").GetComponent<Settings>();
        uiCont = mm.uiCont;
    }

	void Update(){ // polling input...change to events?
        if (mm.MyTurn()) {
            if (Input.GetMouseButton(0)) { // if left mouse is down
                if (targeting.currentTMode == Targeting.TargetMode.Drag) {
                    //				HandleDrag ();
                } else
                    HandleMouse();
                nowClick = true; // move up?
            } else if (Input.GetMouseButtonUp(0)) { // if left mouse was JUST released
                nowClick = false;
                HandleMouse();
            }
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

		if (targeting.currentTMode != Targeting.TargetMode.Cell) {
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
		if (!mm.IsEnded () && !mm.IsCommishTurn()) { // if the game isn't done
			//if (!mm.menu) { // if the menu isn't open
                switch (tb.currentState) {
                    case TileBehav.TileState.Hand:
                        if (!targeting.IsTargetMode() && !mm.menu && mm.LocalP().IsTileMine(tb)) {
                            dropping = true;
                            parentT = tb.transform.parent;
                            tb.transform.SetParent(GameObject.Find("tilesOnBoard").transform);
                            // TODO
                            //tb.gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");

                            dropTile = tb.gameObject;
                            mm.audioCont.PickupSound(tb.gameObject.GetComponent<AudioSource>());
                        }
                        break;
                    case TileBehav.TileState.Placed:
//					    Debug.Log ("INPUTCONTROLLER: TBMouseDown called!");
                        if (targeting.IsTargetMode()
//					        && Targeting.currentTMode == Targeting.TargetMode.Tile
                            ) {
                            Debug.Log("INPUTCONTROLLER: TBMouseDown called and tile is placed.");
                            targeting.OnTBTarget(tb);
//					    } else if (IsTargetMode () && currentTMode == TargetMode.Drag){
////						OnDragTarget (tbs); // TODO
                        } else { // disable during targeting screen?
                            dragClick = Camera.main.WorldToScreenPoint(tb.transform.position);
                            dragged = true;
                        }
                        break;
                }
			//} else { // menu mode
                //uiCont.GetClickEffect(tb); //?
			//}
		}
	}

	void TBMouseDrag(TileBehav tb){
//		Debug.Log ("TBMouseDrag called.");
		if (!mm.IsEnded () && !mm.IsCommishTurn() && !targeting.IsTargetMode()) {
			switch (tb.currentState) {
			    case TileBehav.TileState.Hand:
                    if (!mm.menu && dropping) {
                        Vector3 cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        cursor.z = 0;
                        tb.transform.position = cursor;
                    }
				    break;
			    case TileBehav.TileState.Placed:
				    SwapCheck (tb);
				    break;
			}
		}
	}

	void TBMouseUp(TileBehav tb){
//		Debug.Log ("TBMouseUp called.");
		if (!mm.IsEnded () && !mm.IsCommishTurn() && !targeting.IsTargetMode()) {
            if (!mm.menu) {
                switch (tb.currentState) {
                    case TileBehav.TileState.Hand:
                        if (dropping) {
                            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            RaycastHit2D[] hits = Physics2D.LinecastAll(mouse, mouse);

                            CellBehav cb = GetMouseCell(hits);
                            if (cb != null) {
                                mm.DropTile(cb.col, dropTile);
                            } else {
                                tb.transform.SetParent(parentT);
                                parentT = null;
                                mm.ActiveP().AlignHand(.12f, false);
                            }
                            dropping = false;
                        }
                        break;
                }
            } else {
                // TODO bool menuSwap
                //uiCont.GetClickEffect(tb);
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
			if (angle < 60) {         // NE
				if (mm.hexGrid.HasAdjacentCell(tile.col, tile.row, 1))
					mm.SwapTiles(tile.col, tile.row, tile.col + 1, tile.row + 1);
			} else if (angle < 120) { // N
				if (mm.hexGrid.HasAdjacentCell(tile.col, tile.row, 0))
					mm.SwapTiles(tile.col, tile.row, tile.col, tile.row + 1);
			} else if (angle < 180) { // NW
				if (mm.hexGrid.HasAdjacentCell(tile.col, tile.row, 5))
					mm.SwapTiles(tile.col, tile.row, tile.col - 1, tile.row);
			} else if (angle < 240) { // SW
				if (mm.hexGrid.HasAdjacentCell(tile.col, tile.row, 4))
					mm.SwapTiles(tile.col, tile.row, tile.col - 1, tile.row - 1);
			} else if (angle < 300) { // S
				if (mm.hexGrid.HasAdjacentCell(tile.col, tile.row, 3))
					mm.SwapTiles(tile.col, tile.row, tile.col, tile.row - 1);
			} else {                  // SE
				if (mm.hexGrid.HasAdjacentCell(tile.col, tile.row, 2))
					mm.SwapTiles(tile.col, tile.row, tile.col + 1, tile.row);
			}
		}
	}

	void CBMouseDown(CellBehav cb){
//		Debug.Log ("OnMouseDown hit on column " + cb.col);
		if (mm.menu) {
//			MageMatch mm = GameObject.Find ("board").GetComponent<MageMatch> ();
//			Tile.Element element = uiCont.GetClickElement ();
//			if (element != Tile.Element.None) {
////				Debug.Log ("Clicked on col " + col + "; menu element is not None.");
//				GameObject go = mm.GenerateTile (element);
//				go.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
//				mm.DropTile (cb.col, go, .15f);
//			}
		} else if(targeting.IsTargetMode() && targeting.currentTMode == Targeting.TargetMode.Cell) {
			targeting.OnCBTarget (cb);
			//Put target return here!
		}
	}
}