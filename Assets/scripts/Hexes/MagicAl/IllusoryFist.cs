using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IllusoryFist : TileBehav {

    public override void SetInitProps() {
        cost = 2;
        initElements = new Tile.Element[2] { Tile.Element.Air, Tile.Element.Muscle };
        SetQuickdraw();
    }

    public override IEnumerator OnDrop(int col) {
        AudioController.Trigger(SFX.Rune_MagicAl.IllusoryFist);

        List<TileBehav> tbs = HexGrid.GetTilesInCol(col);
        yield return CommonEffects.ShootIntoAirAndRearrange(tbs);
    }
}
