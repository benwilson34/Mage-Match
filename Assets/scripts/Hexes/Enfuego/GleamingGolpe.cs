using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GleamingGolpe : Charm {
    public override IEnumerator DropEffect() {
        Prompt.SetSwapCount(1);
        yield return Prompt.WaitForSwap();
        if (!Prompt.WasSuccessful)
            yield break;

        var tbs = Prompt.GetSwapTBs();
        for (int i = 0; i < 2; i++) {
            if (tbs[i].CanSetEnch(Enchantment.Type.Burning))
                yield return Burning.Set(PlayerId, tbs[i]);
        }

        AudioController.Trigger(SFX.Rune_Enfuego.GleamingGolpe);
        yield return Prompt.ContinueSwap();
    }
}
