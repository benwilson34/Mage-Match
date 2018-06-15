using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CausticCastanet : TileBehav {

    private bool _gainAPthisTurn = false;

    public override void SetInitProps() {
        cost = 2;
        initElements = new Tile.Element[1] { Tile.Element.Fire };
    }

    public override IEnumerator OnDrop(int col) {
        AudioController.Trigger(SFX.Rune_Enfuego.CausticCastanet);

        TileEffect te = new TileEffect(PlayerId, this);

        TurnEffect turnEnd = new TurnEndEffect(PlayerId, "CausticCastanet_Destroy", Effect.Behav.Destruct, OnTurnEnd);
        te.AddEffect(turnEnd);

        TurnEffect turnBegin = new TurnBeginEffect(PlayerId, "CausticCastanet_Begin", Effect.Behav.APChange, OnTurnBegin);
        te.AddEffect(turnBegin);

        AddTileEffect(te);
        yield return null;
    }

    IEnumerator OnTurnEnd(int id) {
        Debug.Log("CausticCastanet: TurnEnd starting...");
        List<TileBehav> tbs = HexGrid.GetSmallAreaTiles(tile.col, tile.row);
        tbs = TileFilter.FilterByEnch(tbs, Enchantment.Type.Burning);
        Debug.Log("CausticCastanet: " + tbs.Count + " available adjacent tiles.");

        if (tbs.Count == 0)
            yield break;

        yield return _mm.syncManager.SyncRand(PlayerId, Random.Range(0, tbs.Count));
        int rand = _mm.syncManager.GetRand();

        // TODO animation?
        HexManager.RemoveTile(tbs[rand].tile, false);
        _gainAPthisTurn = true;

        yield return null;
    }

    IEnumerator OnTurnBegin(int id) {
        if (PlayerId == id && _gainAPthisTurn) {
            //MMDebug.MMLog.LogError("Is this going to work?");
            Debug.LogWarning("CausticCastanet: gaining AP!");
            _mm.GetPlayer(id).IncreaseAP(2);
            _gainAPthisTurn = false;
        }
        yield return null;
    }
}
