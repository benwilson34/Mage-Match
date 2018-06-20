//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class GlowController {

    //private const float GLOW_RANGE = .75f;

    //private static MageMatch _mm;

    //public static void Init(MageMatch mm) {
    //    _mm = mm;
    //}

    //public static void SetGlowingTiles(List<TileSeq> seqs) {
    //    var tiles = new Dictionary<string, TileBehav>();
    //    foreach (var seq in seqs) {
    //        foreach (var tile in seq.sequence) {
    //            string coord = tile.PrintCoord();
    //            if (!tiles.ContainsKey(coord))
    //                tiles.Add(coord, HexGrid.GetTileBehavAt(tile.col, tile.row));
    //        }
    //    }
    //    SetGlowingTiles(new List<TileBehav>(tiles.Values));
    //}

    //public static void SetGlowingTiles(List<TileBehav> glowTBs) {
    //    var tbs = new List<TileBehav>(glowTBs);
    //    foreach (TileBehav tb in HexGrid.GetPlacedTiles()) {
    //        bool glowThisTile = false;
    //        for (int i = 0; i < tbs.Count; i++) {
    //            var glowTB = tbs[i];
    //            if (glowTB.hextag == tb.hextag) {
    //                glowThisTile = true;
    //                tbs.RemoveAt(i);
    //                break;
    //            }
    //        }

    //        var glowDriver = tb.GetComponent<TileGFX>();
    //        if (glowThisTile) {
    //            glowDriver.ChangeGlow(TileGFX.GlowState.Glowing);
    //        } else {
    //            glowDriver.ChangeGlow(TileGFX.GlowState.Faded);
    //        }

    //    }
    //}

    //public static void ClearGlowingTiles() {
    //    foreach (var tb in HexGrid.GetPlacedTiles()) {
    //        var driver = tb.GetComponent<TileGFX>();
    //        driver.ChangeGlow(TileGFX.GlowState.None);
    //    }
    //}

//}

