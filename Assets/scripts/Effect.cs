using UnityEngine;
using System.Collections;
using System;
using MMDebug;

public abstract class Effect {

    public delegate IEnumerator MyEffect(int id); // move to TurnEffect?
    // rename to EffType?
    public enum Type { None = 0, Cooldown, Damage, Healing, Buff, Destruct, Subtract, Add, Enchant, Movement }

    protected MageMatch mm;
    protected int turnsLeft;
    public int playerID;
    public Type type; // protected eventually?
    public string tag;

    public virtual IEnumerator TriggerEffect() { yield return null; }
    public virtual IEnumerator Turn() { yield return null; }
    public virtual IEnumerator EndEffect() { yield return null; }
    public virtual void CancelEffect() { } // IEnumerator??
    public int TurnsLeft() { return turnsLeft; }
    public virtual bool NeedRemove() { return turnsLeft == 0; }
}



public class TurnEffect : Effect {

    private MyEffect turnEffect, endEffect, cancelEffect;

    // TODO add infinite Constructor...or just pass in a negative for turns?
    public TurnEffect(int turns, Type t, MyEffect turnEffect, MyEffect endEffect, MyEffect cancelEffect = null) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = mm.ActiveP().id;
        turnsLeft = turns;
        this.type = t;
        this.turnEffect = turnEffect;
        this.endEffect = endEffect;
        this.cancelEffect = cancelEffect;
    }

    public override IEnumerator Turn() {
        turnsLeft--;
        if (turnsLeft != 0) {
            yield return TriggerEffect();
        } else {
            yield return EndEffect();
        }
    }

    public override IEnumerator TriggerEffect() {
        if(turnEffect != null)
            yield return turnEffect(playerID);
    }

    public override IEnumerator EndEffect() {
        if (endEffect != null)
            yield return endEffect(playerID);
    }

    public override void CancelEffect() {
        if (cancelEffect != null)
            cancelEffect(playerID);
    }
}



public class TileEffect : Effect {

    public delegate IEnumerator MyTileEffect(int id, TileBehav tb);

    private MyTileEffect turnEffect, endEffect, cancelEffect;
    private TileBehav enchantee;
    private ObjectEffects objFX; //?
    protected bool skip = false;

    public TileEffect(int id, int turns, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) : this(id, type, turnEffect, endEffect, cancelEffect) {
        turnsLeft = turns;
    }

    public TileEffect(int id, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id;
        turnsLeft = -1;
        this.type = type;
        this.turnEffect = turnEffect;
        this.endEffect = endEffect;
        this.cancelEffect = cancelEffect;
    }

    public void SetEnchantee(TileBehav tb) {
        enchantee = tb;
    }

    public TileBehav GetEnchantee() {
        return enchantee;
    }

    public void SkipCurrent() {
        skip = true;
    }

    public override IEnumerator Turn() {
        turnsLeft--;
        if (skip) {
            skip = false;
            yield break;
        }
        if (turnsLeft != 0) {
            yield return TriggerEffect();
        } else {
            yield return EndEffect();
        }
    }

    public override IEnumerator TriggerEffect() {
        if (turnEffect != null)
            yield return turnEffect(playerID, enchantee);
    }

    public override IEnumerator EndEffect() {
        if (endEffect != null)
            yield return endEffect(playerID, enchantee);
    }

    public override void CancelEffect() {
        if (cancelEffect != null)
            cancelEffect(playerID, enchantee);
    }

}



public class Enchantment : TileEffect {

    public enum EnchType { None = 0, Burning, Zombify, Cherrybomb, ZombieTok, StoneTok }
    public EnchType enchType; // private?

    private MyTileEffect turnEffect, endEffect, cancelEffect;
    private TileBehav enchantee;
    private ObjectEffects objFX; //?
    private bool hasTurnEffect = false;

    public Enchantment(int id, int turns, EnchType enchType, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) :this(id, enchType, type, turnEffect, endEffect, cancelEffect) {
        turnsLeft = turns;
    }

