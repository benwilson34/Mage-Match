using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sanctuary : Charm {

    public override void SetInitProps() {
        cost = 3;
    }

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_Neutral.Sanctuary);

        // not the best impl but it may work for now
        HealthModEffect he = new HealthModEffect(PlayerId, "Sanctuary_Invul", Invul, HealthModEffect.Type.ReceivingPercent) { turnsLeft = 2 };
        EffectManager.AddHealthMod(he);
        yield return null;
    }

    public float Invul(Player p, int dmg) {
        return 0; // change to 0% dmg
    }
}
