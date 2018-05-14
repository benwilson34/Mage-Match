using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiplayer : MenuScreen {

    private MenuController _menus;

    public override void OnLoad() {
        _menus = GameObject.Find("world ui").GetComponent<MenuController>();
    }

    public override void OnPass(object o) { }

    public override void OnShowScreen() { }

    public void QuickMatch() {
        GameSettings settings = new GameObject("GameSettings").AddComponent<GameSettings>();
        settings.trainingMode = false;

        //if (_toggles != null)
        //    gameSettings.turnTimerOn = _toggles.transform.Find("Toggle_TurnTimer").GetComponent<Toggle>().isOn;
        //else
        //    gameSettings.turnTimerOn = false; // not really needed?

        _menus.ChangeToCharacterSelect();
    }
}
