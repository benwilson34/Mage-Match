using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour, MenuScreen {

    //private Transform characterBlock;
    private Image _charPortraitFrame, _charPortrait;
    private Text _charName, _charT;
    private Button _bConfirm;
    private Dropdown _ddLoadouts;
    private UserData.LoadoutData[] _loadouts;
    private GameObject goldSelectionPF, redSelectionPF;

    private GameSettings _gameSettings;
    private Character.Ch _localChar;
    private bool _thisPlayerLocked = false;

    private MenuController _menu;

    public void OnLoad() {
        //characterBlock = transform.Find("characterBlock");
        _charPortraitFrame = transform.Find("i_charPortraitFrame").GetComponent<Image>();
        _charPortrait = _charPortraitFrame.transform.Find("i_charPortrait").GetComponent<Image>();
        _charName = transform.Find("t_charName").GetComponent<Text>();
        //charT = transform.Find("t_charInfo").GetComponent<Text>();

        _bConfirm = transform.Find("b_confirm").GetComponent<Button>();

        _ddLoadouts = transform.Find("dd_loadouts").GetComponent<Dropdown>();

        _menu = GameObject.Find("world ui").GetComponent<MenuController>();
    }

    public void OnShowScreen() {
        _thisPlayerLocked = false;
        _gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        _charName.text = "";
        _charPortraitFrame.color = Color.clear;
        _charPortrait.enabled = false;
        //charT.text = "";
        _bConfirm.interactable = false;
        _ddLoadouts.ClearOptions();
        _ddLoadouts.interactable = false;

        Text tGamemode = transform.Find("t_gameMode").GetComponent<Text>();
        if (_gameSettings.trainingMode)
            tGamemode.text = "Select character for Training";
        else
            tGamemode.text = "Select character for Online Battle";
    }

    public void OnChooseEnfuego() {
        if (!_thisPlayerLocked)
            CharacterChosen(Character.Ch.Enfuego);
    }

    public void OnChooseGravekeeper() {
        if (!_thisPlayerLocked)
            CharacterChosen(Character.Ch.Gravekeeper);
    }

    public void OnChooseValeria() {
        if (!_thisPlayerLocked)
            CharacterChosen(Character.Ch.Valeria);
    }

    void CharacterChosen(Character.Ch ch) {
        Debug.Log("CharacterSelect: local char chosen is " + ch);
        _localChar = ch;
        _charName.text = CharacterInfo.GetCharacterInfoObj(ch).name;
        _charPortrait.enabled = true;
        _charPortrait.sprite = GetCharacterPortrait(ch);
        //string info = CharacterInfo.GetCharacterInfo(ch);
        //charT.text = info;

        _loadouts = UserData.GetLoadoutList(ch);
        _ddLoadouts.ClearOptions();
        _ddLoadouts.AddOptions((from loadout in _loadouts select loadout.name).ToList());
        _ddLoadouts.interactable = true;

        _bConfirm.interactable = true;
    }

    // TODO move to asset loader once I make that
    public static Sprite GetCharacterPortrait(Character.Ch ch) {
        switch (ch) {
            case Character.Ch.Enfuego:
                return Resources.Load<Sprite>("sprites/characters/enfuego");
            case Character.Ch.Gravekeeper:
                return Resources.Load<Sprite>("sprites/characters/gravekeeper");
            case Character.Ch.Valeria:
                return Resources.Load<Sprite>("sprites/characters/valeria");
            case Character.Ch.Sample:
                return Resources.Load<Sprite>("sprites/characters/dummy");
            default:
                return null;
        }
    }

    public void OnConfirm() {
        _bConfirm.interactable = false;
        StartCoroutine(Confirm());
    }
    IEnumerator Confirm() {
        // store chosen char and loadout
        _gameSettings.chosenChar = _localChar;
        _gameSettings.chosenLoadout = _loadouts[_ddLoadouts.value].runes;

        // disable button & change background to green
        _bConfirm.interactable = false;
        _thisPlayerLocked = true;
        _charPortraitFrame.color = Color.green;

        yield return new WaitForSeconds(1);

        _menu.ChangeToPrematch();
        yield return null;
    }
}
