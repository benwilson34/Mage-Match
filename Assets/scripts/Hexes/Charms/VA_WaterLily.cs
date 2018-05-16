using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VA_WaterLily : Charm {
    public override IEnumerator DropEffect() {
        _mm.audioCont.Trigger(AudioController.Rune_ValeriaSFX.WaterLily);

        var dropCount = 4;
        Valeria valeria = (Valeria)ThisCharacter();
        yield return valeria.DropWaterIntoRandomCols(dropCount);
    }
}
