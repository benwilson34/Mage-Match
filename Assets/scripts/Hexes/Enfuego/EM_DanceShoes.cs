using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EM_DanceShoes : Charm {
    public override IEnumerator DropEffect() {
        Prompt.SetSwapCount(1);
        yield return Prompt.WaitForSwap();
        if (!Prompt.WasSuccessful)
            yield break;

        var tbs = Prompt.GetSwapTBs();
        for (int i = 0; i < 2; i++) {
            if(tbs[i].CanSetEnch(Enchantment.Type.Burning))
                yield return _mm.hexFX.Ench_SetBurning(PlayerId, tbs[i]);
        }

        AudioController.Trigger(AudioController.Rune_EnfuegoSFX.DanceShoes);
        yield return Prompt.ContinueSwap();
    }
}
