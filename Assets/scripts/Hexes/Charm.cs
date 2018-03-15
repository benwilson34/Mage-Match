using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Charm : Hex {

    public abstract IEnumerator DropEffect();

    protected int _playerId;

    public override void Init(MageMatch mm) {
        base.Init(mm);
        _playerId = Hex.TagPlayer(this.hextag);
    }

    public override string GetTooltipInfo() {
        string title = TagTitle(hextag);
        return GetTooltipInfo(title, "Charm", RuneInfo.GetRuneInfo(title).desc);
    }

    public Character ThisCharacter() {
        return _mm.GetPlayer(_playerId).character;
    }
}
