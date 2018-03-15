using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicQuickdraw : TileBehav {

    public override void Init(MageMatch mm) {
        base.Init(mm);
        SetQuickdraw();
    }

    public override string GetTooltipInfo() {
        string elem = tile.element.ToString();
        return GetTooltipInfo(TagTitle(hextag), "Tile", "Basic " + elem + " tile with Quickdraw.");
    }
}
