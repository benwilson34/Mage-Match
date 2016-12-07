using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkController : MonoBehaviour {

    NetworkManager nm;

	// Use this for initialization
	void Start () {
        nm = GameObject.Find("Network Manager").GetComponent<NetworkManager>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Network.connections.Length > 0)
            NumPlayers();
	}

    public static void NumPlayers() {
        Debug.Log("NETWORKCONTROLLER: Number of connected players: " + Network.connections.Length);
    }
}
