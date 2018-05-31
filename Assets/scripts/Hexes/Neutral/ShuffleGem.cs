using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleGem : Charm {
    public override IEnumerator DropEffect() {
        AudioController.Trigger(AudioController.Rune_NeutralSFX.ShuffleGem);

        const int numSwaps = 2;
        for (int i = 0; i < numSwaps; i++) {
            yield return _mm.prompt.WaitForSwap();
            if (_mm.prompt.WasSuccessful())
                yield return _mm.prompt.ContinueSwap();
        }
        yield return null;
    }
}
