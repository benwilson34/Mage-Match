using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMDebug;
using System;

public class DebugTools : MonoBehaviour {

    private enum Submenu { System = 1, Report, Tools };
    private Submenu _submenu;

    public enum ToolMode { Insert, Destroy, Enchant, Clear, AddToHand, Discard, ChangeHealth, ChangeMeter, ChangeAP };
    public ToolMode currentMode = ToolMode.Insert; // private?

    public GameObject menus, systemMenu, reportMenu, toolsMenu;
    public Transform centerPositionTransform;
    // maybe other menus?

    public bool DebugMenuOpen { get { return _debugMenuOpen; } }
    private bool _debugMenuOpen = false;

    private enum DropdownType { None, Hex, Tile, Property, Enchantment };
    private List<string> _ddTileOptions, _ddCharmOptions, _ddPropertyOptions, _ddEnchantmentOptions;

    private MageMatch _mm;
    private Dropdown _dd_toolMode, _dd_options;
    private GameObject _inputBlock;
    private InputField _input;
    private Button _b_inputSign, _b_player, _b_ok;
    private Text _t_toolHelp;
    private Vector3 _debugMenuOrigPos, _debugMenuCenteredPos;

    private Text _debugGridText, _slidingText;
    private GameObject _debugItemPF;
    private Transform _debugContent;
    private GameObject _debugReport;
    private Text _debugReportText;
    private GameObject _systemMenuButton, _reportMenuButton, _toolsMenuButton;

    private int _playerId = 1;
    private bool _relativeDmgMode = true;
    private bool _positiveInputSign = true;

    private ToolMode _oldMode = ToolMode.ChangeAP;

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
        if (!_mm.IsDebugMode) {
            _toolsMenuButton.SetActive(false);
            toolsMenu.SetActive(false);
        } else {
            //toolsMenu.SetActive(true);
            Transform t = toolsMenu.transform;
            _debugMenuOrigPos = t.position;
            _debugMenuCenteredPos = Camera.main.WorldToScreenPoint(centerPositionTransform.position);
            _dd_toolMode = t.Find("dd_toolMode").GetComponent<Dropdown>();
            _dd_options = t.Find("dd_option").GetComponent<Dropdown>();
            _inputBlock = t.Find("InputBlock").gameObject;
            _b_inputSign = _inputBlock.transform.Find("b_sign")
                .GetComponent<Button>();
            _input = _inputBlock.transform.Find("input_amount")
                .GetComponent<InputField>();
            //_b_healthMode = t.Find("b_healthMode").GetComponent<Button>();
            _b_player = t.Find("b_player").GetComponent<Button>();
            _b_ok = t.Find("b_ok").GetComponent<Button>();
            _t_toolHelp = t.Find("t_toolHelp").GetComponent<Text>();

            string[] ddTileOptions = { "B-Fire", "B-Water", "B-Earth", "B-Air", "B-Muscle" };
            _ddTileOptions = new List<string>(ddTileOptions);
            // TODO append list of rune tiles
            _ddTileOptions.AddRange(RuneInfoLoader.GetTileList());

            _ddCharmOptions = new List<string>(RuneInfoLoader.GetCharmList());

            //_ddPropertyOptions = new List<string>();

            // TODO foreach Enchantment in enum
            string[] ddEnchOptions = { "Burning", "Zombie" };
            _ddEnchantmentOptions = new List<string>(ddEnchOptions);

            ToolModeChanged(0);
            _playerId = 2;
            TogglePlayer(); // just to get the right name
        }

        UpdateDebugGrid();

        _submenu = Submenu.System;
        SetPanesActive(true, false, false);
        menus.SetActive(false);

