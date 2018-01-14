using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTimer : MonoBehaviour {

    public static float TIMER_DURATION = 20f, TIMER_WARNING = 5f;

    private float timeRemaining;
    private MageMatch mm;
    private bool pause = false, playedWarningSound = false;

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
        playedWarningSound = false;
        timeRemaining = TIMER_DURATION;
    }

    void DecreaseTimeRemaining() {
        if (!pause && !mm.targeting.IsTargetMode()) {
            timeRemaining -= .1f;

            if (!playedWarningSound && timeRemaining < TIMER_WARNING) {
                mm.audioCont.TurnTimerWarning();
                playedWarningSound = true;
            }

            if (timeRemaining < .01f) {
                Pause();
                mm.eventCont.Timeout();
            }
            mm.uiCont.newsfeed.UpdateTurnTimer(timeRemaining);
        }
    }
}
