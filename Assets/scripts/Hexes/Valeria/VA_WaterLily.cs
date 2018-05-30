using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VA_WaterLily : TileBehav {

    public override IEnumerator OnDrop() {
        _mm.audioCont.Trigger(AudioController.Rune_ValeriaSFX.WaterLily);

        TileEffect te = new TileEffect(PlayerId, Effect.Type.Add, WaterLily_Turn, null);
        AddTileEffect(te, Title);
        yield return null;
    }

    IEnumerator WaterLily_Turn(int id, TileBehav tb) {
        _mm.audioCont.Trigger(AudioController.Rune_ValeriaSFX.WaterLily);

        var dropCount = 4;
        Valeria valeria = (Valeria)_mm.GetPlayer(PlayerId).character;
        yield return valeria.DropWaterIntoRandomCols(dropCount);
    }
}
