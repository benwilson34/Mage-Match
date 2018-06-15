using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorchingSpin : Charm {

    public override void SetInitProps() {
        var tbs = TileFilter.GetTilesByEnch(Enchantment.Type.Burning);
        const int quickdrawBurningCount = 9;
        if (tbs.Count > quickdrawBurningCount)
            SetQuickdraw();
    }

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_Enfuego.ScorchingSpin);

        _mm.GetPlayer(PlayerId).IncreaseAP();
        const int dmg = 30;
        ThisPlayer.Character.DealDamage(dmg);
        yield return null;
    }
}
