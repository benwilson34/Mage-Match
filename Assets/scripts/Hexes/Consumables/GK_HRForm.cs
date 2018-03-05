using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GK_HRForm : Consumable {
    public override IEnumerator DropEffect() {
        yield return _mm.targeting.WaitForTileAreaTarget(false);

        foreach (var tb in _mm.targeting.GetTargetTBs()) {
            yield return _mm.hexFX.Ench_SetZombie(_playerId, tb, false);
        }
    }
}
