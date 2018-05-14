using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunebuildingLoadoutList : MenuScreen {

    private Transform _scrLoadoutList;
    private GameObject _loadoutListItemPF;
    private Text _tPageTitle;

    private Character.Ch _character;

    public override void OnLoad() {
        _scrLoadoutList = transform.Find("scr_loadouts").Find("Viewport").Find("Content");
        _loadoutListItemPF = Resources.Load("prefabs/menu/runebuildingLoadoutListItem") as GameObject;

        _tPageTitle = transform.Find("t_charTitle").GetComponent<Text>();
    }

    public override void OnPass(object o) {
        _character = (Character.Ch)o;
    }

    public override void OnShowScreen() {
        _tPageTitle.text = "Showing stored loadouts for " + 
            CharacterInfo.GetCharacterInfo(_character).name;

        foreach (Transform child in _scrLoadoutList)
            GameObject.Destroy(child.gameObject);

        foreach (var loadout in LoadoutData.GetLoadoutList(_character))
            AddLoadoutEntry(loadout);
    }

    public override void OnBack() {
        OnShowScreen();
    }

    void AddLoadoutEntry(LoadoutData loadout) {
        Transform item = Instantiate(_loadoutListItemPF, _scrLoadoutList).transform;
        // replace name and rune sprites
        Text loadoutName = item.Find("t_loadoutName").GetComponent<Text>();
        loadoutName.text = loadout.name;

        if (loadout.isDefault) {
            item.Find("t_defaultMsg").GetComponent<Text>().enabled = true;
            item.GetComponent<Button>().interactable = false;
        }

        Transform runes = item.Find("Runes");
        for (int i = 0; i < LoadoutData.RUNE_COUNT; i++) {
            Image runePortrait = runes.Find("i_rune" + i).GetComponent<Image>();
            runePortrait.sprite = RuneInfoLoader.GetRuneSprite(_character, loadout.runes[i]);
        }

        item.GetComponent<RunebuildingLoadoutListItem>().character = _character;
        item.GetComponent<RunebuildingLoadoutListItem>().loadout = loadout;
    }

    public void OnNewButtonClick() {
        Transform lastLoadout = _scrLoadoutList.GetChild(_scrLoadoutList.childCount - 1);
        string path = lastLoadout.GetComponent<RunebuildingLoadoutListItem>().loadout.filepath;
        Debug.Log(path);

        int num = 0;
        string directory;
        if (path == null) { // the first user loadout for the character
            directory = LoadoutData.GetLoadoutDirectory(_character);
        } else {
            directory = Path.GetDirectoryName(path);
            num = int.Parse(Path.GetFileNameWithoutExtension(path));
            num++;
        }

        string numStr = string.Format("{0:D2}", num);
        string newPath = directory + "/" + numStr + ".json";
        Debug.Log(newPath);
        //File.CreateText(newPath); // needed here? or once the loadout is saved?
        LoadoutData loadout = LoadoutData.GetDefaultLoadout(_character);
        loadout.name = _character.ToString() + " Loadout " + numStr;
        loadout.filepath = newPath;
        loadout.isDefault = false;

        GameObject.Find("world ui").GetComponent<MenuController>()
            .ChangeToRunebuilding_EditLoadout(_character, loadout);
    }
}