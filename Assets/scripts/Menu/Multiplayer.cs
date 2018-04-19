using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiplayer : MonoBehaviour {

    private MenuController _menus;

    public void Start() {
        _menus = GameObject.Find("world ui").GetComponent<MenuController>();
    }

    public void QuickMatch() {
        GameSettings settings = new GameObject("GameSettings").AddComponent<GameSettings>();

        //if (_toggles != null)
        //    gameSettings.turnTimerOn = _toggles.transform.Find("Toggle_TurnTimer").GetComponent<Toggle>().isOn;
        //else
        //    gameSettings.turnTimerOn = false; // not really needed?

        _menus.ChangeToCharacterSelect(false);
    }
}
