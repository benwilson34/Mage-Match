using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicQuickdraw : TileBehav {

    public override void SetInitProps() {
        SetQuickdraw();
    }

    public override string GetTooltipInfo() {
        var info = RuneInfoLoader.GetPlayerRuneInfo(PlayerId, Title);
        return GetTooltipInfo(info.title, "Tile", 1, "Basic " + tile.ElementsToString(false) + " tile with Quickdraw.");
    }
}
