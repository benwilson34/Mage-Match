using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiveAlarmBell : Charm {
    // TODO lock and key

    public override IEnumerator DropEffect() {
        yield return _mm.targeting.WaitForDragTarget(5);

        int id = TagPlayer(hextag);
        foreach (var tb in _mm.targeting.GetTargetTBs()) {
            _mm.hexFX.Ench_SetBurning(id, tb);
        }

        ThisCharacter().DealDamage(50);
    }
}
