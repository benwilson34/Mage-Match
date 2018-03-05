using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Consumable : Hex {

    // TODO probably an Enum for subtype? Or just have a method that subclasses can call?

    public abstract IEnumerator DropEffect();

    protected int _playerId;

    public override void Init() {
        _playerId = Hex.TagPlayer(this.hextag);
    }

    public override string GetTooltipInfo() {
        string str = "This is a <b>hex</b>.\n";
        str += "Its <color=green>tag</color> is " + hextag;
        str += "\n" + RuneInfo.GetRuneInfo(TagType(hextag)).desc;
        return str;
    }

    public Character ThisCharacter() {
        return _mm.GetPlayer(_playerId).character;
    }


    public IEnumerator Duplicate() {
        // TODO dupe tile if there's an extra space in the hand
        yield return null;
    }
}
