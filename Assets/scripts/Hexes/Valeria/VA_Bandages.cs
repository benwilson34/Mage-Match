using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VA_Bandages : Charm {

    public override void Init(MageMatch mm) {
        base.Init(mm);
        SetDuplicate();
    }

    public override IEnumerator DropEffect() {
        yield return _mm.syncManager.SyncRand(PlayerId, Random.Range(30, 41));
        int rand = _mm.syncManager.GetRand();
        ThisCharacter().Heal(rand);

        AudioController.Trigger(AudioController.Rune_ValeriaSFX.Bandages);

        // mult, receiving
        HealthModEffect buff = new HealthModEffect(PlayerId, Bandages_HE, false, true, 4);
        _mm.effectCont.AddHealthEffect(buff, "Bandg");
    }

    public float Bandages_HE(Player p, int dmg) {
        return .90f;  // -10% dmg recieved
    }
}
