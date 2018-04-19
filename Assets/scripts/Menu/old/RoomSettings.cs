using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSettings : MonoBehaviour {

    public bool isNewRoom = true;
    public string roomName = "";

	// Use this for initialization
	void Start () {
        GameObject.DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
