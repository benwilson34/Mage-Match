﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.SoupSkull.MageMatch {
    public class GameManager : Photon.PunBehaviour {

        public void OnLeftRoom() {
            SceneManager.LoadScene(0);
        }

        public void LeaveRoom() {
            PhotonNetwork.LeaveRoom();
        }

        void LoadArena() {
            if (!PhotonNetwork.isMasterClient) {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            PhotonNetwork.LoadLevel("MM Game Screen (Landscape) PHOTON");
        }

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
    }
}
