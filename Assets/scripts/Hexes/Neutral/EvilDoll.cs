using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilDoll : TileBehav {

    int _amountHealed = 0;

    public override IEnumerator OnDrop() {
        EventController.playerHealthChange += OnPlayerHealthChange;
        TileEffect te = new TileEffect(PlayerId, Effect.Type.Damage, OnTurnEnd, OnRemove);
        AddTileEffect(te, Title);
        yield return null;
    }

    void OnPlayerHealthChange(int id, int amount, int newHealth, bool dealt) {
        if (PlayerId == id && amount > 0)
            _amountHealed += amount;
    }

    public IEnumerator OnTurnEnd(int id, TileBehav tb) {
        if (_amountHealed > 0) {
            _mm.GetPlayer(id).character.DealDamage(_amountHealed);
            _amountHealed = 0;
        }
        yield return null;
    }

    public IEnumerator OnRemove(int id, TileBehav tb) {
        EventController.playerHealthChange -= OnPlayerHealthChange;
        yield return null;
    }
}
