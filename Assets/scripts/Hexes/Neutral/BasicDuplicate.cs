using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDuplicate : TileBehav {

	public override void SetInitProps() {
        SetDuplicate();
    }

    public override string GetTooltipInfo() {
        var info = RuneInfoLoader.GetPlayerRuneInfo(PlayerId, Title);
        return GetTooltipInfo(info.title, "Tile", 1, "Basic " + tile.ElementsToString(false) + " tile with Duplicate.");
    }
}
