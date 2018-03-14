using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Molotov : Charm {
    public override IEnumerator DropEffect() {
        ThisCharacter().DealDamage(30);
        yield return _mm.targeting.WaitForTileAreaTarget(false);

        int id = TagPlayer(hextag);
        foreach (TileBehav tb in _mm.targeting.GetTargetTBs()) {
            _mm.hexFX.Ench_SetBurning(id, tb);
        }

        _mm.GetPlayer(id).DiscardRandom(1);
    }
}
