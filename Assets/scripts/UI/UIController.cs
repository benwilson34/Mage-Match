using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using MMDebug;

public class UIController : MonoBehaviour {

    public AnimationCurve slidingEase;
    [HideInInspector]
    public Sprite miniFire, miniWater, miniEarth, miniAir, miniMuscle;
    [HideInInspector]
    public TooltipManager tooltipMan;
    [HideInInspector]
    public Newsfeed newsfeed;

	private Text _debugGridText, _slidingText;
    private GameObject _debugItemPF;
    private Transform _debugContent;
    private GameObject _debugReport;
    private Text _debugReportText;

    private ButtonController _drawButton;
    private Text _tDeckCount, _tRemovedCount;
    private MageMatch _mm;
    private Transform _leftPinfo, _rightPinfo, _leftPspells, _rightPspells, _board;
    private GameObject _spellOutlineEnd, _spellOutlineMid;
    private GameObject _prereqPF, _targetPF;
    private GameObject _gradient, _targetingBG;
    private GameObject _tCancelB, _tClearB;
    private GameObject _menus; // ?
    private GameObject _newsfeedMenu;
    private GameObject _overlay;
    private GameObject _screenScroll;
    private List<GameObject> _outlines, _spellOutlines;
    private SpriteRenderer[,] _cellOverlays;
    private Vector3 _slidingTextStart;
    private bool _localOnRight = false;

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
        GameObject.Find("b_saveFiles").GetComponent<ButtonController>().Init(_mm, _mm.myID); // id not needed here

        // menus
        _menus = GameObject.Find("Menus");
        Transform scroll = _menus.transform.Find("DebugMenu").Find("scr_debugEffects");
        _debugContent = scroll.transform.Find("Viewport").Find("Content");
		_debugGridText = GameObject.Find ("t_debugHex").GetComponent<Text>(); // UI debug grid
        _debugReport = _menus.transform.Find("scr_report").gameObject;
        _debugReportText = _debugReport.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();
        _debugReport.SetActive(false);

        GameObject toolsMenu = _menus.transform.Find("ToolsMenu").gameObject;
        if (!_mm.IsDebugMode())
            toolsMenu.SetActive(false);

        _menus.SetActive(false);


        // newsfeed
        newsfeed = GameObject.Find("Newsfeed").GetComponent<Newsfeed>();
        newsfeed.Init(_mm);


        // other
        //overlay = GameObject.Find("Targeting Overlay");
        //overlay.SetActive(false);
        
        _spellOutlines = new List<GameObject>();
        _outlines = new List<GameObject>();

        LoadPrefabs();

        tooltipMan = GetComponent<TooltipManager>();
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

        _debugItemPF = Resources.Load("prefabs/ui/debug_statusItem") as GameObject;
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
            SendSlidingText(_mm.ActiveP().name + ", make your move!");
            //UpdateMoveText("Completed turns: " + (mm.stats.turns - 1));

