using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using MMDebug;

public class UIController : MonoBehaviour {

    public enum ScreenSide { Left, Right };

    //public AnimationCurve slidingEase;
    public Gradient healthbarGradient;
    [HideInInspector]
    public Sprite miniFire, miniWater, miniEarth, miniAir, miniMuscle;
    [HideInInspector]
    public TooltipManager tooltipMan;

    public Newsfeed newsfeed;
    public ResultScreen resultsScreen;

    public GameObject alertbar, quickdrawButton, loadingText, loadingScreen;
    public GameObject gameStartScreen, coinFlip, knockoutScreen, errorButton; 

    private MageMatch _mm;
    private Transform _leftPinfo, _rightPinfo, _leftPspells, _rightPspells, _board;
    private GameObject _healthChangeNumPF;
    private GameObject _spellOutlineEndPF, _spellOutlineMidPF;
    private GameObject _prereqPF, _targetPF;
    private GameObject _gradient, TargetingBG;
    private GameObject _tCancelB, _tClearB;
    private GameObject _screenScroll;
    private List<GameObject> _outlines, _spellOutlines;
    private SpriteRenderer[,] _cellOverlays;

    private const float SHIFT_DUR = .4f;
    private float _camOffset, _spellSlidingOffset;
    private bool _shiftedLeft = true, _localOnRight = false;
    private Transform _leftPportrait, _rightPportrait;
    private float _portraitOffset;

    private ButtonController _leftDrawButtonCont, _rightDrawButtonCont;
    private Transform _leftDrawButton, _rightDrawButton;
    private float _drawButtonOffset;

    private GameObject _hexGlow;

    private const float ALERT_DELAY = 3f;
    private float _alertbarOffset;
    private bool _alertShowing = false;

    private Reporter reporter;

    #region ---------- INIT ----------

