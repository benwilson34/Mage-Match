using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Consumable : Hex {
    public abstract IEnumerator DropEffect();
}
