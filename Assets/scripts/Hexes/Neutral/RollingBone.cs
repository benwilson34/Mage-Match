using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingBone : Charm {

    public override IEnumerator DropEffect() {
        Hand hand = _mm.GetPlayer(PlayerId).hand;
        List<Hex> hexes = new List<Hex>();
        hexes.AddRange(hand.GetAllHexes());
        foreach (Hex hex in hexes) {
            hand.Discard(hex);
            yield return new WaitForSeconds(.05f);
        }

        // TODO SFX
        yield return new WaitForSeconds(.2f);

        yield return _mm._Draw(PlayerId, hexes.Count);
    }
}
