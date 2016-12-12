//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;

//public class MyHook : Prototype.NetworkLobby.LobbyHook {
//    int count = 0;

//    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer) {
//        MageMatch.Init();
//        count++;
//        Debug.Log("Player " + count + " loaded scene!");
//        MageMatch mm = GameObject.Find("board").GetComponent<MageMatch>();
//        mm.SetPlayer(gamePlayer);
//    }
//}
