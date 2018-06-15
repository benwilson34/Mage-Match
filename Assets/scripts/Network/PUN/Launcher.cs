using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Com.SoupSkull.MageMatch {
    public class Launcher : Photon.PunBehaviour {

        public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

        bool isConnecting;
        private GameObject controlPanel, progressLabel, toggles;
        private Text progText;
        private RoomSettings rs;
        private InputField nameInput;

        private bool nameSet = false;

        void Awake() {
            rs = GameObject.Find("roomSettings").GetComponent<RoomSettings>();

            PhotonNetwork.autoJoinLobby = true; // needed?

            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;

            PhotonNetwork.logLevel = Loglevel; // debug log level

            controlPanel = GameObject.Find("Control Panel");
            progressLabel = GameObject.Find("Progress Label");
            toggles = GameObject.Find("Toggles");
            progText = progressLabel.GetComponent<Text>();
            nameInput = GameObject.Find("input_Name").GetComponent<InputField>();
        }

        void Start() {
            if (rs.isNewRoom) {
                GameObject.Find("b_Play").transform.Find("Text").GetComponent<Text>().text = "Create room";
            } else {
                toggles.SetActive(false);
            }

            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        /// <summary>
        /// Start the connection process. 
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect() {
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            isConnecting = true;

            progressLabel.SetActive(true);
            controlPanel.SetActive(false);

            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.connected) {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
                //PhotonNetwork.JoinRandomRoom();
                HandleRoomAction();
            } else {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings("1");
            }
        }

        public override void OnConnectedToMaster() {
            Debug.Log("DemoAnimator/Launcher: OnConnectedToMaster() was called by PUN");
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()  
            // we don't want to do anything if we are not attempting to join a room. 
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything.
            if (isConnecting) {
                HandleRoomAction();
            }
        }

        private void HandleRoomAction(){
            if (rs.isNewRoom) {
                // make a new room
                string name = nameInput.transform.Find("Text").GetComponent<Text>().text;
                Debug.Log("Setting hostName to " + name);
                Hashtable props = new Hashtable();
                props.Add("hostName", name);
                RoomOptions options = new RoomOptions() {
                    MaxPlayers = 2,
                    CustomRoomProperties = props,
                    CustomRoomPropertiesForLobby = new string[] { "hostName" }
                };
                PhotonNetwork.CreateRoom(null, options, null);
                progText.text = "Connected to PUN, waiting for opponent!";
            } else {
                // join room named rs.roomName
                PhotonNetwork.JoinRoom(rs.roomName);
                progText.text = "Joining room...";
            }
        }

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
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");
        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg) {
            base.OnPhotonJoinRoomFailed(codeAndMsg);
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer other) {
            Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting
            if (PhotonNetwork.isMasterClient && PhotonNetwork.room.PlayerCount == 2) {
                PhotonView view = PhotonView.Get(this);
                view.RPC("SyncSettings", PhotonTargets.All);

                //Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected
                //PhotonNetwork.LoadLevel("Game Screen (Landscape)");
            }
        }

        [PunRPC]
        public void SyncSettings() { StartCoroutine(Settings()); }
        public IEnumerator Settings() {
            int id = PhotonNetwork.player.ID;
            string pName = nameInput.text;
            PhotonView photonView = PhotonView.Get(this);

            GameSettings gameSettings = GameObject.Find("gameSettings").GetComponent<GameSettings>();
            Debug.Log("id=" + id + ", name=" + pName);

            photonView.RPC("SetPlayerName", PhotonTargets.Others, id, pName);

            if (id == 1) {
                gameSettings.p1name = pName;

                if (toggles != null)
                    gameSettings.turnTimerOn = toggles.transform.Find("Toggle_TurnTimer").GetComponent<Toggle>().isOn;
                else
                    gameSettings.turnTimerOn = false; // not really needed?

                photonView.RPC("SetToggles", PhotonTargets.Others, gameSettings.turnTimerOn);

                // warn that the toggle RPC might outrun this "callback"...
                yield return new WaitUntil(() => nameSet); // wait for RPC

                LoadGameScreen();
            } else {
                gameSettings.p2name = pName;
            }
        }

        [PunRPC]
        public void SetPlayerName(int id, string pName) {
            //GameSettings gameSettings = GameObject.Find("gameSettings").GetComponent<GameSettings>();
            //gameSettings.SetPlayerName(id, pName);
            nameSet = true;
        }

        [PunRPC]
        public void SetToggles(bool turnTimerOn) {
            //Debug.Log("GAMESETTINGS: SetTurnTimer to " + b);
            GameSettings gameSettings = GameObject.Find("gameSettings").GetComponent<GameSettings>();
            gameSettings.turnTimerOn = turnTimerOn;
        }

        public void LoadGameScreen() {
            Debug.Log("LAUNCHER: Loading...");
            PhotonNetwork.LoadLevel("Character Select");
        }
    }
}