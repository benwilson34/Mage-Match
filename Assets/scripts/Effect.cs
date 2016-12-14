using UnityEngine;
using System.Collections;
using System;

public abstract class Effect {

    public delegate void MyEffect(int id); // move to TurnEffect?

    protected int turnsLeft;
    protected int playerID;
    public int priority; // protected eventually?

    public abstract void TriggerEffect();
    public abstract bool ResolveEffect();
    public abstract void EndEffect();
    public virtual void CancelEffect() { }
    public int TurnsRemaining() { return turnsLeft; }
    //public void SetPriority(int p) { priority = p; }
}

public class TurnEffect : Effect {

    private MyEffect turnEffect, endEffect, cancelEffect;

    // TODO add infinite Constructor
    public TurnEffect(int turns, MyEffect turnEffect, MyEffect endEffect, MyEffect cancelEffect) {
        playerID = MageMatch.ActiveP().id;
        turnsLeft = turns;
        this.turnEffect = turnEffect;
        this.endEffect = endEffect;
        this.cancelEffect = cancelEffect;
    }

    public override void TriggerEffect() {
        if(turnEffect != null)
            turnEffect(playerID);
    }

    public override bool ResolveEffect() {
        turnsLeft--;
        if (turnsLeft != 0) {
            TriggerEffect();
            return false;
        } else {
            EndEffect();
            return true;
        }
    }

    public override void EndEffect() {
        if (endEffect != null)
            endEffect(playerID);
    }

    public override void CancelEffect() {
        if (cancelEffect != null)
            cancelEffect(playerID);
    }
}

public class Enchantment : Effect {

    public delegate void MyTileEffect(int id, TileBehav tb);

    public enum EnchType { None, Cherrybomb, Burning, Zombify, ZombieTok }
    public EnchType type = EnchType.None; // private?

    private MyTileEffect turnEffect, endEffect, cancelEffect;
    private TileBehav enchantee;
    private bool skip = false;
    private int tier;

    public Enchantment(MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect) {
        playerID = MageMatch.ActiveP().id;
        turnsLeft = -1;
        this.turnEffect = turnEffect;
        this.endEffect = endEffect;
        this.cancelEffect = cancelEffect;
    }

    public Enchantment(int turns, MyTileEffect turnEffect, MyTileEffect endEffect, MyTileEffect cancelEffect) {
        playerID = MageMatch.ActiveP().id;
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

    // TODO test
    public override void TriggerEffect() {
        if(turnEffect != null)
            turnEffect(playerID, enchantee);
    }

    public override bool ResolveEffect() {
        turnsLeft--;
        if (skip) {
            skip = false;
            return false;
        }
        if (turnsLeft != 0) {
            TriggerEffect();
            return false;
        } else {
            EndEffect();
            return true;
        }
    }

    public override void EndEffect() {
        if (endEffect != null)
            endEffect(playerID, enchantee);
    }

    public override void CancelEffect() {
        if (cancelEffect != null)
            cancelEffect(playerID, enchantee);
    }

}

public class MatchEffect : Effect {

    private MyEffect matchEffect, endEffect;

    public MatchEffect(int turns, MyEffect matchEffect, MyEffect endEffect) {
        playerID = MageMatch.ActiveP().id;
        turnsLeft = turns;
        this.matchEffect = matchEffect;
        this.endEffect = endEffect;
    }

    public override void TriggerEffect() {
        if (matchEffect != null)
            matchEffect(playerID);
    }

    public override bool ResolveEffect() {
        TriggerEffect();
        turnsLeft--;
        if (turnsLeft != 0) {
            return false;
        } else {
            EndEffect();
            return true;
        }
    }

    public override void EndEffect() {
        if (endEffect != null)
            endEffect(playerID);
    }
}