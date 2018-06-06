using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiveAlarmBell : Charm {
    // TODO lock and key

    public override IEnumerator DropEffect() {
        yield return Targeting.WaitForDragTarget(5);

        AudioController.Trigger(AudioController.Rune_NeutralSFX.FiveAlarmBell);

        int id = PlayerId;
        foreach (var tb in Targeting.GetTargetTBs()) {
            _mm.hexFX.Ench_SetBurning(id, tb);
        }

        ThisCharacter().DealDamage(50);
    }
}
