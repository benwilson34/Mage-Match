using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegWeights : TileBehav {

    public override void SetInitProps() {
        cost = 2;
        initElements = new Tile.Element[1] { Tile.Element.Muscle };
        SetDuplicate();
    }

    public override IEnumerator OnDrop(int col) {
        AudioController.Trigger(SFX.Rune_Neutral.LegWeights);

        TileEffect te = new TileEffect(PlayerId, this);
        TurnEndEffect e = new TurnEndEffect(PlayerId, "LegWeights_Persist", Effect.Behav.TickDown, Persist);
        te.AddEffect(e);
        AddTileEffect(te);
        yield return null;
    }

    public IEnumerator Persist(int id) {
        TurnBeginEffect e = new TurnBeginEffect(PlayerId, "LegWeights_AP", Effect.Behav.APChange, OnTurnBegin) { turnsLeft = 2 }; // not the cleanest thing, but one of the next two turns will be the opponent's
        EffectManager.AddEventEffect(e);
        yield return null;
    }

    public IEnumerator OnTurnBegin(int id) {
        if (id == Opponent.ID) {
            Opponent.DecreaseAP();
        }
        yield return null;
    }
}
