using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunebuildingLoadoutListItem : MonoBehaviour {

    public Character.Ch character;
    public LoadoutData loadout;

    public void OnEntryClick() {
        GameObject.Find("world ui").GetComponent<MenuController>()
            .ChangeToRunebuilding_EditLoadout(character, loadout);
    }
}
