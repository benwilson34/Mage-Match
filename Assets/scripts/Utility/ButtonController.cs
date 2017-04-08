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
		StartCoroutine(mm.CastSpell (spellNum));
	}

    public void OnSpellCancelButtonClick() {
        Debug.Log("BUTTONCONTROLLER: Spell canceled.");
        mm.targeting.CancelTargeting();
    }

    public void OnClearTargetsButtonClick() {
        Debug.Log("BUTTONCONTROLLER: Targets cleared.");
        mm.targeting.ClearTargets();
    }

    public void OnDrawButtonClick() {
        mm.DrawTile();
    }

    public void OnFileButtonClick() {
        mm.stats.SaveStatsCSV();
        mm.stats.SaveReportTXT();
    }
}
