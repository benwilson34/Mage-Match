using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieToken : TileBehav {

    protected override void Init() {
        ableTarget = false; //?
        tile = new Tile(initElement);
        currentState = TileState.Placed; //?

        //SpellEffects spellfx = new SpellEffects();
        //spellfx.Ench_SetZombieTok(mm.ActiveP().id, this); // not sure about the activep here...
    }
}
