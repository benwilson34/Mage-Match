using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMDebug;

public class DebugTools : MonoBehaviour {

    public enum ToolMode { Insert, Destroy, Enchant, Properties, Clear, AddToHand, Discard, ChangeHealth, ChangeMeter };
    public ToolMode mode = ToolMode.Insert;

    public GameObject menus, systemMenu, reportMenu, toolsMenu;
    // maybe other menus?

    private enum DropdownType { None, Hex, Tile, Property, Enchantment };
    private List<string> _ddTileOptions, _ddCharmOptions, _ddPropertyOptions, _ddEnchantmentOptions;

    private MageMatch _mm;
    private Dropdown _dd_options;
    private InputField _input;
    private Button _b_healthMode, _b_player, _b_ok;

    private Text _debugGridText, _slidingText;
    private GameObject _debugItemPF;
    private Transform _debugContent;
    private GameObject _debugReport;
    private Text _debugReportText;
    private GameObject _systemMenuButton, _reportMenuButton, _toolsMenuButton;

    private int _playerId = 1;
    private bool _relativeDmgMode = true;

    private string _oldFunc;

	public void Init(MageMatch mm) {
        this._mm = mm;
        //menus.SetActive(true);

        // System pane
        //systemMenu.SetActive(true);
        _systemMenuButton = menus.transform.Find("b_system").gameObject;

        _debugItemPF = Resources.Load("prefabs/ui/debug_statusItem") as GameObject;

        Transform scroll = systemMenu.transform.Find("scr_debugEffects");
        _debugContent = scroll.transform.Find("Viewport").Find("Content");
        _debugGridText = systemMenu.transform.Find("t_debugGrid").GetComponent<Text>(); // UI debug grid

        // Report pane
        //reportMenu.SetActive(true);
        _reportMenuButton = menus.transform.Find("b_report").gameObject;
        _debugReport = reportMenu.transform.Find("scr_report").gameObject;
        _debugReportText = _debugReport.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();

        // Tools pane
        _toolsMenuButton = menus.transform.Find("b_tools").gameObject;
        if (!_mm.IsDebugMode()) {
            _toolsMenuButton.SetActive(false);
            toolsMenu.SetActive(false);
        } else {
            //toolsMenu.SetActive(true);
            Transform t = toolsMenu.transform;
            _dd_options = t.Find("dd_option").GetComponent<Dropdown>();
            _input = t.Find("input_dmg").GetComponent<InputField>();
            _b_healthMode = t.Find("b_healthMode").GetComponent<Button>();
            _b_player = t.Find("b_player").GetComponent<Button>();
            _b_ok = t.Find("b_ok").GetComponent<Button>();

            string[] ddTileOptions = { "B-F", "B-W", "B-E", "B-A", "B-M" };
            _ddTileOptions = new List<string>(ddTileOptions);
            // TODO append list of rune tiles
            _ddTileOptions.AddRange(RuneInfo.GetTileList());

            _ddCharmOptions = new List<string>(RuneInfo.GetCharmList());

            //_ddPropertyOptions = new List<string>();

            string[] ddEnchOptions = { "Burning", "Zombie" };
            _ddEnchantmentOptions = new List<string>(ddEnchOptions);
        }

        UpdateDebugGrid();

        ShowPane(1);
        menus.SetActive(false);
    }

