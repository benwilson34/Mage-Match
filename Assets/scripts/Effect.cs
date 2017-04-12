using UnityEngine;
using System.Collections;
using System;

public abstract class Effect {

    public delegate IEnumerator MyEffect(int id); // move to TurnEffect?

    protected MageMatch mm;
    protected int turnsLeft; // maybe this field should just get renamed to count?
    public int playerID;
    public int priority; // protected eventually?
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
    public TurnEffect(int turns, MyEffect turnEffect, MyEffect endEffect, MyEffect cancelEffect) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = mm.ActiveP().id;
        turnsLeft = turns;
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

public class Enchantment : Effect {

    public delegate IEnumerator MyTileEffect(int id, TileBehav tb);

    public enum EnchType { None, Cherrybomb, Burning, Zombify, ZombieTok, StoneTok }
    public EnchType type = EnchType.None; // private?
    public int tier;

    private MyTileEffect turnEffect, endEffect, cancelEffect;
    private TileBehav enchantee;
    private SpellEffects spellfx;
    private bool skip = false;

    //public Enchantment(int id, EnchType type) {
    //    spellfx = mm.spellfx;
    //    this.type = type;
    //    switch (type) {
    //        case EnchType.Burning:
    //            tier = 1;
    //            break;
    //        case EnchType.Zombify:
    //            tier = 1;
    //            new Enchantment(id, spellfx.Ench_Zombify_TEffect, null, null);
    //            break;
    //        case EnchType.Cherrybomb:
    //            tier = 2;
    //            break;
    //        case EnchType.StoneTok:
    //            tier = 3; //?
    //            break;
    //    }
    //}

    public Enchantment(int id, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id; // NO!!!
        turnsLeft = -1;
        this.turnEffect = turnEffect;
        this.endEffect = endEffect;
        this.cancelEffect = cancelEffect;
    }

    public Enchantment(int id, int turns, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect) :this(id, turnEffect, endEffect, cancelEffect) {
        turnsLeft = turns;
    }

    public void SetAsEnchantment(TileBehav tb) {
        enchantee = tb;
    }

    public TileBehav GetEnchantee() {
        return enchantee;
    }

    public void SetTypeTier(EnchType type, int tier) {
        this.type = type;
        this.tier = tier;
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

    // TODO test
    public override IEnumerator TriggerEffect() {
        if(turnEffect != null)
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

public class MatchEffect : Effect {

    // own delegate? if more params are needed
    private MyEffect matchEffect, endEffect; // needs endEffect?
    public int countLeft = 0; // -1?

    public MatchEffect(int turns, int count, MyEffect matchEffect, MyEffect endEffect) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = mm.ActiveP().id; // is this ok? pass in?
        turnsLeft = turns;
        this.countLeft = count;
        this.matchEffect = matchEffect;
        this.endEffect = endEffect;
    }

    public override IEnumerator TriggerEffect() {
        if (matchEffect != null)
            yield return matchEffect(playerID);
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

    public SwapEffect(int turns, int count, MySwapEffect swapEffect, MySwapEffect endEffect) {
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
            Debug.Log("SWAPEFFECT: about to trigger swapEffect!");
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