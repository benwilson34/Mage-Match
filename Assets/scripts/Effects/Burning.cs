using MMDebug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Burning : Enchantment {

    private const int BURNING_TURNS = 5, BURNING_DMG = 5;

    private const string BURNING_COLOR = "#F86262FF";
    private static Color _burningColor;

    public Burning(int id, TileBehav tb) : base(id, tb, Type.Burning) {
        turnsLeft = BURNING_TURNS;
        AddEffect(new TurnEndEffect(id, "Burning_Damage", Behav.Damage, Burning_OnTurnEnd));
        _burningColor = new Color();
        ColorUtility.TryParseHtmlString(BURNING_COLOR, out _burningColor);
    }

    public static IEnumerator Set(int id, TileBehav tb) {
        yield return AnimationController._Burning(tb);
        AudioController.Trigger(SFX.Enfuego.Burning_Enchant);

        new Burning(id, tb); // looks weird but this is how it is right now

        yield return tb.GetComponent<TileGFX>()._AnimateTint(_burningColor);
    }

    IEnumerator Burning_OnTurnEnd(int id) {
        MMLog.Log_EnchantFx("Burning TurnEffect at " + enchantee.PrintCoord());
        yield return AnimationController._Burning_Turn(_mm.GetOpponent(playerId), enchantee);
        AudioController.Trigger(SFX.Enfuego.Burning_Damage);
        _mm.GetPC(playerId).DealDamage(BURNING_DMG);
        //yield return null; // for now
    }

    public override IEnumerator OnEndEffect() {
        AudioController.Trigger(SFX.Enfuego.Burning_Timeout);
        yield return null; // for now
    }
}
