using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandObject : MonoBehaviour {

	public enum State { Hand, Placed, Removed };
	public State currentState;
    public string tag;

    protected MageMatch mm;

	void Awake () {
		mm = GameObject.Find("board").GetComponent<MageMatch>();
	}

    public bool EqualsTag(string tag) { return this.tag.Equals(tag); }

    public bool EqualsTag(HandObject hex) { return this.tag.Equals(hex.tag); }

    public static int TagPlayer(string tag) { return int.Parse(tag.Substring(1, 1)); }

    public static string TagCat(string tag) { return tag.Split(new char[] { '-' })[1]; }

    public static string TagType(string tag) { return tag.Split(new char[] { '-' })[2]; }

    public static int TagNum(string tag) { return int.Parse(tag.Split(new char[] { '-' })[3]); }

}
