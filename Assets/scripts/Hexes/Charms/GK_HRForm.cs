using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GK_HRForm : Charm {
    public override IEnumerator DropEffect() {
        yield return _mm.targeting.WaitForTileAreaTarget(false);

        var tbs = _mm.targeting.GetTargetTBs();
        tbs = TileFilter.FilterByAbleEnch(tbs, Enchantment.Type.Zombie);
        foreach (var tb in tbs) {
            if(tb.tile.element == Tile.Element.Muscle)
                yield return _mm.hexFX.Ench_SetZombie(_playerId, tb, false);
        }
    }
}
