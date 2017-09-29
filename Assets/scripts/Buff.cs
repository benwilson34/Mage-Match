using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff {

    public enum Type { Dmg_Bonus, Dmg_Mult, Dmg_Extra };
    public Type type;

    public delegate int Additional(Player p);
    private Additional additional;
    private float multiplier;

    public Buff() { } //?

    public void SetAdditional(Additional additional, bool isBuff) {
        if(isBuff)
            type = Type.Dmg_Bonus;
        else
            type = Type.Dmg_Extra;
        this.additional = additional;
    }

    public void SetMultiplier(float multiplier) {
        type = Type.Dmg_Mult;
        this.multiplier = multiplier;
    }

    public int GetModifiedValue(int dmg, Player p) {
        if (type == Type.Dmg_Mult)
            return (int)(dmg * multiplier);
        else
            return additional(p);
    }
}
