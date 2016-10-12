using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TileBehav : MonoBehaviour {

	public Tile tile;
	public Tile.Element initElement;
	public Sprite flipSprite;

	private MageMatch mm; //?
	private Transform parentT = null;
	private Vector3 dragClick;
	private bool flipped = false, placed = false, dragged = false;

//	public delegate void EnchantEffect(TileBehav tb);
//	private EnchantEffect enchantEffect;
	private TurnEffect enchantment;
	private bool resolved = false;
	private bool inPos = true;

	void Awake(){
		tile = new Tile (initElement);
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
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

	public void SetPlaced(){
		placed = true;
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

	public void FlipTile(){
		SpriteRenderer rend = gameObject.GetComponent<SpriteRenderer> ();
		Sprite temp = rend.sprite;
		rend.sprite = flipSprite;
		flipSprite = temp;
		flipped = !flipped;
	}

	public void SetOutOfPosition(){
		inPos = false;
	}

	public bool isInPosition(){
		return inPos;
	}
	
	public bool HasEnchantment(){
//		return this.enchantEffect != null;
		return this.enchantment != null;
	}

	public void ClearEnchantment(){
//		this.enchantEffect = null;
		this.enchantment = null;
		this.GetComponent<SpriteRenderer> ().color = Color.white;
	}

	public bool SetEnchantment(TurnEffect effect){
		if (HasEnchantment()) {
			return false; // where we decide whether new enchantments should overwrite current ones
		}
		resolved = false;
//		this.enchantEffect = effect;
		effect.SetAsEnchantment(this);
		this.enchantment = effect; 
		return true;
	}

	public bool ResolveEnchantment(){
		if (HasEnchantment() && !resolved) {
//			Debug.Log ("About to resolve enchant: " + (this.enchantEffect != null));
			resolved = true;
//			this.enchantEffect (this);
			enchantment.CancelEffect(this);
			MageMatch.endTurnEffects.Remove (enchantment); // assumes end-of-turn list
			return true;
		}
		return false;
	}


	// ----------------------- MOUSE EVENTS -----------------------
	void OnMouseDown(){
		if (!MageMatch.IsEnded () && !MageMatch.IsCommishTurn()) { // if the game isn't done
			if (!MageMatch.menu) { // if the menu isn't open
				if (!placed) { // dragging from hand
					if (!flipped) {
						parentT = transform.parent;
						transform.SetParent (GameObject.Find ("tilesOnBoard").transform);
						gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");
						MageMatch.currentTile = gameObject;
						AudioController.PickupSound (this.GetComponent<AudioSource> ());
					}
				} else if (SpellEffects.IsTargetMode ()) { // if it's target mode
					SpellEffects.OnTargetClick (this);
				} else { // if it's on the board - SWAP handling
					dragClick = Camera.main.WorldToScreenPoint (transform.position);
					dragged = true;
				}
			} else { // menu mode
				Settings.GetClickEffect(this);
			}
		}
	}

	void OnMouseDrag(){
		if (!MageMatch.IsEnded () && !MageMatch.menu && !MageMatch.IsCommishTurn()) {
			if (!placed) { // dragging from hand
				if (!flipped && parentT != null) {
					Vector3 cursor = Camera.main.ScreenToWorldPoint (Input.mousePosition);
					cursor.z = 0;
					transform.position = cursor;
				}
			} else { // SWAP handling
				SwapCheck ();
			}
		}
	}

	void OnMouseUp(){
		if (!MageMatch.IsEnded () && !MageMatch.menu && !MageMatch.IsCommishTurn()) {
			if (!placed && !flipped && parentT != null) { // dragging from hand
				Vector3 mouse = Camera.main.ScreenToWorldPoint (Input.mousePosition);
				RaycastHit2D hit = Physics2D.Raycast (new Vector2 (mouse.x, mouse.y), Vector2.zero);
				if (hit.collider != null) { // if dropped on a column
//				Debug.Log ("Tile dropped on " + hit.collider.name);
					CellBehav cb = hit.collider.GetComponent<CellBehav> ();
					if (cb == null || !mm.PlaceTile (cb.col)) {
						transform.SetParent (parentT);
						parentT = null;
						MageMatch.activep.AlignHand (.12f, false);
					}
				} else { // if not dropped on a column
					transform.SetParent (parentT);
					parentT = null;
					MageMatch.activep.AlignHand (.12f, false);
				}
				gameObject.layer = LayerMask.NameToLayer ("Default");
			}
		}
	}

	void SwapCheck(){
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
}
