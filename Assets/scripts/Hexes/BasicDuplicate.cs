using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicDuplicate : TileBehav {

	public override void Init(MageMatch mm) {
        base.Init(mm);
        SetDuplicate();
    }

    public override string GetTooltipInfo() {
        string elem = tile.element.ToString();
        return GetTooltipInfo(TagTitle(hextag), "Tile", "Basic " + elem + " tile with Duplicate.");
    }
}
