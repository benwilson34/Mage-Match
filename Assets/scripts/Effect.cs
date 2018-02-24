using UnityEngine;
using System.Collections;
using System;
using MMDebug;

public abstract class Effect {

    public delegate IEnumerator MyEffect(int id); // move to TurnEffect?
    // rename to EffType?
    public enum Type { None = 0, Cooldown, Damage, Healing, Buff, Destruct, Subtract, Add, Enchant, Movement }

    public int playerID;
    public Type type; // protected eventually?
    public string tag;

    protected MageMatch _mm;
    protected int _turnsLeft;

    public virtual IEnumerator TriggerEffect() { yield return null; }
    public virtual IEnumerator Turn() { yield return null; }
    public virtual IEnumerator EndEffect() { yield return null; }
    public virtual void CancelEffect() { } // IEnumerator??
    public int TurnsLeft() { return _turnsLeft; }
    public virtual bool NeedRemove() { return _turnsLeft == 0; }
}



public class TurnEffect : Effect {

    private MyEffect _turnEffect, _endEffect, _cancelEffect;

    // TODO add infinite Constructor...or just pass in a negative for turns?
    public TurnEffect(int turns, Type t, MyEffect turnEffect, MyEffect endEffect, MyEffect cancelEffect = null) {
        _mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = _mm.ActiveP().id;
        _turnsLeft = turns;
        this.type = t;
        this._turnEffect = turnEffect;
        this._endEffect = endEffect;
        this._cancelEffect = cancelEffect;
    }

    public override IEnumerator Turn() {
        _turnsLeft--;
        if (_turnsLeft != 0) {
            yield return TriggerEffect();
        } else {
            yield return EndEffect();
        }
    }

    public override IEnumerator TriggerEffect() {
        if(_turnEffect != null)
            yield return _turnEffect(playerID);
    }

    public override IEnumerator EndEffect() {
        if (_endEffect != null)
            yield return _endEffect(playerID);
    }

    public override void CancelEffect() {
        if (_cancelEffect != null)
            _cancelEffect(playerID);
    }
}



public class TileEffect : Effect {

    public delegate IEnumerator MyTileEffect(int id, TileBehav tb);

    private MyTileEffect _turnEffect, _endEffect, _cancelEffect;
    private TileBehav _enchantee;
    private ObjectEffects _objFX; //?
    protected bool _skip = false;

    public TileEffect(int id, int turns, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) : this(id, type, turnEffect, endEffect, cancelEffect) {
        _turnsLeft = turns;
    }

    public TileEffect(int id, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) {
        _mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id;
        _turnsLeft = -1;
        this.type = type;
        this._turnEffect = turnEffect;
        this._endEffect = endEffect;
        this._cancelEffect = cancelEffect;
    }

    public void SetEnchantee(TileBehav tb) {
        _enchantee = tb;
    }

    public TileBehav GetEnchantee() {
        return _enchantee;
    }

    public void SkipCurrent() {
        _skip = true;
    }

    public override IEnumerator Turn() {
        _turnsLeft--;
        if (_skip) {
            _skip = false;
            yield break;
        }
        if (_turnsLeft != 0) {
            yield return TriggerEffect();
        } else {
            yield return EndEffect();
        }
    }

    public override IEnumerator TriggerEffect() {
        if (_turnEffect != null)
            yield return _turnEffect(playerID, _enchantee);
    }

    public override IEnumerator EndEffect() {
        if (_endEffect != null)
            yield return _endEffect(playerID, _enchantee);
    }

    public override void CancelEffect() {
        if (_cancelEffect != null)
            _cancelEffect(playerID, _enchantee);
    }

}



public class Enchantment : TileEffect {

    public enum EnchType { None = 0, Burning, Zombify, Cherrybomb, ZombieTok, StoneTok }
    public EnchType enchType; // private?

    private MyTileEffect _turnEffect, _endEffect, _cancelEffect;
    private TileBehav _enchantee;
    private ObjectEffects _objFX; //?
    private bool _hasTurnEffect = false;

    public Enchantment(int id, int turns, EnchType enchType, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) :this(id, enchType, type, turnEffect, endEffect, cancelEffect) {
        _turnsLeft = turns;
    }

    public Enchantment(int id, EnchType enchType, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) :base(id, type, turnEffect, endEffect) {
        _mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id; // NO!!!
        _turnsLeft = -1;
        this.enchType = enchType;
    }

    public void TriggerEffectEveryTurn() { _hasTurnEffect = true; }

    public override IEnumerator Turn() {
        _turnsLeft--;
        if (_skip) {
            _skip = false;
            yield break;
        }
        if (_turnsLeft != 0 && _hasTurnEffect) { // important difference here
            yield return TriggerEffect();
        } else {
            yield return EndEffect();
        }
    }

