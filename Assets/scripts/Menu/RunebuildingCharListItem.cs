using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunebuildingCharListItem : MonoBehaviour {

    public Character.Ch character;

    public void OnEntryClick() {
        GameObject.Find("world ui").GetComponent<MenuController>()
            .ChangeToRunebuilding_LoadoutList(character);
    }
}
