using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiplayer : MonoBehaviour, MenuScreen {

    private MenuController _menus;

    public void OnLoad() {
        _menus = GameObject.Find("world ui").GetComponent<MenuController>();
    }

    public void OnShowScreen() { }

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
