using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFilter {
    private static HexGrid _hexGrid;

    public static void Init(HexGrid hexGrid) {
        _hexGrid = hexGrid;
    }

    public static List<TileBehav> GetTilesByEnch(Enchantment.Type ench, bool inverse = false) {
        return FilterByEnch(_hexGrid.GetPlacedTiles(), ench, inverse);
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
        return FilterByAbleEnch(_hexGrid.GetPlacedTiles(), ench);
    }

    public static List<TileBehav> FilterByAbleEnch(List<TileBehav> tbs, Enchantment.Type ench) {
        List<TileBehav> filtTBs = new List<TileBehav>();
        foreach (TileBehav tb in tbs) {
            if (tb.CanSetEnch(ench))
                filtTBs.Add(tb);
        }
        return filtTBs;
    }
}
