using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLily : TileBehav {

    public override void SetInitProps() {
        initElements = new Tile.Element[1] { Tile.Element.Water };
    }

    public override IEnumerator OnDrop(int col) {
        AudioController.Trigger(SFX.Rune_Valeria.WaterLily);

        TileEffect te = new TileEffect(PlayerId, this);
        te.AddEffect(new TurnEndEffect(PlayerId, "WaterLily_Drop", Effect.Behav.Add, OnTurnEnd));
        AddTileEffect(te);
        yield return null;
    }

    IEnumerator OnTurnEnd(int id) {
        AudioController.Trigger(SFX.Rune_Valeria.WaterLily);

        var dropCount = 4;
        yield return CommonEffects.DropBasicsIntoRandomCols(PlayerId, Tile.Element.Water, dropCount);
    }
}
