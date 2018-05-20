using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ResultScreen : MonoBehaviour {

    private MageMatch _mm;
    private Transform _resultList;
    private Transform _panel;
    private GameObject _resultListItemPF;

    private int _totalReward, _userTotal;

    public void Init(MageMatch mm) {
        _mm = mm;
        _panel = transform.Find("p_results");
        _resultList = _panel.Find("ResultList");

        _resultListItemPF = Resources.Load<GameObject>("prefabs/ui/resultListItem");
        gameObject.SetActive(false);
    }

    public IEnumerator Display(int losingPlayerId) {
        gameObject.SetActive(true);
        _panel.Find("t_win").GetComponent<Text>().text =
            _mm.myID == losingPlayerId ? "YOU LOST..." : "YOU WIN!!";
        PopulateResultList(losingPlayerId);

        yield return transform.DOMoveY(transform.position.y, .5f).From().WaitForCompletion();

        yield return null;
    }

    void PopulateResultList(int losingPlayerId) {
        foreach (Transform child in _resultList)
            GameObject.Destroy(child.gameObject);

        _totalReward = 0;

        AddResultItem("Match Completed:", 100);

        if (_mm.myID != losingPlayerId)
            AddResultItem("Match Victory:", 50);

        // TODO if first victory today
        // TODO win streak

        AddResultItem("Match Length:", _mm.stats.turns);

        // TODO 10 victories in one day

        Transform matchTotal = _panel.Find("resultListItem_total");
        matchTotal.Find("t_coin").GetComponent<Text>().text = _totalReward + " M$";

        UserData.Init();
        _userTotal = UserData.MMCoin + _totalReward;
        Transform newBalance = _panel.Find("resultListItem_newBalance");
        newBalance.Find("t_coin").GetComponent<Text>().text = _userTotal + " M$";

        UserData.MMCoin = _userTotal;
    }

    void AddResultItem(string msg, int amount) {
        Transform item = Instantiate(_resultListItemPF, _resultList).transform;

        item.Find("t_desc").GetComponent<Text>().text = msg;
        item.Find("t_coin").GetComponent<Text>().text = "+" + amount + " M$";

        _totalReward += amount;
    }

    public void OnConfirm() {
        SceneManager.LoadScene("Menu");
    }
}
