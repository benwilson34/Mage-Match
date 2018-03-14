using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour, Tooltipable {

	public enum State { Hand, Placed, Removed };
	public State currentState;
    public string hextag;
    public bool putBackIntoDeck = false;

    protected MageMatch _mm;
    private Sprite _flipSprite;
    private bool _flipped = false;
    private bool _quickdraw = false, _duplicate = false;

    public virtual void Init(MageMatch mm) {
        _mm = mm;
    }

    protected void SetQuickdraw() { _quickdraw = true; }
    protected void SetDuplicate() { _duplicate = true; }
    public IEnumerator OnDraw() {
        if (_mm.GetState() == MageMatch.State.BeginningOfGame)
            yield break;

        int playerId = TagPlayer(hextag);
        if (playerId == _mm.ActiveP().id) { // if this hex was generated on that player's turn
            if (_quickdraw)
                yield return _mm.prompt.WaitForQuickdrawAction(this);
            if (_duplicate)
                yield return _mm._Duplicate(playerId, hextag);
        }
        yield return null;
    }

    public bool EqualsTag(string tag) { return this.hextag.Equals(tag); }

    public bool EqualsTag(Hex hex) { return this.hextag.Equals(hex.hextag); }

    // ex: p2-B-W-005 (player 2 created this Basic tile, type is Water, and it's the fifth one)
    public static int TagPlayer(string tag) { return int.Parse(tag.Substring(1, 1)); }

    public static string TagCat(string tag) { return tag.Split(new char[] { '-' })[1]; }

    public static bool IsCharm(string tag) { return TagCat(tag) == "C"; }

    public static string TagTitle(string tag) { return tag.Split(new char[] { '-' })[2]; }

    public static int TagNum(string tag) { return int.Parse(tag.Split(new char[] { '-' })[3]); }

    public void Flip() {
        SpriteRenderer rend = GetComponent<SpriteRenderer>();

        Sprite newSprite;
        if (!_flipped) {
            newSprite = _mm.hexMan.flipSprite;
        } else {
            newSprite = _flipSprite;
        }

        _flipSprite = rend.sprite;
        rend.sprite = newSprite;
        _flipped = !_flipped;
    }

    public void Reveal() {
        if (_flipped)
            Flip();
    }

    //public IEnumerator Duplicate() {
    //    yield return null;
    //}

    public virtual string GetTooltipInfo() {
        string str = "This is a <b>hex</b>.\n";
        str += "Its <color=green>tag</color> is " + hextag;
        return str;
    }
}
