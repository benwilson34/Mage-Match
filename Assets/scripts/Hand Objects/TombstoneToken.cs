﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TombstoneToken : TileBehav {

    protected override void Init() {
        ableTarget = false; //?
        tile = new Tile(initElement);
        currentState = State.Placed; //?
    }

    public IEnumerator Tombstone_Turn(int id) {
        yield return mm.syncManager.SyncRand(id, Random.Range(0, 2));
        Tile.Element elem = Tile.Element.None;
        if (mm.syncManager.GetRand() == 0)
            elem = Tile.Element.Earth;
        else
            elem = Tile.Element.Muscle;

        TileBehav tb = (TileBehav)mm.tileMan.GenerateTile(id, elem);
        yield return mm.objFX.Ench_SetZombify(id, tb, true, false);
        mm.hexGrid.RaiseTileBehavIntoCell(tb, tile.col, tile.row + 1);
    }

    public IEnumerator Tombstone_TEnd(int id) {
        yield return mm.tileMan._RemoveTile(tile.col, tile.row, false); // remove itself
    }
}