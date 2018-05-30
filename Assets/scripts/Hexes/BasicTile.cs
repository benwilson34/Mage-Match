using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTile : TileBehav {
    public override string GetTooltipInfo() {
        string ench = "";
        if (HasEnchantment())
            ench = "\nThis tile is enchanted with " + GetEnchType().ToString();
        return GetTooltipInfo(Title, "Basic Tile", 1, ench);
    }
}
