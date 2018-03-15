using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTile : TileBehav {
    public override string GetTooltipInfo() {
        string title = tile.element.ToString();
        return GetTooltipInfo(title, "Basic Tile", "");
    }
}
