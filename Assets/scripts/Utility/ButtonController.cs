using UnityEngine;
using System.Collections;
using MMDebug;

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
        MMLog.Log("BUTTONCONT", "black", "Spell canceled.");
        mm.targeting.CancelTargeting();
    }

    public void OnClearTargetsButtonClick() {
        MMLog.Log("BUTTONCONT", "black", "Targets cleared.");
        mm.targeting.ClearTargets();
    }

    public void OnDrawButtonClick() {
        mm.PlayerDrawTile();
    }

    public void OnFileButtonClick() {
        mm.stats.SaveStatsCSV();
        mm.stats.SaveReportTXT();
        MMLog.SaveReportTXT();
    }
}
