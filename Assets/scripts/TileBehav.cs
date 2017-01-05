using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TileBehav : MonoBehaviour {

	public enum TileState { Hand, Flipped, Placed, Removed };
	public TileState currentState;

	public Tile tile;
	public Tile.Element initElement;
	public Sprite flipSprite;

	public bool ableSwap = true, ableMatch = true, ableGrav = true, ableDestroy = true;
	public bool ableTarget = true; // will eventually need a list of valid spells - maybe a hierarchy? categories?

    private MageMatch mm;
	private Enchantment enchantment;
//	private bool resolved = false;
	private bool inPos = true;

	void Awake(){
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        Init();
	}

    protected virtual void Init() {
        tile = new Tile(initElement);
        currentState = TileState.Hand;
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
		mm.hexGrid.SetTileBehavAt (this, col, row);

		inPos = false;
		StartCoroutine(ChangePos_Anim(col, startrow, duration));
	}

	IEnumerator ChangePos_Anim(int col, int row, float duration){
		mm.IncAnimating ();
//		inPos = false;
		Tween moveTween = transform.DOMove(new Vector3(mm.hexGrid.GridColToPos(col), mm.hexGrid.GridRowToPos(col, row)),
			duration, false);

		yield return moveTween.WaitForCompletion ();
//		MageMatch.DecAnimating ();
		mm.BoardChanged (); //?
		StartCoroutine(Grav_Anim());
	}

	public void HardSetPos(int col, int row){ // essentially a "teleport"
		tile.SetPos(col, row);
        mm.hexGrid.HardSetTileBehavAt (this, col, row);
		transform.position = mm.hexGrid.GridCoordToPos (col, row);
		mm.BoardChanged ();
	}

	public void SetPlaced(){
		currentState = TileState.Placed;
	}

	// TODO TODO
	public IEnumerator Grav_Anim(){
		Vector2 newPos = mm.hexGrid.GridCoordToPos (tile.col, tile.row);
		float height = transform.position.y - newPos.y;
		Tween tween = transform.DOMove (newPos, .08f * height, false);
		tween.SetEase (Ease.InQuad);
//		tween.SetEase (Bounce);

		yield return tween.WaitForCompletion ();
//		Debug.Log (transform.name + " is in position: (" + tile.col + ", " + tile.row + ")");
//		if(vy < -.1f) // swapping sound fix - kinda sloppy
			mm.audioCont.DropSound (this.GetComponent<AudioSource>());
		inPos = true;
		mm.DecAnimating ();
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
		if (currentState == TileState.Hand)
			currentState = TileState.Flipped;
		else if (currentState == TileState.Flipped)
			currentState = TileState.Hand;
	}

	public void SetOutOfPosition(){
		inPos = false;
	}

	public bool IsInPosition(){
		return inPos;
	}
	
	public bool HasEnchantment(){ // just use GetEnchType?
		return enchantment != null;
	}

    public Enchantment.EnchType GetEnchType() {
        if (enchantment != null)
            return enchantment.type;
        else
            return Enchantment.EnchType.None;
    }

    public void TriggerEnchantment() {
        Debug.Log("Triggering enchantment at " + tile.col + ", " + tile.row);
        enchantment.TriggerEffect();
    }

	public void ClearEnchantment(){
        //		this.enchantEffect = null;
        // TODO remove Effect from whichever list in MageMatch...
        mm.effectCont.RemoveEndTurnEffect(enchantment);
		enchantment = null;
		this.GetComponent<SpriteRenderer> ().color = Color.white;
	}

	public bool SetEnchantment(Enchantment ench){
		if (HasEnchantment()) {
			return false; // where we decide whether new enchantments should overwrite current ones
		}
//		resolved = false;
//		this.enchantEffect = effect;
		ench.SetAsEnchantment(this);
		this.enchantment = ench; 
		return true;
	}

	public bool ResolveEnchantment(){
		if (HasEnchantment() && currentState == TileState.Placed) {
//			Debug.Log ("About to resolve enchant: " + (this.enchantEffect != null));
//			resolved = true;
			currentState = TileState.Removed;
//			this.enchantEffect (this);
			enchantment.CancelEffect();
            mm.effectCont.RemoveEndTurnEffect (enchantment); // TODO assumes end-of-turn list
			return true;
		}
		return false;
	}

}