    public static int GetEnchTier(EnchType enchType) {
        if (enchType == EnchType.None)
            return 0;
        else if ((int)enchType < (int)EnchType.Cherrybomb)
            return 1;
        else
            return 2;
        // TODO handle tier 3 ench (like tombstone...or should those just have the ableEnchant flag off?
    }
}



//public class MatchEffect : Effect {

//    // own delegate? if more params are needed
//    private MyEffect matchEffect, endEffect; // needs endEffect?
//    public int countLeft = -1;

//    public MatchEffect(int turns, MyEffect matchEffect, MyEffect endEffect, int count = -1) {
//        _mm = GameObject.Find("board").GetComponent<MageMatch>();
//        playerID = _mm.ActiveP().id; // is this ok? pass in?
//        _turnsLeft = turns;
//        this.countLeft = count;
//        this.matchEffect = matchEffect;
//        this.endEffect = endEffect;
//    }

//    public override IEnumerator TriggerEffect() {
//        if (matchEffect != null) {
//            yield return matchEffect(playerID);
//            countLeft--;
//        }
//        yield return null;
//    }

//    public override IEnumerator Turn() {
//        _turnsLeft--;
//        if (_turnsLeft == 0) {
//            yield return EndEffect(); //???
//        }
//    }

//    public override IEnumerator EndEffect() {
//        if (endEffect != null)
//            yield return endEffect(playerID);
//    }

//    public override bool NeedRemove() { return _turnsLeft == 0 || countLeft == 0; }
//}



public class DropEffect : Effect {

    public delegate IEnumerator MyDropEffect(int id, bool playerAction, string tag, int col);
    private MyDropEffect _dropEffect;

    public int countLeft = -1;
    public bool isGlobal = false;

    public DropEffect(int id, MyDropEffect dropEffect, int turns = -1, int count = -1) {
        _mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id;
        _turnsLeft = turns;
        this.countLeft = count;
        this._dropEffect = dropEffect;
    }

    public IEnumerator TriggerEffect(bool playerAction, string tag, int col) {
        if (_dropEffect != null) { // would it ever be? maybe if like, your third swap does something
            MMLog.Log("DROPEFFECT", "black", "About to trigger dropEffect!");
            yield return _dropEffect(_mm.ActiveP().id, playerAction, tag, col);
            countLeft--;
        }
        yield return null;
    }

    public override IEnumerator Turn() { //?
        _turnsLeft--;
        yield return null;
    }

    public override bool NeedRemove() { return _turnsLeft == 0 || countLeft == 0; }
}



public class SwapEffect : Effect {

    public delegate IEnumerator MySwapEffect(int id, int c1, int r1, int c2, int r2);
    private MySwapEffect _swapEffect;

    public int countLeft = -1;
    public bool isGlobal = false;

    public SwapEffect(int id, MySwapEffect swapEffect, int turns = -1, int count = -1) {
        _mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id;
        _turnsLeft = turns;
        this.countLeft = count;
        this._swapEffect = swapEffect;
    }

    public IEnumerator TriggerEffect(int c1, int r1, int c2, int r2) {
        if (_swapEffect != null) { // would it ever be? maybe if like, your third swap does something
            MMLog.Log("SWAPEFFECT", "black", "About to trigger swapEffect!");
            yield return _swapEffect(_mm.ActiveP().id, c1, r1, c2, r2); // should be playerID?
            countLeft--;
        }
        yield return null;
    }

    public override IEnumerator Turn() { // this could just be the default in Effect...
        _turnsLeft--;
        yield return null;
    }

    public override bool NeedRemove() { return _turnsLeft == 0 || countLeft == 0; }
}



public class HealthEffect : Effect {

    public delegate float MyHealthEffect(Player p, int dmg);
    public bool isAdditive;    // additive or multiplicative? could be an enum in time
    public bool isBuff = true; // TODO this could safely be refactored to isDealing?
    public int countLeft = -1;

    private MyHealthEffect _healthEffect;

    // TODO add infinite Constructor...or just pass in a negative for turns?
    public HealthEffect(int id, MyHealthEffect healthEffect, bool isAdditive, bool isBuff = true, int turns = -1, int count = -1) {
        _mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id;
        _turnsLeft = turns;
        this.type = Type.Buff; //?
        this._healthEffect = healthEffect;
        this.isAdditive = isAdditive;
        this.isBuff = isBuff;
        countLeft = count;
    }

    public override IEnumerator Turn() {
        _turnsLeft--;
        yield return null;
    }

    public float GetResult(Player p, int dmg) {
        countLeft--;
        return _healthEffect(p, dmg);
    }

    public override bool NeedRemove() { return _turnsLeft == 0 || countLeft == 0; }

}