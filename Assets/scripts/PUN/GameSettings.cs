using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;

public class GameSettings : PunBehaviour {

    public string p1name, p2name;
    public bool turnTimerOn, localPlayerOnLeft, hideOpponentHand;

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
            localPlayerOnLeft = control.Find("Toggle_PlayerOnLeft").GetComponent<Toggle>().isOn;
            hideOpponentHand = control.Find("Toggle_HideOpponentHand").GetComponent<Toggle>().isOn;
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
}
