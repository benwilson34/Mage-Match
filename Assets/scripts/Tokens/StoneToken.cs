using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneToken : TileBehav {

    protected override void Init() {
        ableTarget = false; //?
        tile = new Tile(initElement);
        currentState = TileState.Placed; //?

        SpellEffects spellfx = new SpellEffects();
        spellfx.Ench_SetStoneTok(this);
    }
}
