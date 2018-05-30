using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunebuildingEditLoadout : MenuScreen {

    public GameObject renameDialog;

    private const int DECK_MIN = 80, DECK_MAX = 120;

    private Character.Ch _character;
    private LoadoutData _loadout;

    private Button _bSave;
    private Text _tLoadoutButton;
    private Image[] _slots;
    private Sprite _emptySlot;
    private Image _deckCountMeter;
    private Text _deckCountText;
    private Transform _scrRuneList;
    private GameObject _runeListItemPF;

    private RuneInfoLoader.RuneInfo[] _usedRunes;
    private int _deckCount;

    private InputField _inputNewName;

    public override void OnLoad() {
        _bSave = transform.Find("b_save").GetComponent<Button>();

        _tLoadoutButton = transform.Find("b_loadout").GetChild(0).GetComponent<Text>();

        Transform hexagram = transform.Find("i_hexagram");
        _slots = new Image[LoadoutData.RUNE_COUNT];
        for (int i = 0; i < LoadoutData.RUNE_COUNT; i++)
            _slots[i] = hexagram.GetChild(i).GetComponent<Image>();
        _emptySlot = _slots[0].sprite;

        Transform meter = transform.Find("Meter");
        _deckCountMeter = meter.Find("i_meter").GetComponent<Image>();
        _deckCountText = meter.Find("t_deckCount").GetComponent<Text>();

        Transform scrRunes = transform.Find("scr_runes");
        scrRunes.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
        _scrRuneList = scrRunes.Find("Viewport").Find("Content");
        _runeListItemPF = Resources.Load("prefabs/menu/runebuildingRuneListItem") as GameObject;

        _inputNewName = renameDialog.transform.GetChild(0)
            .Find("input_newName").GetComponent<InputField>();
        SetRenameDialogActive(false);
    }

    public override void OnPass(object o) {
        var objs = (object[])o;
        _character = (Character.Ch)objs[0];
        _loadout = (LoadoutData)objs[1];
    }

    public override void OnShowScreen() {
        _bSave.interactable = false;

        transform.Find("b_char").GetComponentInChildren<Text>()
            .text = CharacterInfo.GetCharacterInfo(_character).name;

        transform.Find("b_loadout").GetComponentInChildren<Text>()
            .text = _loadout.name;

        //for (int i = 0; i < LoadoutData.RUNE_COUNT; i++) {
        //    var rune = _loadout.runes[i];
        //    _slots[i].sprite = RuneInfoLoader.GetRuneSprite(_character, rune);
        //    _deckCount += RuneInfoLoader.GetRuneInfo(_character, rune).deckCount;
        //}

        _deckCount = 0;
        PopulateRuneList();
        UpdateDeckCountMeter();
    }

    void PopulateRuneList() {
        foreach (Transform child in _scrRuneList)
            GameObject.Destroy(child.gameObject);

        _usedRunes = new RuneInfoLoader.RuneInfo[LoadoutData.RUNE_COUNT];
        int usedCount = 0;
        foreach (var info in RuneInfoLoader.GetRuneList(_character)) {
            bool used = false;
            foreach (string rune in _loadout.runes) {
                if (info.tagTitle == rune) {
                    _usedRunes[usedCount] = info;
                    _slots[usedCount].sprite = RuneInfoLoader.GetRuneSprite(_character, rune);
                    _deckCount += RuneInfoLoader.GetRuneInfo(_character, rune).deckCount;
                    usedCount++;
                    used = true;
                }
            }
            AddRuneEntry(info, used); 
        }
    }

    void AddRuneEntry(RuneInfoLoader.RuneInfo info, bool flip) {
        Transform item = Instantiate(_runeListItemPF, _scrRuneList).transform;

        var runebuildingRune = item.GetComponent<RunebuildingRune>();
        runebuildingRune.Init(this, _character, info);

        if (flip)
            runebuildingRune.ToggleUsed();
    }

    public bool AddUsedRune(RuneInfoLoader.RuneInfo rune) {
        for (int i = 0; i < _usedRunes.Length; i++) {
            if (_usedRunes[i] == null) {
                _usedRunes[i] = rune;
                _slots[i].sprite = RuneInfoLoader.GetRuneSprite(_character, rune.tagTitle);
                _deckCount += rune.deckCount;
                UpdateDeckCountMeter();
                UpdateSaveButton();
                return true;
            }
        }
        return false;
    }

    public void RemoveUsedRune(RuneInfoLoader.RuneInfo rune) {
        for (int i = 0; i < _usedRunes.Length; i++) {
            if (_usedRunes[i] != null && _usedRunes[i].tagTitle == rune.tagTitle) {
                _usedRunes[i] = null;
                _slots[i].sprite = _emptySlot;
                _deckCount -= rune.deckCount;
                UpdateDeckCountMeter();
                _bSave.interactable = false;
                return;
            }
        }
    }

    public void UpdateDeckCountMeter() {
        _deckCountText.text = "Deck Count: " + _deckCount;
        _deckCountMeter.fillAmount = (float)_deckCount / DECK_MAX;
        if (_deckCount < DECK_MIN)
            _deckCountMeter.color = Color.red;
        else
            _deckCountMeter.color = Color.green;
    }

    public void UpdateSaveButton() {
        bool active = true;
        for (int i = 0; i < _usedRunes.Length; i++) {
            if (_usedRunes[i] == null) {
                active = false;
            }
        }

        _bSave.interactable = active;
    }

    public void OnCharacterButtonClick() {
        // TODO go to info screen?
    }

    public void OnLoadoutNameButtonClick() {
        SetRenameDialogActive(true);
    }

    public void OnSaveButtonClick() {
        // TODO serialize json
        _loadout.runes = (from rune in _usedRunes select rune.tagTitle).ToArray<string>();
        Debug.Log(string.Join(", ", _loadout.runes));
        LoadoutData.SaveLoadout(_loadout);
        // TODO go back thru menu
        GameObject.Find("world ui").GetComponent<MenuController>().GoBack();
    }


    // ----------- rename dialog ----------

    public void SetRenameDialogActive(bool on) {
        renameDialog.SetActive(on);

        if (on) {
            _inputNewName.text = _loadout.name;
        }
    }

    public void OnCancelButtonClick() {
        SetRenameDialogActive(false);
    }

    public void OnOkButtonClick() {
        _loadout.name = _inputNewName.text;
        SetRenameDialogActive(false);
        _tLoadoutButton.text = _loadout.name;
    }
}
