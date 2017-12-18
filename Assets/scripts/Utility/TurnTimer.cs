using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTimer : MonoBehaviour {

    private float timeRemaining;
    private MageMatch mm;
    private bool pause;

    private const float TIMER_DURATION = 20f;

	void Start () {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        pause = true;
        InvokeRepeating("DecreaseTimeRemaining", .1f, .1f);
	}

	//void Update () {
	//	// ?
	//}

    public void Pause() {
        pause = true;
    }

    public void StartTimer() {
        pause = false;
        timeRemaining = TIMER_DURATION;
    }

    void DecreaseTimeRemaining() {
        if (!pause && !mm.targeting.IsTargetMode()) {
            timeRemaining -= .1f;
            if (timeRemaining < .01f) {
                Pause();
                mm.eventCont.Timeout();
            }
            mm.uiCont.newsfeed.UpdateTurnTimer(timeRemaining);
        }
    }
}
