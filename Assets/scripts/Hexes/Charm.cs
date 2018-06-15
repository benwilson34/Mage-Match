using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Charm : Hex {

    public abstract IEnumerator DropEffect();

    public override IEnumerator OnDrop(int col) {
        yield return DropEffect();
    }

    public override string GetTooltipInfo() {
        RuneInfoLoader.RuneInfo info = RuneInfoLoader.GetPlayerRuneInfo(PlayerId, Title);
        return GetTooltipInfo(info.title, "Charm", info.cost, info.desc);
    }

}
