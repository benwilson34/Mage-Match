using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private List<TileEffect> tileEffects;
	private bool inPos = true;

	void Awake(){
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        tileEffects = new List<TileEffect>(); // move to Init?
        Init();
	}

    protected virtual void Init() {
        tile = new Tile(initElement);
        currentState = TileState.Hand;
    }

	public void ChangePos(int col, int row){
		ChangePos(row, col, row, .15f);
	}

    // startrow needed??
	public void ChangePos(int startRow, int col, int row, float duration){
		StartCoroutine(_ChangePos(startRow, col, row, duration, "x"));
	}

	public IEnumerator _ChangePos(int startRow, int col, int row, float duration, string anim){
		tile.SetPos(col, row);
		mm.hexGrid.SetTileBehavAt (this, col, row);
		inPos = false;

        switch (anim) { // not great, but simple...
            case "raise":
                yield return mm.animCont._UpwardInsert(this);
                break;
            default:
                yield return mm.animCont._MoveTile(this, startRow, duration);
                break;
        }

		mm.audioCont.DropSound(GetComponent<AudioSource>());
		inPos = true;
	}

	public void HardSetPos(int col, int row){ // essentially a "teleport"
		tile.SetPos(col, row);
        mm.hexGrid.HardSetTileBehavAt (this, col, row);
		transform.position = mm.hexGrid.GridCoordToPos (col, row);
	}

	public void SetPlaced(){
		currentState = TileState.Placed;
	}

    // delete?
	//public void FlipTile(){
	//	SpriteRenderer rend = gameObject.GetComponent<SpriteRenderer> ();
	//	Sprite temp = rend.sprite;
	//	rend.sprite = flipSprite;
	//	flipSprite = temp;
	//	if (currentState == TileState.Hand)
	//		currentState = TileState.Flipped;
	//	else if (currentState == TileState.Flipped)
	//		currentState = TileState.Hand;
	//}

	public void SetOutOfPosition(){
		inPos = false;
	}

	public bool IsInPosition(){
		return inPos;
	}

    public bool CanSetEnch(Enchantment.EnchType type) {
        if (type == GetEnchType()) {
            Debug.Log("TILEBEHAV: Tile has same enchant: " + type.ToString());
            return false;
        }

        return (int)type >= GetEnchTier() && ableTarget;
    }

	public bool HasEnchantment(){ // just use GetEnchType?
		return enchantment != null;
	}

    public Enchantment.EnchType GetEnchType() {
        if (HasEnchantment())
            return enchantment.enchType;
        else
            return Enchantment.EnchType.None;
    }

    int GetEnchTier() {
        if (HasEnchantment())
            return (int)enchantment.enchType;
        else
            return 0;
    }

    public IEnumerator TriggerEnchantment() {
        Debug.Log("TILEBEHAV: Triggering enchantment at " + tile.col + ", " + tile.row);
        yield return enchantment.TriggerEffect();
    }

	public void ClearEnchantment(bool removeFromList = true){
        Debug.Log("TILEBEHAV: About to remove enchantment...");
        if (HasEnchantment()) {
            Debug.Log("TILEBEHAV: About to remove enchantment with tag " + enchantment.tag);
            if(removeFromList)
                mm.effectCont.RemoveTurnEffect(enchantment);
            enchantment = null;
            this.GetComponent<SpriteRenderer>().color = Color.white;
        }
	}

	public bool SetEnchantment(Enchantment ench){
		if (!CanSetEnch(ench.enchType)) {
            Debug.LogError("TILEBEHAV: Can't set ench at "+PrintCoord()+"! enchType="+ench.enchType);
			return false;
		}
        if(HasEnchantment())
            ClearEnchantment(); //?

        Debug.Log("TILEBEHAV: About to set ench with tag="+ench.tag);
		ench.SetEnchantee(this);
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
			return true;
		}
		return false;
	}

    public void AddTileEffect(TileEffect te) { tileEffects.Add(te); }

    public void RemoveTileEffect(TileEffect te) { tileEffects.Remove(te); }

    public void ClearTileEffects() {
        foreach (TileEffect te in tileEffects) {
            mm.effectCont.RemoveTurnEffect(te);
        }
    }

    public string PrintCoord() {
        return "(" + tile.col + ", " + tile.row + ")";
    }

}
