using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MMDebug;

public class ButtonController : MonoBehaviour {

    public enum Type { Spell };
    public delegate void ButtonClick();
    public Type type = Type.Spell;
	public int spellNum;

	private MageMatch mm;
    private GameObject simpleTextPF;
    private GameObject mainView, cancelView;
    private ButtonClick onClick, mainClick;

	void Start () {
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
        if(spellNum >= 0)
            mainView = transform.Find("main").gameObject;
        simpleTextPF = Resources.Load<GameObject>("prefabs/ui/simpleTextView");

        switch (type) {
            case Type.Spell:
                onClick = OnSpellButtonClick;
                break;
            default:
                MMLog.LogError("BUTTONCONT: Tried to init a button with bad type!");
                break;
        }
	}

    public void OnClick() {
        onClick();
    }

    IEnumerator Transition_Cancel() {
        cancelView = Instantiate(simpleTextPF, this.transform, false);
        mainView.SetActive(false);

        cancelView.transform.Find("t").GetComponent<Text>().text = "Cancel";

        mainClick = onClick;
        onClick = OnSpellCancelClick;

        yield return null;
    }

    public IEnumerator Transition_MainView() {
        if(cancelView != null)
            Destroy(cancelView.gameObject);
        mainView.gameObject.SetActive(true);
        onClick = mainClick;

        yield return null;
    }

	void OnSpellButtonClick(){
        //spellNum = int.Parse (gameObject.name.Substring (12)); // kinda shitty but it works
        StartCoroutine(Transition_Cancel());
		StartCoroutine(mm.CastSpell (spellNum));
	}

    public void OnSpellCancelClick() {
        StartCoroutine(Transition_MainView());
        StartCoroutine(mm.CancelSpell());
    }

    public void Targeting_OnCancelClick() {
        MMLog.Log("BUTTONCONT", "black", "Spell canceled.");
        mm.targeting.CancelTargeting();
    }

    public void Targeting_OnClearButton() {
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
