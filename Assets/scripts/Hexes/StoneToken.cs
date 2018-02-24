using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneToken : TileBehav {

    protected override void Init() {
        ableTarget = ableDestroy = ableSwap = ablePrereq = ableTarget = false;
        tile = new Tile(initElement);

        _mm.hexFX.Ench_SetStone(this);
    }
}
