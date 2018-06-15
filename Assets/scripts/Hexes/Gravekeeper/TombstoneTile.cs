using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TombstoneTile : TileBehav {

    public override void SetInitProps() {
        ableTarget = false; //?
        //currentState = State.Placed; //?
    }

    public override IEnumerator OnDrop(int col) {
        TileEffect te = new TileEffect(PlayerId, this);
        TurnEffect e = new TurnEndEffect(PlayerId, "Tombstone", Effect.Behav.Add, OnTurnEnd)
            { turnsLeft = 5, onEndEffect = OnEndEffect };
        te.AddEffect(e);
        AddTileEffect(te);
        yield return null;
    }

    public IEnumerator OnTurnEnd(int id) {
        yield return _mm.syncManager.SyncRand(id, Random.Range(0, 2));
        Tile.Element elem = Tile.Element.None;
        if (_mm.syncManager.GetRand() == 0)
            elem = Tile.Element.Earth;
        else
            elem = Tile.Element.Muscle;

        TileBehav tb = HexManager.GenerateBasicTile(PlayerId, elem);
        yield return Zombie.Set(PlayerId, tb);
        HexGrid.RaiseTileBehavIntoCell(tb, tile.col, tile.row + 1);
        AudioController.Trigger(SFX.Gravekeeper.Sig_TSEffect);
    }

    public IEnumerator OnEndEffect() {
        yield return HexManager._RemoveTile(this, false); // remove itself
        AudioController.Trigger(SFX.Gravekeeper.Sig_Bell2);
    }
}
