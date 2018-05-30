using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leeches : Charm {
    public override IEnumerator DropEffect() {
        _mm.audioCont.Trigger(AudioController.Rune_NeutralSFX.Leeches);

        _mm.eventCont.playerHealthChange += Leeches_Buff;
        TurnEffect turn = new TurnEffect(PlayerId, Effect.Type.Healing, null, Leeches_End, 1);
        _mm.effectCont.AddEndTurnEffect(turn, "Leech");
        yield return null;
    }

    // this isn't a HealthModEffect because it's not modifying the actual value (damage).
    void Leeches_Buff(int id, int amount, int newHealth, bool dealt) {
        if (id == _mm.OpponentId(PlayerId) && dealt) { // if this player dealt dmg
            int heal = (int)(amount * .3f);
            ThisCharacter().Heal(heal);
        }
    }

    IEnumerator Leeches_End(int id) {
        _mm.eventCont.playerHealthChange -= Leeches_Buff;
        yield return null;
    }
}
