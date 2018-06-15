using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilDoll : TileBehav {

    int _amountHealed = 0;

    public override void SetInitProps() {
        cost = 3;
        initElements = new Tile.Element[2] { Tile.Element.Fire, Tile.Element.Water };
    }

    public override IEnumerator OnDrop(int col) {
        AudioController.Trigger(SFX.Rune_Neutral.EvilDoll);

        EventController.playerHealthChange += OnPlayerHealthChange;

        TileEffect te = new TileEffect(PlayerId, this);
        TurnEffect e = new TurnEndEffect(PlayerId, "EvilDoll_Dmg", Effect.Behav.Damage, OnTurnEnd);
        e.onEndEffect = OnRemove;
        te.AddEffect(e);

        AddTileEffect(te);
        yield return null;
    }

    void OnPlayerHealthChange(int id, int amount, int newHealth, bool dealt) {
        if (PlayerId == id && amount > 0)
            _amountHealed += amount;
    }

    public IEnumerator OnTurnEnd(int id) {
        if (_amountHealed > 0) {
            _mm.GetPlayer(id).Character.DealDamage(_amountHealed);
            _amountHealed = 0;
        }
        yield return null;
    }

    public IEnumerator OnRemove() {
        EventController.playerHealthChange -= OnPlayerHealthChange;
        yield return null;
    }
}
