using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leeches : Charm {
    public override IEnumerator DropEffect() {
        _mm.eventCont.playerHealthChange += Leeches_Buff;
        TurnEffect turn = new TurnEffect(_playerId, 1, Effect.Type.Healing, null, Leeches_End);
        _mm.effectCont.AddEndTurnEffect(turn, "Leech");
        yield return null;
    }

    void Leeches_Buff(int id, int amount, int newHealth, bool dealt) {
        if (id == _mm.OpponentId(_playerId) && dealt) { // if this player dealt dmg
            int heal = (int)(amount * .2f);
            ThisCharacter().Heal(heal);
        }
    }

    IEnumerator Leeches_End(int id) {
        _mm.eventCont.playerHealthChange -= Leeches_Buff;
        yield return null;
    }
}
