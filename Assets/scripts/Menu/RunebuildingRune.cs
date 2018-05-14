using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunebuildingRune : MonoBehaviour {

    private RuneInfoLoader.RuneInfo _runeInfo;
    private RunebuildingEditLoadout _editLoadout;

    private bool _usedForLoadout = false;
    private Image _iBackground;

    public void Init(RunebuildingEditLoadout edit, Character.Ch ch, RuneInfoLoader.RuneInfo info) {
        _iBackground = GetComponent<Image>();
        _iBackground.color = Color.grey;

        _editLoadout = edit;

        _runeInfo = info;
        transform.Find("i_sprite").GetComponent<Image>()
            .sprite = RuneInfoLoader.GetRuneSprite(ch, info.tagTitle);
        transform.Find("t_title").GetComponent<Text>().text = info.title;
        transform.Find("t_desc").GetComponent<Text>().text = info.desc;
        transform.Find("t_count").GetComponent<Text>().text = info.deckCount.ToString();
    }

    public void OnClick() {
        //Debug.Log("Clicked on " + _runeInfo.title);
        if (!_usedForLoadout) {
            if (_editLoadout.AddUsedRune(_runeInfo))
                ToggleUsed();
        } else {
            _editLoadout.RemoveUsedRune(_runeInfo);
            ToggleUsed();
        }
    }

    public void ToggleUsed() {
        _usedForLoadout = !_usedForLoadout;
        _iBackground.color = _usedForLoadout ? Color.yellow : Color.grey;
    }
}
