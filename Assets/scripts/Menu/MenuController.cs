using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    public enum Screen { MainMenu, Training, Multiplayer, Runebuilding_CharList, Runebuilding_LoadoutList, Runebuilding_EditLoadout, PlayerProfile, Store, Options, CharacterSelect, Prematch };

    public GameObject[] screens;

    private string[] _screenTitles;
    private Text _tTitle;
    private GameObject _backButton;
    private Stack<Screen> _backStack;

	void Start () {
        UserData.Init();

        foreach (GameObject go in screens)
            go.GetComponent<MenuScreen>().OnLoad();

        Transform menus = GameObject.Find("scrolling").transform;
        _tTitle = menus.Find("t_title").GetComponent<Text>();
        _backButton = menus.Find("b_back").gameObject;

        _backStack = new Stack<Screen>();
        ChangeToMainMenu();
	}

    public void ChangeToMainMenu() { ChangeScreens(Screen.MainMenu); }
    public void ChangeToTraining() { ChangeScreens(Screen.Training); }
    public void ChangeToRunebuilding() { ChangeScreens(Screen.Runebuilding_CharList); }
    public void ChangeToRunebuilding_LoadoutList(Character.Ch ch) {
        ChangeScreens(Screen.Runebuilding_LoadoutList, ch);
    }
    public void ChangeToRunebuilding_EditLoadout(Character.Ch ch, LoadoutData loadout) {
        ChangeScreens(Screen.Runebuilding_EditLoadout, new object[2] { ch, loadout });
    }
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

    void ChangeScreens(Screen screen, object pass = null) {
        _backStack.Push(screen);
        ActivateScreen(screen);

        MenuScreen ms = GetMenuScreen(screen);
        if (pass != null)
            ms.OnPass(pass);
        ms.OnShowScreen();

    }

    void ActivateScreen(Screen screen) {
        foreach (Screen s in Screen.GetValues(typeof(Screen))) {
            GetScreenObj(s).SetActive(s == screen);
        }

        switch (screen) {
            case Screen.MainMenu:
                SetScreenBarActive(false);
                break;
            case Screen.Training:
                SetScreenBarActive(true, "Training!!");
                break;
            case Screen.Multiplayer:
                SetScreenBarActive(true, "Multiplayer!!");
                break;
            case Screen.Runebuilding_CharList:
                SetScreenBarActive(true, "Runebuilding!!");
                break;
            case Screen.Runebuilding_LoadoutList:
                SetScreenBarActive(true, "Runebuilding!!");
                break;
            case Screen.Runebuilding_EditLoadout:
                SetScreenBarActive(true, "Runebuilding!!");
                break;
            case Screen.PlayerProfile:
                SetScreenBarActive(true, "Player Profile!!");
                break;
            case Screen.Store:
                SetScreenBarActive(true, "Store!!");
                break;
            case Screen.Options:
                SetScreenBarActive(true, "Options!!");
                break;
            case Screen.CharacterSelect:
                SetScreenBarActive(true, "Character Select!!");
                break;
            case Screen.Prematch:
                SetScreenBarActive(false);
                break;
            default:
                break;
        }
    }

    void SetScreenBarActive(bool on, string title = "") {
        _tTitle.gameObject.SetActive(on);
        _backButton.SetActive(on);

        if (on)
            _tTitle.text = title;
    }

    public void GoBack() {
        Screen current = _backStack.Pop();
        if (current == Screen.CharacterSelect) { // don't really like this
            ClearSettingsObjs();
        }

        Screen back = _backStack.Peek();

        ActivateScreen(back);
        GetMenuScreen(back).OnBack();
    }

    public void ClearSettingsObjs() {
        Destroy(GameObject.Find("GameSettings"));
        var go = GameObject.Find("DebugSettings");
        if (go != null)
            Destroy(go);
    }

    public void CancelFromPrematch() {
        _backStack.Pop();
        _backStack.Pop();
        ActivateScreen(_backStack.Peek());
        ClearSettingsObjs();
    }

}





public class MenuScreen : Photon.PunBehaviour {
    public virtual void OnLoad() { }
    public virtual void OnPass(object o) { }
    public virtual void OnShowScreen() { }
    public virtual void OnBack() { }
}
