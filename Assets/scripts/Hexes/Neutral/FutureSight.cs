using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FutureSight : Charm {

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_Neutral.FutureSight);

        _mm.uiCont.ShowAlertText("That charm doesn't do anything right now. Sorry!");
        // TODO need an interface and all that
        yield return null;
    }
}
