using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GK_PartySnacks : Charm {
    public override IEnumerator DropEffect() {
        var zombs = TileFilter.GetTilesByEnch(Enchantment.Type.Zombie);
        yield return _mm.targeting.WaitForTileTarget(1, zombs);

        var tbs = _mm.targeting.GetTargetTBs();
        if (tbs.Count != 1)
            yield break; // whiff

        var tb = tbs[0];
        var adjTBs = _mm.hexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
        int destroyCount = 2;
        for (int i = 0; i < destroyCount && adjTBs.Count > 0; i++) {
            yield return _mm.syncManager.SyncRand(_playerId, Random.Range(0, adjTBs.Count));
            int index = _mm.syncManager.GetRand();

            _mm.hexMan.RemoveHex(adjTBs[index]);
            adjTBs.RemoveAt(index);
        }

        _mm.hexMan.RemoveHex(tb);

        yield return _mm.syncManager.SyncRand(_playerId, Random.Range(60, 81));
        int dmg = _mm.syncManager.GetRand();
        ThisCharacter().DealDamage(dmg);
    }
}
