using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Com.SoupSkull.MageMatch {
    public class GameManager : Photon.PunBehaviour {
        #region Photon Messages
        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public void OnLeftRoom() {
            SceneManager.LoadScene(0);
        }
        #endregion

        #region Public Methods
        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }
        #endregion

        #region Private Methods
        void LoadArena() {
            if (!PhotonNetwork.isMasterClient) {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            Debug.Log("GAMEMANAGER: PhotonNetwork: Loading GameScreen for " + PhotonNetwork.room.PlayerCount + " players.");
            PhotonNetwork.LoadLevel("MM Game Screen (Landscape) PHOTON");
        }
        #endregion

        #region Photon Messages
        public override void OnPhotonPlayerConnected(PhotonPlayer other) {
            Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting

            if (PhotonNetwork.isMasterClient) {
                Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected

                LoadArena();
            }
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer other) {
            Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects
        }
        #endregion
    }
}
