using UnityEngine;
using UnityEngine.UI;

namespace Com.SoupSkull.MageMatch {
    public class Launcher : Photon.PunBehaviour {

        public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

        bool isConnecting;
        private GameObject controlPanel, progressLabel;
        private Text progText;
        private RoomSettings rs;

        void Awake() {
            rs = GameObject.Find("roomSettings").GetComponent<RoomSettings>();

            // #Critical
            // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
            PhotonNetwork.autoJoinLobby = true;

            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;

            // #NotImportant
            // Force LogLevel
            PhotonNetwork.logLevel = Loglevel;

            controlPanel = GameObject.Find("Control Panel");
            progressLabel = GameObject.Find("Progress Label");
            progText = progressLabel.GetComponent<Text>();
        }

        void Start() {
            if (rs.isNewRoom) {
                GameObject.Find("b_play").transform.Find("Text").GetComponent<Text>().text = "Create room";
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
                PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2 }, null);
                progText.text = "Connected to PUN, waiting for opponent!";
            } else {
                // join room named rs.roomName
                PhotonNetwork.JoinRoom(rs.roomName);
                progText.text = "Joining room...";
            }
        }

        public override void OnDisconnectedFromPhoton() {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");
        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg) {
            base.OnPhotonJoinRoomFailed(codeAndMsg);
        }

        //public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
        //    Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");


        //    // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        //    PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2 }, null);
        //    progText.text = "Connected to PUN, waiting for opponent!";
        //}

        public override void OnPhotonPlayerConnected(PhotonPlayer other) {
            Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting

            if (PhotonNetwork.isMasterClient && PhotonNetwork.room.PlayerCount == 2) {
                Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected
                PhotonNetwork.LoadLevel("MM Game Screen (Landscape) PHOTON");
            }
        }
    }
}