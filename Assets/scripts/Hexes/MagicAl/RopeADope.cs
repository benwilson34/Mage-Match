using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeADope : Charm {

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_MagicAl.RopeADope);

        TurnBeginEffect te = new TurnBeginEffect(PlayerId, "RopeADope_AP", Effect.Behav.APChange, OnTurnBegin) { turnsLeft = 2 };
        EffectManager.AddEventEffect(te);

        yield return _mm.GetOpponent(PlayerId).Hand._DiscardRandom();
    }

    public IEnumerator OnTurnBegin(int id) {
        _mm.GetPlayer(id).IncreaseAP(2);
        yield return null;
    }
}
