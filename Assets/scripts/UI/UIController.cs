using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using MMDebug;

public class UIController : MonoBehaviour {

    //public AnimationCurve slidingEase;
    [HideInInspector]
    public Sprite miniFire, miniWater, miniEarth, miniAir, miniMuscle;
    [HideInInspector]
    public TooltipManager tooltipMan;
    [HideInInspector]
    public Newsfeed newsfeed;

    public GameObject alertbar, quickdrawButton; 

    private ButtonController _drawButton;
    private Text _tDeckCount, _tRemovedCount;
    private MageMatch _mm;
    private Transform _leftPinfo, _rightPinfo, _leftPspells, _rightPspells, _board;
    private GameObject _spellOutlineEnd, _spellOutlineMid;
    private GameObject _prereqPF, _targetPF;
    private GameObject _gradient, _targetingBG;
    private GameObject _tCancelB, _tClearB;
    private GameObject _screenScroll;
    private List<GameObject> _outlines, _spellOutlines;
    private SpriteRenderer[,] _cellOverlays;
    private Vector3 _slidingTextStart;
    private bool _localOnRight = false;

    private GameObject _hexGlow;

    private const float ALERT_DIS = 55f, ALERT_DELAY = 3f;
    private bool _alertShowing = false;

    public void Init(){
        _board = GameObject.Find("cells").transform;
        _mm = GameObject.Find("board").GetComponent<MageMatch>();
        _screenScroll = GameObject.Find("scrolling");

        //slidingText = GameObject.Find("Text_Sliding").GetComponent<Text>();
        //slidingTextStart = new Vector3(Screen.width, slidingText.rectTransform.position.y);
        //slidingText.rectTransform.position = slidingTextStart;

        _leftPinfo = GameObject.Find("LeftPlayer_Info").transform;
        _rightPinfo = GameObject.Find("RightPlayer_Info").transform;
        _leftPspells = GameObject.Find("LeftPlayer_Spells").transform;
        _rightPspells = GameObject.Find("RightPlayer_Spells").transform;


        // buttons
        InitSprites();
        for (int id = 1; id <= 2; id++) {
            for (int i = 0; i < 5; i++) {
                //MMLog.Log_UICont("Init button " + i);
                var button = GetButtonCont(id, i);
                //MMLog.Log_UICont("mm = " + _mm.myID);
                button.Init(_mm, id);
                button.Deactivate();
                if (id == _mm.myID) { // only make my buttons interactable for the match
                    button.SetInteractable();
                }
            }
        }
        Transform drawT = GameObject.Find("b_draw").transform;
        _drawButton = drawT.GetComponent<ButtonController>();
        _drawButton.Init(_mm, _mm.myID);
        _tDeckCount = drawT.Find("t_deckCount").GetComponent<Text>();
        _tRemovedCount = drawT.Find("t_removedCount").GetComponent<Text>();


        // newsfeed
        newsfeed = GameObject.Find("Newsfeed").GetComponent<Newsfeed>();
        newsfeed.Init(_mm);


        // other
        _spellOutlines = new List<GameObject>();
        _outlines = new List<GameObject>();

        LoadPrefabs();

        tooltipMan = GetComponent<TooltipManager>();

        quickdrawButton.SetActive(false);

        alertbar.transform.Translate(0, ALERT_DIS, 0);
        alertbar.SetActive(true);
    }

