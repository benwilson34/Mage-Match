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
        for(int i = 0; i < tbs.Count; i++) {
            var tb = tbs[i];
            if (!tb.tile.IsElement(Tile.Element.Muscle)) {
                tbs.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < tbs.Count; i++) {
            if (i == tbs.Count - 1)
                yield return Zombie.Set(PlayerId, tbs[i]);
            else
                StartCoroutine(Zombie.Set(PlayerId, tbs[i]));
        }
    }
}
