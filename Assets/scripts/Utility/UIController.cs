using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class UIController : MonoBehaviour {

    public Sprite miniFire, miniWater, miniEarth, miniAir, miniMuscle;
    public AnimationCurve slidingEase;

	private Text moveText, debugGridText, turnTimerText, slidingText;
	private Text beginTurnEffText, endTurnEffText, matchEffText, swapEffText;
    private MageMatch mm;
	private Dropdown DD1, DD2;
    private Transform p1info, p2info, p1load, p2load;
    private GameObject gradient, targetingBG;
    private GameObject tCancelB, tClearB;
    private GameObject settingsMenu; // ?
    private SpellEffects spellfx;
    private Vector3 slidingTextStart;

    public void Init(){ // Awake()?
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();

		moveText = GameObject.Find ("Text_Move").GetComponent<Text> (); // UI move announcement
		moveText.text = "";
		debugGridText = GameObject.Find ("Text_Debug1").GetComponent<Text> (); // UI debug grid
        slidingText = GameObject.Find("Text_Sliding").GetComponent<Text>();

        slidingTextStart = new Vector3(Screen.width, slidingText.rectTransform.position.y);
        slidingText.rectTransform.position = slidingTextStart;

        beginTurnEffText = GameObject.Find("Text_BeginTurnEff").GetComponent<Text>();
        endTurnEffText = GameObject.Find("Text_EndTurnEff").GetComponent<Text>();
        matchEffText = GameObject.Find("Text_MatchEff").GetComponent<Text>();
        swapEffText = GameObject.Find("Text_SwapEff").GetComponent<Text>();

        p1info = GameObject.Find("Player1_Info").transform;
        p2info = GameObject.Find("Player2_Info").transform;
        p1load = GameObject.Find("Player1_Loadout").transform;
        p2load = GameObject.Find("Player2_Loadout").transform;

        DD1 = GameObject.Find ("Dropdown_p1").GetComponent<Dropdown> ();
		DD2 = GameObject.Find ("Dropdown_p2").GetComponent<Dropdown> ();

        gradient = GameObject.Find("green-gradient");
        targetingBG = GameObject.Find("targetingBG");
        targetingBG.SetActive(false);
        tCancelB = GameObject.Find("Button_CancelSpell");
        tCancelB.SetActive(false);
        tClearB = GameObject.Find("Button_ClearTargets");
        tClearB.SetActive(false);

        settingsMenu = GameObject.Find("SettingsMenu");

        turnTimerText = GameObject.Find("Text_Timer").GetComponent<Text>();

        spellfx = new SpellEffects();
    }

    public void InitEvents() {
        mm.eventCont.AddTurnBeginEvent(OnTurnBegin, 1);
        mm.eventCont.AddTurnEndEvent(OnTurnEnd, 1);
        mm.eventCont.gameAction += OnGameAction;
        mm.eventCont.match += OnMatch;
        mm.eventCont.cascade += OnCascade;
        mm.eventCont.playerHealthChange += OnPlayerHealthChange;
        mm.eventCont.playerMeterChange += OnPlayerMeterChange;
    }

    #region EventCont calls
    public IEnumerator OnTurnBegin(int id) {
        if (mm.MyTurn())
            SendSlidingText(mm.ActiveP().name + ", make your move!");
        UpdateMoveText("Completed turns: " + (mm.stats.turns - 1));
        SetDrawButton(mm.ActiveP(), true);
        FlipGradient(); // ugly
        UpdateAPText(mm.GetPlayer(id));

        ChangePinfoColor(id, new Color(0, 1, 0, .4f));
        UpdateEffTexts();
        yield return null;
    }

    public IEnumerator OnTurnEnd(int id) {
        UpdateEffTexts();
        ChangePinfoColor(id, new Color(1, 1, 1, .4f));
        yield return null;
    }

    public void OnGameAction(int id, bool costsAP) {
        UpdateAPText(mm.GetPlayer(id));
        UpdateEffTexts(); // could be considerable overhead...
    }

    public void OnMatch(int id, string[] seqs) {
        if (seqs.Length > 1)
            SendSlidingText("Wow, nice combo!");
    }

    public void OnCascade(int id, int chain) {
        UpdateMoveText("Wow, a cascade of " + chain + " matches!");
        SendSlidingText("Wow, a cascade of " + chain + " matches!");
    }

    public void OnPlayerHealthChange(int id, int amount, bool dealt, bool sent) {
        StartCoroutine(UpdateHealthbar(mm.GetPlayer(id)));
    }

    public void OnPlayerMeterChange(int id, int amount) {
        StartCoroutine(UpdateMeterbar(mm.GetPlayer(id)));
    }
    #endregion

    public void Reset() { // could just get players from MM
        UpdateDebugGrid();
        UpdateMoveText("Fight!!");

        for (int id = 1; id <= 2; id++) {
            Player p = mm.GetPlayer(id);
            Transform pinfo = GetPinfo(id);

            Text nameText = pinfo.Find("Text_Name").GetComponent<Text>();
            nameText.text = "P"+id+": " + p.name;
            if (id == mm.myID)
                nameText.text += " (ME!)";

            ShowLoadout(p);
            DeactivateAllSpellButtons(p);
            UpdateAPText(p);

            // TODO start health from zero so it animates
            Text healthT = pinfo.Find("Health_Outline").Find("Text_Health").GetComponent<Text>();
            healthT.text = p.health + "/" + p.character.GetMaxHealth();

            Text meterT = pinfo.Find("Meter_Outline").Find("Text_Meter").GetComponent<Text>();
            meterT.text = "0/" + p.character.meterMax;
            StartCoroutine(UpdateMeterbar(p));
        }

        ChangePinfoColor(1, new Color(0, 1, 0, .4f));

        settingsMenu.SetActive(mm.menu); //?
    }

    public void UpdateDebugGrid(){
		string grid = "   0  1  2  3  4  5  6 \n";
		for (int r = HexGrid.numRows - 1; r >= 0; r--) {
			grid += r + " ";
			for (int c = 0; c < HexGrid.numCols; c++) {
				if (r <= mm.hexGrid.TopOfColumn (c) && r >= mm.hexGrid.BottomOfColumn (c)) {
					if (mm.hexGrid.IsSlotFilled (c, r))
						grid += "[" + mm.hexGrid.GetTileAt (c, r).ThisElementToChar() + "]";
					else
						grid += "[ ]";
				} else
					grid += " - ";
			}
			grid += '\n';
		}
		debugGridText.text = grid;
	}

	public void UpdateMoveText(string str){
		moveText.text = str;
	}

    public void SendSlidingText(string str) {
        slidingText.text = str;
        StartCoroutine(_SlidingText());
    }

    // TODO prevent retriggering
    IEnumerator _SlidingText() {
        RectTransform boxRect = slidingText.rectTransform;
        Vector3 end = new Vector3(-boxRect.rect.width, slidingTextStart.y);
        //Debug.Log("UICONT: _SlidingText: start=" + slidingTextStart.ToString() + ", end=" + end.ToString());
        Tween t = slidingText.rectTransform.DOMoveX(end.x, 3f).SetEase(slidingEase);
        yield return t.WaitForCompletion();
        slidingText.rectTransform.position = slidingTextStart;
    }

    Transform GetPinfo(int id) {
        if (id == 1)
            return p1info;
        else
            return p2info;
    }

    public void ChangePinfoColor(int id, Color c) {
        GetPinfo(id).GetComponent<Image>().color = c; // idk why
    }

    public void UpdateAPText(Player p) {
        Text APText = GetPinfo(p.id).Find ("Text_AP").GetComponent<Text>();
		APText.text = "AP left: " + p.AP;
    }

    IEnumerator UpdateHealthbar(Player p) {
        Transform healthOutline = GetPinfo(p.id).Find("Health_Outline");
        RectTransform healthbar = healthOutline.Find("Healthbar").GetComponent<RectTransform>();
        Text healthText = healthOutline.Find("Text_Health").GetComponent<Text>();

        Tween tween = TextNumTween(healthText, p.health, p.character.GetMaxHealth());

        float slideRatio = (float)p.health / p.character.GetMaxHealth();

        // health bar coloring; green -> yellow -> red
        float thresh = .6f; // point where health bar is yellow (0.6 = 60% health)
        float r = (((Mathf.Clamp(slideRatio, thresh, 1) - thresh) / (1 - thresh)) * -1 + 1);
        float g = Mathf.Clamp(slideRatio, 0, thresh) / thresh;
        healthbar.GetComponent<Image>().color = new Color(r, g, 0);

        yield return healthbar.DOScaleX(slideRatio, .8f).SetEase(Ease.OutCubic);
    }

    IEnumerator UpdateMeterbar(Player p) {
        Transform meterOutline = GetPinfo(p.id).Find("Meter_Outline");
        RectTransform meter = meterOutline.Find("Meterbar").GetComponent<RectTransform>();
        Text meterText = meterOutline.Find("Text_Meter").GetComponent<Text>();

        Tween tween = TextNumTween(meterText, p.character.meter, p.character.meterMax);

        float slideRatio = (float) p.character.meter / p.character.meterMax;
        yield return meter.DOScaleX(slideRatio, .8f).SetEase(Ease.OutCubic);
    }

    Tween TextNumTween(Text t, int newValue, int maxValue) {
        int oldValue = int.Parse(t.text.Split('/')[0]);
        return DOTween.To(() => oldValue,
            x => { oldValue = x; t.text = oldValue + "/" + maxValue; },
            newValue, .8f)
            .SetEase(Ease.OutCubic);
    }

	public void ShowLoadout(Player player){
		Transform pload;
        if (player.id == 1)
            pload = p1load;
        else
            pload = p2load;
		Text loadoutText = pload.Find ("Text_LoadoutName").GetComponent<Text>();
		loadoutText.text = player.character.characterName + " - " + player.character.loadoutName;

		for (int i = 0; i < 4; i++){
			Transform t = pload.Find ("Button_Spell" + i);
			Spell currentSpell = player.character.GetSpell (i);

			Text spellName = t.Find ("Text_SpellName").GetComponent<Text> ();
			spellName.text = currentSpell.name + " " + currentSpell.APcost + " AP";

			for (int m = 0; m < 5; m++) {
				Image minitile = t.Find ("minitile" + m).GetComponent<Image> ();
				minitile.color = Color.white;
				Tile.Element currentEl = currentSpell.GetElementAt (m);
//				Debug.Log (currentEl);
				if (!currentEl.Equals(Tile.Element.None)) {
					switch (currentEl) {
					case Tile.Element.Fire:
						minitile.sprite = miniFire;
						break;
					case Tile.Element.Water:
						minitile.sprite = miniWater;
						break;
					case Tile.Element.Earth:
						minitile.sprite = miniEarth;
						break;
					case Tile.Element.Air:
						minitile.sprite = miniAir;
						break;
					case Tile.Element.Muscle:
						minitile.sprite = miniMuscle;
						break;
					}
				} else {
					// turn image transparent
					minitile.color = new Color(1,1,1,0);
				}
					
			}
		}
	}

    public void FlipGradient() {
        //		Vector3 scale = go.transform.localScale;
        //		go.transform.localScale.Set (scale.x * -1, scale.y, scale.z);
        gradient.transform.Rotate(0, 0, 180);
    }

    public void ToggleTargetingUI() {
        gradient.SetActive(!gradient.activeSelf);
        targetingBG.SetActive(!targetingBG.activeSelf);
        if (mm.MyTurn()) {
            tCancelB.SetActive(!tCancelB.activeSelf);
            tClearB.SetActive(!tClearB.activeSelf);
        }
    }

    public void SetDrawButton(Player p, bool interactable) {
        if (interactable && !p.ThisIsLocal()) // if not the localp, no draw button
            return;
        if (p.id == 1)
            p1info.Find("Button_Draw").GetComponent<Button>().interactable = interactable;
        else
            p2info.Find("Button_Draw").GetComponent<Button>().interactable = interactable;
    }

    Button GetButton(Player player, int index){
		Transform pload;
        if (player.id == 1)
            pload = p1load;
        else
            pload = p2load;
		return pload.Find ("Button_Spell" + index).GetComponent<Button>();
	}

	public void ActivateSpellButton(Player player, int index){
        if (player.ThisIsLocal()) {
            Button button = GetButton(player, index);
            button.interactable = true;
        }
	}

	public void DeactivateSpellButton(Player player, int index){
		Button button = GetButton (player, index);
		button.interactable = false;
	}

	public void DeactivateAllSpellButtons(Player player){
		for (int i = 0; i < 4; i++) {
			Button button = GetButton (player, i);
			button.interactable = false;
		}
	}

	public int GetLoadoutNum(int id){
		if (id == 1)
            return DD1.value; 
        else
			return DD2.value;
	}

    public void ToggleMenu() {
        mm.menu = !mm.menu;
        Text menuButtonText = GameObject.Find("MenuButtonText").GetComponent<Text>();
        if (mm.menu) {
            menuButtonText.text = "Close Menu";
        } else {
            menuButtonText.text = "Menu";
        }
        settingsMenu.SetActive(mm.menu);
    }

    // TODO methods for two edit dropdowns
    //public Tile.Element GetClickElement() {
    //    Dropdown dd = GameObject.Find("Dropdown_DropColor").GetComponent<Dropdown>();
    //    switch (dd.value) {
    //        default:
    //            return Tile.Element.None;
    //        case 1:
    //            return Tile.Element.Fire;
    //        case 2:
    //            return Tile.Element.Water;
    //        case 3:
    //            return Tile.Element.Earth;
    //        case 4:
    //            return Tile.Element.Air;
    //        case 5:
    //            return Tile.Element.Muscle;
    //    }
    //}

    //public void GetClickEffect(TileBehav tb) {
    //    Dropdown dd = GameObject.Find("Dropdown_ClickEffect").GetComponent<Dropdown>();
    //    MageMatch mm = GameObject.Find("board").GetComponent<MageMatch>();

    //    switch (dd.value) {
    //        default: // none
    //            break;
    //        case 1: // destroy tile
    //            mm.RemoveTile(tb.tile, false); // FIXME will probably throw null reference exception
    //            break;
    //        case 2: // clear enchant
    //            tb.ClearEnchantment();
    //            break;
    //        case 3: // cherrybomb
    //            spellfx.Ench_SetCherrybomb(mm.ActiveP().id, tb);
    //            break;
    //        case 4: // burning
    //            spellfx.Ench_SetBurning(mm.ActiveP().id, tb);
    //            break;
    //        case 5: // zombify
    //            spellfx.Ench_SetZombify(mm.ActiveP().id, tb, false);
    //            break;
    //    }
    //}

    public void UpdateTurnTimer(float time) {
        turnTimerText.text = time.ToString("0.0");
    }

    public void UpdateEffTexts() {
        object[] lists = mm.effectCont.GetLists();

        List<Effect> beginTurnEff = (List<Effect>)lists[0];
        string bte = "BeginTurnEffs:\n";
        foreach (Effect e in beginTurnEff) {
            bte += e.tag + "\n";
        }
        beginTurnEffText.text = bte;

        List<Effect> endTurnEff = (List<Effect>)lists[1];
        string ete = "EndTurnEffs:\n";
        foreach (Effect e in endTurnEff) {
            ete += e.tag + "\n";
        }
        endTurnEffText.text = ete;

        List<MatchEffect> matchEff = (List<MatchEffect>)lists[2];
        string me = "MatchEffs:\n";
        foreach (MatchEffect e in matchEff) {
            me += e.tag + "\n";
        }
        matchEffText.text = me;

        List<SwapEffect> swapEff = (List<SwapEffect>)lists[3];
        string se = "SwapEffs:\n";
        foreach (SwapEffect e in swapEff) {
            se += e.tag + "\n";
        }
        swapEffText.text = se;
    }
}
