using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TombstoneTile : TileBehav {

    public override void Init(MageMatch mm) {
        base.Init(mm);
        ableTarget = false; //?
        //currentState = State.Placed; //?
    }

    public override IEnumerator OnDrop() {
        _mm.effectCont.AddEndTurnEffect(new TurnEffect(PlayerId, Effect.Type.Add, Tombstone_OnTurnEnd, Tombstone_OnEffectRemove, 5), "tombs");
        yield return null;
    }

    public IEnumerator Tombstone_OnTurnEnd(int id) {
        yield return _mm.syncManager.SyncRand(id, Random.Range(0, 2));
        Tile.Element elem = Tile.Element.None;
        if (_mm.syncManager.GetRand() == 0)
            elem = Tile.Element.Earth;
        else
            elem = Tile.Element.Muscle;

        TileBehav tb = _mm.hexMan.GenerateBasicTile(id, elem);
        yield return _mm.hexFX.Ench_SetZombie(id, tb);
        _mm.hexGrid.RaiseTileBehavIntoCell(tb, tile.col, tile.row + 1);
        AudioController.Trigger(AudioController.GravekeeperSFX.SigEffect);
    }

    public IEnumerator Tombstone_OnEffectRemove(int id) {
        yield return _mm.hexMan._RemoveTile(this, false); // remove itself
        AudioController.Trigger(AudioController.GravekeeperSFX.SigBell2);
    }
}
