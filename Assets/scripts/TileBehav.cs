using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class TileBehav : Hex, Tooltipable {

	public Tile tile;
	public Tile.Element initElement;

	public bool ableSwap = true, ableMatch = true, ableGrav = true, ableDestroy = true;
	public bool ablePrereq = true, ableTarget = true;

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
        currentState = State.Hand;
    }

	public void ChangePos(int col, int row){
        StartCoroutine(_ChangePos(col, row));
    }

	public IEnumerator _ChangePos(int col, int row, float duration = .15f, string anim = ""){
		tile.SetPos(col, row);
		mm.hexGrid.SetTileBehavAt (this, col, row); // i hate this being here...
		inPos = false;

        switch (anim) { // not great, but simple...
            case "raise":
                yield return mm.animCont._UpwardInsert(this);
                break;
            default:
                yield return mm.animCont._MoveTile(this, duration);
                break;
        }

		inPos = true;
	}

    public void ChangePosAndDrop(int startRow, int col, int row, float duration) {
        StartCoroutine(_ChangePosAndDrop(startRow, col, row, duration));
    }

    public IEnumerator _ChangePosAndDrop(int startRow, int col, int row, float duration = .15f) {
        tile.SetPos(col, row);
		mm.hexGrid.SetTileBehavAt (this, col, row); // i hate this being here...
		inPos = false;

        yield return mm.animCont._MoveTileAndDrop(this, startRow, duration);

		inPos = true;
    }

	public void HardSetPos(int col, int row){ // essentially a "teleport"
		tile.SetPos(col, row);
        mm.hexGrid.HardSetTileBehavAt (this, col, row);
		transform.position = mm.hexGrid.GridCoordToPos (col, row);
        SetPlaced();
	}

	public void SetPlaced(){
		currentState = State.Placed;
	}

	public void SetOutOfPosition(){
		inPos = false;
	}

	public bool IsInPosition(){
		return inPos;
	}

    public bool CanSetEnch(Enchantment.EnchType type) {
        if (type == GetEnchType()) {
            MMLog.Log_TileBehav("Tile has same enchant: " + type.ToString());
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
        return Enchantment.GetEnchTier(GetEnchType());
    }

    public IEnumerator TriggerEnchantment() {
        MMLog.Log_TileBehav("Triggering enchantment at " + tile.col + ", " + tile.row);
        yield return enchantment.TriggerEffect();
    }

	public void ClearEnchantment(bool removeFromList = true){
        MMLog.Log_TileBehav("About to remove enchantment...");
        if (HasEnchantment()) {
            MMLog.Log_TileBehav("About to remove enchantment with tag " + enchantment.tag);
            if(removeFromList)
                mm.effectCont.RemoveTurnEffect(enchantment);
            enchantment = null;
            this.GetComponent<SpriteRenderer>().color = Color.white;
        }
	}

	public bool SetEnchantment(Enchantment ench){
		if (!CanSetEnch(ench.enchType)) {
            MMLog.LogError("TILEBEHAV: Can't set ench at "+PrintCoord()+"! enchType="+ench.enchType);
			return false;
		}
        if(HasEnchantment())
            ClearEnchantment(); //?

        MMLog.Log_TileBehav("About to set ench with tag="+ench.tag);
		ench.SetEnchantee(this);
		enchantment = ench; 
		return true;
	}

	public bool ResolveEnchantment(){
		if (HasEnchantment() && currentState == State.Placed) {
//			Debug.Log ("About to resolve enchant: " + (this.enchantEffect != null));
//			resolved = true;
			currentState = State.Removed;
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
