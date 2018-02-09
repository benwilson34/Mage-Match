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
    public TooltipManager tooltipMan;
    public Newsfeed newsfeed;

	private Text debugGridText, slidingText;
    private GameObject debugItemPF;
    private Transform debugContent;
    private GameObject debugReport;
    private Text debugReportText;

    private ButtonController localDrawButton;
    private MageMatch mm;
	private Dropdown DD1, DD2;
    private Transform leftPinfo, rightPinfo, leftPspells, rightPspells, board;
    private GameObject spellOutlineEnd, spellOutlineMid;
    private GameObject prereqPF, targetPF;
    private GameObject gradient, targetingBG;
    private GameObject tCancelB, tClearB;
    private GameObject menus; // ?
    private GameObject newsfeedMenu;
    private GameObject overlay;
    private GameObject screenScroll;
    private List<GameObject> outlines, spellOutlines;
    private SpriteRenderer[,] cellOverlays;
    private Vector3 slidingTextStart;
    private bool localOnRight = false;

    public void Init(){
        board = GameObject.Find("cells").transform;
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        screenScroll = GameObject.Find("scrolling");

        //slidingText = GameObject.Find("Text_Sliding").GetComponent<Text>();
        //slidingTextStart = new Vector3(Screen.width, slidingText.rectTransform.position.y);
        //slidingText.rectTransform.position = slidingTextStart;

        leftPinfo = GameObject.Find("LeftPlayer_Info").transform;
        rightPinfo = GameObject.Find("RightPlayer_Info").transform;
        leftPspells = GameObject.Find("LeftPlayer_Spells").transform;
        rightPspells = GameObject.Find("RightPlayer_Spells").transform;



        // buttons
        InitSprites();
        for (int id = 1; id <= 2; id++) {
            for (int i = 0; i < 5; i++) {
                MMLog.Log_UICont("Init button " + i);
                var button = GetButtonCont(id, i);
                MMLog.Log_UICont("mm = " + mm.myID);
                button.Init(mm, id);
                button.Deactivate();
                if (id == mm.myID) { // only make my buttons interactable for the match
                    button.SetInteractable();
                }
            }
        }
        localDrawButton = GameObject.Find("b_draw").GetComponent<ButtonController>();
        localDrawButton.Init(mm, mm.myID);
        GameObject.Find("b_saveFiles").GetComponent<ButtonController>().Init(mm, mm.myID); // id not needed here


        // menus
        menus = GameObject.Find("Menus");
        Transform scroll = menus.transform.Find("DebugMenu").Find("scr_debugEffects");
        debugContent = scroll.transform.Find("Viewport").Find("Content");
		debugGridText = GameObject.Find ("t_debugHex").GetComponent<Text>(); // UI debug grid
        debugReport = menus.transform.Find("scr_report").gameObject;
        debugReportText = debugReport.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();
        debugReport.SetActive(false);

        GameObject toolsMenu = menus.transform.Find("ToolsMenu").gameObject;
        if (!mm.IsDebugMode())
            toolsMenu.SetActive(false);

        menus.SetActive(false);


        // newsfeed
        newsfeed = GameObject.Find("Newsfeed").GetComponent<Newsfeed>();
        newsfeed.Init(mm);


        // other
        //overlay = GameObject.Find("Targeting Overlay");
        //overlay.SetActive(false);
        
        spellOutlines = new List<GameObject>();
        outlines = new List<GameObject>();

        LoadPrefabs();

        tooltipMan = GetComponent<TooltipManager>();
    }

    public void InitEvents() {
        mm.eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.LastStep);
        mm.eventCont.AddTurnEndEvent(OnTurnEnd, EventController.Type.LastStep);
        mm.eventCont.gameAction += OnGameAction;
        mm.eventCont.AddDrawEvent(OnDraw, EventController.Type.LastStep, EventController.Status.Begin);
        mm.eventCont.AddMatchEvent(OnMatch, EventController.Type.LastStep);
        mm.eventCont.playerHealthChange += OnPlayerHealthChange;
        mm.eventCont.playerMeterChange += OnPlayerMeterChange;
    }

    void LoadPrefabs() {
        spellOutlineEnd = Resources.Load<GameObject>("prefabs/ui/spell-outline_end");
        spellOutlineMid = Resources.Load<GameObject>("prefabs/ui/spell-outline_mid");
        prereqPF = Resources.Load("prefabs/outline_prereq") as GameObject;
        targetPF = Resources.Load("prefabs/outline_target") as GameObject;

        debugItemPF = Resources.Load("prefabs/ui/debug_statusItem") as GameObject;
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
        if (mm.MyTurn())
            SendSlidingText(mm.ActiveP().name + ", make your move!");
        //UpdateMoveText("Completed turns: " + (mm.stats.turns - 1));

        SetDrawButton(true);
        FlipGradient(); // ugly
        UpdateAP(mm.GetPlayer(id));

        //ChangePinfoColor(id, new Color(0, 1, 0, .4f));
        //PinfoColorTween(id, CorrectPinfoColor(id));
        UpdateEffTexts();
        yield return null;
    }

    public IEnumerator OnTurnEnd(int id) {
        UpdateEffTexts();
        newsfeed.UpdateTurnCount(mm.stats.turns);
        //ChangePinfoColor(id, new Color(1, 1, 1, .4f));
        yield return null;
    }

    public void OnGameAction(int id, bool costsAP) {
        UpdateAP(mm.GetPlayer(id));
        UpdateEffTexts(); // could be considerable overhead...
    }

    public IEnumerator OnDraw(int id, string tag, bool playerAction, bool dealt) {
        // TODO update button
        yield return null;
    }

    public IEnumerator OnMatch(int id, string[] seqs) {
        if (seqs.Length > 1)
            SendSlidingText("Wow, nice combo!");
        yield return null;
    }

    public void OnCascade(int id, int chain) {
        UpdateMoveText("Wow, a cascade of " + chain + " matches!");
        SendSlidingText("Wow, a cascade of " + chain + " matches!");
    }

    public void OnPlayerHealthChange(int id, int amount, int newHealth, bool dealt) {
        StartCoroutine(UpdateHealthbar(mm.GetPlayer(id)));
        StartCoroutine(DamageAnim(mm.GetPlayer(id), amount));
    }

    public void OnPlayerMeterChange(int id, int amount, int newMeter) {
        StartCoroutine(UpdateMeterbar(mm.GetPlayer(id)));
    }
    #endregion

    public void Reset() {
        UpdateDebugGrid();
        UpdateMoveText("Fight!!");

        if (!mm.MyTurn())
            FlipGradient();

        for (int id = 1; id <= 2; id++) {
            Player p = mm.GetPlayer(id);
            Transform pinfo = GetPinfo(id);

            Text nameText = pinfo.Find("t_name").GetComponent<Text>();
            nameText.text = "P"+id+": " + p.name;
            if (id == mm.myID)
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
        if (!mm.MyTurn() ^ localOnRight) // because it's currently relative, only do this if needed
            StartCoroutine(ShiftScreen());
    }

    float camOffset;

    public IEnumerator ShiftScreen() {
        //RectTransform rect = screenScroll.GetComponent<RectTransform>();

        Transform camPos = Camera.main.transform;
        
        if (mm.MyTurn() ^ localOnRight) { // left side
            yield return camPos.DOMoveX(camPos.position.x - (2*camOffset), .3f).WaitForCompletion();
            //yield return rect.DOMoveX(68, .3f).WaitForCompletion();
            //float newPos = Camera.main.ScreenToWorldPoint(new Vector3(68, 0)).x;
            //yield return rect.DOMoveX(newPos + 7, .3f).WaitForCompletion();
        } else { // right side
            yield return camPos.DOMoveX(camPos.position.x + (2*camOffset), .3f).WaitForCompletion();
            //yield return rect.DOMoveX(-81, .3f).WaitForCompletion();
            //float newPos = Camera.main.ScreenToWorldPoint(new Vector3(-78, 0)).x;
            //yield return rect.DOMoveX(newPos + 9.1f, .3f).WaitForCompletion();
        }
        //MMLog.Log_UICont("rect x screen pos=" + rect.position.x);
        //MMLog.Log_UICont("rect x world pos=" + Camera.main.ScreenToWorldPoint(rect.position).x);
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
    void FlipGradient() {
        //		Vector3 scale = go.transform.localScale;
        //		go.transform.localScale.Set (scale.x * -1, scale.y, scale.z);
        //gradient.transform.Rotate(0, 0, 180);
    }

    // delete
    public void UpdateMoveText(string str){
        //moveText.text = str;
    }


    #region ----- PLAYER INFO -----
    public Transform GetPinfo(int id) {
        if (id == mm.myID ^ localOnRight)
            return leftPinfo;
        else
            return rightPinfo;
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
    }

    IEnumerator UpdateHealthbar(Player p) {
        Transform pinfo = GetPinfo(p.id);
        Image health = pinfo.Find("i_healthbar").GetComponent<Image>();
        Text healthText = pinfo.Find("t_health").GetComponent<Text>();

        TextNumTween(healthText, p.health);

        float slideRatio = (float)p.health / p.character.GetMaxHealth();

        // TODO I have a feeling I can just Lerp this? lol
        // health bar coloring; green -> yellow -> red
        //float thresh = .6f; // point where health bar is yellow (0.6 = 60% health)
        //float r = (((Mathf.Clamp(slideRatio, thresh, 1) - thresh) / (1 - thresh)) * -1 + 1);
        //float g = Mathf.Clamp(slideRatio, 0, thresh) / thresh;
        //healthbar.GetComponent<Image>().color = new Color(r, g, 0);

        //yield return healthbar.DOScaleX(slideRatio, .8f).SetEase(Ease.OutCubic);
        yield return health.DOFillAmount(slideRatio, .8f).SetEase(Ease.OutCubic).WaitForCompletion();
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
            GameObject start = Instantiate(spellOutlineEnd);
            int length = seq.GetSeqLength();
            for (int i = 1; i < length; i++) {
                GameObject piece;
                if (i == length - 1)
                    piece = Instantiate(spellOutlineEnd, start.transform);
                else
                    piece = Instantiate(spellOutlineMid, start.transform);
                piece.transform.position = new Vector3(0, i);
                piece.transform.rotation = Quaternion.Euler(0, 0, -90);
            }

            Vector2 tilePos = mm.hexGrid.GridCoordToPos(seq.sequence[0]);
            start.transform.position = new Vector3(tilePos.x, tilePos.y);
            start.transform.Rotate(0, 0, mm.hexGrid.GetDirection(seq) * -60);

            spellOutlines.Add(start);
        }
    }

    public void HideSpellSeqs() {
        for (int i = 0; i < spellOutlines.Count;) {
            Destroy(spellOutlines[0]);
            spellOutlines.RemoveAt(0);
        }
    }

    public void GetCellOverlays() {
        cellOverlays = new SpriteRenderer[7,7];
        for(int i = 0; i < 7; i++) {
            Transform col = board.GetChild(i);
            for(int j = mm.hexGrid.BottomOfColumn(i); j <= mm.hexGrid.TopOfColumn(i); j++) {
                //Debug.Log(">>>2nd for of getCellOverlays at " + (i) + (j));
                cellOverlays[i,j] = col.Find("cell" + (i) + (j)).GetComponent<SpriteRenderer>();
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
            for(int j = mm.hexGrid.BottomOfColumn(i); j <= mm.hexGrid.TopOfColumn(i); j++) {
                Color c = cellOverlays[i,j].color;
                c.a = 0.6f;
                cellOverlays[i,j].color = c;
                //Debug.Log("Color = " + cellOverlays[i,j].color);
            }
        }

        OutlinePrereq(mm.targeting.GetSelection());

        //gradient.SetActive(!gradient.activeSelf);
        //targetingBG.SetActive(!targetingBG.activeSelf);
    }

    public void ActivateTargetingUI(List<TileBehav> tbs) {
        ActivateTargetingUI();

        foreach(TileBehav tb in tbs) {
            Tile t = tb.tile;
            Color c = cellOverlays[t.col,t.row].color;
            c.a = 0.0f;
            cellOverlays[t.col,t.row].color = c;
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
            for (int j = mm.hexGrid.BottomOfColumn(i); j <= mm.hexGrid.TopOfColumn(i); j++) {
                Color c = cellOverlays[i,j].color;
                c.a = 0.0f;
                cellOverlays[i,j].color = c;
            }
        }

        ClearOutlines();
    }

    void OutlinePrereq(TileSeq seq) {
        outlines = new List<GameObject>(); // move to Init?
        GameObject go;
        foreach (Tile t in seq.sequence) {
            go = Instantiate(prereqPF);
            go.transform.position = mm.hexGrid.GridCoordToPos(t.col, t.row);
            outlines.Add(go);
        }
    }

    public void OutlineTarget(int col, int row) {
        GameObject go = Instantiate(targetPF);
        go.transform.position = mm.hexGrid.GridCoordToPos(col, row);
        outlines.Add(go);
    }

    public void ClearTargets() {
        int prereqs = mm.targeting.GetSelection().sequence.Count;
        for (int i = 0; i < outlines.Count - prereqs;) { // clear just the target outlines
            GameObject go = outlines[prereqs];
            GameObject.Destroy(go);
            outlines.Remove(go);
        }
    }

    void ClearOutlines() {
        for (int i = 0; i < outlines.Count;) {
            GameObject go = outlines[0];
            GameObject.Destroy(go);
            outlines.Remove(go);
        }
    }

    #endregion


    #region ----- BUTTONS -----
    public void SetDrawButton(bool interactable) {
        if (mm.MyTurn()) {
            if (interactable)
                localDrawButton.Activate();
            else
                localDrawButton.Deactivate();
        }
    }

    public Transform GetPspells(int id) {
        if (id == mm.myID ^ localOnRight)
            return leftPspells;
        else
            return rightPspells;
    }

    public ButtonController GetButtonCont(int id, int index) {
        //Transform t = GetPspells(id).Find("b_Spell" + index);
        //MMLog.Log_UICont("Found " + t.name + ", parent=" + t.parent.name);
        return GetPspells(id).Find("b_Spell" + index)
            .GetComponent<ButtonController>();
    }
    
	public void ActivateSpellButton(int id, int index){
        if (mm.MyTurn()) {
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
    #endregion


    #region ----- DEBUG MENU -----
    public void ToggleDebugMenu() {
        bool menuOpen = !menus.GetActive();
        Text menuButtonText = GameObject.Find("MenuButtonText").GetComponent<Text>();
        if (menuOpen) {
            menuButtonText.text = "Close Menu";
            mm.EnterState(MageMatch.State.DebugMenu);
        } else {
            menuButtonText.text = "Menu";
            mm.ExitState();
        }
        menus.SetActive(menuOpen);
    }

    public bool IsDebugMenuOpen() { return menus.GetActive(); }

    public void UpdateDebugGrid() {
        string grid = "   0  1  2  3  4  5  6 \n";
        for (int r = HexGrid.NUM_ROWS - 1; r >= 0; r--) {
            grid += r + " ";
            for (int c = 0; c < HexGrid.NUM_COLS; c++) {
                if (r <= mm.hexGrid.TopOfColumn(c) && r >= mm.hexGrid.BottomOfColumn(c)) {
                    if (mm.hexGrid.IsCellFilled(c, r)) {
                        TileBehav tb = mm.hexGrid.GetTileBehavAt(c, r);
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
        debugGridText.text = grid;
    }

    public void UpdateEffTexts() {
        object[] lists = mm.effectCont.GetLists();
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

        foreach (Transform child in debugContent) {
            Destroy(child.gameObject);
        }
        foreach (GameObject go in debugItems) {
            go.transform.SetParent(debugContent);
        }
    }

    public GameObject InstantiateDebugEntry(string t, Color c) {
        GameObject item = Instantiate(debugItemPF);
        item.GetComponent<Image>().color = c;
        item.transform.Find("Text").GetComponent<Text>().text = t;
        return item;
    }

    public void ToggleReport(bool b) {
        if (b)
            debugReportText.text = mm.stats.GetReportText();

        debugReport.SetActive(b);
    }
    #endregion
}
