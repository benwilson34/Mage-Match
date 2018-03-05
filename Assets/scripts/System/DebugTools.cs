using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMDebug;

public class DebugTools : MonoBehaviour {

    public enum ToolMode { Insert, Destroy, Enchant, Properties, Clear, AddToHand, Discard, ChangeHealth, ChangeMeter };
    public ToolMode mode = ToolMode.Insert;

    private MageMatch _mm;
    private GameObject _toolsMenu;
    private Dropdown _dd_hex, _dd_enchant, _dd_property;
    private InputField _input;
    private Button _b_healthMode, _b_player, _b_ok;

    private int playerId = 1;
    private bool relativeDmgMode = true;

    private string oldFunc;

	public void Init (MageMatch mm) {
        this._mm = mm;

        _toolsMenu = GameObject.Find("ToolsMenu");
        Transform t = _toolsMenu.transform;
        _dd_hex = t.Find("dd_hex").GetComponent<Dropdown>();
        _dd_enchant = t.Find("dd_enchant").GetComponent<Dropdown>();
        _dd_property = t.Find("dd_prop").GetComponent<Dropdown>();
        _input = t.Find("input_dmg").GetComponent<InputField>();
        _b_healthMode = t.Find("b_healthMode").GetComponent<Button>();
        _b_player = t.Find("b_player").GetComponent<Button>();
        _b_ok = t.Find("b_ok").GetComponent<Button>();
    }

    //void Update () {

    //}


        // TODO break the inner part out into another method and just set a string here? or enum?
    public void ValueChanged(string func) {
        if (func == oldFunc)
            return;
        oldFunc = func;
        MMLog.Log_UICont("DEBUGTOOLS: ValueChanged=" + func);

        switch (func) {
            case "insert":
                mode = ToolMode.Insert;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Cell);
                SetInputs(true, false, false, false, false);
                break;
            case "destroy":
                mode = ToolMode.Destroy;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(false, false, false, false, false);
                break;
            case "enchant":
                mode = ToolMode.Enchant;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(false, false, true, false, false);
                break;
            case "properties":
                mode = ToolMode.Properties;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(false, true, false, false, false);
                break;
            case "clear":
                mode = ToolMode.Clear;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(false, false, false, false, false);
                break;
            case "addToHand":
                mode = ToolMode.AddToHand;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.None);
                SetInputs(true, false, false, false, true, "ADD");
                break;
            case "discard":
                mode = ToolMode.Discard;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.Hex);
                SetInputs(false, false, false, false, false);
                break;
            case "changeHealth":
                mode = ToolMode.ChangeHealth;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.None);
                SetInputs(false, false, false, true, true, "APPLY");
                break;
            case "changeMeter":
                mode = ToolMode.ChangeMeter;
                _mm.inputCont.SetDebugInputMode(InputController.InputContext.ObjType.None);
                SetInputs(false, false, false, true, true, "APPLY");
                break;
        }
    }

    public void SetInputs(bool hex, bool prop, bool ench, bool health, bool player, string buttonText = "") {
        _dd_hex.interactable = hex;
        _dd_property.interactable = prop;
        _dd_enchant.interactable = ench;
        _input.interactable = health;
        _b_healthMode.interactable = health;
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
        string selection = _dd_hex.options[_dd_hex.value].text;
        switch (selection) {
            case "Fire":
                return "p" + id + "-B-F";
            case "Water":
                return "p" + id + "-B-W";
            case "Earth":
                return "p" + id + "-B-E";
            case "Air":
                return "p" + id + "-B-A";
            case "Muscle":
                return "p" + id + "-B-M";
            default:
                MMLog.LogError("DEBUGTOOLS: Couldn't make tag from \"" + selection + "\"");
                return "";
        }
    }

    public void TogglePlayer() {
        if (playerId == 1)
            playerId = 2;
        else
            playerId = 1;

        _b_player.transform.GetChild(0).GetComponent<Text>().text = "P" + playerId;
    }

    int GetPlayerId() {
        return playerId;
    }

    public void ToggleDmgMode() {
        relativeDmgMode = !relativeDmgMode;
        if(relativeDmgMode)
            _b_healthMode.transform.GetChild(0).GetComponent<Text>().text = "relative";
        else
            _b_healthMode.transform.GetChild(0).GetComponent<Text>().text = "absolute";
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




    void InsertMode_OnClick(CellBehav cb) {
        TileBehav insertTB = (TileBehav) _mm.hexMan.GenerateHex(_mm.ActiveP().id, GetHexGenTag(1));
        _mm.PutTile(insertTB, cb.col, cb.row);
        //insertTB.HardSetPos(cb.col, cb.row);
    }

    void DestroyMode_OnClick(TileBehav tb) {
        MMLog.Log("DebugTools", "orange", "calling destroy mode!"); 
        _mm.hexMan.RemoveTile(tb.tile, false);
    }

    void EnchantMode_OnClick(TileBehav tb) {
        string option = _dd_enchant.options[_dd_enchant.value].text;
        switch (option) {
            case "Burning":
                StartCoroutine(_mm.hexFX.Ench_SetBurning(playerId, tb));
                break;
            //case "Cherrybomb":
            //    StartCoroutine(mm.hexFX.Ench_SetCherrybomb(id, tb));
            //    break;
            //case "Stone":
            //    mm.hexFX.Ench_SetStone(tb);
            //    break;
            case "Zombify":
                MMLog.Log("DebugTools", "orange", "calling enchant, id=" + playerId);
                StartCoroutine(_mm.hexFX.Ench_SetZombie(playerId, tb, false));
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
        StartCoroutine(_mm._Draw(GetPlayerId(), GetHexGenTag(), false));
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