    public void InitEvents() {
        _mm.eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.LastStep);
        _mm.eventCont.AddTurnEndEvent(OnTurnEnd, EventController.Type.LastStep);
        _mm.eventCont.gameAction += OnGameAction;
    }

    public IEnumerator OnTurnBegin(int id) {
        UpdateEffTexts();
        yield return null;
    }

    public IEnumerator OnTurnEnd(int id) {
        UpdateEffTexts();
        yield return null;
    }

    public void OnGameAction(int id, bool costsAP) {
        UpdateEffTexts(); // could be considerable overhead...
    }

    public void ShowPane(int p) {
        switch (p) {
            case 1:
                SetPanesActive(true, false, false);
                break;
            case 2:
                SetPanesActive(false, true, false);
                break;
            case 3:
                SetPanesActive(false, false, true);
                break;
        }
    }

    void SetPanesActive(bool system, bool report, bool tools) {
        systemMenu.SetActive(system);
        SetButtonColor(_systemMenuButton, system);

        reportMenu.SetActive(report);
        SetButtonColor(_reportMenuButton, report);

        toolsMenu.SetActive(tools);
        SetButtonColor(_toolsMenuButton, tools);
    }

    void SetButtonColor(GameObject go, bool active) {
        Image bg = go.GetComponent<Image>();
        if (active)
            bg.color = Color.white;
        else
            bg.color = Color.grey;
    }

    public void ToggleDebugMenu() {
        bool openingMenu = !menus.GetActive();
        Text menuButtonText = GameObject.Find("MenuButtonText").GetComponent<Text>();
        if (openingMenu) {
            menuButtonText.text = "Close";
            _mm.EnterState(MageMatch.State.DebugMenu);
        } else {
            menuButtonText.text = "Debug";
            _mm.ExitState();
        }
        menus.SetActive(openingMenu);
    }

    public bool IsDebugMenuOpen() { return menus.GetActive(); }

    public void UpdateDebugGrid() {
        string grid = "   0  1  2  3  4  5  6 \n";
        for (int r = HexGrid.NUM_ROWS - 1; r >= 0; r--) {
            grid += r + " ";
            for (int c = 0; c < HexGrid.NUM_COLS; c++) {
                if (r <= _mm.hexGrid.TopOfColumn(c) && r >= _mm.hexGrid.BottomOfColumn(c)) {
                    if (_mm.hexGrid.IsCellFilled(c, r)) {
                        TileBehav tb = _mm.hexGrid.GetTileBehavAt(c, r);
                        if (tb.wasInvoked)
                            grid += "[*]";
                        else
                            grid += "[" + tb.tile.ThisElementToChar() + "]";

                    } else
                        grid += "[ ]";
                } else
                    grid += " - ";
            }
            grid += '\n';
        }
        _debugGridText.text = grid;
    }

    public void UpdateEffTexts() {
        foreach (Transform child in _debugContent) {
            Destroy(child.gameObject);
        }

        object[] lists = _mm.effectCont.GetLists();
        List<GameObject> debugItems = new List<GameObject>();

        Color lightBlue = new Color(.07f, .89f, .93f, .8f);

        List<Effect> beginTurnEff = (List<Effect>)lists[0];
        if (beginTurnEff.Count > 0) {
            debugItems.Add(InstantiateDebugEntry("<b>BeginTurnEffs:</b>", lightBlue));
            foreach (Effect e in beginTurnEff) {
                debugItems.Add(InstantiateDebugEntry(e.tag, Color.white));
            }
        }

        List<Effect> endTurnEff = (List<Effect>)lists[1];
        if (endTurnEff.Count > 0) {
            debugItems.Add(InstantiateDebugEntry("<b>EndTurnEffs:</b>", lightBlue));
            foreach (Effect e in endTurnEff) {
                debugItems.Add(InstantiateDebugEntry(e.tag, Color.white));
            }
        }

        List<DropEffect> dropEff = (List<DropEffect>)lists[2];
        if (dropEff.Count > 0) {
            debugItems.Add(InstantiateDebugEntry("<b>DropEffs:</b>", lightBlue));
            foreach (DropEffect e in dropEff) {
                debugItems.Add(InstantiateDebugEntry(e.tag, Color.white));
            }
        }

        List<SwapEffect> swapEff = (List<SwapEffect>)lists[3];
        if (swapEff.Count > 0) {
            debugItems.Add(InstantiateDebugEntry("<b>SwapEffs:</b>", lightBlue));
            foreach (SwapEffect e in swapEff) {
                debugItems.Add(InstantiateDebugEntry(e.tag, Color.white));
            }
        }
    }

    GameObject InstantiateDebugEntry(string t, Color c) {
        GameObject item = Instantiate(_debugItemPF, _debugContent);
        item.GetComponent<Image>().color = c;
        item.transform.Find("Text").GetComponent<Text>().text = t;
        return item;
    }

    public void SaveFiles() {
        MMLog.Log("ButtonCont", "black", "Saving files...");
        _mm.stats.SaveFiles();
    }

    public void UpdateReport(string str) {
        _debugReportText.text = str;
    }


    // ---------- TOOLS MENU ----------

    public void ValueChanged(string func) {
        if (func == _oldFunc)
            return;
        _oldFunc = func;
        MMLog.Log_UICont("DEBUGTOOLS: ValueChanged=" + func);

        switch (func) {
            case "insert":
                mode = ToolMode.Insert;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Cell);
                SetInputs(DropdownType.Hex, false, true);
                break;
            case "destroy":
                mode = ToolMode.Destroy;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(DropdownType.None, false, false);
                break;
            case "enchant":
                mode = ToolMode.Enchant;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(DropdownType.Enchantment, false, false);
                break;
            case "properties":
                mode = ToolMode.Properties;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(DropdownType.Property, false, false);
                break;
            case "clear":
                mode = ToolMode.Clear;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(DropdownType.None, false, false);
                break;
            case "addToHand":
                mode = ToolMode.AddToHand;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.None);
                SetInputs(DropdownType.Hex, false, true, "ADD");
                break;
            case "discard":
                mode = ToolMode.Discard;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(DropdownType.None, false, false);
                break;
            case "changeHealth":
                mode = ToolMode.ChangeHealth;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.None);
                SetInputs(DropdownType.None, true, true, "APPLY");
                break;
            case "changeMeter":
                mode = ToolMode.ChangeMeter;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.None);
                SetInputs(DropdownType.None, true, true, "APPLY");
                break;
        }
    }

    void SetInputs(DropdownType ddType, bool input, bool player, string buttonText = "") {
        if (ddType == DropdownType.None)
            _dd_options.interactable = false;
        else {
            _dd_options.interactable = true;

            switch (ddType) {
                case DropdownType.Hex:
                    _dd_options.ClearOptions();
                    var hexList = _ddTileOptions;
                    hexList.AddRange(_ddCharmOptions);
                    _dd_options.AddOptions(hexList);
                    break;
                case DropdownType.Tile:
                    _dd_options.ClearOptions();
                    _dd_options.AddOptions(_ddCharmOptions);
                    break;
                case DropdownType.Property:
                    //_dd_options.ClearOptions();
                    //_dd_options.AddOptions(_ddPropertyOptions);
                    break;
                case DropdownType.Enchantment:
                    _dd_options.ClearOptions();
                    _dd_options.AddOptions(_ddEnchantmentOptions);
                    break;
            }
        }

        _input.interactable = input;
        //_b_healthMode.interactable = input;
        _b_player.interactable = player;

        if (buttonText == "")
            _b_ok.interactable = false;
        else {
            _b_ok.interactable = true;
            _b_ok.transform.GetChild(0).GetComponent<Text>().text = buttonText;
        }
    }

    string GetHexGenTag() {
        return GetHexGenTag(GetPlayerId());
    }

    string GetHexGenTag(int id) {
        string selection = _dd_options.options[_dd_options.value].text;
        return "p" + id + "-" + selection;
    }

    public void TogglePlayer() {
        if (_playerId == 1)
            _playerId = 2;
        else
            _playerId = 1;

        _b_player.transform.GetChild(0).GetComponent<Text>().text = "P" + _playerId;
    }

    int GetPlayerId() {
        return _playerId;
    }

    public void ToggleDmgMode() {
        _relativeDmgMode = !_relativeDmgMode;
        if(_relativeDmgMode)
            _b_healthMode.transform.GetChild(0).GetComponent<Text>().text = "rel";
        else
            _b_healthMode.transform.GetChild(0).GetComponent<Text>().text = "abs";
    }

    public void HandleInput(MonoBehaviour obj) {
        switch (mode) {
            case ToolMode.Insert:
                InsertMode_OnClick((CellBehav)obj);
                break;
            case ToolMode.Destroy:
                DestroyMode_OnClick((TileBehav)obj);
                break;
            case ToolMode.Enchant:
                EnchantMode_OnClick((TileBehav)obj);
                break;
            case ToolMode.Properties:
                ApplyPropertiesMode_OnClick((TileBehav)obj);
                break;
            case ToolMode.Clear:
                ClearMode_OnClick((TileBehav)obj);
                break;
            case ToolMode.Discard:
                DiscardMode_OnClick((Hex)obj);
                break;
        }
    }

    public void HandleButtonClick() {
        switch (mode) {
            case ToolMode.AddToHand:
                AddToHandMode_OnClick();
                break;
            case ToolMode.ChangeHealth:
                ChangeHealthMode_OnClick();
                break;
            case ToolMode.ChangeMeter:
                ChangeMeterMode_OnClick();
                break;
        }
    }


    // ---------- ACTUAL TOOLS ----------

    void InsertMode_OnClick(CellBehav cb) {
        string genTag = GetHexGenTag(GetPlayerId());
        TileBehav insertTB = (TileBehav) _mm.hexMan.GenerateHex(_mm.ActiveP().id, genTag);
        _mm.PutTile(insertTB, cb.col, cb.row);
        //insertTB.HardSetPos(cb.col, cb.row);
    }

    void DestroyMode_OnClick(TileBehav tb) {
        MMLog.Log("DebugTools", "orange", "calling destroy mode!"); 
        _mm.hexMan.RemoveTile(tb.tile, false);
    }

    void EnchantMode_OnClick(TileBehav tb) {
        string option = _dd_options.options[_dd_options.value].text;
        switch (option) {
            case "Burning":
                StartCoroutine(_mm.hexFX.Ench_SetBurning(_playerId, tb));
                break;
            //case "Cherrybomb":
            //    StartCoroutine(mm.hexFX.Ench_SetCherrybomb(id, tb));
            //    break;
            //case "Stone":
            //    mm.hexFX.Ench_SetStone(tb);
            //    break;
            case "Zombie":
                MMLog.Log("DebugTools", "orange", "calling enchant, id=" + _playerId);
                StartCoroutine(_mm.hexFX.Ench_SetZombie(_playerId, tb, false));
                break;
        }
    }

    void ApplyPropertiesMode_OnClick(TileBehav tb) {
        // TODO get props from dd and apply to TB
    }

    void ClearMode_OnClick(TileBehav tb) {
        // TODO reset props
        tb.ClearEnchantment();
    }

    void AddToHandMode_OnClick() {
        int id = GetPlayerId();
        Player p = _mm.GetPlayer(id);
        if (p.hand.IsFull())
            return;

        Hex hex = _mm.hexMan.GenerateHex(id, GetHexGenTag());
        if (id != _mm.myID)
            hex.Flip();

        p.hand.Add(hex);

        StartCoroutine(hex.OnDraw());
    }

    void DiscardMode_OnClick(Hex h) {
        if (_mm.GetPlayer(1).IsHexMine(h)) {
            _mm.GetPlayer(1).Discard(h);
        } else if (_mm.GetPlayer(2).IsHexMine(h)) {
            _mm.GetPlayer(2).Discard(h);
        } else
            MMLog.LogWarning("DebugTools: user clicked on a non-hand hex! Naughty!");
    }

    void ChangeHealthMode_OnClick() {
        // TODO relative vs absolute
        int amt = int.Parse(_input.text);
        int pid = GetPlayerId();
        if (amt < 0)
            _mm.GetPC(pid).SelfDamage(amt);
        else
            _mm.GetPC(pid).Heal(amt);
    }

    void ChangeMeterMode_OnClick() {
        int amt = int.Parse(_input.text);
        _mm.GetPlayer(GetPlayerId()).character.ChangeMeter(amt);
    }
}
