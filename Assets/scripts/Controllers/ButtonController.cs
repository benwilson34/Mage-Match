using UnityEngine;
using System.Collections;

public class ButtonController : MonoBehaviour {

	private MageMatch mm;
	private int spellNum;

	void Start () {
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
	}

	public void OnSpellButtonClick(){
		spellNum = int.Parse (gameObject.name.Substring (12)); // kinda shitty but it works
		mm.CastSpell (spellNum);
	}

    public void OnSpellCancelButtonClick() {
        Debug.Log("BUTTONCONTROLLER: Spell canceled.");
        Targeting.CancelTargeting();
    }

    public void OnClearTargetsButtonClick() {
        Debug.Log("BUTTONCONTROLLER: Targets cleared.");
        Targeting.ClearTargets();
    }
}
