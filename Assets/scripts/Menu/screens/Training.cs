using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Training : MenuScreen {

    private MenuController _menus;

    private Toggle _togTwoChar;
    private Transform _secondCharBlock;
    private Dropdown _ddSecondChar;

    public override void OnLoad() {
        _menus = GameObject.Find("world ui").GetComponent<MenuController>();

        _togTwoChar = transform.Find("tog_twoChar").GetComponent<Toggle>();
        _secondCharBlock = transform.Find("SecondChar");

        _ddSecondChar = _secondCharBlock.Find("dd_secondChar").GetComponent<Dropdown>();
        _ddSecondChar.ClearOptions();
        List<string> ddOptions = new List<string>();
        foreach (Character.Ch ch in Enum.GetValues(typeof(Character.Ch))) {
            if (ch != Character.Ch.Neutral) {
                ddOptions.Add(CharacterInfo.GetCharacterInfo(ch).name);
            }
        }
        _ddSecondChar.AddOptions(ddOptions);
    }

    //public void OnPass(object o) { }

    //public void OnShowScreen() { }

    public void StartTutorial() {
        // TODO
    }

    public void StartTraining() {
        DebugSettings dbs = new GameObject("DebugSettings").AddComponent<DebugSettings>();
        dbs.trainingMode = DebugSettings.TrainingMode.OneCharacter;
        if (_togTwoChar.isOn)
            dbs.trainingMode = DebugSettings.TrainingMode.TwoCharacters;

        GameSettings settings = new GameObject("GameSettings").AddComponent<GameSettings>();
        settings.trainingMode = true;

        if (_togTwoChar.isOn)
            settings.p2char = (Character.Ch)(_ddSecondChar.value + 1);

        //if (_toggles != null)
        //    gameSettings.turnTimerOn = _toggles.transform.Find("Toggle_TurnTimer").GetComponent<Toggle>().isOn;
        //else
        //    gameSettings.turnTimerOn = false; // not really needed?

        _menus.ChangeToCharacterSelect();
    }

    public void LoadTraining() {
        // TODO get filename from dropdown
    }

    public void OnTwoCharToggle() {
        _secondCharBlock.gameObject.SetActive(_togTwoChar.isOn);
    }
}
