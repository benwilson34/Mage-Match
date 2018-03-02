﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleConsumable : Consumable {
    public override IEnumerator DropEffect() {
        var tbs = _mm.hexGrid.GetPlacedTiles();
        for (int i = 0; i < tbs.Count; i++) {
            _mm.hexMan.RemoveTile(tbs[0].tile, false);
            tbs.RemoveAt(0);
            i--;
            yield return new WaitForSeconds(.05f);
        }
        yield return null;
    }

    public override string GetTooltipInfo() {
        string str = "This is a <b>hex</b>.\n";
        str += "Its <color=green>tag</color> is " + hextag;
        str += "\n" + RuneInfo.GetRuneInfo("sampl").desc;
        return str;
    }
}
