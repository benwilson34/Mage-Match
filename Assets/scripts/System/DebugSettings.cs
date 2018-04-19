using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSettings : MonoBehaviour {

    public bool applyAPcost = false, onePlayerMode = true, midiMode = false;

    public bool replayMode = false, animateReplay = false;
    public string replayFile = "";

    void Start () {
        DontDestroyOnLoad(this);
	}

}
