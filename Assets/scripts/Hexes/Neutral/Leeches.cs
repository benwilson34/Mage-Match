using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leeches : Charm {

    public override void SetInitProps() {
        cost = 3;
    }

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_Neutral.Leeches);

        EventController.playerHealthChange += Leeches_Buff;
        TurnEffect turn = new TurnEndEffect(PlayerId, "Leeches", Effect.Behav.Healing, null) 
            { turnsLeft = 1 };
        turn.onEndEffect = Leeches_End;
        EffectManager.AddEventEffect(turn);
        yield return null;
    }

    // this isn't a HealthModEffect because it's not modifying the actual value (damage).
    void Leeches_Buff(int id, int amount, int newHealth, bool dealt) {
        if (id == _mm.OpponentId(PlayerId) && dealt) { // if this player dealt dmg
            int healAmt = (int)(amount * .3f);
            ThisPlayer.Character.Heal(healAmt);
        }
    }

    IEnumerator Leeches_End() {
        EventController.playerHealthChange -= Leeches_Buff;
        yield return null;
    }
}
