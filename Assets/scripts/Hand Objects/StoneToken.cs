using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneToken : TileBehav {

    protected override void Init() {
        ableTarget = ableDestroy = ableSwap = ablePrereq = ableTarget = false;
        tile = new Tile(initElement);

        mm.objFX.Ench_SetStoneTok(this);
    }
}
