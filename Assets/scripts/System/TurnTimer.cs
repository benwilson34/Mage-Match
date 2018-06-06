using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTimer : MonoBehaviour {

    public static float TIMER_DURATION = 20f, TIMER_WARNING = 5f;

    private MageMatch _mm;
    private float _timeRemaining;
    private bool _pause = false, _playedWarningSound = false;

    public void Init(MageMatch mm) {
        _mm = mm;
        _pause = true;
        InvokeRepeating("DecreaseTimeRemaining", .1f, .1f);
    }

    public void Pause() {
        _pause = true;
    }

    public void StartTimer() {
        _pause = false;
        _playedWarningSound = false;
        _timeRemaining = TIMER_DURATION;
    }

    void DecreaseTimeRemaining() {
        if (!_pause && !Targeting.IsTargetMode()) {
            _timeRemaining -= .1f;

            if (!_playedWarningSound && _timeRemaining < TIMER_WARNING) {
                AudioController.Trigger(AudioController.OtherSoundEffect.TurnTimerWarning);
                _playedWarningSound = true;
            }

            if (_timeRemaining < .01f) {
                Pause();
                EventController.Timeout();
            }
            _mm.uiCont.newsfeed.UpdateTurnTimer(_timeRemaining);
        }
    }
}
