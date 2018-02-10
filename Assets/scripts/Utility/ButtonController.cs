using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MMDebug;
using DG.Tweening;

public class ButtonController : MonoBehaviour {

    public enum Type { Spell, Draw };
    public delegate void ButtonClick();
    public Type type = Type.Spell;
	public int spellNum;

	private MageMatch mm;
    private Button button;
    private GameObject simpleTextPF;
    private GameObject mainView, cancelView;
    private ButtonClick onClick, mainClick;
    private int playerId;
    private bool newSpell = false;
    private bool isActivated = false, interactable = false;

    public void Init(MageMatch mm, int id) {
        MMLog.Log("ButtonCont", "black", "Init button " + spellNum + " with id="+id);
        this.mm = mm;
        this.playerId = id;

        button = this.GetComponent<Button>();

        if(spellNum >= 0)
            mainView = transform.Find("main").gameObject;
        simpleTextPF = Resources.Load<GameObject>("prefabs/ui/simpleTextView");

        switch (type) {
            case Type.Spell:
                SetOnClick(OnSpellButtonClick);
                break;
            case Type.Draw:
                SetOnClick(OnDrawButtonClick);
                SetInteractable();
                break;
            default:
                MMLog.LogError("BUTTONCONT: Tried to init a button with bad type!");
                break;
        }

        if(onClick == null)
            MMLog.LogError("BUTTONCONT: Button onClick is somehow null!");
    }

    public void SetInteractable() {
        button.interactable = true;
        interactable = true;
    }

    //public bool IsInteractable() { return interactable && isActivated; }

    public void Activate() {
        isActivated = true; // maybe not needed?
        if (interactable)
            button.interactable = true;
        StartCoroutine(_Activate());
    }
    IEnumerator _Activate() {
        var bg = transform.Find("i_bg").GetComponent<Image>();
        bg.DOColor(new Color(1, 1, 1, 1), .15f);
        var glow = transform.Find("i_glow").GetComponent<Image>();
        Tween t = glow.DOColor(new Color(1, 1, 1, 1), .15f);
        yield return t.WaitForCompletion();
    }

    public void Deactivate() {
        isActivated = false; // maybe not needed?
        if (interactable)
            button.interactable = false;
        StartCoroutine(_Deactivate());
    }
    IEnumerator _Deactivate() {
        var bg = transform.Find("i_bg").GetComponent<Image>();
        bg.DOColor(new Color(.33f, .33f, .33f, 1), .15f); // medium-dark grey
        var glow = transform.Find("i_glow").GetComponent<Image>();
        Tween t = glow.DOColor(new Color(1, 1, 1, 0), .15f);
        yield return t.WaitForCompletion();
    }

    public void TurnOffScreen() {
        // TODO "turn off" TV screen but persist active state
        mainView.SetActive(false);
        if (interactable)
            button.interactable = false;
        StartCoroutine(_TurnOffScreen());
    }
    IEnumerator _TurnOffScreen() {
        var bg = transform.Find("i_bg").GetComponent<Image>();
        bg.DOColor(new Color(.11f, .11f, .11f, 1), .15f); // dark grey
        var glow = transform.Find("i_glow").GetComponent<Image>();
        Tween t = glow.DOColor(new Color(1, 1, 1, 0), .15f);
        yield return t.WaitForCompletion();
    }

    public void TurnOnScreen() {
        mainView.SetActive(true);
        if (isActivated)
            Activate();
        else
            Deactivate();
        //StartCoroutine(_TurnOnScreen());
    }
    //IEnumerator _TurnOnScreen() {
        
    //}

    void SetOnClick(ButtonClick click) {
        MMLog.Log("ButtonCont", "black","Setting onClick of spell" + spellNum + " to " + click.ToString());
        onClick = click;
    }

    public void OnClick() {
        MMLog.Log("ButtonCont","black","Button " + spellNum + " clicked...");
        if (onClick != null)
            onClick();
        else
            MMLog.LogError("BUTTONCONT: Button was clicked and onClick is somehow null!");
    }

    public void ShowSpellInfo() {
        Spell currentSpell = mm.GetPlayer(playerId).character.GetSpell(spellNum);
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

        GetComponent<UITooltip>().tooltipInfo = currentSpell.info;
    }

    IEnumerator Transition_Cancel() {
        cancelView = Instantiate(simpleTextPF, this.transform, false);
        mainView.SetActive(false);

        cancelView.transform.Find("t").GetComponent<Text>().text = "Cancel";

        mainClick = onClick;
        SetOnClick(OnSpellCancelClick);
        //onClick = OnSpellCancelClick;

        yield return null;
    }

    public IEnumerator Transition_MainView() {
        if (newSpell) {
            MMLog.Log("BUTTONCONT", "black", "button" + spellNum + " showing new spell info");
            mainView.SetActive(true);
            ShowSpellInfo();
            mainView.SetActive(false);
            newSpell = false;
        }

        if(cancelView != null)
            Destroy(cancelView.gameObject);
        mainView.SetActive(true);

        SetOnClick(mainClick);
        //onClick = mainClick;

        yield return null;
    }

    public void SpellChanged() { newSpell = true; }

	void OnSpellButtonClick(){
        StartCoroutine(Transition_Cancel());
		StartCoroutine(mm._CastSpell (spellNum));
	}

    public void OnSpellCancelClick() {
        StartCoroutine(Transition_MainView());
        StartCoroutine(mm._CancelSpell());
    }

    public void Targeting_OnCancelClick() {
        MMLog.Log("BUTTONCONT", "black", "This currently does nothing. Thanks!");
        //mm.targeting.CancelTargeting();
    }

    public void Targeting_OnClearButton() {
        MMLog.Log("BUTTONCONT", "black", "This currently does nothing. Thanks!");
        //mm.targeting.ClearTargets();
    }

    public void OnDrawButtonClick() {
        mm.PlayerDrawTile();
    }

    public void OnFileButtonClick() {
        MMLog.Log("ButtonCont", "black", "Saving files...");
        mm.stats.SaveFiles();
    }

    public void DoAThing() {
        MMLog.Log("ButtonCont", "green", "Doing a thing!");
    }
}