    public void InitEvents() {
        _mm.eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.LastStep);
        _mm.eventCont.AddTurnEndEvent(OnTurnEnd, EventController.Type.LastStep);
        _mm.eventCont.gameAction += OnGameAction;
        _mm.eventCont.AddDrawEvent(OnDraw, EventController.Type.LastStep, EventController.Status.Begin);
        //_mm.eventCont.AddMatchEvent(OnMatch, EventController.Type.LastStep);
        _mm.eventCont.playerHealthChange += OnPlayerHealthChange;
        _mm.eventCont.playerMeterChange += OnPlayerMeterChange;
    }

    void LoadPrefabs() {
        _spellOutlineEnd = Resources.Load<GameObject>("prefabs/ui/spell-outline_end");
        _spellOutlineMid = Resources.Load<GameObject>("prefabs/ui/spell-outline_mid");
        _prereqPF = Resources.Load("prefabs/outline_prereq") as GameObject;
        _targetPF = Resources.Load("prefabs/outline_target") as GameObject;
    }

    void InitSprites() {
        Sprite[] minitiles = Resources.LoadAll<Sprite>("sprites/ui-spelltiles");
        miniFire = minitiles[0];
        miniWater = minitiles[1];
        miniEarth = minitiles[2];
        miniAir = minitiles[3];
        miniMuscle = minitiles[4];
    }

    #region EventCont calls
    public IEnumerator OnTurnBegin(int id) {
        if (_mm.MyTurn()) {
            ShowAlertText(_mm.ActiveP().name + ", make your move!");
            //UpdateMoveText("Completed turns: " + (mm.stats.turns - 1));

            SetDrawButton(true);
        }
        UpdateAP(_mm.GetPlayer(id));

        //ChangePinfoColor(id, new Color(0, 1, 0, .4f));
        //PinfoColorTween(id, CorrectPinfoColor(id));
        yield return null;
    }

    public IEnumerator OnTurnEnd(int id) {
        newsfeed.UpdateTurnCount(_mm.stats.turns);
        //ChangePinfoColor(id, new Color(1, 1, 1, .4f));
        yield return null;
    }

    public void OnGameAction(int id, bool costsAP) {
        UpdateAP(_mm.GetPlayer(id));
    }

    public IEnumerator OnDraw(int id, string tag, bool playerAction, bool dealt) {
        // TODO update button
        if (_mm.GetPlayer(id).hand.IsFull())
            _drawButton.Deactivate();
        yield return null;
    }

    public void OnPlayerHealthChange(int id, int amount, int newHealth, bool dealt) {
        StartCoroutine(UpdateHealthbar(_mm.GetPlayer(id)));
        StartCoroutine(DamageAnim(_mm.GetPlayer(id), amount));
    }

    public void OnPlayerMeterChange(int id, int amount, int newMeter) {
        StartCoroutine(UpdateMeterbar(_mm.GetPlayer(id)));
    }
    #endregion

    public void Reset() {
        ShowAlertText("Fight!!");

        if (!_mm.MyTurn())
            SetDrawButton(false);

        for (int id = 1; id <= 2; id++) {
            Player p = _mm.GetPlayer(id);
            Transform pinfo = GetPinfo(id);

            Text nameText = pinfo.Find("t_name").GetComponent<Text>();
            nameText.text = p.name;
            SetPinfoColor(id);
            //if (id == _mm.myID)
            //    nameText.text += " (ME!)";

            ShowAllSpellInfo(id);
            DeactivateAllSpellButtons(id); // not needed now
            UpdateAP(p);

            // start health from zero so it animates
            pinfo.Find("i_healthbar").GetComponent<Image>().fillAmount = 0f;
            pinfo.Find("t_health").GetComponent<Text>().text = "0";
            StartCoroutine(UpdateHealthbar(p));

            // maybe just set it to empty?
            StartCoroutine(UpdateMeterbar(p));
        }

        // camera position relative to center of the board
        camOffset = Camera.main.transform.position.x;
        camOffset = GameObject.Find("s_board").transform.position.x - camOffset;
        if (!_mm.MyTurn() ^ _localOnRight) // because it's currently relative, only do this if needed
            StartCoroutine(ShiftScreen());
    }

    float camOffset;

    public IEnumerator ShiftScreen() {
        //RectTransform rect = screenScroll.GetComponent<RectTransform>();

        Transform camPos = Camera.main.transform;
        
        if (_mm.MyTurn() ^ _localOnRight) { // left side
            yield return camPos.DOMoveX(camPos.position.x - (2*camOffset), .3f).WaitForCompletion();
        } else { // right side
            yield return camPos.DOMoveX(camPos.position.x + (2*camOffset), .3f).WaitForCompletion();
        }
        yield return null;
    }

    public void ShowLocalAlertText(int id, string str) {
        if (id == _mm.myID)
            ShowAlertText(str);
    }
    public void ShowAlertText(string str) {
        StartCoroutine(_ShowAlertText(str));
    }
    IEnumerator _ShowAlertText(string str) {
        Transform t = alertbar.transform;
        t.Find("t_alert").GetComponent<Text>().text = str;

        if (!_alertShowing) {
            _alertShowing = true;
            float posY = t.position.y;
            yield return t.DOMoveY(posY - ALERT_DIS, .25f).WaitForCompletion();
            yield return new WaitForSeconds(ALERT_DELAY);
            yield return t.DOMoveY(posY, .25f).WaitForCompletion();
            _alertShowing = false;
        }
    }


    #region ----- PLAYER INFO -----
    public Transform GetPinfo(int id) {
        if (id == _mm.myID ^ _localOnRight)
            return _leftPinfo;
        else
            return _rightPinfo;
    }

    void SetPinfoColor(int id) {
        Image healthImg = GetPinfo(id).Find("i_healthbar").GetComponent<Image>();
        switch (_mm.GetPlayer(id).character.ch) {
            case Character.Ch.Enfuego:
                healthImg.color = Color.red;
                break;
            case Character.Ch.Gravekeeper:
                healthImg.color = Color.green;
                break;
            case Character.Ch.Valeria:
                healthImg.color = Color.blue;
                break;

            case Character.Ch.Sample:
                healthImg.color = Color.gray;
                break;
        }
    }

    //Tween PinfoColorTween(int id, Color newColor) {
    //    Transform pinfo = GetPinfo(id);
    //    return pinfo.GetComponent<Image>().DOColor(newColor, 0.25f)
    //        .SetEase(Ease.InOutQuad).SetLoops(7, LoopType.Yoyo);
    //} 

    void UpdateAP(Player p) {
        var APimage = GetPinfo(p.id).Find("i_AP").GetComponent<Image>();
        APimage.fillAmount = (float)p.AP / Player.MAX_AP;
    }

    IEnumerator UpdateHealthbar(Player p) {
        Transform pinfo = GetPinfo(p.id);
        Image healthImg = pinfo.Find("i_healthbar").GetComponent<Image>();
        Text healthText = pinfo.Find("t_health").GetComponent<Text>();

        int healthAmt = p.character.GetHealth();
        TextNumTween(healthText, healthAmt);

        float slideRatio = (float)healthAmt / p.character.GetMaxHealth();

        // TODO I have a feeling I can just Lerp this? lol
        // health bar coloring; green -> yellow -> red
        //float thresh = .6f; // point where health bar is yellow (0.6 = 60% health)
        //float r = (((Mathf.Clamp(slideRatio, thresh, 1) - thresh) / (1 - thresh)) * -1 + 1);
        //float g = Mathf.Clamp(slideRatio, 0, thresh) / thresh;
        //healthbar.GetComponent<Image>().color = new Color(r, g, 0);

        yield return healthImg.DOFillAmount(slideRatio, .8f).SetEase(Ease.OutCubic).WaitForCompletion();
    }

    IEnumerator UpdateMeterbar(Player p) {
        Transform pinfo = GetPinfo(p.id);
        Image sig = pinfo.Find("i_meterbar").GetComponent<Image>();
        Text sigText = pinfo.Find("t_meter").GetComponent<Text>();

        int meter = p.character.GetMeter();
        TextNumTween(sigText, meter / 10, "%"); // change if a character has meter of different amount 

        float slideRatio = (float) meter / Character.METER_MAX;
        //yield return meter.DOScaleX(slideRatio, .8f).SetEase(Ease.OutCubic);
        yield return sig.DOFillAmount(slideRatio, .8f).SetEase(Ease.OutCubic).WaitForCompletion();
    }

    IEnumerator DamageAnim(Player p, int amount) {
        if (amount < 0) { // damage
            Image pinfoImg = GetPinfo(p.id).GetComponent<Image>();
            pinfoImg.color = new Color(1, 0, 0, 0.4f); // red
            Color pColor = CorrectPinfoColor(p.id);
            yield return new WaitForSeconds(.2f);
            pinfoImg.DOColor(pColor, .3f).SetEase(Ease.OutQuad);
        }
    }

    Color CorrectPinfoColor(int id) {
        //if (id == mm.ActiveP().id && mm.currentTurn != MageMatch.Turn.CommishTurn) {
        //    return new Color(0, 1, 0);
        //} else
            return new Color(1, 1, 1);
    }

    Tween TextNumTween(Text t, int newValue, string suffix = "") {
        string current = t.text;
        if (suffix != "")
            current = current.Split(suffix[0])[0];

        int oldValue = int.Parse(current);
        return DOTween.To(() => oldValue,
            x => { oldValue = x; t.text = oldValue + suffix; },
            newValue, .8f)
            .SetEase(Ease.OutCubic);
    }

	void ShowAllSpellInfo(int id){
        // for each spell
		for (int i = 0; i < 5; i++)
            GetButtonCont(id, i).ShowSpellInfo();
	}
    #endregion


    #region ----- TARGETING -----
    public void ShowSpellSeqs(List<TileSeq> seqs) {
        foreach (TileSeq seq in seqs) {
            MMLog.Log_UICont("showing " + seqs.Count + " seqs");
            GameObject start = Instantiate(_spellOutlineEnd);
            int length = seq.GetSeqLength();
            for (int i = 1; i < length; i++) {
                GameObject piece;
                if (i == length - 1)
                    piece = Instantiate(_spellOutlineEnd, start.transform);
                else
                    piece = Instantiate(_spellOutlineMid, start.transform);
                piece.transform.position = new Vector3(0, i);
                piece.transform.rotation = Quaternion.Euler(0, 0, -90);
            }

            Vector2 tilePos = _mm.hexGrid.GridCoordToPos(seq.sequence[0]);
            start.transform.position = new Vector3(tilePos.x, tilePos.y);
            start.transform.Rotate(0, 0, _mm.hexGrid.GetDirection(seq) * -60);

            _spellOutlines.Add(start);
        }
    }

    public void HideSpellSeqs() {
        for (int i = 0; i < _spellOutlines.Count;) {
            Destroy(_spellOutlines[0]);
            _spellOutlines.RemoveAt(0);
        }
    }

    public void GetCellOverlays() {
        _cellOverlays = new SpriteRenderer[7,7];
        for(int i = 0; i < 7; i++) {
            Transform col = _board.GetChild(i);
            for(int j = _mm.hexGrid.BottomOfColumn(i); j <= _mm.hexGrid.TopOfColumn(i); j++) {
                //Debug.Log(">>>2nd for of getCellOverlays at " + (i) + (j));
                _cellOverlays[i,j] = col.Find("cell" + (i) + (j)).GetComponent<SpriteRenderer>();
            }
        } 
    }

    void ActivateTargetingUI() {
        //overlay.SetActive(true);
        //if (mm.MyTurn()) {
        //    tCancelB.SetActive(true);
        //    tClearB.SetActive(true);
        //}

        for(int i = 0; i < 7; i++) {
            for(int j = _mm.hexGrid.BottomOfColumn(i); j <= _mm.hexGrid.TopOfColumn(i); j++) {
                Color c = _cellOverlays[i,j].color;
                c.a = 0.6f;
                _cellOverlays[i,j].color = c;
                //Debug.Log("Color = " + cellOverlays[i,j].color);
            }
        }

        TileSeq selection = _mm.targeting.GetSelection();
        if(selection != null)
            OutlinePrereq(selection);

        //gradient.SetActive(!gradient.activeSelf);
        //targetingBG.SetActive(!targetingBG.activeSelf);
    }

    public void ActivateTargetingUI(List<TileBehav> tbs) {
        ActivateTargetingUI();

        foreach(TileBehav tb in tbs) {
            Tile t = tb.tile;
            Color c = _cellOverlays[t.col,t.row].color;
            c.a = 0.0f;
            _cellOverlays[t.col,t.row].color = c;
        }
    }

    public void ActivateTargetingUI(List<CellBehav> cbs) {
        ActivateTargetingUI();

        foreach (CellBehav cb in cbs) {
            // TODO do something with the filtered cells
        }
    }

    public void DeactivateTargetingUI(){
        //overlay.SetActive(false);
        //if (mm.MyTurn()) {
        //    tCancelB.SetActive(false);
        //    tClearB.SetActive(false);
        //}

        for (int i = 0; i < 7; i++) {
            for (int j = _mm.hexGrid.BottomOfColumn(i); j <= _mm.hexGrid.TopOfColumn(i); j++) {
                Color c = _cellOverlays[i,j].color;
                c.a = 0.0f;
                _cellOverlays[i,j].color = c;
            }
        }

        ClearOutlines();
    }

    void OutlinePrereq(TileSeq seq) {
        _outlines = new List<GameObject>(); // move to Init?
        GameObject go;
        foreach (Tile t in seq.sequence) {
            go = Instantiate(_prereqPF);
            go.transform.position = _mm.hexGrid.GridCoordToPos(t.col, t.row);
            _outlines.Add(go);
        }
    }

    public void OutlineTarget(int col, int row) {
        GameObject go = Instantiate(_targetPF);
        go.transform.position = _mm.hexGrid.GridCoordToPos(col, row);
        _outlines.Add(go);
    }

    //public void ClearTargets() {
    //    int prereqs = _mm.targeting.GetSelection().sequence.Count;
    //    for (int i = 0; i < _outlines.Count - prereqs;) { // clear just the target outlines
    //        GameObject go = _outlines[prereqs];
    //        GameObject.Destroy(go);
    //        _outlines.Remove(go);
    //    }
    //}

    void ClearOutlines() {
        for (int i = 0; i < _outlines.Count;) {
            GameObject go = _outlines[0];
            GameObject.Destroy(go);
            _outlines.Remove(go);
        }
    }

    #endregion

    public void ToggleQuickdrawUI(bool on, Hex hex = null) {
        if (!_mm.MyTurn())
            return;

        // TODO glow under hex
        quickdrawButton.SetActive(on);

        int id = _mm.ActiveP().id;
        TurnAllSpellButtons(id, !on);
        if (on) {
            _drawButton.Deactivate();

            Vector3 newPos = hex.transform.position;
            newPos.y += 1;
            quickdrawButton.transform.position = newPos;
        } else {
            _drawButton.Activate();
        }
    }

    public void KeepQuickdraw() {
        _mm.prompt.SetQuickdrawHand();
        _mm.syncManager.SendKeepQuickdraw();
        _mm.stats.Report("$ QUICKDRAW KEEP", false);
    }


    #region ----- BUTTONS -----
    public void SetDrawButton(bool interactable) {
        if (_mm.MyTurn()) {
            if (interactable)
                _drawButton.Activate();
            else
                _drawButton.Deactivate();
        }
    }

    public void UpdateDeckCount(int count) {
        _tDeckCount.text = count + "";
    }

    public void UpdateRemovedCount(int count) {
        _tRemovedCount.text = count + "";
    }

    public Transform GetPspells(int id) {
        if (id == _mm.myID ^ _localOnRight)
            return _leftPspells;
        else
            return _rightPspells;
    }

    public ButtonController GetButtonCont(int id, int index) {
        //Transform t = GetPspells(id).Find("b_Spell" + index);
        //MMLog.Log_UICont("Found " + t.name + ", parent=" + t.parent.name);
        return GetPspells(id).Find("b_Spell" + index)
            .GetComponent<ButtonController>();
    }
    
	public void ActivateSpellButton(int id, int index){
        if (_mm.MyTurn()) {
            var button = GetButtonCont(id, index);
            button.Activate();
        }
	}

	public void DeactivateSpellButton(int id, int index){
		var button = GetButtonCont(id, index);
        button.Deactivate();
	}

	public void DeactivateAllSpellButtons(int id){
        for (int i = 0; i < 5; i++) {
            var button = GetButtonCont(id, i);
            button.Deactivate();
        }
	}

    public void TurnAllSpellButtons(int id, bool on) {
        for (int i = 0; i < 5; i++) {
            var button = GetButtonCont(id, i);
            if (on)
                button.TurnOnScreen();
            else
                button.TurnOffScreen();
        }
    }

    public void TurnOffSpellButtonsDuringCast(int id, int spellNum) {
        for (int i = 0; i < 5; i++) {
            if (i == spellNum)
                continue;
            var button = GetButtonCont(id, i);
            button.TurnOffScreen();
        }
    }

    public void TurnOnSpellButtonsAfterCast(int id, int spellNum) {
        for (int i = 0; i < 5; i++) {
            if (i == spellNum)
                continue;
            var button = GetButtonCont(id, i);
            button.TurnOnScreen();
        }
    }
    #endregion

}
