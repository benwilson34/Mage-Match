using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class GameSettings : PunBehaviour {

    public string p1name, p2name;
    public bool turnTimerOn;

    private bool nameSet = false;
    private Transform control;
    private Com.SoupSkull.MageMatch.Launcher launcher; // hideous class name

    void Start () {
        DontDestroyOnLoad(this);
        control = GameObject.Find("Control Panel").transform;
        launcher = GameObject.Find("Launcher").GetComponent<Com.SoupSkull.MageMatch.Launcher>();
    }

    [PunRPC]
    public void PutSettings() { StartCoroutine(Settings()); }
    public IEnumerator Settings() {
        int id = PhotonNetwork.player.ID;
        PhotonView photonView = PhotonView.Get(this);
        Transform nameText = control.Find("Name InputField").Find("Text");
        string name = nameText.GetComponent<Text>().text;
        Debug.Log("id=" + id + ", name=" + name);
        photonView.RPC("SetPlayerName", PhotonTargets.Others, id, name);

        if (id == 1) {
            p1name = name;
            turnTimerOn = control.Find("Toggle_TurnTimer").GetComponent<Toggle>().isOn;
            Debug.Log("GAMESETTINGS: player 1 here, setting turnTimerOn to " + turnTimerOn);
            photonView.RPC("SetTurnTimerToggle", PhotonTargets.Others, turnTimerOn);

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
    public void SetTurnTimerToggle(bool b) {
        Debug.Log("GAMESETTINGS: SetTurnTimer to " + b);
        turnTimerOn = b;
    }
}
