using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour, Tooltipable {

	public enum State { Hand, Placed, Removed };
	public State currentState;
    public string tag;

    protected MageMatch mm;
    private Sprite flipSprite;
    private bool flipped = false;

	void Awake () {
		mm = GameObject.Find("board").GetComponent<MageMatch>();
	}

    public bool EqualsTag(string tag) { return this.tag.Equals(tag); }

    public bool EqualsTag(Hex hex) { return this.tag.Equals(hex.tag); }

    public static int TagPlayer(string tag) { return int.Parse(tag.Substring(1, 1)); }

    public static string TagCat(string tag) { return tag.Split(new char[] { '-' })[1]; }

    public static string TagType(string tag) { return tag.Split(new char[] { '-' })[2]; }

    public static int TagNum(string tag) { return int.Parse(tag.Split(new char[] { '-' })[3]); }

    public void Flip() {
        SpriteRenderer rend = GetComponent<SpriteRenderer>();

        Sprite newSprite;
        if (!flipped) {
            newSprite = mm.tileMan.flipSprite;
        } else {
            newSprite = flipSprite;
        }

        flipSprite = rend.sprite;
        rend.sprite = newSprite;
        flipped = !flipped;
    }

    public void Reveal() {
        if (flipped)
            Flip();
    }

    public virtual string GetTooltipInfo() {
        string str = "This is a <b>hex</b>.\n";
        str += "Its <color=green>tag</color> is " + tag;
        return str;
    }
}
