﻿using UnityEngine;
using MMDebug;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

// TODO eventually handle mobile tap input instead of clicking
public class InputController : MonoBehaviour {

	private MageMatch mm;
    private Targeting targeting;
    private MonoBehaviour mouseObj;
    private Reporter reporter;

    private bool holdingHex = false;
    private Hex heldHex;

	private bool dragging = false;
    private Vector3 dragHexPos;

	private bool lastClick = false, nowClick = false;
	//private Hex clickHex; // needed?
 //   private CellBehav clickCB; // needed?
	//private bool isClickHex = false;

    private bool validClick = true;

	void Start() {
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
        targeting = mm.targeting;
        InitContexts();
        GameObject reporterGO = GameObject.Find("Reporter");
        if (reporterGO != null)
            reporter = reporterGO.GetComponent<Reporter>();
    }

	void Update(){
        if (Input.GetMouseButton(0) || lastClick) { // if left mouse is down
            nowClick = true;
            if (Input.GetMouseButtonUp(0)) // if left mouse was JUST released
                nowClick = false;

            InputStatus status = InputStatus.Unhandled;
            MouseState state = GetMouseState();
            if (state == MouseState.Down)
                mouseObj = GetObject(state, currentContext.type);

            //if (mouseObj == null)
            //    return;

            // return if the report overlay is showing
            if (reporter != null && reporter.show)
                return;

            // LAYER 1 current context
            if (mm.MyTurn() && IsValidClick() && mouseObj != null)
                status = currentContext.TakeInput(state, mouseObj);

            if (state == MouseState.Down) {
                if(!mm.uiCont.IsDebugMenuOpen()) // i don't like this
                    ((StandardContext)standardContext).SetTooltip(GetTooltipable()); // hoo
                if (mouseObj != null && mouseObj is CellBehav) // only getObject if other context gets a CellBehav
                    mouseObj = GetObject(state, InputContext.ObjType.Hex);
            }

            // LAYER 2 standard context
            if (status != InputStatus.FullyHandled)
                standardContext.TakeInput(state, mouseObj, status);

            UpdateMouseState();
        } else if (!validClick) {
            validClick = true;
        }

	}

    public enum MouseState { None, Down, Drag, Up };

    MouseState GetMouseState() { // only get once
        if (!lastClick && nowClick) { // MouseDown
            return MouseState.Down;
        } else if (lastClick && nowClick) { // MouseDrag
            return MouseState.Drag;
        } else if (lastClick && !nowClick) { // MouseUp
            return MouseState.Up;
        } else
            return MouseState.None; // shouldn't be needed?
    }

    void UpdateMouseState() {
        switch (GetMouseState()) {
            case MouseState.Down:
                lastClick = true;
                break;
            case MouseState.Up:
                lastClick = false;
                break;
        }
    }

    public void InvalidateClick() {
        if(mm.MyTurn() && nowClick)
            validClick = false;
    }

    public bool IsValidClick() {
        if (!validClick) {
            //lastClick = true;
            MMLog.Log_InputCont("Picked up input, but the click isn't valid!");
            if (!nowClick) {
                validClick = true;
                //lastClick = false;
            }
            return false;
        }
        return true;
    }

    MonoBehaviour GetObject(MouseState state, InputContext.ObjType type) {
        if (state == MouseState.Down) {
            MonoBehaviour obj = null;
                // if type == none, break?
            if (type == InputContext.ObjType.Hex) {
                obj = GetMouseHex();
            } else if (currentContext.type == InputContext.ObjType.Cell) {
                obj = GetMouseCell();
            }
            return obj;
        }
        MMLog.LogWarning("GetObject shouldn't be passed any state except Down?");
        return null;
    }

    Hex GetMouseHex() {
        Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition);
        return GetMouseHex(hits);
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

    CellBehav GetMouseCell() {
        Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition);
        return GetMouseCell(hits);
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

