using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stardust : Charm {

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_Neutral.Stardust);

        List<TileBehav> tbs = new List<TileBehav>();
        Tile.Element[] elems = new Tile.Element[5] { Tile.Element.Fire, Tile.Element.Water, Tile.Element.Earth, Tile.Element.Air, Tile.Element.Muscle };

        int dropCount = 5;
        dropCount = Mathf.Min(dropCount, HexGrid.GetEmptyCellCount());

        for (int i = 0; i < dropCount; i++)
            tbs.Add(HexManager.GenerateBasicTile(PlayerId, elems[i]));

        yield return CommonEffects.DropIntoRandomCols(PlayerId, tbs, dropCount);
    }
}
