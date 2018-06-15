using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingBone : Charm {

    public override void SetInitProps() {
        cost = 3;
    }

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_Neutral.RollingBone);

        Hand hand = _mm.GetPlayer(PlayerId).Hand;
        List<Hex> hexes = new List<Hex>();
        hexes.AddRange(hand.GetAllHexes());
        foreach (Hex hex in hexes) {
            hand.Discard(hex);
            yield return new WaitForSeconds(.05f);
        }

        yield return new WaitForSeconds(.2f);

        yield return _mm._Draw(PlayerId, hexes.Count);
    }
}
