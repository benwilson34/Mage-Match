using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class GameSettings : PunBehaviour {

    public string p1name, p2name;
    public Character.Ch p1char = Character.Ch.Enfuego;
    public Character.Ch p2char = Character.Ch.Gravekeeper;
    public int p1loadout = 0, p2loadout = 0;
    public bool turnTimerOn, localPlayerOnLeft, hideOpponentHand;

    private bool nameSet = false;

    private Transform control, toggles;
    private Com.SoupSkull.MageMatch.Launcher launcher; // hideous class name

    void Start () {
        DontDestroyOnLoad(this);
        control = GameObject.Find("Control Panel").transform;
        toggles = control.Find("Toggles");
        launcher = GameObject.Find("Launcher").GetComponent<Com.SoupSkull.MageMatch.Launcher>();
    }

    [PunRPC]
    public void PutSettings() { StartCoroutine(Settings()); }
    public IEnumerator Settings() {
        int id = PhotonNetwork.player.ID;
        PhotonView photonView = PhotonView.Get(this);
        Transform nameText = control.Find("input_Name").Find("Text");
        string name = nameText.GetComponent<Text>().text;
        Debug.Log("id=" + id + ", name=" + name);

        photonView.RPC("SetPlayerName", PhotonTargets.Others, id, name);

        if (id == 1) {
            p1name = name;
            turnTimerOn = toggles.Find("Toggle_TurnTimer").GetComponent<Toggle>().isOn;
            localPlayerOnLeft = toggles.Find("Toggle_PlayerOnLeft").GetComponent<Toggle>().isOn;
            hideOpponentHand = toggles.Find("Toggle_HideOpponentHand").GetComponent<Toggle>().isOn;
            //Debug.Log("GAMESETTINGS: player 1 here, setting turnTimerOn to " + turnTimerOn);
            photonView.RPC("SetToggles", PhotonTargets.Others, turnTimerOn, localPlayerOnLeft, hideOpponentHand);

            // warn that the toggle RPC might outrun this "callback"...
            yield return new WaitUntil(() => nameSet); // wait for RPC

            launcher.LoadGameScreen();
        } else {
            p2name = name;
        }
    }

    [PunRPC]
    public void SetPlayerName(int id, string name) {
        Debug.Log("GAMESETTINGS: Set player" + id + " name to " + name);
        if (id == 1)
            p1name = name;
        else
            p2name = name;
        nameSet = true;
    }

    [PunRPC]
    public void SetToggles(bool turnTimerOn, bool localPlayerOnLeft, bool hideOpponentHand) {
        //Debug.Log("GAMESETTINGS: SetTurnTimer to " + b);
        this.turnTimerOn = turnTimerOn;
        this.localPlayerOnLeft = localPlayerOnLeft;
        this.hideOpponentHand = hideOpponentHand;
    }

    public void SetLocalChar(int id, Character.Ch ch) {
        if (id == 1)
            p1char = ch;
        else
            p2char = ch;
    }

    public Character.Ch GetLocalChar(int id) {
        if (id == 1)
            return p1char;
        else
            return p2char;
    }

    public void SetLocalLoadout(int id, int index) {
        if (id == 1)
            p1loadout = index;
        else
            p2loadout = index;
    }

    public int GetLocalLoadout(int id) {
        if (id == 1)
            return p1loadout;
        else
            return p2loadout;
    }

    [PunRPC]
    public void OtherPlayerLocked() {
        GameObject.Find("CharacterSelect").GetComponent<CharacterSelect>().OtherPlayerLocked();
    }

    public void SyncCharAndLoadout(int id) {
        PhotonView view = PhotonView.Get(this);
        view.RPC("SetCharAndLoadout", PhotonTargets.Others, id, GetLocalChar(id), GetLocalLoadout(id));
    }

    [PunRPC]
    public void SetCharAndLoadout(int id, Character.Ch ch, int loadout) {
        Debug.Log("Setting player" + id + " char/loadout to " + ch + "/" + loadout);
        if (id == 1) {
            p1char = ch;
            p1loadout = loadout;
        } else {
            p2char = ch;
            p2loadout = loadout;
        }
        GameObject.Find("CharacterSelect").GetComponent<CharacterSelect>().OtherCharacterSynced((int)ch);
    }
}
