using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EM_DanceShoes : Charm {
    public override IEnumerator DropEffect() {
        yield return _mm.prompt.WaitForSwap();
        if (!_mm.prompt.WasSuccessful())
            yield break;

        var tbs = _mm.prompt.GetSwapTBs();
        for (int i = 0; i < 2; i++) {
            if(tbs[i].CanSetEnch(Enchantment.Type.Burning))
                yield return _mm.hexFX.Ench_SetBurning(TagPlayer(hextag), tbs[i]);
        }
        yield return _mm.prompt.ContinueSwap();
    }
}
