﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Newsfeed : MonoBehaviour {

    private GameObject newsfeedMenu;
    private GameObject actionLogItemPF;
    private Transform actionLog;
    private Text turnTimerText, turnCounterText, newsText;
    private int turn;
    private bool isFirstAction = true;

    private GameObject lastActionLog;

	// Use this for initialization
	void Start () {
        newsfeedMenu = GameObject.Find("Newsfeed_Menu");
        actionLogItemPF = Resources.Load("prefabs/ui/actionLogItem") as GameObject;

        Transform actionLogT = GameObject.Find("scr_actionLog").transform;
        actionLog = actionLogT.Find("Viewport").Find("Content");

        turnTimerText = transform.Find("p_timer").Find("t_timer").GetComponent<Text>();
        UpdateTurnTimer(20f);
        turnCounterText = transform.Find("p_turns").Find("t_turnCount").GetComponent<Text>();
        UpdateTurnCount(1);
        newsText = transform.Find("p_news").Find("t_news").GetComponent<Text>();
        UpdateNewsfeed("!!! FIGHT !!!");

        newsfeedMenu.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void UpdateTurnTimer(float time) {
        turnTimerText.text = time.ToString("0.0");
    }

    public void UpdateNewsfeed(string str) {
        AddActionLogItem(str);
        StartCoroutine(_UpdateNewsfeed(str));
    }

    IEnumerator _UpdateNewsfeed(string str) {
        yield return newsText.DOFade(0, .2f);

        yield return new WaitForSeconds(.3f);
        newsText.text = str;

        yield return newsText.DOFade(1, .2f);
    }

    public void AddActionLogItem(string msg) {
        Transform item = Instantiate(actionLogItemPF).transform;

        if (isFirstAction) { // only show turn if it's the first action
            Transform turnT = item.Find("p_turns").Find("t_turnCount");
            turnT.GetComponent<Text>().text = turn + "";
            isFirstAction = false;
        } else {
            item.Find("p_turns").gameObject.SetActive(false);
            item.Find("i_rule").gameObject.SetActive(false);
        }

        Transform msgT = item.Find("p_news").Find("t_news");
        msgT.GetComponent<Text>().text = msg;

        item.SetParent(actionLog);
        RevealPreviousNews();
        lastActionLog = item.gameObject;
        lastActionLog.SetActive(false);
    }

    void RevealPreviousNews() {
        if (lastActionLog != null) {
            lastActionLog.SetActive(true);
        }
    }

    public void UpdateTurnCount(int count) {
        isFirstAction = true;
        turn = count;
        turnCounterText.text = count + "";
    }

    public void ToggleMenu() {
        newsfeedMenu.SetActive(!newsfeedMenu.GetActive());
    }

    public bool isMenuOpen() { return newsfeedMenu.GetActive(); }

}
