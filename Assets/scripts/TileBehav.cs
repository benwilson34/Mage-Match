using UnityEngine;
using System.Collections;

public class TileBehav : MonoBehaviour {

	public enum TileState { Hand, Flipped, Placed, Removed };
	public TileState currentState;

	public Tile tile;
	public Tile.Element initElement;
	public Sprite flipSprite;

	public bool ableSwap = true, ableMatch = true, ableGrav = true, ableDestroy = true;
	public bool ableTarget = true; // will eventually need a list of valid spells - maybe a hierarchy? categories?

    protected MageMatch mm;
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
//		Debug.Log ("ChangePos: pos set to = (" + pos [0] + ", " + pos [1] + ")");
		tile.SetPos(col, row);
		mm.hexGrid.SetTileBehavAt (this, col, row);
		inPos = false;
		StartCoroutine(ChangePos_Anim(col, startrow, duration));
	}

	IEnumerator ChangePos_Anim(int col, int row, float duration){
        yield return mm.animCont._MoveTile(this, row, duration);

		mm.audioCont.DropSound (GetComponent<AudioSource>());
		inPos = true;
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

    // delete?
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

    public int GetEnchTier() {
        return enchantment.tier;
    }

    public void TriggerEnchantment() {
        Debug.Log("TILEBEHAV: Triggering enchantment at " + tile.col + ", " + tile.row);
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
		if (HasEnchantment() && ench.tier <= enchantment.tier) {
			return false; // if ench tier is not greater than current enchantment
		}
		ench.SetAsEnchantment(this);
		enchantment = ench; 
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
