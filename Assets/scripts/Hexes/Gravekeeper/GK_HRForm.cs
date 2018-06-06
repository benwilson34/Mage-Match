﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GK_HRForm : Charm {
    public override IEnumerator DropEffect() {
        yield return Targeting.WaitForTileAreaTarget(false);

        AudioController.Trigger(AudioController.Rune_GravekeeperSFX.HRForm);

        var tbs = Targeting.GetTargetTBs();
        tbs = TileFilter.FilterByAbleEnch(tbs, Enchantment.Type.Zombie);
        foreach (var tb in tbs) {
            if(tb.tile.IsElement(Tile.Element.Muscle))
                yield return _mm.hexFX.Ench_SetZombie(PlayerId, tb);
        }
    }
}