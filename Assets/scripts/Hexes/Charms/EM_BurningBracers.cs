using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EM_BurningBracers : Charm {

    public override void Init(MageMatch mm) {
        base.Init(mm);

        var tbs = TileFilter.GetTilesByEnch(Enchantment.Type.Burning);
        const int quickdrawBurningCount = 9;
        if (tbs.Count > quickdrawBurningCount)
            SetQuickdraw();
    }

    public override IEnumerator DropEffect() {
        _mm.GetPlayer(TagPlayer(hextag)).AP += 1;
        yield return null;
    }
}
