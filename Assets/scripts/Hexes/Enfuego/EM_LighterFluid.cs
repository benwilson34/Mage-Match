using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EM_LighterFluid : Charm {
    public override IEnumerator DropEffect() {
        Hand hand = _mm.GetPlayer(PlayerId).hand;
        List<Hex> hexes = new List<Hex>();
        hexes.AddRange(hand.GetAllHexes());
        foreach (Hex hex in hexes) {
            hand.Discard(hex);
            yield return new WaitForSeconds(.05f);
        }

        const int damagePerDiscard = 30;
        ThisCharacter().DealDamage(hexes.Count * damagePerDiscard);

        yield return null;
    }
}
