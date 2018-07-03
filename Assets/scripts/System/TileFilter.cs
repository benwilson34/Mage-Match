using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileFilter {

    //public static void Init() {
    //}

    public static List<TileBehav> GetTilesByEnch(Enchantment.Type ench, bool inverse = false) {
        return FilterByEnch(HexGrid.GetPlacedTiles(), ench, inverse);
    }

    public static List<TileBehav> FilterByEnch(List<TileBehav> tbs, Enchantment.Type ench, bool inverse = false) {
        List<TileBehav> filtTBs = new List<TileBehav>();
        foreach (TileBehav tb in tbs) {
            if (tb.GetEnchType() == ench ^ inverse)
                filtTBs.Add(tb);
        }
        return filtTBs;
    }

    public static List<TileBehav> GetTilesByAbleEnch(Enchantment.Type ench) {
        return FilterByAbleEnch(HexGrid.GetPlacedTiles(), ench);
    }

    public static List<TileBehav> FilterByAbleEnch(List<TileBehav> tbs, Enchantment.Type ench) {
        List<TileBehav> filtTBs = new List<TileBehav>();
        foreach (TileBehav tb in tbs) {
            if (tb.CanSetEnch(ench))
                filtTBs.Add(tb);
        }
        return filtTBs;
    }

    public static List<Hex> FilterByCategory(List<Hex> hexes, Hex.Category cat, bool inverse = false) {
        List<Hex> filtHexes = new List<Hex>();
        foreach (Hex hex in hexes) {
            if (hex.Cat == cat ^ inverse)
                filtHexes.Add(hex);
        }
        return filtHexes;
    }
}
