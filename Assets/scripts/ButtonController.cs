using UnityEngine;
using System.Collections;

public class ButtonController : MonoBehaviour {

//	[Range(1,2)]public int playerID;
//	[Range(0,3)]public int spellNum;

	private MageMatch mm;
	private int spellNum;

	void Start () {
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
//		Debug.Log ("Button num = " + gameObject.name.Substring (12));
		spellNum = int.Parse (gameObject.name.Substring (12)); // kinda shitty but it works
	}

	public void OnSpellButtonClick(){
		mm.CastSpell (spellNum);
	}
}
