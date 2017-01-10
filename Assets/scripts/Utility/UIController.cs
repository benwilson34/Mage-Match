using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class UIController : MonoBehaviour {

	private Text moveText, debugGridText;
	private MageMatch mm;
	private Dropdown DD1, DD2;

    private Transform p1info, p2info, p1load, p2load;
    private GameObject gradient, targetingBG;
    private GameObject tCancelB, tClearB;
    private GameObject settingsMenu; // ?
    private RectTransform moodMarker, moodMeter;
    private SpellEffects spellfx;

    public Sprite miniFire, miniWater, miniEarth, miniAir, miniMuscle;

    public void Init(){ // Awake()?
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();

		moveText = GameObject.Find ("Text_Move").GetComponent<Text> (); // UI move announcement
		moveText.text = "";
		debugGridText = GameObject.Find ("Text_Debug1").GetComponent<Text> (); // UI debug grid
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

        moodMarker = GameObject.Find("MoodMarker").GetComponent<RectTransform>();
        moodMeter = GameObject.Find("MoodMeter").GetComponent<RectTransform>();

        spellfx = new SpellEffects();
    }

    public void Reset(Player p1, Player p2) { // could just get players from MM
        UpdateDebugGrid();
        UpdateMoveText("");

        Text nameText = p1info.Find("Text_Name").GetComponent<Text>();
        nameText.text = "P" + p1.id + " - " + p1.name;
        ShowLoadout(p1);
        DeactivateAllSpellButtons(p1);

        nameText = p2info.Find("Text_Name").GetComponent<Text>();
        nameText.text = "P" + p2.id + " - " + p2.name;
        ShowLoadout(p2);
        DeactivateAllSpellButtons(p2);

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

	public void UpdatePlayerInfo(){
		UpdatePlayerInfo (mm.ActiveP());
		UpdatePlayerInfo (mm.InactiveP());
	}

    public void UpdatePlayerInfo(Player p){
		Transform pinfo;
		if (p.id == 1)
            pinfo = p1info;
		else
            pinfo = p2info;

		if (p.id == mm.ActiveP().id)
			pinfo.GetComponent<Image> ().color = new Color (0, 1, 0, .4f);
		else
			pinfo.GetComponent<Image> ().color = new Color (1, 1, 1, .4f);

		Text APText = pinfo.Find ("Text_AP").GetComponent<Text>();
		APText.text = "AP left: " + p.AP;

        StartCoroutine(UpdateHealthbar(p, pinfo.Find("Health_Outline")));
        StartCoroutine(UpdateMeterbar(p, pinfo.Find("Meter_Outline")));
	}

    IEnumerator UpdateHealthbar(Player p, Transform healthOutline) {
        RectTransform healthbar = healthOutline.Find("Healthbar").GetComponent<RectTransform>();
        Text healthText = healthOutline.Find("Text_Health").GetComponent<Text>();
        int maxHealth = p.character.GetMaxHealth();

        healthText.text = p.health + "/" + maxHealth;
        float slideRatio = (float)p.health / maxHealth;

        // health bar coloring; green -> yellow -> red
        float thresh = .6f; // point where health bar is yellow (0.6 = 60% health)
        float r = (((Mathf.Clamp(slideRatio, thresh, 1) - thresh) / (1 - thresh)) * -1 + 1);
        float g = Mathf.Clamp(slideRatio, 0, thresh) / thresh;
        healthbar.GetComponent<Image>().color = new Color(r, g, 0);

        yield return healthbar.DOScaleX(slideRatio, .4f);
    }

    IEnumerator UpdateMeterbar(Player p, Transform meterOutline) { // no args eventually?
        RectTransform meter = meterOutline.Find("Meterbar").GetComponent<RectTransform>();
        Text meterText = meterOutline.Find("Text_Meter").GetComponent<Text>();
        meterText.text = p.character.meter + "/" + p.character.meterMax;
        float slideRatio = (float) p.character.meter / p.character.meterMax;
        yield return meter.DOScaleX(slideRatio, .8f);
    }

	public void ShowLoadout(Player player){
		Transform pload;
        if (player.id == 1)
            pload = p1load;
        else
            pload = p2load;
		Text loadoutText = pload.Find ("Text_LoadoutName").GetComponent<Text>();
		loadoutText.text = player.character.characterName + " - " + player.character.techniqueName;

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
        tCancelB.SetActive(!tCancelB.activeSelf);
        tClearB.SetActive(!tClearB.activeSelf);
    }

    public void SetDrawButton(Player p, bool interactable) {
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
		Button button = GetButton (player, index);
		button.interactable = true;
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
    public Tile.Element GetClickElement() {
        Dropdown dd = GameObject.Find("Dropdown_DropColor").GetComponent<Dropdown>();
        switch (dd.value) {
            default:
                return Tile.Element.None;
            case 1:
                return Tile.Element.Fire;
            case 2:
                return Tile.Element.Water;
            case 3:
                return Tile.Element.Earth;
            case 4:
                return Tile.Element.Air;
            case 5:
                return Tile.Element.Muscle;
        }
    }

    public void GetClickEffect(TileBehav tb) {
        Dropdown dd = GameObject.Find("Dropdown_ClickEffect").GetComponent<Dropdown>();
        MageMatch mm = GameObject.Find("board").GetComponent<MageMatch>();

        switch (dd.value) {
            default: // none
                break;
            case 1: // destroy tile
                mm.RemoveTile(tb.tile, true, false); // FIXME will probably throw null reference exception
                break;
            case 2: // clear enchant
                tb.ClearEnchantment();
                break;
            case 3: // cherrybomb
                spellfx.Ench_SetCherrybomb(tb);
                break;
            case 4: // burning
                spellfx.Ench_SetBurning(tb);
                break;
            case 5: // zombify
                spellfx.Ench_SetZombify(tb, false);
                break;
        }
    }

    public void UpdateCommishMeter(){
		mm.StartAnim(SlideMoodMarker());
	}

	IEnumerator SlideMoodMarker(){
		float slideRatio = (float)(mm.commish.GetMood() + 100) / 200f;
		float meterwidth = moodMeter.rect.width;
		yield return moodMarker.DOAnchorPosX(slideRatio * meterwidth, .4f, false);
	}
}
