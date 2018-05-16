using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class GameSettings : PunBehaviour {

    public string p1name, p2name;
    public Character.Ch chosenChar;
    public Character.Ch p1char = Character.Ch.Valeria;
    public Character.Ch p2char = Character.Ch.Enfuego;
    public string[] chosenLoadout, p1loadout, p2loadout;

    public bool turnTimerOn;
    public bool trainingMode;

    void Start () {
        DontDestroyOnLoad(this);
    }

    public void SetPlayerInfo(int id, string pName, Character.Ch ch) {
        Debug.Log("GAMESETTINGS: Set player" + id + " name to " + pName);
        if (id == 1) {
            p1name = pName;
            p1char = ch;
        } else {
            p2name = pName;
            p2char = ch;
        }
    }

    public Character.Ch GetChar(int id) {
        if (id == 1)
            return p1char;
        else
            return p2char;
    }

    public void SetPlayerLoadout(int id, string[] runes) {
        if (id == 1)
            p1loadout = runes;
        else
            p2loadout = runes;
    }

    public string[] GetLoadout(int id) {
        if (id == 1)
            return p1loadout;
        else
            return p2loadout;
    }

    public string SettingsToString() {
        string str = "";
        str += string.Format("p1: {0} ({1}) \n  loadout:[{2}];\n", 
            p1name, p1char, string.Join(", ", p1loadout));
        str += string.Format("p2: {0} ({1}) \n  loadout:[{2}];\n",
            p2name, p2char, string.Join(", ", p2loadout));
        str += "turnTimerOn=" + turnTimerOn + ", trainingMode=" + trainingMode;
        return str;
    }

}
