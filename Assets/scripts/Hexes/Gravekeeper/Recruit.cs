using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recruit : Charm {

    public override void SetInitProps() {
        cost = 2;
    }

    public override IEnumerator DropEffect() {
        yield return Targeting.WaitForTileAreaTarget(false);

        AudioController.Trigger(SFX.Rune_Gravekeeper.Recruit);

        var tbs = Targeting.GetTargetTBs();
        tbs = TileFilter.FilterByAbleEnch(tbs, Enchantment.Type.Zombie);
        foreach (var tb in tbs) {
            if (tb.tile.IsElement(Tile.Element.Muscle))
                yield return Zombie.Set(PlayerId, tb);
        }
    }
}
