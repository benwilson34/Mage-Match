using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO rename to TombstoneToken
public class ZombieToken : TileBehav {

    protected override void Init() {
        ableTarget = false; //?
        tile = new Tile(initElement);
        currentState = TileState.Placed; //?

        //SpellEffects spellfx = new SpellEffects();
        //spellfx.Ench_SetZombieTok(mm.ActiveP().id, this); // not sure about the activep here...
    }

    public IEnumerator Tombstone_Turn(int id) {
        yield return mm.syncManager.SyncRand(id, Random.Range(0, 2));
        Tile.Element elem = Tile.Element.None;
        if (mm.syncManager.GetRand() == 0)
            elem = Tile.Element.Earth;
        else
            elem = Tile.Element.Muscle;

        TileBehav tb = mm.GenerateTile(elem).GetComponent<TileBehav>();
        mm.spellfx.Ench_SetZombify(id, tb, true);
        mm.hexGrid.RaiseTileBehavIntoCell(tb, tile.col, tile.row + 1);
    }

    public IEnumerator Tombstone_TEnd(int id) {
        Destroy(this.gameObject);
        yield return null;
    }
}
