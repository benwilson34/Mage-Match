using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Newsfeed : MonoBehaviour {

    private GameObject _newsfeedMenu;
    private GameObject _actionLogItemPF;
    private Transform _actionLog;
    private Text _turnCounterText, _newsText;
    private Image _timerHourglass;
    private int _turn;
    private bool _isFirstAction = true;

    private GameObject _lastActionLog;
    private MageMatch _mm;

	// Use this for initialization
	void Start () {
        _newsfeedMenu = GameObject.Find("Newsfeed_Menu");
        _actionLogItemPF = Resources.Load("prefabs/ui/actionLogItem") as GameObject;

        Transform actionLogT = GameObject.Find("scr_actionLog").transform;
        _actionLog = actionLogT.Find("Viewport").Find("Content");

        _timerHourglass = transform.Find("i_hourglass").GetComponent<Image>();
        UpdateTurnTimer(TurnTimer.TIMER_DURATION);
        _turnCounterText = transform.Find("t_turns").GetComponent<Text>();
        UpdateTurnCount(1);
        _newsText = transform.Find("t_news").GetComponent<Text>();
        UpdateNewsfeed("!!! FIGHT !!!");

        _newsfeedMenu.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init(MageMatch mm) {
        _mm = mm;
    }

    public void UpdateTurnTimer(float time) {
        _timerHourglass.fillAmount = time / TurnTimer.TIMER_DURATION;
    }

    public void UpdateNewsfeed(string str) {
        AddActionLogItem(str);
        StartCoroutine(_UpdateNewsfeed(str));
    }

    IEnumerator _UpdateNewsfeed(string str) {
        yield return _newsText.DOFade(0, .2f);

        yield return new WaitForSeconds(.3f);
        _newsText.text = str;

        yield return _newsText.DOFade(1, .2f);
    }

    public void AddActionLogItem(string msg) {
        Transform item = Instantiate(_actionLogItemPF, _actionLog).transform;

        if (_isFirstAction) { // only show turn if it's the first action
            Transform turnT = item.Find("t_turnCount");
            turnT.GetComponent<Text>().text = _turn + "";
            _isFirstAction = false;
        } else {
            item.Find("t_turnLabel").gameObject.SetActive(false);
            item.Find("t_turnCount").gameObject.SetActive(false);
            item.Find("i_rule").gameObject.SetActive(false);
        }

        Transform msgT = item.Find("t_news");
        msgT.GetComponent<Text>().text = msg;

        RevealPreviousNews();
        _lastActionLog = item.gameObject;
        _lastActionLog.SetActive(false);
    }

    void RevealPreviousNews() {
        if (_lastActionLog != null) {
            _lastActionLog.SetActive(true);
        }
    }

    public void UpdateTurnCount(int count) {
        _isFirstAction = true;
        _turn = count;
        _turnCounterText.text = count + "";
    }

    public void ToggleMenu() {
        bool menuOpen = !_newsfeedMenu.GetActive();
        if (menuOpen)
            _mm.EnterState(MageMatch.State.NewsfeedMenu);
        else
            _mm.ExitState();

        _newsfeedMenu.SetActive(menuOpen);
    }

    public bool isMenuOpen() { return _newsfeedMenu.GetActive(); }

}
