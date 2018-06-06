using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Molotov : Charm {
    public override IEnumerator DropEffect() {
        const int dmg = 40;
        ThisCharacter().DealDamage(dmg);
        yield return Targeting.WaitForTileAreaTarget(false);

        AudioController.Trigger(AudioController.Rune_NeutralSFX.Molotov);

        var tbs = Targeting.GetTargetTBs();
        tbs = TileFilter.FilterByAbleEnch(tbs, Enchantment.Type.Burning);

        int id = PlayerId;
        foreach (TileBehav tb in tbs) {
            yield return _mm.hexFX.Ench_SetBurning(id, tb);
        }

        yield return _mm.GetPlayer(id).hand._DiscardRandom();
    }
}