    public void Init(MageMatch mm){
        loadingScreen.SetActive(true);
        gameStartScreen.SetActive(false);

        _mm = mm;
        _board = GameObject.Find("cells").transform;
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
                if (id == _mm.myID || _mm.gameMode == MageMatch.GameMode.TrainingTwoChars) { 
                    // only make my buttons interactable for the match
                    button.SetInteractable();
                }
            }
        }

        _leftDrawButton = GameObject.Find("LeftPlayer_Draw").transform;
        _leftDrawButtonCont = _leftDrawButton.GetComponent<ButtonController>();
        _leftDrawButtonCont.Init(_mm, _mm.myID);

        _rightDrawButton = GameObject.Find("RightPlayer_Draw").transform;
        _rightDrawButtonCont = _rightDrawButton.GetComponent<ButtonController>();
        _rightDrawButtonCont.Init(_mm, _mm.myID);

        // TODO get the right number for this
        //_drawButtonOffset = Camera.main.transform.position.y;
        //_drawButtonOffset += Screen.height / 2;
        _drawButtonOffset = Camera.main.ScreenToWorldPoint(Vector3.zero).y;
        Debug.Log("_drawButtonOffset=" + _drawButtonOffset);
        _drawButtonOffset = Camera.main
            .ScreenToWorldPoint(_leftDrawButton.GetComponent<RectTransform>().rect.size).y
            - _drawButtonOffset;
        _drawButtonOffset *= 1.5f;
        Debug.Log("_drawButtonOffset=" + _drawButtonOffset);
        _rightDrawButton.Translate(0, _drawButtonOffset, 0);


        // scripts
        newsfeed.Init(_mm);
        resultsScreen.Init(_mm);
        tooltipMan = GetComponent<TooltipManager>();


        // other
        _spellOutlines = new List<GameObject>();
        _outlines = new List<GameObject>();

        LoadPrefabs();

        quickdrawButton.SetActive(false);

        coinFlip.SetActive(false);


        // alert bar offset

        //_alertbarOffset = loadingScreen.GetComponent<RectTransform>().rect.yMax;
        //_alertbarOffset = GameObject.Find("static ui").GetComponent<RectTransform>().rect.yMax;
        _alertbarOffset = Camera.main.WorldToScreenPoint(Camera.main.transform.position).y;
        _alertbarOffset += Screen.height / 2;
        //GameObject.Find("TEST").transform.position = new Vector3(0, _alertbarOffset);
        Debug.Log("alertBarOffset=" + _alertbarOffset);
        _alertbarOffset -= alertbar.transform.position.y;
        Debug.Log("alertBarOffset=" + _alertbarOffset);
        _alertbarOffset *= 3; // only needs to be 2 but for some reason that's not quite enough?
        Debug.Log("alertBarOffset=" + _alertbarOffset);
        alertbar.transform.Translate(0, _alertbarOffset, 0);
        alertbar.SetActive(true);


        _mm.AddEventContLoadEvent(OnEventContLoaded);
        _mm.AddPlayersLoadEvent(OnPlayersLoaded);

        errorButton.SetActive(false);
        Application.logMessageReceived += Debug_OnError;
    }

    public void OnEventContLoaded() {
        EventController.AddTurnBeginEvent(OnTurnBegin, MMEvent.Behav.LastStep);
        EventController.AddTurnEndEvent(OnTurnEnd, MMEvent.Behav.LastStep);
        //EventController.gameAction += OnGameAction;
        EventController.AddHandChangeEvent(OnHandChange, MMEvent.Behav.LastStep, MMEvent.Moment.Begin);
        //EventController.AddMatchEvent(OnMatch, EventController.Type.LastStep);
        EventController.playerHealthChange += OnPlayerHealthChange;
        EventController.playerMeterChange += OnPlayerMeterChange;
    }

    void LoadPrefabs() {
        _healthChangeNumPF = Resources.Load<GameObject>("prefabs/ui/healthChangeNum");
        _spellOutlineEndPF = Resources.Load<GameObject>("prefabs/ui/spell-outline_end");
        _spellOutlineMidPF = Resources.Load<GameObject>("prefabs/ui/spell-outline_mid");
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

    public void OnPlayersLoaded() {
        _leftPportrait = GameObject.Find("LeftPlayer_Portrait").transform;
        _rightPportrait = GameObject.Find("RightPlayer_Portrait").transform;

        for (int id = 1; id <= 2; id++) {
            Player p = _mm.GetPlayer(id);
            Transform pinfo = GetPinfo(id);

            Transform gameStartSide = GetGameStartSide(id);
            gameStartSide.Find("i_char").GetComponent<Image>().sprite =
                Resources.Load<Sprite>("sprites/characters/" + p.Character.ch.ToString());
            gameStartSide.Find("t_name").GetComponent<Text>().text = p.Name;

            pinfo.Find("t_name").GetComponent<Text>().text = p.Name;
            //SetPinfoColor(id);
            //if (id == _mm.myID)
            //    nameText.text += " (ME!)";

            ShowAllSpellInfo(id);
            DeactivateAllSpellButtons(id); // not needed now
            GetPortrait(id).Find("i_portrait").GetComponent<Image>().sprite =
                Resources.Load<Sprite>("sprites/character-thumbs/" + p.Character.ch.ToString());

            UpdateAP(p);
        }


        // camera position relative to center of the board
        _camOffset = Camera.main.transform.position.x;
        _camOffset = GameObject.Find("s_board").transform.position.x - _camOffset;

        float boardPos = GameObject.Find("s_board").transform.position.x;

        RectTransform leftPlayerSpells = transform.Find("Spells")
            .Find("LeftPlayer_Spells").GetComponent<RectTransform>();
        float leftSpellGap = boardPos - leftPlayerSpells.position.x;

        RectTransform rightPlayerSpells = transform.Find("Spells")
            .Find("RightPlayer_Spells").GetComponent<RectTransform>();
        float rightSpellGap = rightPlayerSpells.position.x - boardPos;

        //Debug.Log("spelloffset leftSpellPos=" + leftSpellGap +
        //    ", boardPos=" + boardPos +
        //    ", rightSpellPos=" + rightSpellGap);

        _spellSlidingOffset =  rightSpellGap - leftSpellGap + .3f; // spell towers aren't quite centered

        float camCenter = Camera.main.WorldToScreenPoint(Camera.main.transform.position).x;
        _portraitOffset = (camCenter - _leftPportrait.position.x) 
            - (_rightPportrait.position.x - camCenter);

        //Debug.Log("camOffset=" + _camOffset + ", portraitOffset=" + _portraitOffset);
    }

    public IEnumerator AnimateBeginningOfGame() {
        //yield return new WaitForEndOfFrame();

        // Game start screen
        RectTransform leftSide = gameStartScreen.transform
            .Find("LeftSide").GetComponent<RectTransform>();
        RectTransform rightSide = gameStartScreen.transform
            .Find("RightSide").GetComponent<RectTransform>();

        float origLeft = leftSide.position.x, origRight = rightSide.position.x;
        float center = Camera.main.WorldToScreenPoint(Camera.main.transform.position).x;

        gameStartScreen.SetActive(true);

        leftSide.DOMoveX(center, 1f);
        yield return rightSide.DOMoveX(center, 1f).WaitForCompletion();

        loadingScreen.SetActive(false);
        yield return new WaitForSeconds(2.3f);

        leftSide.DOMoveX(origLeft, .6f);
        yield return rightSide.DOMoveX(origRight, .6f).WaitForCompletion();

        gameStartScreen.SetActive(false);



        ShowAlertText("Fight!!");

        for (int id = 1; id <= 2; id++) {
            Player p = _mm.GetPlayer(id);
            Transform pinfo = GetPinfo(id);

            // start health from zero so it animates
            pinfo.Find("i_healthbar").GetComponent<Image>().fillAmount = 0f;
            pinfo.Find("t_health").GetComponent<Text>().text = "0";
            StartCoroutine(UpdateHealthbar(p));

            // maybe just set it to empty?
            StartCoroutine(UpdateMeterbar(p));
        }
    }

    public IEnumerator AnimateCoinFlip() {
        // TODO animate spinner sliding from top of screen
        coinFlip.SetActive(true);
        Transform spinningArrow = coinFlip.transform.Find("i_spinningArrow");
        Transform tFirstTurn = coinFlip.transform.Find("t_firstTurn");
        tFirstTurn.gameObject.SetActive(false);

        Vector3 endRot = new Vector3(0, 0, 1800);
        endRot.z += IDtoSide(_mm.ActiveP.ID) == ScreenSide.Right ? 180 : 0;
        yield return spinningArrow.DORotate(endRot, 1.5f, RotateMode.LocalAxisAdd).WaitForCompletion();

        tFirstTurn.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);

        // TODO this will have to be relative too with the option for localOnRight
        if (IDtoSide(_mm.ActiveP.ID) == ScreenSide.Right) {
            SetDrawButton(_mm.myID, false);
            StartCoroutine(ShiftScreen());
        }

        coinFlip.SetActive(false);
    }
    #endregion


    #region ---------- EVENT CALLBACKS ----------
    public IEnumerator OnTurnBegin(int id) {
        if (_mm.MyTurn()) {
            ShowAlertText(_mm.ActiveP.Name + ", make your move!");
            //UpdateMoveText("Completed turns: " + (mm.stats.turns - 1));

            SetDrawButton(_mm.myID, true);
        }
        //UpdateAP(_mm.GetPlayer(id));

        //ChangePinfoColor(id, new Color(0, 1, 0, .4f));
        //PinfoColorTween(id, CorrectPinfoColor(id));
        yield return null;
    }

    public IEnumerator OnTurnEnd(int id) {
        newsfeed.UpdateTurnCount(Report.Turns);
        //ChangePinfoColor(id, new Color(1, 1, 1, .4f));
        yield return null;
    }

    //public void OnGameAction(int id, bool costsAP) {
    //    UpdateAP(_mm.GetPlayer(id));
    //}

    public IEnumerator OnHandChange(HandChangeEventArgs args) {
        if (args.state == EventController.HandChangeState.PlayerDraw ||
            args.state == EventController.HandChangeState.DrawFromEffect) {
            if (_mm.GetPlayer(args.id).Hand.IsFull)
                GetDrawButtonCont(args.id).Deactivate();
        }
        yield return null;
    }

    public void OnPlayerHealthChange(int id, int amount, int newHealth, bool dealt) {
        StartCoroutine(UpdateHealthbar(_mm.GetPlayer(id)));
        StartCoroutine(HealthChangeNumbers(id, amount));
        StartCoroutine(DamageAnim(_mm.GetPlayer(id), amount));
    }

    public void OnPlayerMeterChange(int id, int amount, int newMeter) {
        StartCoroutine(UpdateMeterbar(_mm.GetPlayer(id)));
    }
    #endregion


    ScreenSide IDtoSide(int id) {
        if (_mm.gameMode == MageMatch.GameMode.TrainingTwoChars) {
            return id == 1 ? ScreenSide.Left : ScreenSide.Right;
        }

        if (id == _mm.myID ^ _localOnRight)
            return ScreenSide.Left;
        else
            return ScreenSide.Right;
    }


    #region ---------- SLIDING ELEMENTS ----------

    public IEnumerator ShiftScreen() {
        Transform camPos = Camera.main.transform;
        Transform spellSliding = transform.Find("Spells");

        Debug.Log("alertBarOffset=" + GameObject.Find("static ui").GetComponent<RectTransform>().rect.yMax);
        //GameObject.Find("TEST").transform.position = new Vector3(0, _alertbarOffset);

        StartCoroutine(SlidePortrait(!_shiftedLeft, false));
        StartCoroutine(SlideDrawButton(_shiftedLeft, false));
        StartCoroutine(SlideDrawButton(!_shiftedLeft, true));

        if (_shiftedLeft) { // shift to right side
            spellSliding.DOMoveX(spellSliding.position.x - ( _spellSlidingOffset), SHIFT_DUR);
            yield return camPos.DOMoveX(camPos.position.x + (2*_camOffset), SHIFT_DUR).WaitForCompletion();
        } else { // shift to left side
            spellSliding.DOMoveX(spellSliding.position.x + ( _spellSlidingOffset), SHIFT_DUR);
            yield return camPos.DOMoveX(camPos.position.x - (2* _camOffset), SHIFT_DUR).WaitForCompletion();
        }

        yield return SlidePortrait(_shiftedLeft, true);

        _shiftedLeft = !_shiftedLeft;
    }

    IEnumerator SlidePortrait(bool left, bool active) {
        Transform portrait = left ? _leftPportrait : _rightPportrait;
        float amt = portrait.position.x;
        amt += left ^ active ? -1 * _portraitOffset : _portraitOffset;
        yield return portrait.DOMoveX(amt, SHIFT_DUR).WaitForCompletion();
    }

    IEnumerator SlideDrawButton(bool left, bool active) {
        Transform deck = left ? _leftDrawButton : _rightDrawButton;
        float amt = deck.position.y;
        amt += active ? -1 * _drawButtonOffset : _drawButtonOffset;
        yield return deck.DOMoveY(amt, SHIFT_DUR).WaitForCompletion();
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
            yield return t.DOMoveY(posY - _alertbarOffset, .25f).WaitForCompletion();
            yield return _mm.animCont.WaitForSeconds(ALERT_DELAY);
            yield return t.DOMoveY(posY, .25f).WaitForCompletion();
            _alertShowing = false;
        }
    }
    #endregion


    #region ---------- PLAYER INFO ----------
    public Transform GetPinfo(int id) {
        if (IDtoSide(id) == ScreenSide.Left)
            return _leftPinfo;
        else
            return _rightPinfo;
    }

    //void SetPinfoColor(int id) {
    //    Image healthImg = GetPinfo(id).Find("i_healthbar").GetComponent<Image>();
    //    switch (_mm.GetPlayer(id).character.ch) {
    //        case Character.Ch.Enfuego:
    //            healthImg.color = Color.red;
    //            break;
    //        case Character.Ch.Gravekeeper:
    //            healthImg.color = Color.green;
    //            break;
    //        case Character.Ch.Valeria:
    //            healthImg.color = Color.blue;
    //            break;

    //        case Character.Ch.Neutral:
    //            healthImg.color = Color.gray;
    //            break;
    //    }
    //}

    //Tween PinfoColorTween(int id, Color newColor) {
    //    Transform pinfo = GetPinfo(id);
    //    return pinfo.GetComponent<Image>().DOColor(newColor, 0.25f)
    //        .SetEase(Ease.InOutQuad).SetLoops(7, LoopType.Yoyo);
    //} 

    public void UpdateAP(Player p) {
        MMLog.Log_UICont("Updating AP for p" + p.ID);
        var APimage = GetPinfo(p.ID).Find("i_AP").GetComponent<Image>();
        APimage.fillAmount = (float) p.AP / Player.MAX_AP;
    }

    IEnumerator UpdateHealthbar(Player p) {
        Transform pinfo = GetPinfo(p.ID);
        Image healthImg = pinfo.Find("i_healthbar").GetComponent<Image>();
        Text healthText = pinfo.Find("t_health").GetComponent<Text>();

        int healthAmt = p.Character.GetHealth();
        TextNumTween(healthText, healthAmt);

        float slideRatio = (float)healthAmt / p.Character.GetMaxHealth();

        // TODO I have a feeling I can just Lerp this? lol
        // health bar coloring; green -> yellow -> red
        //float thresh = .6f; // point where health bar is yellow (0.6 = 60% health)
        //float r = (((Mathf.Clamp(slideRatio, thresh, 1) - thresh) / (1 - thresh)) * -1 + 1);
        //float g = Mathf.Clamp(slideRatio, 0, thresh) / thresh;
        //healthImg.color = new Color(r, g, 0);
        healthImg.color = healthbarGradient.Evaluate(slideRatio);

        yield return healthImg.DOFillAmount(slideRatio, .8f).SetEase(Ease.OutCubic).WaitForCompletion();
    }

    IEnumerator HealthChangeNumbers(int id, int amount) {
        Transform pHealthNums = GetPinfo(id).Find("HealthChangeNums");
        var tNumEntry = Instantiate(_healthChangeNumPF, pHealthNums).GetComponent<Text>();

        if (amount < 0) {
            tNumEntry.text = amount + "";
        } else {
            tNumEntry.color = Color.green;
            tNumEntry.text = "+" + amount;
        }

        yield return new WaitForSeconds(2.5f);

        yield return tNumEntry.DOFade(0, .5f).WaitForCompletion();

        Destroy(tNumEntry.gameObject);
    }

    IEnumerator UpdateMeterbar(Player p) {
        Transform pinfo = GetPinfo(p.ID);
        Image sig = pinfo.Find("i_meterbar").GetComponent<Image>();
        Text sigText = pinfo.Find("t_meter").GetComponent<Text>();

        int meter = p.Character.GetMeter();
        TextNumTween(sigText, meter / 10, "%"); // change if a character has meter of different amount 

        float slideRatio = (float) meter / Character.METER_MAX;
        //yield return meter.DOScaleX(slideRatio, .8f).SetEase(Ease.OutCubic);
        yield return sig.DOFillAmount(slideRatio, .8f).SetEase(Ease.OutCubic).WaitForCompletion();
    }

    IEnumerator DamageAnim(Player p, int amount) {
        if (amount < 0) { // damage
            Image pinfoImg = GetPinfo(p.ID).GetComponent<Image>();
            pinfoImg.color = new Color(1, 0, 0, 0.4f); // red
            Color pColor = CorrectPinfoColor(p.ID);
            yield return _mm.animCont.WaitForSeconds(.2f);
            pinfoImg.DOColor(pColor, .3f).SetEase(Ease.OutQuad);
        }
    }

    Color CorrectPinfoColor(int id) {
        //if (id == mm.ActiveP.id && mm.currentTurn != MageMatch.Turn.CommishTurn) {
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
        for (int i = 0; i < 5; i++) {
            GetButtonCont(id, i).ShowSpellInfo();
        }
	}

    Transform GetPortrait(int id) {
        return IDtoSide(id) == ScreenSide.Left ? _leftPportrait : _rightPportrait;
    }
    #endregion


    #region ---------- TARGETING ----------
    public void ShowSpellSeqs(List<TileSeq> seqs) {
        foreach (TileSeq seq in seqs) {
            MMLog.Log_UICont("showing " + seqs.Count + " seqs");
            GameObject start = Instantiate(_spellOutlineEndPF);
            int length = seq.GetSeqLength();
            for (int i = 1; i < length; i++) {
                GameObject piece;
                if (i == length - 1)
                    piece = Instantiate(_spellOutlineEndPF, start.transform);
                else
                    piece = Instantiate(_spellOutlineMidPF, start.transform);
                piece.transform.position = new Vector3(0, i);
                piece.transform.rotation = Quaternion.Euler(0, 0, -90);
            }

            Vector2 tilePos = HexGrid.GridCoordToPos(seq.sequence[0]);
            start.transform.position = new Vector3(tilePos.x, tilePos.y);
            start.transform.Rotate(0, 0, (int)HexGrid.GetDirection(seq) * -60);

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
            for(int j = HexGrid.BottomOfColumn(i); j <= HexGrid.TopOfColumn(i); j++) {
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
            for(int j = HexGrid.BottomOfColumn(i); j <= HexGrid.TopOfColumn(i); j++) {
                Color c = _cellOverlays[i,j].color;
                c.a = 0.6f;
                _cellOverlays[i,j].color = c;
                //Debug.Log("Color = " + cellOverlays[i,j].color);
            }
        }

        TileSeq selection = Targeting.GetSelection();
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
            for (int j = HexGrid.BottomOfColumn(i); j <= HexGrid.TopOfColumn(i); j++) {
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
            go.transform.position = HexGrid.GridCoordToPos(t.col, t.row);
            _outlines.Add(go);
        }
    }

    public void OutlineTarget(int col, int row) {
        GameObject go = Instantiate(_targetPF);
        go.transform.position = HexGrid.GridCoordToPos(col, row);
        _outlines.Add(go);
    }

    //public void ClearTargets() {
    //    int prereqs = Targeting.GetSelection().sequence.Count;
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

        int id = _mm.ActiveP.ID;
        TurnAllSpellButtons(id, !on);
        if (on) {
            GetDrawButtonCont(id).Deactivate();

            Vector3 newPos = hex.transform.position;
            newPos.y += 1;
            quickdrawButton.transform.position = newPos;
        } else {
            GetDrawButtonCont(id).Activate();
        }
    }

    public void KeepQuickdraw() {
        Prompt.SetQuickdrawHand();
    }


    #region ---------- BUTTONS ----------
    public void SetDrawButton(int id, bool interactable) {
        if (_mm.MyTurn()) {
            if (interactable)
                GetDrawButtonCont(id).Activate();
            else
                GetDrawButtonCont(id).Deactivate();
        }
    }

    public void UpdateDeckCount(int id, int count) {
        Text deckCount = GetDrawButton(id).Find("t_deckCount").GetComponent<Text>();
        deckCount.text = count + "";
    }

    public void UpdateRemovedCount(int id, int count) {
        Text removedCount = GetDrawButton(id).Find("t_removedCount").GetComponent<Text>();
        removedCount.text = count + "";
    }

    Transform GetPspells(int id) {
        if (_mm.gameMode == MageMatch.GameMode.TrainingTwoChars) {
            return id == 1 ? _leftPspells : _rightPspells;
        }

        if (IDtoSide(id) == ScreenSide.Left)
            return _leftPspells;
        else
            return _rightPspells;
    }

    Transform GetDrawButton(int id) {
        return IDtoSide(id) == ScreenSide.Left ? _leftDrawButton : _rightDrawButton;
    }

    ButtonController GetDrawButtonCont(int id) {
        return GetDrawButton(id).GetComponent<ButtonController>();
    }

    public ButtonController GetButtonCont(int id, int index) {
        //Transform t = GetPspells(id).Find("b_Spell" + index);
        //MMLog.Log_UICont("Found " + t.name + ", parent=" + t.parent.name);
        //Debug.LogWarning("Getting player " + id + " button " + index);
        return GetPspells(id).Find("b_Spell" + index)
            .GetComponent<ButtonController>();
    }
    
	public void ActivateSpellButton(int id, int index){
        if (_mm.IsMe(id)) {
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


    #region ---------- BEGINNING AND END SCREENS ----------

    public void ToggleLoadingText(bool on) {
        loadingText.SetActive(on);
    }

    public void TriggerEndOfMatchScreens(int losingPlayerId) {
        StartCoroutine(EndOfGame(losingPlayerId));
    }
    IEnumerator EndOfGame(int losingPlayerId) {
        knockoutScreen.SetActive(true);
        // TODO animate KO text 1 and 2

        yield return new WaitForSeconds(2f);
        //knockoutScreen.SetActive(false);

        // show result screen
        yield return resultsScreen.Display(losingPlayerId);
    }

    Transform GetGameStartSide(int id) {
        if (IDtoSide(id) == ScreenSide.Left)
            return gameStartScreen.transform
            .Find("LeftSide").GetComponent<RectTransform>();
        else
            return gameStartScreen.transform
            .Find("RightSide").GetComponent<RectTransform>();
    }
    #endregion

    public void Debug_OnError(string logString, string stackTrace, LogType type) {
        if (type == LogType.Error)
            errorButton.SetActive(true);
    }

    public void Debug_OnErrorClick() {
        GameObject reporterGO = GameObject.Find("Reporter");
        if (reporterGO != null) {
            Reporter reporter = reporterGO.GetComponent<Reporter>();
            reporter.doShow();
        }
    }

}
