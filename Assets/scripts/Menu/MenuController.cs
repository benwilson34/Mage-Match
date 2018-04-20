using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    public enum Screen { MainMenu, Training, Multiplayer, PlayerProfile, CharacterSelect, Prematch };

    public GameObject[] screens;

    private Stack<Screen> backStack;

	void Start () {
        backStack = new Stack<Screen>();
        PlayerProfile.Init();

        ChangeScreens(Screen.MainMenu);
	}

    public void ChangeToMainMenu() { ChangeScreens(Screen.MainMenu); }
    public void ChangeToTraining() { ChangeScreens(Screen.Training); }
    public void ChangeToMultiplayer() { ChangeScreens(Screen.Multiplayer); }
    public void ChangeToPlayerProfile() { ChangeScreens(Screen.PlayerProfile); }
    public void ChangeToCharacterSelect(bool training) {
        ChangeScreens(Screen.CharacterSelect);
        GetScreen(Screen.CharacterSelect).GetComponent<CharacterSelect>().Init(training);
    }
    public void ChangeToPrematch(bool training) {
        ChangeScreens(Screen.Prematch);
        GetScreen(Screen.Prematch).GetComponent<Prematch>().Init(training);
    }

    GameObject GetScreen(Screen screen) {
        return screens[(int)screen];
    }

    void ChangeScreens(Screen screen) {
        backStack.Push(screen);
        ActivateScreen(screen);
    }

    void ActivateScreen(Screen screen) {
        foreach (Screen s in Screen.GetValues(typeof(Screen))) {
            GetScreen(s).SetActive(s == screen);
        }
    }

    public void GoBack() {
        Screen current = backStack.Pop();
        if (current == Screen.CharacterSelect) { // don't really like this
            ClearSettingsObjs();
        }

        Screen back = backStack.Peek();
        ActivateScreen(back);
    }

    public void ClearSettingsObjs() {
        Destroy(GameObject.Find("GameSettings"));
        var go = GameObject.Find("DebugSettings");
        if (go != null)
            Destroy(go);
    }

    public void CancelFromPrematch() {
        backStack.Pop();
        backStack.Pop();
        ActivateScreen(backStack.Peek());
        ClearSettingsObjs();
    }

}
