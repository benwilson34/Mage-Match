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

	private MageMatch _mm;
    private Button _button;
    private GameObject _simpleTextPF;
    private GameObject _mainView, _cancelView;
    private ButtonClick _onClick, _mainClick;
    private int _playerId;
    private bool _newSpell = false;
    private bool _isActivated = false, _interactable = false;

    public void Init(MageMatch mm, int id) {
        MMLog.Log("ButtonCont", "black", "Init button " + spellNum + " with id="+id);
        this._mm = mm;
        this._playerId = id;

        _button = this.GetComponent<Button>();

        if(spellNum >= 0)
            _mainView = transform.Find("main").gameObject;
        _simpleTextPF = Resources.Load<GameObject>("prefabs/ui/simpleTextView");

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

        if(_onClick == null)
            MMLog.LogError("BUTTONCONT: Button onClick is somehow null!");
    }

    public void SetInteractable() {
        _button.interactable = true;
        _interactable = true;
    }

    //public bool IsInteractable() { return interactable && isActivated; }

    public void Activate() {
        _isActivated = true; // maybe not needed?
        if (_interactable)
            _button.interactable = true;
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
        _isActivated = false; // maybe not needed?
        if (_interactable)
            _button.interactable = false;
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
        _mainView.SetActive(false);
        if (_interactable)
            _button.interactable = false;
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
        _mainView.SetActive(true);
        if (_isActivated)
            Activate();
        else
            Deactivate();
        //StartCoroutine(_TurnOnScreen());
    }
    //IEnumerator _TurnOnScreen() {
        
    //}

    void SetOnClick(ButtonClick click) {
        MMLog.Log("ButtonCont", "black","Setting onClick of spell" + spellNum + " to " + click.ToString());
        _onClick = click;
    }

    public void OnClick() {
        MMLog.Log("ButtonCont","black","Button " + spellNum + " clicked...");
        if (_onClick != null)
            _onClick();
        else
            MMLog.LogError("BUTTONCONT: Button was clicked and onClick is somehow null!");
    }

    public void ShowSpellInfo() {
        Spell currentSpell = _mm.GetPlayer(_playerId).character.GetSpell(spellNum);
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
                    minitile.sprite = _mm.uiCont.miniFire;
                    break;
                case Tile.Element.Water:
                    minitile.sprite = _mm.uiCont.miniWater;
                    break;
                case Tile.Element.Earth:
                    minitile.sprite = _mm.uiCont.miniEarth;
                    break;
                case Tile.Element.Air:
                    minitile.sprite = _mm.uiCont.miniAir;
                    break;
                case Tile.Element.Muscle:
                    minitile.sprite = _mm.uiCont.miniMuscle;
                    break;
                case Tile.Element.None:
                    minitile.gameObject.SetActive(false);
                    break;
            }
        }

        GetComponent<UITooltip>().tooltipInfo = currentSpell.info;
    }

    IEnumerator Transition_Cancel() {
        _cancelView = Instantiate(_simpleTextPF, this.transform, false);
        _mainView.SetActive(false);

        _cancelView.transform.Find("t").GetComponent<Text>().text = "Cancel";

        _mainClick = _onClick;
        SetOnClick(OnSpellCancelClick);
        //onClick = OnSpellCancelClick;

        yield return null;
    }

    public IEnumerator Transition_MainView() {
        if (_newSpell) {
            MMLog.Log("BUTTONCONT", "black", "button" + spellNum + " showing new spell info");
            _mainView.SetActive(true);
            ShowSpellInfo();
            _mainView.SetActive(false);
            _newSpell = false;
        }

        if(_cancelView != null)
            Destroy(_cancelView.gameObject);
        _mainView.SetActive(true);

        SetOnClick(_mainClick);
        //onClick = mainClick;

        yield return null;
    }

    public void SpellChanged() { _newSpell = true; }

	void OnSpellButtonClick(){
        StartCoroutine(Transition_Cancel());
		StartCoroutine(_mm._CastSpell (spellNum));
	}

    public void OnSpellCancelClick() {
        StartCoroutine(Transition_MainView());
        StartCoroutine(_mm._CancelSpell());
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
        _mm.PlayerDrawHex();
    }

    public void OnFileButtonClick() {
        MMLog.Log("ButtonCont", "black", "Saving files...");
        _mm.stats.SaveFiles();
    }

    public void DoAThing() {
        MMLog.Log("ButtonCont", "green", "Doing a thing!");
    }
}
