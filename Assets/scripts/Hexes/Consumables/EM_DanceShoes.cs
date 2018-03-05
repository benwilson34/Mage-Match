﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EM_DanceShoes : Consumable {
    public override IEnumerator DropEffect() {
        yield return _mm.prompt.WaitForSwap(new TileSeq()); // safe?
        var tbs = _mm.prompt.GetSwapTBs();
        _mm.hexFX.Ench_SetBurning(TagPlayer(hextag), tbs[0]);
        _mm.hexFX.Ench_SetBurning(TagPlayer(hextag), tbs[1]);
        yield return _mm.prompt.ContinueSwap();
    }
}
