//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class RunebuildingInputController : MonoBehaviour {

//    private bool _nowClick, _lastClick;
//    private RunebuildingRune _mouseRune;
//    private RunebuildingContext _context;

//    private bool _holdingRune = false;
//    private RunebuildingRune _heldRune;

//    // Use this for initialization
//    void Start() {
//        _context = new RunebuildingContext(this);
//    }

//    // Update is called once per frame
//    void Update() {
//        if (Input.GetMouseButton(0) || _lastClick) { // if left mouse is down
//            _nowClick = true;
//            if (Input.GetMouseButtonUp(0)) // if left mouse was JUST released
//                _nowClick = false;

//            MouseState state = GetMouseState();
//            if (state == MouseState.Down)
//                _mouseRune = GetRune();

//            //if (mouseObj == null)
//            //    return;

//            // LAYER 1 current context
//            if (_mouseRune != null)
//                _context.TakeInput(state, _mouseRune);

//            //if (state == MouseState.Down) {
//                //_context.SetTooltip(GetTooltipable()); // hoo
//            //}

//            UpdateMouseState();
//        }

//    }

//    public RunebuildingRune GetRune() {
//        Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//		RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition);

//        foreach (RaycastResult hit in GetUIRaycast()) {
//            var rune = hit.gameObject.GetComponent<RunebuildingRune>();
//            if (rune != null)
//                return rune;
//        }
//        return null;
//    }

//    public RunebuildingSlot GetSlot() {
//        Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//		RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition);

//        foreach (RaycastResult hit in GetUIRaycast()) {
//            var slot = hit.gameObject.GetComponent<RunebuildingSlot>();
//            if (slot != null)
//                return slot;
//        }
//        return null;
//    }

//    List<RaycastResult> GetUIRaycast() {
//        //MMLog.Log_InputCont("calling UIRaycast");
//        GraphicRaycaster gr = GameObject.Find("world ui").GetComponent<GraphicRaycaster>();
//        //Create the PointerEventData with null for the EventSystem
//        PointerEventData ped = new PointerEventData(null);
//        //Set required parameters, in this case, mouse position
//        ped.position = Input.mousePosition;

//        List<RaycastResult> results = new List<RaycastResult>();
//        gr.Raycast(ped, results);
//        return results;
//    }

//    public enum MouseState { None, Down, Drag, Up };

//    MouseState GetMouseState() { // only get once
//        if (!_lastClick && _nowClick) { // MouseDown
//            return MouseState.Down;
//        } else if (_lastClick && _nowClick) { // MouseDrag
//            return MouseState.Drag;
//        } else if (_lastClick && !_nowClick) { // MouseUp
//            return MouseState.Up;
//        } else
//            return MouseState.None; // shouldn't be needed?
//    }

//    void UpdateMouseState() {
//        switch (GetMouseState()) {
//            case MouseState.Down:
//                _lastClick = true;
//                break;
//            case MouseState.Up:
//                _lastClick = false;
//                break;
//        }
//    }



//    private class RunebuildingContext {

//        private Vector3 _mouseDownPos;
//        private RunebuildingInputController _input;

//        public RunebuildingContext(RunebuildingInputController input) {
//            _input = input;
//        }

//        public void TakeInput(MouseState state, RunebuildingRune rune) {
//            switch (state) {
//                case MouseState.Down:
//                    OnMouseDown(rune);
//                    break;
//                case MouseState.Drag:
//                    OnMouseDrag(rune);
//                    break;
//                case MouseState.Up:
//                    OnMouseUp(rune);
//                    break;
//                default:
//                    Debug.LogError("Something is really wrong with an inputcontext.");
//                    break;
//            }
//        }

//        void OnMouseDown(RunebuildingRune rune) {
//            _mouseDownPos = Input.mousePosition;
//            //_mm.uiCont.tooltipMan.SetTooltip(_tooltip); // where to check for null?

//            if (rune == null) // shouldn't be
//                return;

//            //MMLog.Log_InputCont("Standard mouse ; hex state=" + hex.currentState);

//            //MMLog.Log_InputCont("Standard mouse down");

//            //if (!AbleToInteract(hex))
//            //    return InputStatus.Unhandled;

//            _input._holdingRune = true;

//            //hex.GetComponent<SpriteRenderer>().sortingOrder = 1;
//            _input._heldRune = rune;
//            //_mm.LocalP().hand.GrabHex(hex); //?
//            //EventController.GrabTile(_mm.myID, hex.hextag);
//    }

//        void OnMouseDrag(RunebuildingRune rune) {
//            //MMLog.Log_InputCont("Standard mouse drag, holdingHex="+input.holdingHex);

//            //if (_tooltip != null) { // if there's a tooltip
//            //    //MMLog.Log_InputCont(">>>Tooltip is not null<<< ");
//            //    if (Vector3.Distance(_mouseDownPos, Input.mousePosition) > TOOLTIP_MOUSE_RADIUS) {
//            //        _mm.uiCont.tooltipMan.HideOrCancelTooltip();
//            //        _tooltip = null;
//            //    }
//            //}

//            //Hex hex = (Hex)obj;
//            //if (hex.currentState == Hex.State.Hand) {
//            if (_input._holdingRune) {
//                Vector3 cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//                cursor.z = 0;
//                _input._heldRune.transform.position = cursor;
//            }
//            //}
//        }

//        void OnMouseUp(RunebuildingRune rune) {
//            //if (_tooltip != null) {
//            //    _mm.uiCont.tooltipMan.HideOrCancelTooltip();
//            //    _tooltip = null;
//            //}

//            //MMLog.Log_InputCont("Standard mouse up");

//            //if (hex.currentState == Hex.State.Hand) {
//            if (_input._holdingRune) {
//                //_input._heldRune.GetComponent<SpriteRenderer>().sortingOrder = 0;
//                RunebuildingSlot slot = _input.GetSlot();
//                if (slot != null) {
//                    // TODO place into slot
//                } else {
//                    // TODO place back into list
//                }

//                _input._holdingRune = false;
//            }
//            //}
//        }

//        public void SetTooltip(Tooltipable tt) {
            
//        }
//    }
//}