    TileBehav GetDragTarget() {
        Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition);
        return (TileBehav)GetMouseHex(hits);
    }

    Tooltipable GetTooltipable() {
        Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition);

        foreach (RaycastHit2D hit in hits) {
            //MMLog.Log_InputCont("2D Raycast hit "+hit.collider.gameObject.name);
            Tooltipable tt = hit.collider.GetComponent<Tooltipable>(); // is this okay?
            if (tt != null) {
                MMLog.Log_InputCont("GetTooltipable found a Tooltipable! " + 
                    tt.GetTooltipInfo());
                return tt;
            }
        }

        foreach (RaycastResult hit in GetUIRaycast()) {
            //MMLog.Log_InputCont("UI Raycast hit "+hit.gameObject.name);
            Tooltipable tt = hit.gameObject.GetComponent<Tooltipable>(); // is this okay?
            if (tt != null) {
                MMLog.Log_InputCont("GetTooltipable found a Tooltipable! " + 
                    tt.GetTooltipInfo());
                return tt;
            }
        }
        return null;
    }

    List<RaycastResult> GetUIRaycast() {
        //MMLog.Log_InputCont("calling UIRaycast");
        GraphicRaycaster gr = mm.uiCont.GetComponent<GraphicRaycaster>();
        //Create the PointerEventData with null for the EventSystem
        PointerEventData ped = new PointerEventData(null);
        //Set required parameters, in this case, mouse position
        ped.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(ped, results);
        return results;
    }

	void SwapCheck(TileBehav tb){
		Tile tile = tb.tile;
		Vector3 mouse = Input.mousePosition;
		if(Vector3.Distance(mouse, dragHexPos) > 50 && dragging){ // if dragged more than 50 px away
			mouse -= dragHexPos;
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

			dragging = false; // TODO move into cases below for continuous dragging
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

    bool PromptedDrop() { return mm.MyTurn() && mm.prompt.currentMode == Prompt.PromptMode.Drop; }

    bool PromptedSwap() { return mm.MyTurn() && mm.prompt.currentMode == Prompt.PromptMode.Swap; }


    // --------------------------------- CONTEXTS ------------------------------------

    public enum InputStatus { Unhandled, PartiallyHandled, FullyHandled };

    public class InputContext {
        public enum ObjType { None, Hex, Cell };
        public ObjType type = ObjType.None;

        protected MageMatch mm;
        protected InputController input;

        public InputContext(MageMatch mm, InputController input, ObjType type = ObjType.None) {
            this.mm = mm;
            this.input = input;
            this.type = type;
        }

        public InputStatus TakeInput(MouseState state, MonoBehaviour obj, 
            InputStatus status = InputStatus.Unhandled) {
            switch (state) {
                case MouseState.Down:
                    return OnMouseDown(obj, status);
                case MouseState.Drag:
                    return OnMouseDrag(obj, status);
                case MouseState.Up:
                    return OnMouseUp(obj, status);
                default:
                    MMLog.LogError("Something is really wrong with an inputcontext.");
                    return InputStatus.Unhandled;
            }
        }

        public virtual InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            return InputStatus.FullyHandled;
        }

        public virtual InputStatus OnMouseDrag(MonoBehaviour obj, InputStatus status) {
            return InputStatus.FullyHandled;
        }

        public virtual InputStatus OnMouseUp(MonoBehaviour obj, InputStatus status) {
            return InputStatus.FullyHandled;
        }
    }

    private class Target_TileContext : InputContext {
        public Target_TileContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Hex) { }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            Hex h = (Hex)obj;
            if (h.currentState == Hex.State.Hand)
                return InputStatus.Unhandled;

            TileBehav tb = (TileBehav)obj;
            mm.targeting.OnTBTarget(tb);
            return InputStatus.FullyHandled;
        }
    }

    private class Target_CellContext : InputContext {
        public Target_CellContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Cell) { }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            CellBehav cb = (CellBehav)obj;
            mm.targeting.OnCBTarget(cb);
            return InputStatus.FullyHandled;
        }
    }

    private class Target_DragContext : InputContext {

        private Vector3 dragClick;

        public Target_DragContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Hex) { }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            TileBehav tb = null;
            tb = input.GetDragTarget();
            if (tb == null)
                return InputStatus.Unhandled;
            mm.targeting.OnTBTarget(tb);
            dragClick = Camera.main.WorldToScreenPoint(tb.transform.position);
            return InputStatus.FullyHandled;
        }

        public override InputStatus OnMouseDrag(MonoBehaviour obj, InputStatus status) {
            TileBehav tb = null;
            Vector3 mouse = Input.mousePosition;
            MMLog.Log_InputCont("drag=" + dragClick + ", mouse=" + mouse);
            if (Vector3.Distance(dragClick, mouse) > 50) {
                MMLog.Log_InputCont("Drag more than 50px.");
                tb = input.GetDragTarget();
                if (tb == null)
                    mm.targeting.EndDragTarget();
                mm.targeting.OnTBTarget(tb);
                if (!mm.targeting.TargetsRemain())
                    mm.targeting.EndDragTarget();
                dragClick = Camera.main.WorldToScreenPoint(tb.transform.position);
                return InputStatus.FullyHandled;
            }
            return InputStatus.Unhandled; //?
        }

        public override InputStatus OnMouseUp(MonoBehaviour obj, InputStatus status) {
            mm.targeting.EndDragTarget();
            return InputStatus.FullyHandled;
        }

    }

    private class Target_SelectionContext : InputContext {
        public Target_SelectionContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Hex) { }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            Hex h = (Hex)obj;
            if (h.currentState == Hex.State.Hand)
                return InputStatus.Unhandled;

            TileBehav tb = (TileBehav)obj;
            mm.targeting.OnSelection(tb);
            return InputStatus.FullyHandled;
        }
    }

    private class DebugToolsContext : InputContext {
        public DebugToolsContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Hex) { }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            mm.debugTools.HandleInput(obj);
            return InputStatus.FullyHandled;
        }
    }

    private class MyTurnContext : InputContext {
        public MyTurnContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Hex) { }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            Hex hex = (Hex)obj;
            //MMLog.Log_InputCont("MyTurn mouse down, hex state="+hex.currentState);
            if (hex.currentState == Hex.State.Placed) {
                    input.dragging = true;
                    input.dragHexPos = Camera.main.WorldToScreenPoint(hex.transform.position);
                    return InputStatus.PartiallyHandled;

            }
            return InputStatus.Unhandled;
        }

        public override InputStatus OnMouseDrag(MonoBehaviour obj, InputStatus status) {
            Hex hex = (Hex)obj;
            if (hex.currentState == Hex.State.Placed) {
                if (!mm.IsPerformingAction() || input.PromptedSwap()) // i want to change this check now since there's more uniform game states
                    input.SwapCheck((TileBehav)hex); // move here?
                return InputStatus.FullyHandled;
            }
            return InputStatus.Unhandled;
        }

        public override InputStatus OnMouseUp(MonoBehaviour obj, InputStatus status) {
            MMLog.Log_InputCont("MyTurn mouse up");

            Hex hex = (Hex)obj;
            if (hex.currentState == Hex.State.Hand) {
                if (input.holdingHex) {
                    input.heldHex.GetComponent<SpriteRenderer>().sortingOrder = 0;
                    Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D[] hits = Physics2D.LinecastAll(mouse, mouse);
                    CellBehav cb = input.GetMouseCell(hits); // get cell underneath

                    if ((!mm.IsPerformingAction() || input.PromptedDrop()) && cb != null) {
                        if (input.DropCheck(cb.col)) {
                            if (input.PromptedDrop())
                                mm.prompt.SetDrop(cb.col, (TileBehav)input.heldHex);
                            else
                                mm.PlayerDropTile(cb.col, input.heldHex);
                            return InputStatus.PartiallyHandled;
                        }
                    }
                }
            }
            return InputStatus.Unhandled; // fall through to standard context
        }
    }

    private class StandardContext : InputContext {

        private Tooltipable tooltip;
        private Vector3 mouseDownPos;

        private const int TOOLTIP_MOUSE_RADIUS = 40;   // in pixels

        public StandardContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Hex) { }

        public void SetTooltip(Tooltipable tooltip) {
            this.tooltip = tooltip;
        }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            mouseDownPos = Input.mousePosition;
            mm.uiCont.tooltipMan.SetTooltip(tooltip); // where to check for null?

            if (obj == null)
                return InputStatus.FullyHandled;

            Hex hex = (Hex)obj;
            MMLog.Log_InputCont("Standard mouse ; hex state="+hex.currentState);

            if (hex.currentState == Hex.State.Hand) {
                if (mm.LocalP().IsHexMine(hex)) {
                    MMLog.Log_InputCont("Standard mouse down");

                    input.holdingHex = true;

                    hex.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    input.heldHex = hex;
                    mm.LocalP().hand.GrabHex(hex); //?
                    mm.eventCont.GrabTile(mm.myID, hex.tag);
                    return InputStatus.FullyHandled;
                }
            }
            return InputStatus.Unhandled; // probably can just return fullyhandled unless there's going to be an context layer chained after this one...
        }

        // doesn't need obj passed...
        public override InputStatus OnMouseDrag(MonoBehaviour obj, InputStatus status) {
            //MMLog.Log_InputCont("Standard mouse drag, holdingHex="+input.holdingHex);

            if (tooltip != null) { // if there's a tooltip
                //MMLog.Log_InputCont(">>>Tooltip is not null<<< ");
                if (Vector3.Distance(mouseDownPos, Input.mousePosition) > TOOLTIP_MOUSE_RADIUS) {
                    mm.uiCont.tooltipMan.HideOrCancelTooltip();
                    tooltip = null;
                }
            }

            //Hex hex = (Hex)obj;
            //if (hex.currentState == Hex.State.Hand) {
            if (input.holdingHex) {
                Vector3 cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                cursor.z = 0;
                input.heldHex.transform.position = cursor;

                RaycastHit2D[] hits = Physics2D.LinecastAll(cursor, cursor);
                HandSlot slot = input.GetHandSlot(hits);
                if (slot != null && Vector3.Distance(cursor, slot.transform.position) < 10)
                    mm.LocalP().hand.Rearrange(slot);
                return InputStatus.FullyHandled;
            }
            //}
            return InputStatus.Unhandled;
        }

        // doesn't need obj passed...
        public override InputStatus OnMouseUp(MonoBehaviour obj, InputStatus status) {

            if (tooltip != null) {
                mm.uiCont.tooltipMan.HideOrCancelTooltip();
                tooltip = null;
            }

            MMLog.Log_InputCont("Standard mouse up");

            //Hex hex = (Hex)obj;
            //if (hex.currentState == Hex.State.Hand) {
                if (input.holdingHex) {
                    input.heldHex.GetComponent<SpriteRenderer>().sortingOrder = 0;
                    //Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    //RaycastHit2D[] hits = Physics2D.LinecastAll(mouse, mouse);
                    //CellBehav cb = input.GetMouseCell(hits); // get cell underneath

                    //if ((!mm.IsPerformingAction() || input.PromptedDrop()) && cb != null) {
                    //    if (input.DropCheck(cb.col)) {
                    //        if (input.PromptedDrop())
                    //            mm.prompt.SetDrop(cb.col, (TileBehav)input.heldHex);
                    //        else
                    //            mm.PlayerDropTile(cb.col, input.heldHex);
                    //    }
                    if(status == InputStatus.Unhandled)
                        mm.LocalP().hand.ReleaseTile(input.heldHex); //?
                    else
                        mm.LocalP().hand.ClearPlaceholder();

                    input.holdingHex = false;
                    return InputStatus.FullyHandled;
                }
        //}
            return InputStatus.Unhandled;
        }
    }

    private InputContext currentContext, standardContext;
    private InputContext target_tile, target_cell, target_drag;
    private InputContext selection, debugMenu, myTurn, block;

    void InitContexts() {
        block = new InputContext(mm, this);
        target_tile = new Target_TileContext(mm, this);
        target_cell = new Target_CellContext(mm, this);
        target_drag = new Target_DragContext(mm, this);
        selection = new Target_SelectionContext(mm, this);
        debugMenu = new DebugToolsContext(mm, this);
        myTurn = new MyTurnContext(mm, this);

        standardContext = new StandardContext(mm, this);
        MMLog.Log_InputCont("~~~~~~~~~~~~~~Contexts init!");
    }

    public void SetDebugInputMode(InputContext.ObjType type) {
        debugMenu.type = type;
    }

    public void SwitchContext(MageMatch.State state) {
        MMLog.Log_InputCont("Switching context with state=" + state);
        switch (state) {
            case MageMatch.State.Targeting:
                Targeting.TargetMode tMode = targeting.currentTMode;
                if (tMode == Targeting.TargetMode.Drag)
                    currentContext = target_drag;
                else if (tMode == Targeting.TargetMode.Tile || tMode == Targeting.TargetMode.TileArea)
                    currentContext = target_tile;
                else
                    currentContext = target_cell;
                break;

            case MageMatch.State.Selecting:
                currentContext = selection;
                break;

            case MageMatch.State.NewsfeedMenu:
                currentContext = block;
                break;

            case MageMatch.State.DebugMenu:
                if (mm.IsDebugMode()) {
                    currentContext = debugMenu;
                    mm.debugTools.ValueChanged("insert");
                } else
                    currentContext = block;
                break;

            case MageMatch.State.Normal:
                // check for my turn?
                MMLog.Log_InputCont("Normal context set.");
                currentContext = myTurn;
                break;

            case MageMatch.State.TurnSwitching:
                currentContext = block; // idk
                break;
        }


        // TODO Invalidate click?

    }
}