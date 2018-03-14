using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TombstoneTile : TileBehav {

    public override void Init(MageMatch mm) {
        base.Init(mm);
        ableTarget = false; //?
        tile = new Tile(initElement);
        currentState = State.Placed; //?
    }

    public IEnumerator Tombstone_Turn(int id) {
        yield return _mm.syncManager.SyncRand(id, Random.Range(0, 2));
        Tile.Element elem = Tile.Element.None;
        if (_mm.syncManager.GetRand() == 0)
            elem = Tile.Element.Earth;
        else
            elem = Tile.Element.Muscle;

        TileBehav tb = (TileBehav)_mm.hexMan.GenerateBasicTile(id, elem);
        yield return _mm.hexFX.Ench_SetZombie(id, tb, true, false);
        _mm.hexGrid.RaiseTileBehavIntoCell(tb, tile.col, tile.row + 1);
    }

    public IEnumerator Tombstone_TEnd(int id) {
        yield return _mm.hexMan._RemoveTile(this, false); // remove itself
    }
}
