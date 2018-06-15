using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoaringFlame : Charm {

    public override void SetInitProps() {
        cost = 2;
    }

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_Enfuego.RoaringFlame);

        Hand hand = _mm.GetPlayer(PlayerId).Hand;
        List<Hex> hexes = new List<Hex>();
        hexes.AddRange(hand.GetAllHexes());
        foreach (Hex hex in hexes) {
            hand.Discard(hex);
            yield return new WaitForSeconds(.05f);
        }

        const int damagePerDiscard = 30;
        ThisPlayer.Character.DealDamage(hexes.Count * damagePerDiscard);

        yield return null;
    }
}