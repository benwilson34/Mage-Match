using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProteinPills : Charm {
    public override void Init(MageMatch mm) {
        base.Init(mm);
        SetDuplicate();
    }

    public override IEnumerator DropEffect() {
        _mm.audioCont.Trigger(AudioController.Rune_NeutralSFX.ProteinPills);

        HealthModEffect buffDealing = new HealthModEffect(PlayerId, PP_BuffDeal, false, true, 2);
        _mm.effectCont.AddHealthEffect(buffDealing, "PPdea");
        HealthModEffect buffReceiving = new HealthModEffect(PlayerId, PP_BuffDeal, false, false, 2);
        _mm.effectCont.AddHealthEffect(buffReceiving, "PPrec");

        yield return null;
    }

    float PP_BuffDeal(Player p, int dmg) {
        return 1.20f; // +20% dmg dealt
    }

    float PP_BuffRec(Player p, int dmg) {
        return .80f; // -20% dmg received
    }
}
