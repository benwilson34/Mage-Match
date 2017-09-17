using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

// TODO eventually handle mobile tap input instead of clicking
public class InputController : MonoBehaviour {

	private MageMatch mm;
    private Targeting targeting;
    private bool dropping = false;

    private Vector3 dragClick;
	private bool dragged = false;

	private bool lastClick = false, nowClick = false;
	private Hex clickHex; // needed?
    private Hex dropHex;
    private CellBehav clickCB; // needed?
	private bool isClickHex = false;

	void Start () {
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
        targeting = mm.targeting;
    }

	void Update(){ // polling input...change to events if too much overhead
        if (Input.GetMouseButton(0) || lastClick) { // if left mouse is down
            nowClick = true; // move up?
            if (Input.GetMouseButtonUp(0)) // if left mouse was JUST released
                nowClick = false;

            if (targeting.currentTMode == Targeting.TargetMode.Drag)
                HandleDrag();
            else
                HandleMouse();
        }
	}

	void HandleMouse(){
		if (!lastClick && !GetClick()) { // first frame of click i.e. MouseDown
			return;
		}

		if (!lastClick && nowClick) { // MouseDown
			if (isClickHex)
				HexMouseDown(clickHex);
			else
				CBMouseDown(clickCB);
			lastClick = true;
		} else if (lastClick && nowClick) { // MouseDrag
			if (isClickHex)
				HexMouseDrag(clickHex);
		    else {
//				CBMouseDrag(clickCB);
			}
		} else if (lastClick && !nowClick) { // MouseUp
			if (isClickHex)
				HexMouseUp(clickHex);
			else {
//				CBMouseUp(clickCB);
			}
			lastClick = false;
		}
	}

	bool GetClick(){
		isClickHex = false;
		Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition);

