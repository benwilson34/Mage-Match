using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour {

    //private Transform characterBlock;
    private Image _charPortraitFrame, _charPortrait;
    private Text _charName, _charT;
    private Button _bConfirm;
    private GameObject goldSelectionPF, redSelectionPF;

    private GameSettings _gameSettings;
    private Character.Ch _localChar = Character.Ch.Sample;
    private bool _thisPlayerLocked = false;

    private MenuController _menu;
    private bool _singlePlayer;

    void Awake() {
        //characterBlock = transform.Find("characterBlock");
        _charPortraitFrame = transform.Find("i_charPortraitFrame").GetComponent<Image>();
        _charPortrait = _charPortraitFrame.transform.Find("i_charPortrait").GetComponent<Image>();
        _charName = transform.Find("t_charName").GetComponent<Text>();
        //charT = transform.Find("t_charInfo").GetComponent<Text>();

        _bConfirm = transform.Find("b_confirm").GetComponent<Button>();

        _menu = GameObject.Find("world ui").GetComponent<MenuController>();
    }

    public void Init(bool singlePlayer) {
        _thisPlayerLocked = false;
        _gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        _charName.text = "";
        _charPortraitFrame.color = Color.clear;
        _charPortrait.enabled = false;
        //charT.text = "";
        _bConfirm.interactable = false;

        _singlePlayer = singlePlayer;
        Text tGamemode = transform.Find("t_gameMode").GetComponent<Text>();
        if (_singlePlayer)
            tGamemode.text = "Select character for Training";
        else
            tGamemode.text = "Select character for Online Battle";
    }

    public void OnConfirm() {
        // store chosen char and loadout
        //gameSettings.SetLocalChar(myID, localChar);
        _gameSettings.chosenChar = _localChar;

        // disable button & change background to green
        _bConfirm.interactable = false;
        _thisPlayerLocked = true;
        _charPortraitFrame.color = Color.green;

        _menu.ChangeToPrematch(_singlePlayer);
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
}
