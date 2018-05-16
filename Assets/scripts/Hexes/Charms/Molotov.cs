using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Molotov : Charm {
    public override IEnumerator DropEffect() {
        ThisCharacter().DealDamage(30);
        yield return _mm.targeting.WaitForTileAreaTarget(false);

        _mm.audioCont.Trigger(AudioController.Rune_NeutralSFX.Molotov);

        int id = TagPlayer(hextag);
        var tbs = _mm.targeting.GetTargetTBs();
        tbs = TileFilter.FilterByAbleEnch(tbs, Enchantment.Type.Burning);
        foreach (TileBehav tb in tbs) {
            yield return _mm.hexFX.Ench_SetBurning(id, tb);
        }

        _mm.GetPlayer(id).DiscardRandom(1);
    }
}
