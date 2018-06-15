using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Prematch : MenuScreen {

    public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

    //private GameObject _controlPanel, _toggles;
    //private InputField _nameInput;
    private GameObject _cancelButton;
    private Text _statusText;
    private Image _p1portrait, _p2portrait;
    private Text _p1username, _p2username;

    private bool _isConnecting;
    private bool _allSettingsSynced = false;

    private MenuController _menus;
    private GameSettings _gameSettings;

    public override void OnLoad() {
        // TODO
    }

    public override void OnShowScreen() {
        _menus = GameObject.Find("world ui").GetComponent<MenuController>();
        _gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        _statusText = transform.Find("t_status").GetComponent<Text>();

        _cancelButton = transform.Find("b_cancel").gameObject;

        _p1portrait = transform.Find("i_localCharPortraitFrame")
            .Find("i_charPortrait").GetComponent<Image>();
        _p2portrait = transform.Find("i_oppCharPortraitFrame")
            .Find("i_charPortrait").GetComponent<Image>();

        _p1username = transform.Find("t_localName").GetComponent<Text>();
        _p2username = transform.Find("t_oppName").GetComponent<Text>();

        if (_gameSettings.trainingMode) {
            // Init GameSettings and DebugSettings
            _gameSettings.p1name = UserData.Username;
            _gameSettings.p1char = _gameSettings.chosenChar;
            _gameSettings.p1loadout = _gameSettings.chosenLoadout;
            _gameSettings.p2name = "Training Dummy";
            _gameSettings.p2char = Character.Ch.Neutral;
            _gameSettings.p2loadout = new string[0];

            StartCoroutine(ShowPrematchInfoBeforeLoad(true));
        } else {
            PhotonNetwork.autoJoinLobby = true; // needed?

            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;

            PhotonNetwork.logLevel = Loglevel; // debug log level

            //_controlPanel = GameObject.Find("Control Panel");
            //_toggles = GameObject.Find("Toggles");
            //_nameInput = GameObject.Find("input_Name").GetComponent<InputField>();

            //if (_rs.isNewRoom) {
            //    GameObject.Find("b_Play").transform.Find("Text").GetComponent<Text>().text = "Create room";
            //} else {
            //    _toggles.SetActive(false);
            //}

            //_controlPanel.SetActive(true);

            Connect();

            _p1portrait.sprite = CharacterSelect.GetFullCharacterArt(_gameSettings.chosenChar);
            _p1username.text = UserData.Username;
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

        PhotonNetwork.autoJoinLobby = false;
        PhotonNetwork.automaticallySyncScene = true;

        //_controlPanel.SetActive(false);

        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.connected) {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
            //HandleRoomAction();
        } else {
            // #Critical, we must first and foremost connect to Photon Online Server.
            Debug.Log("PREMATCH: About to connect to PUN...");
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
        //_controlPanel.SetActive(true);
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
        string pName = UserData.Username;
        PhotonView photonView = PhotonView.Get(this);

        Debug.Log("id=" + id + ", name=" + pName);

        photonView.RPC("SetPlayerInfo", PhotonTargets.All, id, pName, _gameSettings.chosenChar);
        photonView.RPC("SetPlayerLoadout", PhotonTargets.All, id, _gameSettings.chosenLoadout);

        if (id == 1) {
            //if (_toggles != null)
            //    gameSettings.turnTimerOn = _toggles.transform.Find("Toggle_TurnTimer").GetComponent<Toggle>().isOn;
            //else
            //    gameSettings.turnTimerOn = false; // not really needed?

            photonView.RPC("SetToggles", PhotonTargets.Others, _gameSettings.turnTimerOn);
        }

        yield return new WaitUntil(() => _allSettingsSynced); // wait for RPC



        StartCoroutine(ShowPrematchInfoBeforeLoad(false));
    }

    [PunRPC]
    public void SetPlayerInfo(int id, string pName, Character.Ch ch) {
        _gameSettings.SetPlayerInfo(id, pName, ch);
    }

    [PunRPC]
    public void SetPlayerLoadout(int id, string[] runes) {
        _gameSettings.SetPlayerLoadout(id, runes);
    }

    [PunRPC]
    public void SetToggles(bool turnTimerOn) {
        //Debug.Log("GAMESETTINGS: SetTurnTimer to " + b);
        _gameSettings.turnTimerOn = turnTimerOn;
        photonView.RPC("ConfirmAllSettingsSynced", PhotonTargets.All);
    }

    [PunRPC]
    public void ConfirmAllSettingsSynced() {
        _allSettingsSynced = true;
    }

    public IEnumerator ShowPrematchInfoBeforeLoad(bool training) {
        // TODO show both, audio, screen transition
        _p1portrait.sprite = CharacterSelect.GetFullCharacterArt(_gameSettings.p1char);
        _p1username.text = _gameSettings.p1name;
        _p2portrait.sprite = CharacterSelect.GetFullCharacterArt(_gameSettings.p2char);
        _p2username.text = _gameSettings.p2name;

        _cancelButton.SetActive(false);

        for (int i = 3; i > 0; i--) {
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
        if(PhotonNetwork.connected)
            PhotonNetwork.Disconnect();

        _menus.CancelFromPrematch();
    }
}
