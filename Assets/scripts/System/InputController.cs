using UnityEngine;
using MMDebug;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

// TODO eventually handle mobile tap input instead of clicking
public class InputController : MonoBehaviour {

	private MageMatch _mm;
    private MonoBehaviour _mouseObj;
    private Reporter _reporter;
    private bool _blocking = false;

    private bool _holdingHex = false;
    private Hex _heldHex;

	private bool _dragging = false;
    private Vector3 _dragHexPos;

	private bool _lastClick = false, _nowClick = false;

    private bool _validClick = true;

	void Start() {
        GameObject reporterGO = GameObject.Find("Reporter");
        if (reporterGO != null)
            _reporter = reporterGO.GetComponent<Reporter>();
    }

    public void Init(MageMatch mm) {
        _mm = mm;
        InitContexts();
    }

	void Update(){
        if (_blocking)
            return;

        if (Input.GetMouseButton(0) || _lastClick) { // if left mouse is down
            _nowClick = true;
            if (Input.GetMouseButtonUp(0)) // if left mouse was JUST released
                _nowClick = false;

            InputStatus status = InputStatus.Unhandled;
            MouseState state = GetMouseState();
            if (state == MouseState.Down)
                _mouseObj = GetObject(state, _currentContext.type);

            //if (mouseObj == null)
            //    return;

            // return if the report overlay is showing, or if the newsfeed is open
            if ((_reporter != null && _reporter.show) || _mm.uiCont.newsfeed.isMenuOpen())
                return;

            // LAYER 1 current context
            if (_mm.MyTurn() && IsValidClick() && _mouseObj != null)
                status = _currentContext.TakeInput(state, _mouseObj);

            if (state == MouseState.Down) {
                if(!_mm.debugTools.IsDebugMenuOpen()) // i don't like this
                    ((StandardContext)_standardContext).SetTooltip(GetTooltipable()); // hoo
                if (_mouseObj != null && _mouseObj is CellBehav) // only getObject if other context gets a CellBehav
                    _mouseObj = GetObject(state, InputContext.ObjType.Hex);
            }

            // LAYER 2 standard context
            if (status != InputStatus.FullyHandled)
                _standardContext.TakeInput(state, _mouseObj, status);

            UpdateMouseState();
        } else if (!_validClick) {
            _validClick = true;
        }

	}

    public enum MouseState { None, Down, Drag, Up };

    MouseState GetMouseState() { // only get once
        if (!_lastClick && _nowClick) { // MouseDown
            return MouseState.Down;
        } else if (_lastClick && _nowClick) { // MouseDrag
            return MouseState.Drag;
        } else if (_lastClick && !_nowClick) { // MouseUp
            return MouseState.Up;
        } else
            return MouseState.None; // shouldn't be needed?
    }

    void UpdateMouseState() {
        switch (GetMouseState()) {
            case MouseState.Down:
                _lastClick = true;
                break;
            case MouseState.Up:
                _lastClick = false;
                break;
        }
    }

    public void InvalidateClick() {
        if(_mm.MyTurn() && _nowClick)
            _validClick = false;
    }

    public bool IsValidClick() {
        if (!_validClick) {
            //lastClick = true;
            MMLog.Log_InputCont("Picked up input, but the click isn't valid!");
            if (!_nowClick) {
                _validClick = true;
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
            } else if (_currentContext.type == InputContext.ObjType.Cell) {
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
            if (tb != null) {
                if (tb.wasInvoked)
                    return null;
                else
                    return tb;
            }
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
                //MMLog.Log_InputCont("GetTooltipable found a Tooltipable! " + 
                    //tt.GetTooltipInfo());
                return tt;
            }
        }

        foreach (RaycastResult hit in GetUIRaycast()) {
            //MMLog.Log_InputCont("UI Raycast hit "+hit.gameObject.name);
            Tooltipable tt = hit.gameObject.GetComponent<Tooltipable>(); // is this okay?
            if (tt != null) {
                //MMLog.Log_InputCont("GetTooltipable found a Tooltipable! " + 
                    //tt.GetTooltipInfo());
                return tt;
            }
        }
        return null;
    }

