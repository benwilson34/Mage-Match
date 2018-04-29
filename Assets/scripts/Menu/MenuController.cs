using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    public enum Screen { MainMenu, Training, Multiplayer, Runebuilding, PlayerProfile, Store, Options, CharacterSelect, Prematch };

    public GameObject[] screens;

    private Stack<Screen> backStack;

	void Start () {
        UserData.Init();

        foreach (GameObject go in screens)
            go.GetComponent<MenuScreen>().OnLoad();

        backStack = new Stack<Screen>();
        ChangeScreens(Screen.MainMenu);
	}

    public void ChangeToMainMenu() { ChangeScreens(Screen.MainMenu); }
    public void ChangeToTraining() { ChangeScreens(Screen.Training); }
    public void ChangeToRunebuilding() { ChangeScreens(Screen.Runebuilding); }
    public void ChangeToMultiplayer() { ChangeScreens(Screen.Multiplayer); }
    public void ChangeToPlayerProfile() { ChangeScreens(Screen.PlayerProfile); }
    public void ChangeToStore() { ChangeScreens(Screen.Store); }
    public void ChangeToOptions() { ChangeScreens(Screen.Options); }
    public void ChangeToCharacterSelect() { ChangeScreens(Screen.CharacterSelect); }
    public void ChangeToPrematch() { ChangeScreens(Screen.Prematch); }

    GameObject GetScreenObj(Screen screen) {
        return screens[(int)screen];
    }

    MenuScreen GetMenuScreen(Screen screen) {
        return screens[(int)screen].GetComponent<MenuScreen>();
    }

    void ChangeScreens(Screen screen) {
        backStack.Push(screen);
        ActivateScreen(screen);
        GetMenuScreen(screen).OnShowScreen();
    }

    void ActivateScreen(Screen screen) {
        foreach (Screen s in Screen.GetValues(typeof(Screen))) {
            GetScreenObj(s).SetActive(s == screen);
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





public interface MenuScreen {
    void OnLoad();
    void OnShowScreen();
}
