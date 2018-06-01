using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour, Tooltipable {

    public enum Category { BasicTile, Tile, Charm };

	public enum State { Hand, Placed, Removed };
	public State currentState;
    public int cost = 1;
    [HideInInspector]
    public bool putBackIntoDeck = false;

    [HideInInspector]
    public string hextag; // auto?
    public int PlayerId { get { return TagPlayer(hextag); } }
    public Category Cat { get { return TagCat(hextag); } }
    public string Title { get { return TagTitle(hextag); } }

    protected MageMatch _mm;
    private Sprite _flipSprite;
    private bool _flipped = false;
    private bool _quickdraw = false, _duplicate = false;

    public virtual void Init(MageMatch mm) {
        _mm = mm;
        _flipSprite = _mm.hexMan.flipSprite;
    }

    public Character ThisCharacter() {
        return _mm.GetPlayer(PlayerId).character;
    }

    protected void SetQuickdraw() { _quickdraw = true; }
    protected void SetDuplicate() { _duplicate = true; }
    public IEnumerator OnDraw() {
        if (_mm.GetState() == MageMatch.State.BeginningOfGame)
            yield break;

        if (PlayerId == _mm.ActiveP().id) { // if this hex was generated on that player's turn
            if (_quickdraw)
                yield return Prompt.WaitForQuickdrawAction(this);
            if (_duplicate)
                yield return _mm._Duplicate(PlayerId, hextag);
        }
        yield return null;
    }

    public virtual IEnumerator OnDrop() { yield return null; }


    #region ---------- TAG ----------

    public bool EqualsTag(string tag) { return this.hextag.Equals(tag); }

    public bool EqualsTag(Hex hex) { return this.hextag.Equals(hex.hextag); }

    // ex: p2-B-W-005 (player 2 created this Basic tile, type is Water, and it's the fifth one)
    public static int TagPlayer(string tag) { return int.Parse(tag.Substring(1, 1)); }

    public static Category TagCat(string tag) {
        string cat = tag.Split(new char[] { '-' })[1];
        switch (cat) {
            case "B": return Category.BasicTile;
            case "T": return Category.Tile;
            case "C": return Category.Charm;
            default:
                MMDebug.MMLog.LogError("HEX: Bad category name: " + cat);
                return Category.BasicTile;
        }
    }

    public static bool IsCharm(string tag) { return TagCat(tag) == Category.Charm; }

    public static string TagTitle(string tag) { return tag.Split(new char[] { '-' })[2]; }

    public static int TagNum(string tag) { return int.Parse(tag.Split(new char[] { '-' })[3]); }
    #endregion


    public void Flip() {
        Flip(!_flipped);
    }

    public void Flip(bool flipped) {
        if (_flipped == flipped)
            return;

        _flipped = flipped;
        SpriteRenderer rend = GetComponent<SpriteRenderer>();

        Sprite newSprite = _flipSprite;
        //if (!_flipped) {
        //    newSprite = _mm.hexMan.flipSprite;
        //} else {
        //    newSprite = _flipSprite;
        //}

        _flipSprite = rend.sprite;
        rend.sprite = newSprite;
    }

    public void Reveal() {
        if (_flipped)
            Flip();
    }

    public bool IsInteractable() {
        return !_flipped;
    }

    public virtual string GetTooltipInfo() {
        return GetTooltipInfo("", "hex", 0, "");
    }

    protected string GetTooltipInfo(string title, string cat, int cost, string desc) {
        string str = "<size=40>" + title + "</size>\n";
        str += "<size=25><i>" + cat + "</i>   <color=red>" + cost + " AP</color></size>\n";
        str += "<size=25><color=grey>tag: " + hextag + "</color></size>\n";
        str += desc;
        return str;
    }
}
