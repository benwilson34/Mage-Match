using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifestealer : TileBehav {

    public override IEnumerator OnDrop() {
        TileEffect te = new TileEffect(PlayerId, Effect.Type.Buff, Lifestealer_Turn, null);
        AddTileEffect(te, Title);
        yield return null;
    }

    IEnumerator Lifestealer_Turn(int id, TileBehav tb) {
        Character character = _mm.GetPlayer(id).character;
        character.DealDamage(20);
        character.Heal(20);

        HealthModEffect buffDeal = new HealthModEffect(id, Lifestealer_Buff_Deal, false, true, 5);
        HealthModEffect buffTake = new HealthModEffect(id, Lifestealer_Buff_Take, false, false, 5);
        _mm.effectCont.AddHealthEffect(buffDeal, "Lifestealer_Deal");
        _mm.effectCont.AddHealthEffect(buffTake, "Lifestealer_Take");

        yield return null;
    }

    float Lifestealer_Buff_Deal(Player p, int dmg) {
        return 1.10f; // +10% dmg dealt
    }

    float Lifestealer_Buff_Take(Player p, int dmg) {
        return .90f; // -10% dmg recieved
    }
}
