using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRandomDropFive : Charm {

    public Tile.Element elem;

    private const float ANIM_INTERVAL = .1f;

    public override IEnumerator DropEffect() {
        const int count = 5;
        yield return CommonEffects.DropBasicsIntoRandomCols(PlayerId, elem, count);
    }
}
