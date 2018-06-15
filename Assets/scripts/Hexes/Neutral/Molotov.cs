using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Molotov : Charm {

    public override void SetInitProps() {
        cost = 2;
    }

    public override IEnumerator DropEffect() {
        const int dmg = 40;
        ThisPlayer.Character.DealDamage(dmg);
        yield return Targeting.WaitForTileAreaTarget(false);

        AudioController.Trigger(SFX.Rune_Neutral.Molotov);

        var tbs = Targeting.GetTargetTBs();
        tbs = TileFilter.FilterByAbleEnch(tbs, Enchantment.Type.Burning);

        int id = PlayerId;
        foreach (TileBehav tb in tbs) {
            yield return Burning.Set(id, tb);
        }

        yield return _mm.GetPlayer(id).Hand._DiscardRandom();
    }
}
