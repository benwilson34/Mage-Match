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
    private bool newSpell = false;

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

    public void ShowSpellInfo() {
        Spell currentSpell = mm.LocalP().character.GetSpell(spellNum);
        Transform t = transform.Find("main");

        Text spellName = t.Find("t_spellName").GetComponent<Text>();
        spellName.text = currentSpell.name;
        if (currentSpell.APcost != 1)
            spellName.text += " " + currentSpell.APcost + " AP";

        Transform minis = t.Find("minis");
        for (int m = 0; m < 5; m++) {
            Image minitile = minis.Find("minitile" + m).GetComponent<Image>();
            minitile.color = Color.white;
            Tile.Element currentEl = currentSpell.GetElementAt(m);
            //				Debug.Log (currentEl);
            switch (currentEl) {
                case Tile.Element.Fire:
                    minitile.sprite = mm.uiCont.miniFire;
                    break;
                case Tile.Element.Water:
                    minitile.sprite = mm.uiCont.miniWater;
                    break;
                case Tile.Element.Earth:
                    minitile.sprite = mm.uiCont.miniEarth;
                    break;
                case Tile.Element.Air:
                    minitile.sprite = mm.uiCont.miniAir;
                    break;
                case Tile.Element.Muscle:
                    minitile.sprite = mm.uiCont.miniMuscle;
                    break;
                case Tile.Element.None:
                    minitile.gameObject.SetActive(false);
                    break;
            }
        }

        GetComponent<UITooltip>().tooltipInfo = CharacterInfo.GetSpellInfo(mm.LocalP().character.ch, spellNum);
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
        if (newSpell) {
            MMLog.Log("BUTTONCONT", "black", "button" + spellNum + " showing new spell info");
            //bool act = mainView.GetActive();
            mainView.SetActive(true);
            ShowSpellInfo();
            mainView.SetActive(false);
            newSpell = false;
        }

        if(cancelView != null)
            Destroy(cancelView.gameObject);
        mainView.gameObject.SetActive(true);
        onClick = mainClick;

        yield return null;
    }

    public void SpellChanged() { newSpell = true; }

	void OnSpellButtonClick(){
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
