using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Prematch : Photon.PunBehaviour {

    public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

    private bool _isConnecting;
    private GameObject _controlPanel, _toggles;
    private Text _statusText;
    private InputField _nameInput;

    private bool _allSettingsSynced = false;

    private GameSettings _gameSettings;

    public void Init(bool training) {
        _gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        _statusText = transform.Find("t_status").GetComponent<Text>();

        if (training) {
            // Init GameSettings and DebugSettings
            _gameSettings.p1name = PlayerProfile.GetUsername();
            _gameSettings.p1char = _gameSettings.chosenChar;
            _gameSettings.p2name = "Training Dummy";
            _gameSettings.p2char = Character.Ch.Sample;

            StartCoroutine(ShowPrematchInfoBeforeLoad(true));
        } else {
            PhotonNetwork.autoJoinLobby = true; // needed?

            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;

            PhotonNetwork.logLevel = Loglevel; // debug log level

            _controlPanel = GameObject.Find("Control Panel");
            _toggles = GameObject.Find("Toggles");
            _nameInput = GameObject.Find("input_Name").GetComponent<InputField>();

            //if (_rs.isNewRoom) {
            //    GameObject.Find("b_Play").transform.Find("Text").GetComponent<Text>().text = "Create room";
            //} else {
            //    _toggles.SetActive(false);
            //}

            _controlPanel.SetActive(true);

            Connect();
        }
    } 
    /// <summary>
    /// Start the connection process. 
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect() {
        // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
        _statusText.text = "Connecting...";
        _isConnecting = true;

        _controlPanel.SetActive(false);

        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.connected) {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
            //HandleRoomAction();
        } else {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings("1");
        }
    }

    public override void OnConnectedToMaster() {
        Debug.Log("Prematch: OnConnectedToMaster() was called by PUN");
        // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()  
        // we don't want to do anything if we are not attempting to join a room. 
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (_isConnecting) {
            PhotonNetwork.JoinRandomRoom();
            //HandleRoomAction();
        }
    }

    //private void HandleRoomAction() {
    //    if (_rs.isNewRoom) {
    //        // make a new room
    //        string name = _nameInput.transform.Find("Text").GetComponent<Text>().text;
    //        Debug.Log("Setting hostName to " + name);
    //        Hashtable props = new Hashtable();
    //        props.Add("hostName", name);
    //        RoomOptions options = new RoomOptions() {
    //            MaxPlayers = 2,
    //            CustomRoomProperties = props,
    //            CustomRoomPropertiesForLobby = new string[] { "hostName" }
    //        };
    //        PhotonNetwork.CreateRoom(null, options, null);
    //        _progText.text = "Connected to PUN, waiting for opponent!";
    //    } else {
    //        // join room named rs.roomName
    //        PhotonNetwork.JoinRoom(_rs.roomName);
    //        _progText.text = "Joining room...";
    //    }
    //}

    //public override void OnJoinedRoom() {
    //    if (rs.isNewRoom) {
    //        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
    //        string name = nameInput.transform.Find("Text").GetComponent<Text>().text;
    //        Debug.Log("Setting hostName to " + name);
    //        hash.Add("hostName", name);
    //        Debug.Log("hostName is " + hash["hostName"]);
    //        PhotonNetwork.room.SetCustomProperties(hash);
    //    }
    //}

    public override void OnDisconnectedFromPhoton() {
        //_progressLabel.SetActive(false);
        _controlPanel.SetActive(true);
        Debug.LogWarning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
        Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");


        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2 }, null);
        _statusText.text = "Connected!\n Waiting for opponent...";
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other) {
        Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting
        if (PhotonNetwork.isMasterClient && PhotonNetwork.room.PlayerCount == 2) {
            PhotonView view = PhotonView.Get(this);
            view.RPC("SyncSettings", PhotonTargets.All);
        }
    }

    [PunRPC]
    public void SyncSettings() { StartCoroutine(Settings()); }
    public IEnumerator Settings() {
        int id = PhotonNetwork.player.ID;
        string pName = PlayerProfile.GetUsername();
        PhotonView photonView = PhotonView.Get(this);

        GameSettings gameSettings = GameObject.Find("gameSettings").GetComponent<GameSettings>();
        Debug.Log("id=" + id + ", name=" + pName);

        photonView.RPC("SetPlayerInfo", PhotonTargets.All, id, pName, gameSettings.chosenChar);

        if (id == 1) {
            //if (_toggles != null)
            //    gameSettings.turnTimerOn = _toggles.transform.Find("Toggle_TurnTimer").GetComponent<Toggle>().isOn;
            //else
            //    gameSettings.turnTimerOn = false; // not really needed?

            photonView.RPC("SetToggles", PhotonTargets.Others, gameSettings.turnTimerOn);
        }
        yield return new WaitUntil(() => _allSettingsSynced); // wait for RPC

        StartCoroutine(ShowPrematchInfoBeforeLoad(false));
    }

    [PunRPC]
    public void SetPlayerInfo(int id, string pName, Character.Ch ch) {
        GameSettings gameSettings = GameObject.Find("gameSettings").GetComponent<GameSettings>();
        gameSettings.SetPlayerInfo(id, pName, ch);
    }

    [PunRPC]
    public void SetToggles(bool turnTimerOn) {
        //Debug.Log("GAMESETTINGS: SetTurnTimer to " + b);
        GameSettings gameSettings = GameObject.Find("gameSettings").GetComponent<GameSettings>();
        gameSettings.turnTimerOn = turnTimerOn;
        photonView.RPC("ConfirmAllSettingsSynced", PhotonTargets.All);
    }

    [PunRPC]
    public void ConfirmAllSettingsSynced() {
        _allSettingsSynced = true;
    }

    public IEnumerator ShowPrematchInfoBeforeLoad(bool training) {
        // TODO show both, audio, screen transition
        for (int i = 5; i > 0; i--) {
            _statusText.text = "Starting game in " + i + "...";
            yield return new WaitForSeconds(1f);
        }

        if (training) {
            SceneManager .LoadScene("Game Screen (Landscape)");
        } else {
            PhotonNetwork.LoadLevel("Game Screen (Landscape)");
        }
        yield return null;
    }

    public void OnCancel() {
        // TODO
    }
}
