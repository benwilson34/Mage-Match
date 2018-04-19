using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Training : MonoBehaviour {

    private MenuController _menus;

    public void Start() {
        _menus = GameObject.Find("world ui").GetComponent<MenuController>();
    }

    public void StartTutorial() {
        // TODO
    }

    public void StartTraining() {
        DebugSettings dbs = new GameObject("DebugSettings").AddComponent<DebugSettings>();
        //dbs.applyAPcost = testSettingsMenu.transform.Find("tog_applyAPcosts").GetComponent<Toggle>().isOn;
        //dbs.onePlayerMode = testSettingsMenu.transform.Find("tog_onlyP1").GetComponent<Toggle>().isOn;
        //dbs.midiMode = testSettingsMenu.transform.Find("tog_midiMode").GetComponent<Toggle>().isOn;

        GameSettings settings = new GameObject("GameSettings").AddComponent<GameSettings>();

        //if (_toggles != null)
        //    gameSettings.turnTimerOn = _toggles.transform.Find("Toggle_TurnTimer").GetComponent<Toggle>().isOn;
        //else
        //    gameSettings.turnTimerOn = false; // not really needed?

        _menus.ChangeToCharacterSelect(true);
    }

    public void LoadTraining() {
        // TODO get filename from dropdown
    }
}
