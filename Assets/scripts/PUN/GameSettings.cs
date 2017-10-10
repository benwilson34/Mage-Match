using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class GameSettings : PunBehaviour {

    public string p1name, p2name;
    public Character.Ch p1char = Character.Ch.Enfuego;
    public Character.Ch p2char = Character.Ch.Gravekeeper;
    public bool turnTimerOn;

    private bool nameSet = false;

    private Transform control, toggles;
    private Com.SoupSkull.MageMatch.Launcher launcher; // hideous class name

    void Start () {
        DontDestroyOnLoad(this);

        GameObject controlGO = GameObject.Find("Control Panel");
        if (controlGO != null) {
            control = controlGO.transform;
            toggles = control.Find("Toggles");
        }

        GameObject launcherGO = GameObject.Find("Launcher");
        if(launcherGO != null)
            launcher = launcherGO.GetComponent<Com.SoupSkull.MageMatch.Launcher>();
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
            photonView.RPC("SetToggles", PhotonTargets.Others, turnTimerOn);

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
    public void SetToggles(bool turnTimerOn) {
        //Debug.Log("GAMESETTINGS: SetTurnTimer to " + b);
        this.turnTimerOn = turnTimerOn;
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

    [PunRPC]
    public void OtherPlayerLocked() {
        GameObject.Find("CharacterSelect").GetComponent<CharacterSelect>().OtherPlayerLocked();
    }

    public void SyncCharAndLoadout(int id) {
        PhotonView view = PhotonView.Get(this);
        view.RPC("SetChar", PhotonTargets.Others, id, GetLocalChar(id));
    }

    [PunRPC]
    public void SetChar(int id, Character.Ch ch) {
        Debug.Log("Setting player" + id + " char to " + ch);
        if (id == 1) {
            p1char = ch;
        } else {
            p2char = ch;
        }
        GameObject.Find("CharacterSelect").GetComponent<CharacterSelect>().OtherCharacterSynced((int)ch);
    }
}
