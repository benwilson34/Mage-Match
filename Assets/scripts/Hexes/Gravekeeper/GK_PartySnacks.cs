using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GK_PartySnacks : Charm {
    public override IEnumerator DropEffect() {
        var zombs = TileFilter.GetTilesByEnch(Enchantment.Type.Zombie);
        yield return Targeting.WaitForTileTarget(1, zombs);

        var tbs = Targeting.GetTargetTBs();
        if (tbs.Count != 1)
            yield break; // whiff

        AudioController.Trigger(AudioController.Rune_GravekeeperSFX.PartySnacks);

        var tb = tbs[0];
        var adjTBs = HexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
        int destroyCount = 4;
        for (int i = 0; i < destroyCount && adjTBs.Count > 0; i++) {
            yield return _mm.syncManager.SyncRand(PlayerId, Random.Range(0, adjTBs.Count));
            int index = _mm.syncManager.GetRand();

            HexManager.RemoveHex(adjTBs[index]);
            adjTBs.RemoveAt(index);
        }

        HexManager.RemoveHex(tb);

        yield return _mm.syncManager.SyncRand(PlayerId, Random.Range(60, 81));
        int dmg = _mm.syncManager.GetRand();
        ThisCharacter().DealDamage(dmg);
    }
}
