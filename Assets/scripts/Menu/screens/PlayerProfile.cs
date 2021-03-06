﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

public class PlayerProfile : MenuScreen {

    //public Sprite[] frames, backgrounds, foregrounds;

    private Text _tName;
    private InputField _inputName;
    private Button _bConfirmName;

    public override void OnLoad() {
        _tName = transform.Find("t_name").GetComponent<Text>();
        _inputName = transform.Find("input_name").GetComponent<InputField>();
        _bConfirmName = transform.Find("b_changeName").GetComponent<Button>();
    }

    public override void OnShowScreen() {
        ShowUsername();
    }

    public void OnNameInputChanged(string str) {
        _bConfirmName.interactable = str.Length > 0;
    }

    public void OnConfirmChangeName() {
        string newName = _inputName.text;
        Debug.Log("Changing name to " + newName);
        UserData.Username = newName;

        _inputName.text = "";
        ShowUsername();
    }

    void ShowUsername() {
        _tName.text = "Name: " + UserData.Username;
    }
}