		if (targeting.currentTMode != Targeting.TargetMode.Cell) {
			Hex hex = GetMouseHex(hits);
			if (hex != null) {
				clickHex = hex;
				isClickHex = true;
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

	Hex GetMouseHex(RaycastHit2D[] hits){
		foreach (RaycastHit2D hit in hits) {
			TileBehav tb = hit.collider.GetComponent<TileBehav> ();
			if (tb != null)
				return tb;
            Hex hex = hit.collider.GetComponent<Hex>();
            if (hex != null)
                return hex;
        }
		return null;
	}

	CellBehav GetMouseCell(RaycastHit2D[] hits){
		foreach (RaycastHit2D hit in hits) {
			CellBehav cb = hit.collider.GetComponent<CellBehav> ();
			if (cb != null)
				return cb;
		}
		return null;
	}

    HandSlot GetHandSlot(RaycastHit2D[] hits) {
        foreach (RaycastHit2D hit in hits) {
            HandSlot slot = hit.collider.GetComponent<HandSlot>();
            if (slot != null) {
                //MMLog.Log_InputCont(">>>>>>Got a handslot!!! Index="+slot.handIndex);
                return slot;
            }
        }
        return null;
    }

    void HandleDrag() {
        TileBehav tb = null;

        if (!lastClick && nowClick) { // MouseDown
            tb = GetDragTarget();
            if (tb == null) return;
            targeting.OnTBTarget(tb);
            dragClick = Camera.main.WorldToScreenPoint(tb.transform.position);
            lastClick = true;
        } else if (lastClick && nowClick) { // MouseDrag
            Vector3 mouse = Input.mousePosition;
            if (Vector3.Distance(dragClick, mouse) > 60) {
                MMLog.Log_InputCont("Drag more than 60px.");
                tb = GetDragTarget();
                if (tb == null)
                    targeting.EndDragTarget();
                targeting.OnTBTarget(tb);
                if (!targeting.TargetsRemain())
                    targeting.EndDragTarget();
                dragClick = Camera.main.WorldToScreenPoint(tb.transform.position);
            }
        } else if (lastClick && !nowClick) { // MouseUp
            targeting.EndDragTarget();
            lastClick = false;
        }

    }

    TileBehav GetDragTarget() {
        Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition);
        return (TileBehav)GetMouseHex(hits);
    }

    bool MenuOpen() { return mm.uiCont.IsMenu(); }

    // ------------------------------- TB & CB handling -------------------------------

    void HexMouseDown(Hex hex){
		if (!mm.IsEnded ()) { // if the game isn't done
			//if (!mm.menu) { // if the menu isn't open
                switch (hex.currentState) {
                    case Hex.State.Hand:
                        if (!targeting.IsTargetMode() && !MenuOpen() && mm.LocalP().IsHexMine(hex)) {
                            dropping = true;

                            hex.GetComponent<SpriteRenderer>().sortingOrder = 1;
                            dropHex = hex;
                            mm.LocalP().hand.GrabHex(hex); //?
                            mm.eventCont.GrabTile(mm.myID, hex.tag);
                        }
                        break;

                    case Hex.State.Placed:
                    //					    Debug.Log ("INPUTCONTROLLER: TBMouseDown called!");
                        if (targeting.IsTargetMode()
                            //					        && Targeting.currentTMode == Targeting.TargetMode.Tile
                            ) {
                            MMLog.Log_InputCont("TBMouseDown called and tile is placed.");
                            targeting.OnTBTarget((TileBehav)hex);
                        } else if (targeting.IsSelectionMode()) {
                            targeting.OnSelection((TileBehav)hex);
//					    } else if (IsTargetMode () && currentTMode == TargetMode.Drag){
////						OnDragTarget (tbs); // TODO
                        } else { // disable during targeting screen?
                            dragClick = Camera.main.WorldToScreenPoint(hex.transform.position);
                            dragged = true;
                        }
                        break;

                }
			//} else { // menu mode
                //uiCont.GetClickEffect(tb); //?
			//}
		}
	}

	void HexMouseDrag(Hex hex){
//		Debug.Log ("TBMouseDrag called.");
		if (!mm.IsEnded () && !targeting.IsTargetMode()) {
			switch (hex.currentState) {
			    case Hex.State.Hand:
                    if (!MenuOpen() && dropping) {
                        Vector3 cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        cursor.z = 0;
                        hex.transform.position = cursor;

                        RaycastHit2D[] hits = Physics2D.LinecastAll(cursor, cursor);
                        HandSlot slot = GetHandSlot(hits);
                        if (slot != null && Vector3.Distance(cursor, slot.transform.position) < 10)
                            mm.LocalP().hand.Rearrange(slot);
                    }
				    break;
			    case Hex.State.Placed:
                    if (targeting.IsTargetMode() && 
                        targeting.currentTMode == Targeting.TargetMode.Drag) {
                        targeting.OnTBTarget((TileBehav)hex); // maybe its own method?
                    }
                    if (!ActionNotAllowed() || PromptedSwap())
				        SwapCheck((TileBehav)hex);
				    break;
			}
		}
	}

	void HexMouseUp(Hex hex){
        if (!mm.IsEnded () && !targeting.IsTargetMode()) {
            if (!MenuOpen()) { //?
                switch (hex.currentState) {
                    case Hex.State.Hand:
                        if (dropping) { // will always be if it's in the hand?
                            hex.GetComponent<SpriteRenderer>().sortingOrder = 0;
                            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                            RaycastHit2D[] hits = Physics2D.LinecastAll(mouse, mouse);
                            CellBehav cb = GetMouseCell(hits); // get cell underneath

                            if (ActionNotAllowed() || cb == null) {
                                mm.LocalP().hand.ReleaseTile(hex); //?
                            } else {
                                if (DropCheck(cb.col)) {
                                    if (PromptedDrop())
                                        mm.prompt.SetDrop(cb.col, (TileBehav)dropHex);
                                    else
                                        mm.PlayerDropTile(cb.col, dropHex);
                                }
                                
                                mm.LocalP().hand.ClearPlaceholder(); //?
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
            Transform t = new GameObject().transform;
            t.position = mouse;

            //MMLog.Log_InputCont("mouse = " + t.position.ToString());
            t.RotateAround(Vector3.zero, Vector3.forward, -30f);
            float angle = Vector3.Angle(Vector3.up, t.position);
            if (t.position.x < 0)
                angle = 360 - angle;

            Destroy(t.gameObject);

            //MMLog.Log_InputCont("mouse = " + t.position.ToString() + "; angle = " + angle);

			dragged = false; // TODO move into cases below for continuous dragging
            int dir = (int)Mathf.Floor(angle / 60);
            int c2, r2;
            mm.hexGrid.GetAdjacentTile(tile.col, tile.row, dir, out c2, out r2);
            if (c2 == -1 || r2 == -1)
                return;

            if (mm.prompt.currentMode == Prompt.PromptMode.Swap) {
                // intercept swaps for Prompt
                mm.prompt.SetSwaps(tile.col, tile.row, c2, r2);
            } else
                mm.PlayerSwapTiles(tile.col, tile.row, c2, r2);

		}
	}

    bool DropCheck(int col) {
        return mm.boardCheck.CheckColumn(col) >= 0;
    }

    bool ActionNotAllowed() {
        return !mm.MyTurn() || mm.switchingTurn || 
            mm.IsCommishTurn() || mm.IsPerformingAction(); // add IsEnded?
    }

    bool PromptedDrop() { return mm.MyTurn() && mm.prompt.currentMode == Prompt.PromptMode.Drop; }

    bool PromptedSwap() { return mm.MyTurn() && mm.prompt.currentMode == Prompt.PromptMode.Swap; }

    void CBMouseDown(CellBehav cb){
//		Debug.Log ("OnMouseDown hit on column " + cb.col);
		if (MenuOpen()) {
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