    public Enchantment(int id, EnchType enchType, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) :base(id, type, turnEffect, endEffect) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id; // NO!!!
        turnsLeft = -1;
        this.enchType = enchType;
    }

    public void TriggerEffectEveryTurn() { hasTurnEffect = true; }

    public override IEnumerator Turn() {
        turnsLeft--;
        if (skip) {
            skip = false;
            yield break;
        }
        if (turnsLeft != 0 && hasTurnEffect) { // important difference here
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



public class MatchEffect : Effect {

    // own delegate? if more params are needed
    private MyEffect matchEffect, endEffect; // needs endEffect?
    public int countLeft = -1;

    public MatchEffect(int turns, MyEffect matchEffect, MyEffect endEffect, int count = -1) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = mm.ActiveP().id; // is this ok? pass in?
        turnsLeft = turns;
        this.countLeft = count;
        this.matchEffect = matchEffect;
        this.endEffect = endEffect;
    }

    public override IEnumerator TriggerEffect() {
        if (matchEffect != null) {
            yield return matchEffect(playerID);
            countLeft--;
        }
        yield return null;
    }

    public override IEnumerator Turn() {
        turnsLeft--;
        if (turnsLeft == 0) {
            yield return EndEffect(); //???
        }
    }

    public override IEnumerator EndEffect() {
        if (endEffect != null)
            yield return endEffect(playerID);
    }

    public override bool NeedRemove() { return turnsLeft == 0 || countLeft == 0; }
}



public class DropEffect : Effect {

    public delegate IEnumerator MyDropEffect(int id, bool playerAction, string tag, int col);
    private MyDropEffect dropEffect;

    public int countLeft = -1;
    public bool isGlobal = false;

    public DropEffect(int id, MyDropEffect dropEffect, int turns = -1, int count = -1) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id;
        turnsLeft = turns;
        this.countLeft = count;
        this.dropEffect = dropEffect;
    }

    public IEnumerator TriggerEffect(bool playerAction, string tag, int col) {
        if (dropEffect != null) { // would it ever be? maybe if like, your third swap does something
            MMLog.Log("DROPEFFECT", "black", "About to trigger dropEffect!");
            yield return dropEffect(mm.ActiveP().id, playerAction, tag, col);
            countLeft--;
        }
        yield return null;
    }

    public override IEnumerator Turn() { //?
        turnsLeft--;
        yield return null;
    }

    public override bool NeedRemove() { return turnsLeft == 0 || countLeft == 0; }
}



public class SwapEffect : Effect {

    public delegate IEnumerator MySwapEffect(int id, int c1, int r1, int c2, int r2);
    private MySwapEffect swapEffect;

    public int countLeft = -1;
    public bool isGlobal = false;

    public SwapEffect(int id, MySwapEffect swapEffect, int turns = -1, int count = -1) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id;
        turnsLeft = turns;
        this.countLeft = count;
        this.swapEffect = swapEffect;
    }

    public IEnumerator TriggerEffect(int c1, int r1, int c2, int r2) {
        if (swapEffect != null) { // would it ever be? maybe if like, your third swap does something
            MMLog.Log("SWAPEFFECT", "black", "About to trigger swapEffect!");
            yield return swapEffect(mm.ActiveP().id, c1, r1, c2, r2); // should be playerID?
            countLeft--;
        }
        yield return null;
    }

    public override IEnumerator Turn() { // this could just be the default in Effect...
        turnsLeft--;
        yield return null;
    }

    public override bool NeedRemove() { return turnsLeft == 0 || countLeft == 0; }
}



public class HealthEffect : Effect {

    public delegate float MyHealthEffect(Player p, int dmg);
    public bool isAdditive;    // additive or multiplicative? could be an enum in time
    public bool isBuff = true; // buff or debuff?
    public int countLeft = -1;

    private MyHealthEffect healthEffect;

    // TODO add infinite Constructor...or just pass in a negative for turns?
    public HealthEffect(int id, MyHealthEffect healthEffect, bool isAdditive, bool isBuff = true, int turns = -1, int count = -1) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id;
        turnsLeft = turns;
        this.type = Type.Buff; //?
        this.healthEffect = healthEffect;
        this.isAdditive = isAdditive;
        this.isBuff = isBuff;
        countLeft = count;
    }

    public override IEnumerator Turn() {
        turnsLeft--;
        yield return null;
    }

    public float GetResult(Player p, int dmg) {
        countLeft--;
        return healthEffect(p, dmg);
    }

    public override bool NeedRemove() { return turnsLeft == 0 || countLeft == 0; }

}