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
    //public void SetPriority(int p) { priority = p; }
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
    private SpellEffects spellfx;
    private bool skip = false;

    public TileEffect(int id, int turns, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) : this(id, type, turnEffect, endEffect, cancelEffect) {
        turnsLeft = turns;
    }

    public TileEffect(int id, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id; // NO!!!
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
            //return false; //?
            yield break;
        }
        if (turnsLeft != 0) {
            yield return TriggerEffect();
            //return false;
        } else {
            yield return EndEffect();
            //return true;
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

    public enum EnchType { None = 0, Burning = 1, Zombify = 1, Cherrybomb = 2, ZombieTok = 3, StoneTok = 3 }
    public EnchType enchType; // private?

    private MyTileEffect turnEffect, endEffect, cancelEffect;
    private TileBehav enchantee;
    private SpellEffects spellfx;
    private bool skip = false;

    public Enchantment(int id, int turns, EnchType enchType, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) :this(id, enchType, type, turnEffect, endEffect, cancelEffect) {
        turnsLeft = turns;
    }

    public Enchantment(int id, EnchType enchType, Type type, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect = null) :base(id, type, turnEffect, endEffect) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id; // NO!!!
        turnsLeft = -1;
        this.enchType = enchType;
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



public class SwapEffect : Effect {

    public delegate IEnumerator MySwapEffect(int id, int c1, int r1, int c2, int r2);
    private MySwapEffect swapEffect, endEffect;
    public int countLeft = -1;

    public SwapEffect(int turns, MySwapEffect swapEffect, MySwapEffect endEffect, int count = -1) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = mm.ActiveP().id;
        turnsLeft = turns;
        this.countLeft = count;
        this.swapEffect = swapEffect;
        this.endEffect = endEffect;
    }

    public override IEnumerator TriggerEffect() {
        throw new NotImplementedException(); // hmmm...should be virtual in Effect?
    }

    public IEnumerator TriggerEffect(int c1, int r1, int c2, int r2) {
        if (swapEffect != null) { // would it ever be? maybe if like, your third swap does something
            MMLog.Log("SWAPEFFECT", "black", "About to trigger swapEffect!");
            yield return swapEffect(mm.ActiveP().id, c1, r1, c2, r2);
            countLeft--;
        }
        yield return null;
    }

    public override IEnumerator Turn() { //?
        turnsLeft--;
        if (turnsLeft == 0) {
            yield return EndEffect(); //???
        }
    }

    public IEnumerator EndEffect(int c1, int r1, int c2, int r2) { //?
        if (endEffect != null)
            yield return endEffect(mm.ActiveP().id, c1, r1, c2, r2);
        yield return null;
    }

    public override bool NeedRemove() { return turnsLeft == 0 || countLeft == 0; }
}