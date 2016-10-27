using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour { // Monobehaviour needed? One InputController per tile?

	private static MageMatch mm;

	private static Vector3 dragClick;
	private static Transform parentT;
	private static bool dragged = false;

//	private static int click = 0;
	private static bool lastClick = false, nowClick = false;
	private static TileBehav clickTB;
	private static CellBehav clickCB;
	public static bool isClickTB = false;

	// Use this for initialization
	void Awake () {
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
	}

	void Update(){
//		if(!MageMatch.IsCommishTurn()){
			if (Input.GetMouseButton (0)) { // whenever left mouse is down
//				Debug.Log("NOT Commish turn...");
				nowClick = true;
				HandleMouse ();
			} else if (Input.GetMouseButtonUp (0)) {
				nowClick = false;
				HandleMouse ();
			}
//		}
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

	public static bool GetClick(){
		isClickTB = false;
		Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D[] hits = Physics2D.LinecastAll(clickPosition, clickPosition);

		// TODO replace with tags eventually
		// filter array
		foreach (RaycastHit2D hit in hits){
			TileBehav tb = hit.collider.GetComponent<TileBehav> ();
			if (tb != null) {
//				HandleTileBehav (tb);
				clickTB = tb;
				isClickTB = true;
				return true;
			}
		}
		foreach (RaycastHit2D hit in hits) {
			CellBehav cb = hit.collider.GetComponent<CellBehav> ();
			if (cb != null) {
//				HandleCellBehav (cb);
				clickCB = cb;
				return true;
			}
		}
		return false;
	}

	static void TBMouseDown(TileBehav tb){
		if (!MageMatch.IsEnded () && !MageMatch.IsCommishTurn()) { // if the game isn't done
			if (!MageMatch.menu) { // if the menu isn't open
				switch(tb.currentState){
				case TileBehav.TileState.Hand:
					parentT = tb.transform.parent;
					tb.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
					// TODO
					//tb.gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");

					MageMatch.currentTile = tb.gameObject;
					AudioController.PickupSound (tb.gameObject.GetComponent<AudioSource> ());
					break;
				case TileBehav.TileState.Placed:
//					Debug.Log ("MouseDown on Placed tile.");
					if (SpellEffects.IsTargetMode ())
						SpellEffects.OnTargetClick (tb);
					else {
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

	public static void TBMouseDrag(TileBehav tb){
//		Debug.Log ("TBMouseDrag called.");
		if (!MageMatch.IsEnded () && !MageMatch.menu && !MageMatch.IsCommishTurn()) {
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

	static void TBMouseUp(TileBehav tb){
//		Debug.Log ("TBMouseUp called.");
		if (!MageMatch.IsEnded () && !MageMatch.menu && !MageMatch.IsCommishTurn()) {
			switch (tb.currentState) {
			case TileBehav.TileState.Hand:
				Vector3 mouse = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				RaycastHit2D[] hits = Physics2D.LinecastAll (mouse, mouse);
//				RaycastHit2D hit = Physics2D.Raycast (new Vector2 (mouse.x, mouse.y), Vector2.zero);

				foreach (RaycastHit2D hit in hits){
					CellBehav cb = hit.collider.GetComponent<CellBehav> ();
					if (cb != null) {
						mm.PlaceTile(cb.col);
						return;
					}
				}
				tb.transform.SetParent (parentT);
				parentT = null;
				MageMatch.activep.AlignHand (.12f, false);
				break;
			}
		}
	}

	static void SwapCheck(TileBehav tb){
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

	static void CBMouseDown(CellBehav cb){
//		Debug.Log ("OnMouseDown hit on column " + cb.col);
		if (MageMatch.menu) {
			MageMatch mm = GameObject.Find ("board").GetComponent<MageMatch> ();
			Tile.Element element = Settings.GetClickElement ();
			if (element != Tile.Element.None) {
				//				Debug.Log ("Clicked on col " + col + "; menu element is not None.");
				GameObject go = mm.GenerateTile (element);
				go.transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
				mm.PlaceTile (cb.col, go, .15f);
			}
		} else if(SpellEffects.IsTargetMode()) {
			//Put target return here!
		}
	}
}
