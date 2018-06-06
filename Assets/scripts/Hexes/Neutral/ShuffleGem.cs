using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleGem : Charm {
    public override IEnumerator DropEffect() {
        AudioController.Trigger(AudioController.Rune_NeutralSFX.ShuffleGem);

        const int numSwaps = 2;
        Prompt.SetSwapCount(numSwaps);
        for (int i = 0; i < numSwaps; i++) {
            yield return Prompt.WaitForSwap();
            if (Prompt.WasSuccessful)
                yield return Prompt.ContinueSwap();
        }
        yield return null;
    }
}
