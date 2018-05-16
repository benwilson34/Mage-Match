﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VA_Bandages : Charm {
    public override IEnumerator DropEffect() {
        yield return _mm.syncManager.SyncRand(_playerId, Random.Range(60, 81));
        int rand = _mm.syncManager.GetRand();
        ThisCharacter().Heal(rand);

        _mm.audioCont.Trigger(AudioController.Rune_ValeriaSFX.Bandages);

        // mult, receiving
        HealthModEffect buff = new HealthModEffect(_playerId, Bandages_HE, false, true, 2);
        _mm.effectCont.AddHealthEffect(buff, "Bandg");
    }

    public float Bandages_HE(Player p, int dmg) {
        return .90f;  // -10% dmg recieved
    }
}
