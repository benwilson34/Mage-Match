using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soulbind : Charm {

    public override void SetInitProps() {
        cost = 3;
    }

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_Neutral.Soulbind);

        const int numTurns = 3;

        TurnEndEffect te = new TurnEndEffect(PlayerId, "Soulbind_Discard", Effect.Behav.None, OnTurnEnd) { turnsLeft = numTurns };
        EffectManager.AddEventEffect(te);

        HandChangeEffect hce = new HandChangeEffect(PlayerId, "Soulbind_Dmg", Effect.Behav.Damage, OnHandChange) { turnsLeft = numTurns };
        EffectManager.AddEventEffect(hce);

        yield return null;
    }

    public IEnumerator OnHandChange(HandChangeEventArgs args) {
        if (args.state == EventController.HandChangeState.Discard) {
            const int dmgPerDiscard = 7;
            ThisPlayer.Character.DealDamage(dmgPerDiscard);
        }
        yield return null;
    }

    public IEnumerator OnTurnEnd(int id) {
        yield return ThisPlayer.Hand._DiscardRandom();
        yield return Opponent.Hand._DiscardRandom();
    }
}
