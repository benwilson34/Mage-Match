using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTimer : MonoBehaviour {

    private float timeRemaining, initTime = 20f;
    private MageMatch mm;
    private bool pause = false;

	void Start () {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        InitTimer();
        InvokeRepeating("DecreaseTimeRemaining", .1f, .1f);
	}

	void Update () {
		// ?
	}

    public void Pause() {
        pause = true;
    }

    public void InitTimer() {
        pause = false;
        timeRemaining = initTime;
    }

    void DecreaseTimeRemaining() {
        if (!pause && !mm.targeting.IsTargetMode()) {
            timeRemaining -= .1f;
            if (timeRemaining < .01f) {
                Pause();
                mm.eventCont.Timeout();
            }
            mm.uiCont.UpdateTurnTimer(timeRemaining);
        }
    }
}