    List<RaycastResult> GetUIRaycast() {
        //MMLog.Log_InputCont("calling UIRaycast");
        GraphicRaycaster gr = _mm.uiCont.GetComponent<GraphicRaycaster>();
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
		if(Vector3.Distance(mouse, _dragHexPos) > 50 && _dragging){ // if dragged more than 50 px away
			mouse -= _dragHexPos;
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

			_dragging = false; // TODO move into cases below for continuous dragging
            int dir = (int)Mathf.Floor(angle / 60);

            int c2, r2;
            _mm.hexGrid.GetAdjacentTile(tile.col, tile.row, dir, out c2, out r2);
            if (!_mm.hexGrid.CanSwap(tile.col, tile.row, c2, r2))
                return;

            if (_mm.prompt.currentMode == Prompt.PromptMode.Swap) {
                // intercept swaps for Prompt
                _mm.prompt.SetSwaps(tile.col, tile.row, c2, r2);
            } else
                _mm.PlayerSwapTiles(tile.col, tile.row, c2, r2);

		}
	}

    bool DropCheck(int col) {
        return _mm.boardCheck.CheckColumn(col) >= 0;
    }

    bool PromptedDrop() {
        return  _mm.MyTurn() && 
               (_mm.prompt.currentMode == Prompt.PromptMode.Drop || 
                _mm.prompt.currentMode == Prompt.PromptMode.QuickdrawDrop);
    }

    bool PromptedSwap() { return _mm.MyTurn() && _mm.prompt.currentMode == Prompt.PromptMode.Swap; }

    public void SetBlocking(bool blocking) { _blocking = blocking; }

    public void SetAllowHandDragging(bool allow) {
        ((StandardContext)_standardContext).allowHandDragging = allow; 
    }

    public void SetAllowHandRearrange(bool allow) {
        ((StandardContext)_standardContext).allowHandRearrangement = allow;
    }

    // This idea might be worth echoing throughout some of the implementation.
    // Slightly more work for the spell programmer, but more control.
    public void RestrictInteractableHexes(List<Hex> ignoredHexes) {
        var sc = (StandardContext)_standardContext;
        sc.restrictInteractableHexes = true;
        sc.ignoredHexes = ignoredHexes;
    }

    public void EndRestrictInteractableHexes() {
        ((StandardContext)_standardContext).restrictInteractableHexes = false;
    }





    // --------------------------------- CONTEXTS ------------------------------------

    public enum InputStatus { Unhandled, PartiallyHandled, FullyHandled };

    public class InputContext {
        public enum ObjType { None, Hex, Cell };
        public ObjType type = ObjType.None;

        protected MageMatch _mm;
        protected InputController _input;

        public InputContext(MageMatch mm, InputController input, ObjType type = ObjType.None) {
            this._mm = mm;
            this._input = input;
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
            _mm.targeting.OnTBTarget(tb);
            return InputStatus.FullyHandled;
        }
    }


