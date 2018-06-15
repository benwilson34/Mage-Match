using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunebuildingCharList : MenuScreen {

    private Transform _scrCharList;
    private GameObject _itemPF;

    public override void OnLoad() {
        _scrCharList = transform.Find("scr_char").Find("Viewport").Find("Content");
        _itemPF = Resources.Load("prefabs/menu/runebuildingCharListItem") as GameObject;
    }

    public override void OnShowScreen() {
        MenuController.LoadRuneInfo();

        foreach (Transform child in _scrCharList)
            GameObject.Destroy(child.gameObject);

        AddCharacterEntry(Character.Ch.Enfuego);
        AddCharacterEntry(Character.Ch.Gravekeeper);
        AddCharacterEntry(Character.Ch.Valeria);
        AddCharacterEntry(Character.Ch.MagicAl);

        // TODO get list of available characters (once they can be bought in-store)
    }

    void AddCharacterEntry(Character.Ch ch) {
        Transform item = Instantiate(_itemPF, _scrCharList).transform;

        // replace icon, name, and loadout count
        Image charPortrait = item.Find("i_charPortrait").GetComponent<Image>();
        charPortrait.sprite = CharacterSelect.GetCharacterPortrait(ch);
        Text charName = item.Find("t_charName").GetComponent<Text>();
        charName.text = CharacterInfo.GetCharacterInfo(ch).name;

        item.GetComponent<RunebuildingCharListItem>().character = ch;
    }
}
