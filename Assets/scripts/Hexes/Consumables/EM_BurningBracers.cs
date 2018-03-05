using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EM_BurningBracers : Consumable {
    public override IEnumerator DropEffect() {
        _mm.GetPlayer(TagPlayer(hextag)).AP += 1;
        yield return null;
    }
}
