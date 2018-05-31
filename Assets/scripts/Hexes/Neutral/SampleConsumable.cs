﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleConsumable : Charm {
    public override IEnumerator DropEffect() {
        AudioController.Trigger(AudioController.Rune_NeutralSFX.SampleCharm);

        var tbs = _mm.hexGrid.GetPlacedTiles();
        for (int i = 0; i < tbs.Count; i++) {
            _mm.hexMan.RemoveTile(tbs[0].tile, false);
            tbs.RemoveAt(0);
            i--;
            yield return _mm.animCont.WaitForSeconds(.05f);
        }
        yield return null;
    }
}
