using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTools : MonoBehaviour {

    private MageMatch mm;
    private GameObject toolsMenu;
    private Dropdown dd_hex, dd_enchant, dd_property;
    private InputField input;
    private Button b_healthMode, b_player, b_ok;

    private string oldFunc;

	public void Init (MageMatch mm) {
        this.mm = mm;

        toolsMenu = GameObject.Find("toolsMenu");
        Transform t = toolsMenu.transform;
        dd_hex = t.Find("dd_hex").GetComponent<Dropdown>();
        dd_enchant = t.Find("dd_enchant").GetComponent<Dropdown>();
        dd_property = t.Find("dd_prop").GetComponent<Dropdown>();
        input = t.Find("input_dmg").GetComponent<InputField>();
        b_healthMode = t.Find("b_healthMode").GetComponent<Button>();
        b_player = t.Find("b_player").GetComponent<Button>();
        b_ok = t.Find("b_ok").GetComponent<Button>();

        ValueChanged("insert");
    }

    //void Update () {

    //}

    public void ValueChanged(string func) {
        if (func == oldFunc)
            return;
        oldFunc = func;
        MMDebug.MMLog.Log_UICont("DEBUGTOOLS: ValueChanged=" + func);

        switch (func) {
            case "insert":
                SetInputs(true, false, false, false, false);
                break;
            case "destroy":
                SetInputs(false, false, false, false, false);
                break;
            case "enchant":
                SetInputs(false, false, true, false, false);
                break;
            case "properties":
                SetInputs(false, true, false, false, false);
                break;
            case "clear":
                SetInputs(false, false, false, false, false);
                break;
            case "addToHand":
                SetInputs(true, false, false, false, true, "ADD");
                break;
            case "discard":
                SetInputs(false, false, false, false, false);
                break;
            case "changeHealth":
                SetInputs(false, false, false, true, true, "APPLY");
                break;
            case "changeMeter":
                SetInputs(false, false, false, true, true, "APPLY");
                break;
        }
    }

    public void SetInputs(bool hex, bool prop, bool ench, bool health, bool player, string buttonText = "") {
        dd_hex.interactable = hex;
        dd_property.interactable = prop;
        dd_enchant.interactable = ench;
        input.interactable = health;
        b_healthMode.interactable = health;
        b_player.interactable = player;
        if (buttonText == "")
            b_ok.interactable = false;
        else {
            b_ok.interactable = true;
            b_ok.transform.GetChild(0).GetComponent<Text>().text = buttonText;
        }
    }
}