    private class Target_CellContext : InputContext {
        public Target_CellContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Cell) { }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            CellBehav cb = (CellBehav)obj;
            _mm.targeting.OnCBTarget(cb);
            return InputStatus.FullyHandled;
        }
    }


    private class Target_DragContext : InputContext {

        private Vector3 _dragClick;

        public Target_DragContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Hex) { }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            TileBehav tb = null;
            tb = _input.GetDragTarget();
            if (tb == null)
                return InputStatus.Unhandled;
            _mm.targeting.OnTBTarget(tb);
            _dragClick = Camera.main.WorldToScreenPoint(tb.transform.position);
            return InputStatus.FullyHandled;
        }

        public override InputStatus OnMouseDrag(MonoBehaviour obj, InputStatus status) {
            TileBehav tb = null;
            Vector3 mouse = Input.mousePosition;
            MMLog.Log_InputCont("drag=" + _dragClick + ", mouse=" + mouse);
            if (Vector3.Distance(_dragClick, mouse) > 50) {
                MMLog.Log_InputCont("Drag more than 50px.");
                tb = _input.GetDragTarget();
                if (tb == null)
                    _mm.targeting.EndDragTarget();
                _mm.targeting.OnTBTarget(tb);
                if (!_mm.targeting.TargetsRemain())
                    _mm.targeting.EndDragTarget();
                _dragClick = Camera.main.WorldToScreenPoint(tb.transform.position);
                return InputStatus.FullyHandled;
            }
            return InputStatus.Unhandled; //?
        }

        public override InputStatus OnMouseUp(MonoBehaviour obj, InputStatus status) {
            _mm.targeting.EndDragTarget();
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
            _mm.targeting.OnSelection(tb);
            return InputStatus.FullyHandled;
        }
    }


    private class DebugToolsContext : InputContext {
        public DebugToolsContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Hex) { }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            _mm.debugTools.HandleInput(obj);
            return InputStatus.FullyHandled;
        }
    }


    private class MyTurnContext : InputContext {
        public MyTurnContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Hex) { }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            Hex hex = (Hex)obj;
            //MMLog.Log_InputCont("MyTurn mouse down, hex state="+hex.currentState);
            if (hex.currentState == Hex.State.Placed) {
                    _input._dragging = true;
                    _input._dragHexPos = Camera.main.WorldToScreenPoint(hex.transform.position);
                    return InputStatus.PartiallyHandled;

            }
            return InputStatus.Unhandled;
        }

        public override InputStatus OnMouseDrag(MonoBehaviour obj, InputStatus status) {
            Hex hex = (Hex)obj;
            if (hex.currentState == Hex.State.Placed) {
                if (!_mm.IsPerformingAction() || _input.PromptedSwap()) // i want to change this check now since there's more uniform game states
                    _input.SwapCheck((TileBehav)hex); // move here?
                return InputStatus.FullyHandled;
            }
            return InputStatus.Unhandled;
        }

        public override InputStatus OnMouseUp(MonoBehaviour obj, InputStatus status) {
            MMLog.Log_InputCont("MyTurn mouse up");

            Hex hex = (Hex)obj;
            if (hex.currentState == Hex.State.Hand) {
                if (_input._holdingHex) {
                    _input._heldHex.GetComponent<SpriteRenderer>().sortingOrder = 0;
                    Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D[] hits = Physics2D.LinecastAll(mouse, mouse);
                    CellBehav cb = _input.GetMouseCell(hits); // get cell underneath

                    if ((!_mm.IsPerformingAction() || _input.PromptedDrop()) && cb != null) {
                        if (Hex.IsCharm(_input._heldHex.hextag)) {
                            if (_input.PromptedDrop())
                                _mm.prompt.SetDrop(_input._heldHex);
                            else
                                _mm.PlayerDropCharm((Charm)_input._heldHex);
                            return InputStatus.PartiallyHandled;
                        } else if (_input.DropCheck(cb.col)) {
                            if (_input.PromptedDrop())
                                _mm.prompt.SetDrop(_input._heldHex, cb.col);
                            else
                                _mm.PlayerDropTile(_input._heldHex, cb.col);
                            return InputStatus.PartiallyHandled;
                        }
                    }
                }
            }
            return InputStatus.Unhandled; // fall through to standard context
        }
    }

    private class StandardContext : InputContext {

        public bool allowHandDragging = true, allowHandRearrangement = true, 
            restrictInteractableHexes = false;
        public List<Hex> ignoredHexes;

        private Tooltipable _tooltip;
        private Vector3 _mouseDownPos;

        private const int TOOLTIP_MOUSE_RADIUS = 40;   // in pixels

        public StandardContext(MageMatch mm, InputController input) : base(mm, input, ObjType.Hex) { }

        public void SetTooltip(Tooltipable tooltip) {
            this._tooltip = tooltip;
        }

        bool AbleToInteract(Hex hex) {
            if (restrictInteractableHexes) {
                foreach (Hex h in ignoredHexes) {
                    if (hex.EqualsTag(h))
                        return false;
                }
                return true;
            } else
                return true;
        }

        public override InputStatus OnMouseDown(MonoBehaviour obj, InputStatus status) {
            _mouseDownPos = Input.mousePosition;
            _mm.uiCont.tooltipMan.SetTooltip(_tooltip); // where to check for null?

            if (obj == null)
                return InputStatus.FullyHandled;

            Hex hex = (Hex)obj;
            MMLog.Log_InputCont("Standard mouse ; hex state="+hex.currentState);

            if (hex.currentState == Hex.State.Hand) {
                if (_mm.LocalP().IsHexMine(hex) && allowHandDragging) {
                    MMLog.Log_InputCont("Standard mouse down");

                    if (!AbleToInteract(hex))
                        return InputStatus.Unhandled;

                    _input._holdingHex = true;

                    hex.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    _input._heldHex = hex;
                    _mm.LocalP().hand.GrabHex(hex); //?
                    _mm.eventCont.GrabTile(_mm.myID, hex.hextag);
                    return InputStatus.FullyHandled;
                }
            }
            return InputStatus.Unhandled; // probably can just return fullyhandled unless there's going to be an context layer chained after this one...
        }

        // doesn't need obj passed...
        public override InputStatus OnMouseDrag(MonoBehaviour obj, InputStatus status) {
            //MMLog.Log_InputCont("Standard mouse drag, holdingHex="+input.holdingHex);

            if (_tooltip != null) { // if there's a tooltip
                //MMLog.Log_InputCont(">>>Tooltip is not null<<< ");
                if (Vector3.Distance(_mouseDownPos, Input.mousePosition) > TOOLTIP_MOUSE_RADIUS) {
                    _mm.uiCont.tooltipMan.HideOrCancelTooltip();
                    _tooltip = null;
                }
            }

            //Hex hex = (Hex)obj;
            //if (hex.currentState == Hex.State.Hand) {
            if (_input._holdingHex) {
                Vector3 cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                cursor.z = 0;
                _input._heldHex.transform.position = cursor;

                if (allowHandRearrangement) {
                    RaycastHit2D[] hits = Physics2D.LinecastAll(cursor, cursor);
                    HandSlot slot = _input.GetHandSlot(hits);
                    if (slot != null && Vector3.Distance(cursor, slot.transform.position) < 10)
                        _mm.LocalP().hand.Rearrange(slot);
                }
                return InputStatus.FullyHandled;
            }
            //}
            return InputStatus.Unhandled;
        }

        // doesn't need obj passed...
        public override InputStatus OnMouseUp(MonoBehaviour obj, InputStatus status) {

            if (_tooltip != null) {
                _mm.uiCont.tooltipMan.HideOrCancelTooltip();
                _tooltip = null;
            }

            MMLog.Log_InputCont("Standard mouse up");

            //if (hex.currentState == Hex.State.Hand) {
            if (_input._holdingHex) {
                _input._heldHex.GetComponent<SpriteRenderer>().sortingOrder = 0;
                if(status == InputStatus.Unhandled)
                    _mm.LocalP().hand.ReleaseTile(_input._heldHex); //?
                else
                    _mm.LocalP().hand.ClearPlaceholder();

                _input._holdingHex = false;
                return InputStatus.FullyHandled;
            }
            //}
            return InputStatus.Unhandled;
        }
    }

    private InputContext _currentContext, _standardContext;
    private InputContext _target_tile, _target_cell, _target_drag;
    private InputContext _selection, _debugMenu, _myTurn, _block;

    void InitContexts() {
        _block = new InputContext(_mm, this);
        _target_tile = new Target_TileContext(_mm, this);
        _target_cell = new Target_CellContext(_mm, this);
        _target_drag = new Target_DragContext(_mm, this);
        _selection = new Target_SelectionContext(_mm, this);
        _debugMenu = new DebugToolsContext(_mm, this);
        _myTurn = new MyTurnContext(_mm, this);

        _standardContext = new StandardContext(_mm, this);
        //MMLog.Log_InputCont("~~~~~~~~~~~~~~Contexts init!");
    }

    public void SetDebugInputMode(InputContext.ObjType type) {
        _debugMenu.type = type;
    }

    public void SwitchContext(MageMatch.State state) {
        MMLog.Log_InputCont("Switching context with state=" + state);
        switch (state) {
            case MageMatch.State.Normal:
                // check for my turn?
                MMLog.Log_InputCont("Normal context set.");
                _currentContext = _myTurn;
                break;

            case MageMatch.State.BeginningOfGame:
                _currentContext = _block;
                break;

            case MageMatch.State.EndOfGame:
                _currentContext = _block;
                break;

            case MageMatch.State.Selecting:
                _currentContext = _selection;
                break;

            case MageMatch.State.Targeting:
                Targeting.TargetMode tMode = _mm.targeting.currentTMode;
                if (tMode == Targeting.TargetMode.Drag)
                    _currentContext = _target_drag;
                else if (tMode == Targeting.TargetMode.Tile || tMode == Targeting.TargetMode.TileArea)
                    _currentContext = _target_tile;
                else
                    _currentContext = _target_cell;
                break;

            case MageMatch.State.NewsfeedMenu:
                _currentContext = _block;
                break;

            case MageMatch.State.DebugMenu:
                if (_mm.IsDebugMode()) {
                    _currentContext = _debugMenu;
                } else
                    _currentContext = _block;
                break;

            case MageMatch.State.TurnSwitching:
                _currentContext = _block; // idk
                break;
        }


        // TODO Invalidate click?

    }
}