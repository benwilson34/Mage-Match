using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingHands : Charm {

    public override void SetInitProps() {
        SetDuplicate();
    }

    public override IEnumerator DropEffect() {
        yield return _mm.syncManager.SyncRand(PlayerId, Random.Range(30, 41));
        int rand = _mm.syncManager.GetRand();
        ThisPlayer.Character.Heal(rand);

        AudioController.Trigger(SFX.Rune_Valeria.HealingHands);

        // mult, receiving
        HealthModEffect buff = new HealthModEffect(PlayerId, "HealingHands", Buff_Rec, HealthModEffect.Type.ReceivingPercent) { turnsLeft = 4 };
        EffectManager.AddHealthMod(buff);
    }

    float Buff_Rec(Player p, int dmg) {
        const float lessPercentRec = .15f;
        return 1 - lessPercentRec;  // -15% dmg recieved
    }
}
