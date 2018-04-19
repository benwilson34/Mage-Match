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
    public bool turnTimerOn;

    void Start () {
        DontDestroyOnLoad(this);
    }

    // ----- name -----

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


    // ----- character -----

    //public void SetLocalChar(int id, Character.Ch ch) {
    //    if (id == 1)
    //        p1char = ch;
    //    else
    //        p2char = ch;
    //}

    //public void SetOpponentChar(int id, Character.Ch ch) {
    //    if (id == 1)
    //        p2char = ch;
    //    else
    //        p1char = ch;
    //}

    public Character.Ch GetLocalChar(int id) {
        if (id == 1)
            return p1char;
        else
            return p2char;
    }

}