            SetDrawButton(true);
        }
        UpdateAP(_mm.GetPlayer(id));

        //ChangePinfoColor(id, new Color(0, 1, 0, .4f));
        //PinfoColorTween(id, CorrectPinfoColor(id));
        UpdateEffTexts();
        yield return null;
    }

    public IEnumerator OnTurnEnd(int id) {
        UpdateEffTexts();
        newsfeed.UpdateTurnCount(_mm.stats.turns);
        //ChangePinfoColor(id, new Color(1, 1, 1, .4f));
        yield return null;
    }

    public void OnGameAction(int id, bool costsAP) {
        UpdateAP(_mm.GetPlayer(id));
        UpdateEffTexts(); // could be considerable overhead...
    }

    public IEnumerator OnDraw(int id, string tag, bool playerAction, bool dealt) {
        // TODO update button
        yield return null;
    }

    //public IEnumerator OnMatch(int id, string[] seqs) {
    //    if (seqs.Length > 1)
    //        SendSlidingText("Wow, nice combo!");
    //    yield return null;
    //}

    public void OnCascade(int id, int chain) {
        UpdateMoveText("Wow, a cascade of " + chain + " matches!");
        SendSlidingText("Wow, a cascade of " + chain + " matches!");
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
        UpdateDebugGrid();
        UpdateMoveText("Fight!!");

        if (!_mm.MyTurn())
            SetDrawButton(false);

        for (int id = 1; id <= 2; id++) {
            Player p = _mm.GetPlayer(id);
            Transform pinfo = GetPinfo(id);

            Text nameText = pinfo.Find("t_name").GetComponent<Text>();
            nameText.text = "P"+id+": " + p.name;
            if (id == _mm.myID)
                nameText.text += " (ME!)";

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

    // delete
    public void SendSlidingText(string str) {
        //slidingText.text = str;
        //StartCoroutine(_SlidingText());
    }

    // TODO prevent retriggering
    //IEnumerator _SlidingText() {
    //    RectTransform boxRect = slidingText.rectTransform;
    //    Vector3 end = new Vector3(-boxRect.rect.width, slidingTextStart.y);
    //    //Debug.Log("UICONT: _SlidingText: start=" + slidingTextStart.ToString() + ", end=" + end.ToString());
    //    Tween t = slidingText.rectTransform.DOMoveX(end.x, 3f).SetEase(slidingEase);
    //    yield return t.WaitForCompletion();
    //    slidingText.rectTransform.position = slidingTextStart;
    //}

    // delete
    public void UpdateMoveText(string str){
        //moveText.text = str;
    }


    #region ----- PLAYER INFO -----
    public Transform GetPinfo(int id) {
        if (id == _mm.myID ^ _localOnRight)
            return _leftPinfo;
        else
            return _rightPinfo;
    }

    //void ChangePinfoColor(int id, Color c) {
    //    GetPinfo(id).GetComponent<Image>().color = c; // idk why
    //}

    //Tween PinfoColorTween(int id, Color newColor) {
    //    Transform pinfo = GetPinfo(id);
    //    return pinfo.GetComponent<Image>().DOColor(newColor, 0.25f)
    //        .SetEase(Ease.InOutQuad).SetLoops(7, LoopType.Yoyo);
    //} 

    // TODO re-enable once this is designed in again
    void UpdateAP(Player p) {
        //      Text APText = GetPinfo(p.id).Find ("Text_AP").GetComponent<Text>();
        //APText.text = "AP left: " + p.AP;

        //Transform APblock = GetPinfo(p.id).Find("i_AP");

        //for (int i = 0; i < 6; i++) {
        //    if (i < p.AP)
        //        APblock.Find("AP" + i).GetComponent<Image>().enabled = true;
        //    else
        //        APblock.Find("AP" + i).GetComponent<Image>().enabled = false;
        //}      

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

        OutlinePrereq(_mm.targeting.GetSelection());

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

    public void ClearTargets() {
        int prereqs = _mm.targeting.GetSelection().sequence.Count;
        for (int i = 0; i < _outlines.Count - prereqs;) { // clear just the target outlines
            GameObject go = _outlines[prereqs];
            GameObject.Destroy(go);
            _outlines.Remove(go);
        }
    }

    void ClearOutlines() {
        for (int i = 0; i < _outlines.Count;) {
            GameObject go = _outlines[0];
            GameObject.Destroy(go);
            _outlines.Remove(go);
        }
    }

    #endregion


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


    #region ----- DEBUG MENU -----
    public void ToggleDebugMenu() {
        bool menuOpen = !_menus.GetActive();
        Text menuButtonText = GameObject.Find("MenuButtonText").GetComponent<Text>();
        if (menuOpen) {
            menuButtonText.text = "Close Menu";
            _mm.EnterState(MageMatch.State.DebugMenu);
        } else {
            menuButtonText.text = "Menu";
            _mm.ExitState();
        }
        _menus.SetActive(menuOpen);
    }

    public bool IsDebugMenuOpen() { return _menus.GetActive(); }

    public void UpdateDebugGrid() {
        string grid = "   0  1  2  3  4  5  6 \n";
        for (int r = HexGrid.NUM_ROWS - 1; r >= 0; r--) {
            grid += r + " ";
            for (int c = 0; c < HexGrid.NUM_COLS; c++) {
                if (r <= _mm.hexGrid.TopOfColumn(c) && r >= _mm.hexGrid.BottomOfColumn(c)) {
                    if (_mm.hexGrid.IsCellFilled(c, r)) {
                        TileBehav tb = _mm.hexGrid.GetTileBehavAt(c, r);
                        if (tb.wasInvoked)
                            grid += "[*]";
                        else
                            grid += "[" + tb.tile.ThisElementToChar() + "]";

                    } else
                        grid += "[ ]";
                } else
                    grid += " - ";
            }
            grid += '\n';
        }
        _debugGridText.text = grid;
    }

    public void UpdateEffTexts() {
        object[] lists = _mm.effectCont.GetLists();
        List<GameObject> debugItems = new List<GameObject>();

        Color lightBlue = new Color(.07f, .89f, .93f, .8f);

        List<Effect> beginTurnEff = (List<Effect>)lists[0];
        debugItems.Add(InstantiateDebugEntry("BeginTurnEffs:", lightBlue));
        foreach (Effect e in beginTurnEff) {
            debugItems.Add(InstantiateDebugEntry(e.tag, Color.white));
        }

        List<Effect> endTurnEff = (List<Effect>)lists[1];
        debugItems.Add(InstantiateDebugEntry("EndTurnEffs:", lightBlue));
        foreach (Effect e in endTurnEff) {
            debugItems.Add(InstantiateDebugEntry(e.tag, Color.white));
        }

        List<DropEffect> dropEff = (List<DropEffect>)lists[2];
        debugItems.Add(InstantiateDebugEntry("DropEffs:", lightBlue));
        foreach (DropEffect e in dropEff) {
            debugItems.Add(InstantiateDebugEntry(e.tag, Color.white));
        }

        List<SwapEffect> swapEff = (List<SwapEffect>)lists[3];
        debugItems.Add(InstantiateDebugEntry("SwapEffs:", lightBlue));
        foreach (SwapEffect e in swapEff) {
            debugItems.Add(InstantiateDebugEntry(e.tag, Color.white));
        }

        foreach (Transform child in _debugContent) {
            Destroy(child.gameObject);
        }
        foreach (GameObject go in debugItems) {
            go.transform.SetParent(_debugContent);
        }
    }

    public GameObject InstantiateDebugEntry(string t, Color c) {
        GameObject item = Instantiate(_debugItemPF);
        item.GetComponent<Image>().color = c;
        item.transform.Find("Text").GetComponent<Text>().text = t;
        return item;
    }

    public void ToggleReport(bool b) {
        if (b)
            _debugReportText.text = _mm.stats.GetReportText();

        _debugReport.SetActive(b);
    }
    #endregion
}
