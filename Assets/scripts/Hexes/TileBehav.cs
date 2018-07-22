using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class TileBehav : Hex {

	public Tile tile;
	public Tile.Element[] initElements;

    // all these should be auto?
	public bool ableSwap = true, ableGrav = true, ableDestroy = true;
    public bool ableInvoke = true, ableTarget = true, ableEnchant = true;
    public bool wasInvoked = false;
    public bool HasEnchantment { get { return _enchantment != null; } }

	protected Enchantment _enchantment;
    protected TileEffect _tileEffect; // just one?
	protected bool _inPos = true; // auto?

    public override void Init(MageMatch mm) {
        base.Init(mm);
        //_tileEffect = new List<TileEffect>();
        tile = new Tile(initElements);
        state = State.Hand;
    }


    #region ---------- POSITION ----------
    public void ChangePos(int col, int row){
        StartCoroutine(_ChangePos(col, row));
    }

	public IEnumerator _ChangePos(int col, int row, float duration = .15f, string anim = ""){
		tile.SetPos(col, row);
		HexGrid.SetTileBehavAt (this, col, row); // i hate this being here...
		_inPos = false;

        switch (anim) { // not great, but simple...
            case "raise":
                yield return AnimationController._UpwardInsert(this);
                break;
            default:
                yield return AnimationController._MoveTile(this, duration);
                break;
        }

		_inPos = true;
	}

    public void ChangePosAndDrop(int startRow, int col, int row, float duration) {
        StartCoroutine(_ChangePosAndDrop(startRow, col, row, duration));
    }

    public IEnumerator _ChangePosAndDrop(int startRow, int col, int row, float duration = .15f) {
        tile.SetPos(col, row);
		HexGrid.SetTileBehavAt (this, col, row); // i hate this being here...
		_inPos = false;

        yield return AnimationController._MoveTileAndDrop(this, startRow, duration);

		_inPos = true;
    }

	public void HardSetPos(int col, int row){ // essentially a "teleport"
		tile.SetPos(col, row);
        HexGrid.HardSetTileBehavAt (this, col, row);
		transform.position = HexGrid.GridCoordToPos (col, row);
        SetPlaced();
	}

	public void SetPlaced(){
		state = State.Placed;
	}

	public void SetOutOfPosition(){
		_inPos = false;
	}

	public bool IsInPosition(){
		return _inPos;
	}
    #endregion


    #region ---------- ENCHANTMENTS ----------
    public bool CanSetEnch(Enchantment.Type type) {
        if (type == GetEnchType()) {
            MMLog.Log_TileBehav("Tile has same enchant: " + type.ToString());
            return false;
        }

        // TODO handle ableEnchant

        return ableEnchant;
    }

    public Enchantment.Type GetEnchType() {
        if (HasEnchantment)
            return _enchantment.enchType;
        else
            return Enchantment.Type.None;
    }

	public void ClearEnchantment(){
        MMLog.Log_TileBehav("About to remove enchantment...");
        if (HasEnchantment) {
            MMLog.Log_TileBehav("About to remove enchantment with tag " + _enchantment.tag);
            EffectManager.RemoveTileEffect(_enchantment);
            _enchantment = null;
            this.GetComponent<SpriteRenderer>().color = Color.white;
        }
	}

	public bool SetEnchantment(Enchantment ench){
		if (!CanSetEnch(ench.enchType)) {
            MMLog.LogError("TILEBEHAV: Can't set ench at "+PrintCoord()+"! enchType="+ench.enchType);
			return false;
		}
        if(HasEnchantment)
            ClearEnchantment(); // needed to remove ench from EffectManager

        MMLog.Log_TileBehav("About to set ench with tag="+ench.tag);
		_enchantment = ench; 
		return true;
	}

    public Enchantment GetEnchantment() { return _enchantment; }

//	public bool ResolveEnchantment(){
//		if (HasEnchantment && currentState == State.Placed) {
////			Debug.Log ("About to resolve enchant: " + (this.enchantEffect != null));
////			resolved = true;
//			currentState = State.Removed;
////			this.enchantEffect (this);
//			_enchantment.CancelEffect();
//			return true;
//		}
//		return false;
//	}
    #endregion


    #region ---------- TILE EFFECTS ----------
    // Maybe not needed?

    public void AddTileEffect(TileEffect te) {
        EffectManager.AddTileEffect(te);
        //_tileEffects.Add(te);
        _tileEffect = te;
    }

    //public void RemoveTileEffect(TileEffect te) {
    //    EffectManager.RemoveTileEffect(te);
        //_tileEffects.Remove(te);
    //}

    public void ClearTileEffect() {
        //foreach (TileEffect te in _tileEffect) {
        if(_tileEffect != null)
            EffectManager.RemoveTileEffect(_tileEffect);
        //}
        //_tileEffects.Clear();
        _tileEffect = null;
    }
    #endregion


    public string PrintCoord() {
        return tile.PrintCoord();
    }

    public override string GetTooltipInfo() {
        string title = Title;
        RuneInfoLoader.RuneInfo info = RuneInfoLoader.GetPlayerRuneInfo(PlayerId, Title);

        string ench = "";
        if (HasEnchantment)
            ench = "\nThis tile is enchanted with " + GetEnchType().ToString();
        return GetTooltipInfo(info.title, "Tile", info.cost, info.desc + ench);
    }

}
