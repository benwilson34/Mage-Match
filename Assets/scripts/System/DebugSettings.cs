using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSettings : MonoBehaviour {

    public enum TrainingMode { OneCharacter, TwoCharacters };
    public TrainingMode trainingMode = TrainingMode.OneCharacter;

    public bool IsOneCharMode { get { return trainingMode == TrainingMode.OneCharacter; } }

    public bool replayMode = false, animateReplay = false;
    public string replayFile = "";

    void Start () {
        DontDestroyOnLoad(this);
	}

}
