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

    public abstract IEnumerator TriggerEffect();
    public abstract IEnumerator ResolveEffect();
    public abstract IEnumerator EndEffect();
    public virtual void CancelEffect() { } // IEnumerator??
    public int TurnsRemaining() { return turnsLeft; }
    //public void SetPriority(int p) { priority = p; }
}

public class TurnEffect : Effect {

    private MyEffect turnEffect, endEffect, cancelEffect;

    // TODO add infinite Constructor
    public TurnEffect(int turns, MyEffect turnEffect, MyEffect endEffect, MyEffect cancelEffect) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = mm.ActiveP().id;
        turnsLeft = turns;
        this.turnEffect = turnEffect;
        this.endEffect = endEffect;
        this.cancelEffect = cancelEffect;
    }

    public override IEnumerator ResolveEffect() {
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

    public delegate IEnumerator MyTileEffect(int id, TileBehav tb); // TODO IEnumerator

    public enum EnchType { None, Cherrybomb, Burning, Zombify, ZombieTok, StoneTok }
    public EnchType type = EnchType.None; // private?
    public int tier;

    private MyTileEffect turnEffect, endEffect, cancelEffect;
    private TileBehav enchantee;
    private bool skip = false;

    public Enchantment(int id, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = id; // NO!!!
        turnsLeft = -1;
        this.turnEffect = turnEffect;
        this.endEffect = endEffect;
        this.cancelEffect = cancelEffect;
    }

    public Enchantment(int id, int turns, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = mm.ActiveP().id;
        turnsLeft = turns;
        this.turnEffect = turnEffect;
        this.endEffect = endEffect;
        this.cancelEffect = cancelEffect;
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

    public override IEnumerator ResolveEffect() {
        turnsLeft--;
        if (skip) {
            skip = false;
            //return false; //?
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

    private MyEffect matchEffect, endEffect;
    private int count = 0; // -1?

    public MatchEffect(int turns, int count, MyEffect matchEffect, MyEffect endEffect) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = mm.ActiveP().id;
        turnsLeft = turns; // maybe this field should just get renamed to count?
        this.count = count;
        this.matchEffect = matchEffect;
        this.endEffect = endEffect;
    }

    public override IEnumerator TriggerEffect() {
        throw new NotImplementedException(); // hmmm...
    }

    public IEnumerator TriggerEffect(int id, int[] lens) {
        if (matchEffect != null)
            yield return matchEffect(playerID);
        yield return null;
    }

    public override IEnumerator ResolveEffect() {
        TriggerEffect();
        turnsLeft--;
        if (turnsLeft != 0) {
            //return false;
            yield return null; //?
        } else {
            yield return EndEffect();
            //return true;
        }
    }

    public override IEnumerator EndEffect() {
        if (endEffect != null)
            yield return endEffect(playerID);
    }
}

public class SwapEffect : Effect {

    public delegate IEnumerator MySwapEffect(int id, int c1, int r1, int c2, int r2);
    private MySwapEffect swapEffect, endEffect;
    private int count = 0; // -1?

    public SwapEffect(int turns, int count, MySwapEffect swapEffect, MySwapEffect endEffect) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        playerID = mm.ActiveP().id;
        turnsLeft = turns;
        this.count = count;
        this.swapEffect = swapEffect;
        this.endEffect = endEffect;
    }

    public override IEnumerator TriggerEffect() {
        throw new NotImplementedException(); // hmmm...
    }

    public IEnumerator TriggerEffect(int c1, int r1, int c2, int r2) {
        if (swapEffect != null) {
            Debug.Log("SWAPEFFECT: about to trigger swapEffect!");
            yield return swapEffect(mm.ActiveP().id, c1, r1, c2, r2);
            count--;
        }
        yield return null;
    }

    public override IEnumerator ResolveEffect() { //?
        throw new NotImplementedException(); 
    }

    public override IEnumerator EndEffect() { //?
        throw new NotImplementedException();
    }
}