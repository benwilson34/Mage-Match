using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TooltipManager : MonoBehaviour {

    private GameObject _tooltipPF;
    private RectTransform _currentTT;
    private Tooltipable _obj;
    private bool _tooltipShowing = false; // can just use currentTT.gameObject.IsActive();

    private const float TOOLTIP_SHOW_DELAY = .65f; // in seconds
    private const float TOOLTIP_ANIM_DUR = .1f; // in seconds


    // Use this for initialization
    void Start () {
        _tooltipPF = Resources.Load("prefabs/ui/tooltip") as GameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetTooltip(Tooltipable obj) {
        if (obj == null)
            return;

        HideOrCancelTooltip();

        Transform staticUI = GameObject.Find("static ui").transform;
        _currentTT = (RectTransform)Instantiate(_tooltipPF, staticUI).transform;
        //MMDebug.MMLog.Log("TooltipMan", "orange", "Setting tooltip: " + obj.GetTooltipInfo());

        RectTransform textRect = (RectTransform)_currentTT.GetChild(0);
        textRect.GetComponent<Text>().text = obj.GetTooltipInfo();


        _currentTT.gameObject.SetActive(false);
        _obj = obj;

        StartCoroutine(ShowTooltipAfterDelay());
    }

    IEnumerator ShowTooltipAfterDelay() {
        yield return new WaitForSeconds(TOOLTIP_SHOW_DELAY);
        if (_obj != null) {
            StartCoroutine(ShowTooltip());
        }
    }

    public IEnumerator ShowTooltip() {
        _currentTT.gameObject.SetActive(true);

        MonoBehaviour mb = (MonoBehaviour)_obj;

        // get screen pos of target
        Vector3 pos;
        if (mb.GetComponent<RectTransform>() != null) // if UI element
            pos = mb.transform.position;
        else {
            pos = Camera.main.WorldToScreenPoint(mb.transform.position);
        }

        // displace so it appears vertically above target
        pos.y += 50;

        // you have to do this so the TT can draw itself...
        yield return new WaitForEndOfFrame();

        Vector3[] corners = new Vector3[4];
        _currentTT.GetWorldCorners(corners);
        float TTwidth  = corners[2].x - corners[0].x;
        float TTheight = corners[2].y - corners[0].y;

        //MMDebug.MMLog.Log("TooltipMan", "orange", "TT dims 1: " + TTwidth + ", " + TTheight);
        // just a bit of breathing room on the margin (20 px)
        TTwidth += 20;
        TTheight += 20;

        //MMDebug.MMLog.Log("TooltipMan", "orange", "pos before=" + pos.ToString());
        //MMDebug.MMLog.Log("TooltipMan", "orange", "screen dims: " + Screen.width + ", " + Screen.height);

        //Vector3 worldDims = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        // center the TT
        pos.x -= TTwidth / 2;
        //MMDebug.MMLog.Log("TooltipMan", "orange", "world dims=" + worldDims.ToString());

        // keep the TT from being drawn off the screen
        pos.x = Mathf.Clamp(pos.x, 0, Screen.width - TTwidth);
        pos.y = Mathf.Clamp(pos.y, 0, Screen.height - TTheight);

        //MMDebug.MMLog.Log("TooltipMan", "orange", "TT dims : (" + TTwidth + ", " + TTheight + ")");

        CanvasGroup cg = _currentTT.GetComponent<CanvasGroup>();
        cg.alpha = 0;

        //MMDebug.MMLog.Log("TooltipMan", "orange", "pos  after=" + pos.ToString());
        _currentTT.SetPositionAndRotation(pos, Quaternion.identity);

        yield return cg.DOFade(1, TOOLTIP_ANIM_DUR); 

        _tooltipShowing = true;
    }

    public void HideOrCancelTooltip() {
        //MMDebug.MMLog.Log("TooltipMan", "orange", ">>>Hiding/canceling the tooltip<<<");
        if (_currentTT != null) {
            if (_tooltipShowing) {
                _tooltipShowing = false;
                // animate out?
            }
            Destroy(_currentTT.gameObject);
            _currentTT = null;
            _obj = null;
        }
    }
}


public interface Tooltipable {
    string GetTooltipInfo();
}
