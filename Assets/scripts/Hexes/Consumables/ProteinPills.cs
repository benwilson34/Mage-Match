using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProteinPills : Consumable {
    public override IEnumerator DropEffect() {
        HealthModEffect buffDealing = new HealthModEffect(_playerId, PP_BuffDeal, false, true, 2);
        _mm.effectCont.AddHealthEffect(buffDealing, "PPdea");
        HealthModEffect buffReceiving = new HealthModEffect(_playerId, PP_BuffDeal, false, false, 2);
        _mm.effectCont.AddHealthEffect(buffReceiving, "PPrec");

        yield return null;
    }

    float PP_BuffDeal(Player p, int dmg) {
        return 1.15f; // +15% dmg dealt
    }

    float PP_BuffRec(Player p, int dmg) {
        return .85f; // -15% dmg received
    }
}