        //_mm.AddEventContLoadEvent(OnEventContLoaded);
    }

    //public void OnEventContLoaded() {
    //    EventController.AddTurnBeginEvent(OnTurnBegin, MMEvent.Behav.LastStep);
    //    EventController.AddTurnEndEvent(OnTurnEnd, MMEvent.Behav.LastStep);
    //    EventController.gameAction += OnGameAction;
    //}

    //public IEnumerator OnTurnBegin(int id) {
    //    UpdateEffTexts();
    //    yield return null;
    //}

    //public IEnumerator OnTurnEnd(int id) {
    //    UpdateEffTexts();
    //    yield return null;
    //}

    //public void OnGameAction(int id, int cost) {
    //    UpdateEffTexts(); // could be considerable overhead...
    //}

    public void ShowPane(int p) {
        _submenu = (Submenu)p;

        switch (p) {
            case 1:
                SetPanesActive(true, false, false);
                UpdateEffTexts();
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
        // logic separated from UI - important for replaying
        ToggleDebugActiveState(); 

        Text menuButtonText = GameObject.Find("MenuButtonText").GetComponent<Text>();
        if (_debugMenuOpen) {
            menuButtonText.text = "Close";
            UpdateEffTexts();
        } else {
            menuButtonText.text = "Debug";
        }
        menus.SetActive(_debugMenuOpen);
    }

    public void ToggleDebugActiveState() {
        //Report.ReportLine("$ DEBUG toggle", false);
        _debugMenuOpen = !_debugMenuOpen;
        if (_debugMenuOpen) {
            _mm.EnterState(MageMatch.State.DebugMenu);
            Report.ReportLine("$ DEBUG start", false);
        } else {
            _mm.ExitState();
            Report.ReportLine("$ DEBUG end", false);
        }
    }

    public void UpdateDebugGrid() {
        string grid = "   0  1  2  3  4  5  6 \n";
        for (int r = HexGrid.NUM_ROWS - 1; r >= 0; r--) {
            grid += r + " ";
            for (int c = 0; c < HexGrid.NUM_COLS; c++) {
                if (r <= HexGrid.TopOfColumn(c) && r >= HexGrid.BottomOfColumn(c)) {
                    if (HexGrid.IsCellFilled(c, r)) {
                        TileBehav tb = HexGrid.GetTileBehavAt(c, r);
                        if (tb.wasInvoked)
                            grid += "[*]";
                        else
                            grid += "[" + tb.tile.ElementsToString() + "]";

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

        LastingEffect[][] lists = EffectManager.GetLists();

        InstantiateEffectList(lists[0], "TurnBeginEffs");
        InstantiateEffectList(lists[1], "TurnEndEffs");
        InstantiateEffectList(lists[2], "HandChangeEffs");
        InstantiateEffectList(lists[3], "DropEffs");
        InstantiateEffectList(lists[4], "SwapEffs");
        InstantiateEffectList(lists[5], "HealthMods");
        InstantiateEffectList(lists[6], "TileEffs");        
    }

    void InstantiateEffectList(LastingEffect[] effs, string title) {
        Color lightBlue = new Color(.07f, .89f, .93f, .8f);
        if (effs.Length > 0) {
            InstantiateDebugEntry(lightBlue, "<b>"+title+":</b>", -1, -1, true);
            foreach (var e in effs) {
                InstantiateDebugEntry(Color.white, e.tag, e.countLeft, e.turnsLeft);
            }
        }
    }

    GameObject InstantiateDebugEntry(Color c, string title, int count, int turns, bool header = false) {
        GameObject item = Instantiate(_debugItemPF, _debugContent);
        item.GetComponent<Image>().color = c;
        Text tTitle = item.transform.Find("t_title").GetComponent<Text>();
        tTitle.text = title;
        if (!header) {
            tTitle.alignment = TextAnchor.MiddleLeft;
            item.transform.Find("t_count").GetComponent<Text>().text = count + "";
            item.transform.Find("t_turns").GetComponent<Text>().text = turns + "";
        }
        return item;
    }

    public void SaveFiles() {
        MMLog.Log("ButtonCont", "black", "Saving files...");
        var input = systemMenu.transform.Find("input_saveName").GetComponent<InputField>();
        Report.SaveFiles(input.text);
    }

    public void UpdateReport(string str) {
        _debugReportText.text = str;
    }


    #region ---------- TOOLS MENU ----------

    public void ToolModeChanged(int choice) {
        ToolMode mode = (ToolMode)_dd_toolMode.value;

        if (mode == _oldMode)
            return;
        _oldMode = mode;
        MMLog.Log_UICont("DEBUGTOOLS: ValueChanged=" + mode.ToString());

        switch (mode) {
            case ToolMode.Insert:
                currentMode = ToolMode.Insert;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Cell);
                SetInputs(DropdownType.Tile, false, true);
                SetMenuPosition(false);
                _t_toolHelp.text = "Click on a cell on the board to insert this tile. If there is already a tile in that spot, it will be destroyed.";
                break;
            case ToolMode.Destroy:
                currentMode = ToolMode.Destroy;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(DropdownType.None, false, false);
                SetMenuPosition(false);
                _t_toolHelp.text = "Click on a tile on the board to destroy it.";
                break;
            case ToolMode.Enchant:
                currentMode = ToolMode.Enchant;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(DropdownType.Enchantment, false, true);
                SetMenuPosition(false);
                _t_toolHelp.text = "Click on a tile on the board to enchant it.";
                break;
            //case "properties":
            //    currentMode = ToolMode.Properties;
            //    _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
            //    SetInputs(DropdownType.Property, false, false);
            //    break;
            case ToolMode.Clear:
                currentMode = ToolMode.Clear;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(DropdownType.None, false, false);
                SetMenuPosition(false);
                _t_toolHelp.text = "Click on a tile on the board to clear its enchantment (if any).";
                break;
            case ToolMode.AddToHand:
                currentMode = ToolMode.AddToHand;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.None);
                SetInputs(DropdownType.Hex, false, true, "ADD");
                SetMenuPosition(true);
                _t_toolHelp.text = "";
                break;
            case ToolMode.Discard:
                currentMode = ToolMode.Discard;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(DropdownType.None, false, false);
                SetMenuPosition(true);
                _t_toolHelp.text = "Click on a hex in either player's hand to discard it.";
                break;
            case ToolMode.ChangeHealth:
                currentMode = ToolMode.ChangeHealth;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.None);
                SetInputs(DropdownType.None, true, true, "APPLY");
                SetMenuPosition(false);
                _t_toolHelp.text = "Specify a (relative) amount to change health. Click the sign button to switch between add and subtract.";
                break;
            case ToolMode.ChangeMeter:
                currentMode = ToolMode.ChangeMeter;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.None);
                SetInputs(DropdownType.None, true, true, "APPLY");
                SetMenuPosition(false);
                _t_toolHelp.text = "Specify a (relative) amount to change meter. Click the sign button to switch between add and subtract.";

                break;
            case ToolMode.ChangeAP:
                currentMode = ToolMode.ChangeAP;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.None);
                SetInputs(DropdownType.None, true, true, "APPLY");
                SetMenuPosition(false);
                _t_toolHelp.text = "Specify a (relative) amount to change AP. Click the sign button to switch between add and subtract.";
                break;
        }
    }

    void SetInputs(DropdownType ddType, bool input, bool player, string buttonText = "") {
        if (ddType == DropdownType.None)
            _dd_options.gameObject.SetActive(false);
        else {
            _dd_options.gameObject.SetActive(true);

            switch (ddType) {
                case DropdownType.Hex:
                    _dd_options.ClearOptions();
                    var hexList = _ddTileOptions;
                    hexList.AddRange(_ddCharmOptions);
                    _dd_options.AddOptions(hexList);
                    _dd_options.value = 0;
                    break;
                case DropdownType.Tile:
                    _dd_options.ClearOptions();
                    _dd_options.AddOptions(_ddTileOptions);
                    _dd_options.value = 0;
                    break;
                case DropdownType.Property:
                    //_dd_options.ClearOptions();
                    //_dd_options.AddOptions(_ddPropertyOptions);
                    break;
                case DropdownType.Enchantment:
                    _dd_options.ClearOptions();
                    _dd_options.AddOptions(_ddEnchantmentOptions);
                    _dd_options.value = 0;
                    break;
            }
        }

        _inputBlock.SetActive(input);
        //_input.interactable = input;
        //_b_healthMode.interactable = input;
        _b_player.gameObject.SetActive(player);

        if (buttonText == "")
            _b_ok.gameObject.SetActive(false);
        else {
            _b_ok.gameObject.SetActive(true);
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
        _playerId = _playerId == 1 ? 2 : 1;

        var p = _mm.GetPlayer(_playerId);
        var str = string.Format("P{0} - {1} ({2})", _playerId, p.Name, p.Character.ch.ToString());
        _b_player.transform.GetChild(0).GetComponent<Text>().text = str;
    }

    int GetPlayerId() {
        // TODO just substitute usages?
        return _playerId;
    }

    int GetInputAmount() {
        int amt = int.Parse(_input.text);
        amt *= _positiveInputSign ? 1 : -1;
        return amt;
    }

    public void ToggleInputSign() {
        _positiveInputSign = !_positiveInputSign;
        if (_positiveInputSign)
            _b_inputSign.transform.GetChild(0).GetComponent<Text>().text = "+";
        else
            _b_inputSign.transform.GetChild(0).GetComponent<Text>().text = "-";
    }

    public void ToggleDmgMode() {
        //_relativeDmgMode = !_relativeDmgMode;
        //if(_relativeDmgMode)
        //    _b_healthMode.transform.GetChild(0).GetComponent<Text>().text = "rel";
        //else
        //    _b_healthMode.transform.GetChild(0).GetComponent<Text>().text = "abs";
    }

    void SetMenuPosition(bool centered) {
        toolsMenu.transform.position = 
            centered ? _debugMenuCenteredPos : _debugMenuOrigPos;
    }

    public void HandleInput(MonoBehaviour obj) {
        if (_submenu != Submenu.Tools)
            return;

        switch (currentMode) {
            case ToolMode.Insert:
                InsertMode_OnClick((CellBehav)obj);
                break;
            case ToolMode.Destroy:
                DestroyMode_OnClick((TileBehav)obj);
                break;
            case ToolMode.Enchant:
                EnchantMode_OnClick((TileBehav)obj);
                break;
            //case ToolMode.Properties:
            //    ApplyPropertiesMode_OnClick((TileBehav)obj);
            //    break;
            case ToolMode.Clear:
                ClearMode_OnClick((TileBehav)obj);
                break;
            case ToolMode.Discard:
                DiscardMode_OnClick((Hex)obj);
                break;
        }
    }

    public void HandleButtonClick() {
        switch (currentMode) {
            case ToolMode.AddToHand:
                AddToHandMode_OnClick();
                break;
            case ToolMode.ChangeHealth:
                ChangeHealthMode_OnClick();
                break;
            case ToolMode.ChangeMeter:
                ChangeMeterMode_OnClick();
                break;
            case ToolMode.ChangeAP:
                ChangeAPMode_OnClick();
                break;
        }
    }
    #endregion


    #region ---------- ACTUAL TOOLS ----------

    void InsertMode_OnClick(CellBehav cb) {
        Insert(GetHexGenTag(GetPlayerId()), cb.col, cb.row);
    }
    public void Insert(string hextag, int col, int row) {
        TileBehav insertTB = (TileBehav) HexManager.GenerateHex(_mm.ActiveP.ID, hextag);
        Report.ReportLine(string.Format("$ DEBUG INSERT {0} ({1},{2})", insertTB.hextag, col, row), false);
        CommonEffects.PutTile(insertTB, col, row);
        //insertTB.HardSetPos(cb.col, cb.row);
    }

    void DestroyMode_OnClick(TileBehav tb) {
        Destroy(tb.tile.col, tb.tile.row);
    }
    public void Destroy(int col, int row) { // maybe need to rename?
        //MMLog.Log("DebugTools", "orange", "calling destroy mode!");
        Report.ReportLine(string.Format("$ DEBUG DESTROY ({0},{1})", col, row), false);
        HexManager.RemoveTile(col, row, false);
    }

    public void Enchant(int col, int row, string ench) {
        TileBehav tb = HexGrid.GetTileBehavAt(col, row);
        EnchantMode_OnClick(tb, ench);
    }
    void EnchantMode_OnClick(TileBehav tb, string ench = "") {
        if(ench == "")
            ench = _dd_options.options[_dd_options.value].text;
        
        if (tb == null)
            MMLog.LogError("DEBUGTOOLS: Somehow enchant TB is null!");

        int id = GetPlayerId();
        switch (ench) {
            case "Burning":
                StartCoroutine(Burning.Set(id, tb));
                break;
            case "Zombie":
                MMLog.Log("DebugTools", "orange", "calling enchant, id=" + id);
                StartCoroutine(Zombie.Set(id, tb));
                break;
        }

        Report.ReportLine(string.Format("$ DEBUG ENCHANT {0} ({1},{2}) ", ench, tb.tile.col, tb.tile.row), false);
    }

    void ApplyPropertiesMode_OnClick(TileBehav tb) {
        // TODO get props from dd and apply to TB
    }

    public void Clear(int col, int row) {
        TileBehav tb = HexGrid.GetTileBehavAt(col, row);
        ClearMode_OnClick(tb);
    }
    void ClearMode_OnClick(TileBehav tb) {
        // TODO reset props
        //MMLog.Log("DebugTools", "orange", "Clearing " + tb.PrintCoord());
        tb.ClearEnchantment();
        Report.ReportLine(string.Format("$ DEBUG CLEAR ({0},{1})", tb.tile.col, tb.tile.row), false);
    }

    void AddToHandMode_OnClick() {
        AddToHand(GetPlayerId(), GetHexGenTag());
    }
    public void AddToHand(int id, string hextag) {
        Player p = _mm.GetPlayer(id);
        if (p.Hand.IsFull)
            return;

        Hex hex = HexManager.GenerateHex(id, hextag);

        p.Hand.Add(hex);

        StartCoroutine(hex.OnDraw());

        Report.ReportLine("$ DEBUG ADDTOHAND " + hex.hextag, false);
    }

    public void Discard(string hextag) {
        //int id = Hex.TagPlayer(hextag); // not safe for bounces
        Hex hex = null;
        for (int id = 1; id <= 2; id++) {
            if (_mm.GetPlayer(id).Hand.IsHexMine(hextag)) {
                hex = _mm.GetPlayer(id).Hand.GetHex(hextag);
                break;
            }
        }
        if (hex != null)
            DiscardMode_OnClick(hex);
        else
            Debug.LogWarning("DebugTools: could not find " + hextag + " in either player's hand...");
    }
    void DiscardMode_OnClick(Hex hex) {
        if (_mm.GetPlayer(1).Hand.IsHexMine(hex)) {
            _mm.GetPlayer(1).Hand.Discard(hex);
        } else if (_mm.GetPlayer(2).Hand.IsHexMine(hex)) {
            _mm.GetPlayer(2).Hand.Discard(hex);
        } else {
            MMLog.LogWarning("DebugTools: user clicked on a non-hand hex! Naughty!");
            return;
        }

        Report.ReportLine("$ DEBUG DISCARD " + hex.hextag, false);
    }

    void ChangeHealthMode_OnClick() {
        int amt = GetInputAmount();
        int pid = GetPlayerId();
        ChangeHealth(pid, amt);
    }
    public void ChangeHealth(int id, int amt) {
        // TODO relative vs absolute
        if (amt < 0)
            _mm.GetPC(id).SelfDamage(amt);
        else
            _mm.GetPC(id).Heal(amt);

        Report.ReportLine("$ DEBUG HEALTH p" + id + " " + amt, false);
    }

    void ChangeMeterMode_OnClick() {
        int amt = GetInputAmount() * 10; // so you can just type in percentage
        int pid = GetPlayerId();
        ChangeMeter(pid, amt);
    }
    public void ChangeMeter(int id, int amt) {
        _mm.GetPlayer(id).Character.ChangeMeter(amt);

        Report.ReportLine("$ DEBUG METER p" + id + " " + amt, false);
    }


    void ChangeAPMode_OnClick() {
        int amt = GetInputAmount();
        int pid = GetPlayerId();
        ChangeAP(pid, amt);
    }
    public void ChangeAP(int id, int amt) {
        // this will actually work whether it's positive or negative right now...
        _mm.GetPlayer(id).IncreaseAP(amt); 

        Report.ReportLine("$ DEBUG AP p" + id + " " + amt, false);
    }
    #endregion
}
