using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class TileBehav : Hex {

	public Tile tile;
	public Tile.Element initElement;

    // all these should be auto?
	public bool ableSwap = true, ableGrav = true, ableDestroy = true;
	public bool ableInvoke = true, ableTarget = true;
    public bool wasInvoked = false;

	private Enchantment _enchantment;
    private List<TileEffect> _tileEffects;
	private bool _inPos = true; // auto?

    public override void Init(MageMatch mm) {
        base.Init(mm);
        _tileEffects = new List<TileEffect>(); // move to Init?
        tile = new Tile(initElement);
        currentState = State.Hand;
    }

	public void ChangePos(int col, int row){
        StartCoroutine(_ChangePos(col, row));
    }

	public IEnumerator _ChangePos(int col, int row, float duration = .15f, string anim = ""){
		tile.SetPos(col, row);
		_mm.hexGrid.SetTileBehavAt (this, col, row); // i hate this being here...
		_inPos = false;

        switch (anim) { // not great, but simple...
            case "raise":
                yield return _mm.animCont._UpwardInsert(this);
                break;
            default:
                yield return _mm.animCont._MoveTile(this, duration);
                break;
        }

		_inPos = true;
	}

    public void ChangePosAndDrop(int startRow, int col, int row, float duration) {
        StartCoroutine(_ChangePosAndDrop(startRow, col, row, duration));
    }

    public IEnumerator _ChangePosAndDrop(int startRow, int col, int row, float duration = .15f) {
        tile.SetPos(col, row);
		_mm.hexGrid.SetTileBehavAt (this, col, row); // i hate this being here...
		_inPos = false;

        yield return _mm.animCont._MoveTileAndDrop(this, startRow, duration);

		_inPos = true;
    }

	public void HardSetPos(int col, int row){ // essentially a "teleport"
		tile.SetPos(col, row);
        _mm.hexGrid.HardSetTileBehavAt (this, col, row);
		transform.position = _mm.hexGrid.GridCoordToPos (col, row);
        SetPlaced();
	}

	public void SetPlaced(){
		currentState = State.Placed;
	}

	public void SetOutOfPosition(){
		_inPos = false;
	}

	public bool IsInPosition(){
		return _inPos;
	}

    public bool CanSetEnch(Enchantment.Type type) {
        if (type == GetEnchType()) {
            MMLog.Log_TileBehav("Tile has same enchant: " + type.ToString());
            return false;
        }

        return (int)type >= GetEnchTier() && ableTarget;
    }

	public bool HasEnchantment(){ // just use GetEnchType?
		return _enchantment != null;
	}

    public Enchantment.Type GetEnchType() {
        if (HasEnchantment())
            return _enchantment.enchType;
        else
            return Enchantment.Type.None;
    }

    int GetEnchTier() {
        return Enchantment.GetEnchTier(GetEnchType());
    }

    public IEnumerator TriggerEnchantment() {
        MMLog.Log_TileBehav("Triggering enchantment at " + tile.col + ", " + tile.row);
        yield return _enchantment.TriggerEffect();
    }

	public void ClearEnchantment(bool removeFromList = true){
        MMLog.Log_TileBehav("About to remove enchantment...");
        if (HasEnchantment()) {
            MMLog.Log_TileBehav("About to remove enchantment with tag " + _enchantment.tag);
            if(removeFromList)
                _mm.effectCont.RemoveTurnEffect(_enchantment);
            _enchantment = null;
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
		_enchantment = ench; 
		return true;
	}

	public bool ResolveEnchantment(){
		if (HasEnchantment() && currentState == State.Placed) {
//			Debug.Log ("About to resolve enchant: " + (this.enchantEffect != null));
//			resolved = true;
			currentState = State.Removed;
//			this.enchantEffect (this);
			_enchantment.CancelEffect();
			return true;
		}
		return false;
	}

    public void AddTileEffect(TileEffect te) { _tileEffects.Add(te); }

    public void RemoveTileEffect(TileEffect te) { _tileEffects.Remove(te); }

    public void ClearTileEffects() {
        foreach (TileEffect te in _tileEffects) {
            _mm.effectCont.RemoveTurnEffect(te);
        }
    }

    public string PrintCoord() {
        return "(" + tile.col + ", " + tile.row + ")";
    }

    public override string GetTooltipInfo() {
        string title = TagTitle(hextag);
        return GetTooltipInfo(title, "Tile", "");
    }

}
