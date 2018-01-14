using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSettings : MonoBehaviour {

    public bool applyAPcost = false, onePlayerMode = true, midiMode = false;

    void Start () {
        DontDestroyOnLoad(this);
	}

}
