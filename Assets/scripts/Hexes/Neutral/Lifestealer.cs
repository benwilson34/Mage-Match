using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifestealer : TileBehav {

    public override void SetInitProps() {
        cost = 3;
        initElements = new Tile.Element[2] { Tile.Element.Earth, Tile.Element.Muscle };
    }

    public override IEnumerator OnDrop(int col) {
        AudioController.Trigger(SFX.Rune_Neutral.Lifestealer);

        TileEffect te = new TileEffect(PlayerId, this);
        te.AddEffect(new TurnEndEffect(PlayerId, "Lifestealer_Buff", Effect.Behav.Healing, OnTurnEnd));
        AddTileEffect(te);
        yield return null;
    }

    IEnumerator OnTurnEnd(int id) {
        Character character = _mm.GetPlayer(PlayerId).Character;
        character.DealDamage(20);
        character.Heal(20);

        const int numTurns = 5;
        HealthModEffect buffDeal = new HealthModEffect(PlayerId, "Lifestealer_Deal", Buff_Deal, HealthModEffect.Type.DealingPercent) { turnsLeft = numTurns };
        EffectManager.AddHealthMod(buffDeal);

        HealthModEffect buffTake = new HealthModEffect(PlayerId, "Lifestealer_Take", Buff_Rec, HealthModEffect.Type.ReceivingPercent) { turnsLeft = numTurns };
        EffectManager.AddHealthMod(buffTake);

        yield return null;
    }

    float Buff_Deal(Player p, int dmg) {
        const float morePercent = .10f;
        return 1 + morePercent; // +10% dmg dealt
    }

    float Buff_Rec(Player p, int dmg) {
        const float lessPercent = .10f;
        return 1 - lessPercent; // -10% dmg recieved
    }
}
