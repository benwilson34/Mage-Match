using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CausticCastanet : TileBehav {

    bool _gainAPthisTurn = false;
    string _turnEffectTag;

    public override IEnumerator OnDrop() {
        TileEffect te = new TileEffect(PlayerId, Effect.Type.Destruct, OnTurnEnd, OnRemove);
        AddTileEffect(te, Title);

        TurnEffect turn = new TurnEffect(PlayerId, Effect.Type.Healing, OnTurnBegin);
        _turnEffectTag = _mm.effectCont.AddBeginTurnEffect(turn, "CausticCastanet_Begin");
        yield return null;
    }


    IEnumerator OnTurnEnd(int id, TileBehav tb) {
        List<TileBehav> tbs = _mm.hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
        for (int i = 0; i < tbs.Count; i++) {
            var atb = tbs[i];
            if (atb.GetEnchType() != Enchantment.Type.Burning) {
                tbs.RemoveAt(i);
                i--;
            }
        }

        if (tbs.Count == 0)
            yield break;

        yield return _mm.syncManager.SyncRand(PlayerId, Random.Range(0, tbs.Count));
        int rand = _mm.syncManager.GetRand();

        // TODO animation?
        _mm.hexMan.RemoveTile(tbs[rand].tile, false);
        _gainAPthisTurn = true;

        yield return null;
    }

    IEnumerator OnTurnBegin(int id) {
        if (PlayerId == id && _gainAPthisTurn) {
            MMDebug.MMLog.LogError("Is this going to work?");
            _mm.GetPlayer(id).IncreaseAP(2);
            _gainAPthisTurn = false;
        }
        yield return null;
    }

    IEnumerator OnRemove(int id, TileBehav tb) {
        _mm.effectCont.RemoveTurnEffect(_turnEffectTag);
        yield return null;
    }
}
