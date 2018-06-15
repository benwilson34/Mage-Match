using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolster : Charm {
    public override void SetInitProps() {
        SetDuplicate();
    }

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_Neutral.Bolster);

        const int turnCount = 2;
        HealthModEffect buffDealing = new HealthModEffect(PlayerId, "ProteinPills_Deal", Buff_Deal, HealthModEffect.Type.DealingPercent) { turnsLeft = turnCount };
        EffectManager.AddHealthMod(buffDealing);

        HealthModEffect buffReceiving = new HealthModEffect(PlayerId, "ProteinPills_Rec", Buff_Rec, HealthModEffect.Type.ReceivingPercent) { turnsLeft = turnCount };
        EffectManager.AddHealthMod(buffReceiving);

        yield return null;
    }

    float Buff_Deal(Player p, int dmg) {
        const float morePercent = .20f;
        return 1 + morePercent; // +20% dmg dealt
    }

    float Buff_Rec(Player p, int dmg) {
        const float lessPercent = .20f;
        return 1 - lessPercent; // -20% dmg received
    }
}
