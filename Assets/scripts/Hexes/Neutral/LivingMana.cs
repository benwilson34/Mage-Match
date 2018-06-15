using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingMana : TileBehav {

    public override void SetInitProps() {
        cost = 2;
        initElements = new Tile.Element[2] { Tile.Element.Water, Tile.Element.Muscle };
        SetDuplicate();
    }

    public override IEnumerator OnDrop(int col) {
        AudioController.Trigger(SFX.Rune_Neutral.LivingMana);

        TileEffect te = new TileEffect(PlayerId, this);
        TurnEndEffect e = new TurnEndEffect(PlayerId, "LivingMana_Persist", Effect.Behav.Healing, Persist);
        te.AddEffect(e);
        AddTileEffect(te);
        yield return null;
    }

    public IEnumerator Persist(int id) {
        const int healAmt = 15;
        ThisPlayer.Character.Heal(healAmt);

        // shuffle muscle and water tile into deck
        ThisPlayer.Deck.AddHextag(HexManager.GetShortTag(PlayerId, Category.BasicTile, "Water"));
        ThisPlayer.Deck.AddHextag(HexManager.GetShortTag(PlayerId, Category.BasicTile, "Muscle"));
        yield return ThisPlayer.Deck.Shuffle();
    }
}
