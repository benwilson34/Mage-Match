﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Com.SoupSkull.MageMatch {
    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputField : MonoBehaviour {
        // Store the PlayerPref Key to avoid typos
        static string playerNamePrefKey = "PlayerName";

        void Start() {
            string defaultName = "";
            InputField _inputField = this.GetComponent<InputField>();
            if (_inputField != null) {
                if (PlayerPrefs.HasKey(playerNamePrefKey)) {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    _inputField.text = defaultName;
                }
            }

            PhotonNetwork.playerName = defaultName;
        }

        public void SetPlayerName(string value) {
            // #Important
            PhotonNetwork.playerName = value + " "; // force a trailing space string in case value is an empty string, else playerName would not be updated.

            PlayerPrefs.SetString(playerNamePrefKey, value);
        }
    }
}