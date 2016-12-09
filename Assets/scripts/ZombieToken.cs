using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieToken : TileBehav {

    protected override void Init() {
        ableTarget = false; //?
        tile = new Tile(initElement);
        currentState = TileState.Placed; //?

        SpellEffects spellfx = new SpellEffects();
        SpellEffects.Init();
        spellfx.Ench_SetZombieTok(this);
    }
}
