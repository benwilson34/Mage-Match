using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public static class UIController {

	private static Text moveText, turnText, debugGridText;
	private static MageMatch mm;
	private static UIResources uires;
	private static Dropdown DD1, DD2;

	public static void Init(){
		moveText = GameObject.Find ("Text_Move").GetComponent<Text> (); // UI move announcement
		moveText.text = "";
		turnText = GameObject.Find ("Text_Turns").GetComponent<Text> (); // UI turn counter
		debugGridText = GameObject.Find ("Text_Debug1").GetComponent<Text> (); // UI debug grid

		mm = GameObject.Find ("board").GetComponent<MageMatch> ();
		uires = GameObject.Find ("Canvas").GetComponent<UIResources> ();

		DD1 = GameObject.Find ("Dropdown_p1").GetComponent<Dropdown> ();
		DD2 = GameObject.Find ("Dropdown_p2").GetComponent<Dropdown> ();
	}

	public static void UpdateDebugGrid(){
		string grid = "   0  1  2  3  4  5  6 \n";
		for (int r = HexGrid.numRows - 1; r >= 0; r--) {
			grid += r + " ";
			for (int c = 0; c < HexGrid.numCols; c++) {
				if (r <= HexGrid.TopOfColumn (c) && r >= HexGrid.BottomOfColumn (c)) {
					if (HexGrid.IsSlotFilled (c, r))
						grid += "[" + HexGrid.GetTileAt (c, r).ThisElementToChar() + "]";
					else
						grid += "[ ]";
				} else
					grid += " - ";
			}
			grid += '\n';
		}
		debugGridText.text = grid;
	}

	public static void UpdateTurnText(){
		turnText.text = "Completed Turns: " + MageMatch.turns;
	}

	public static void UpdateMoveText(string str){
		moveText.text = str;
	}

	public static void UpdatePlayerInfo(){
		UpdatePlayerInfo (MageMatch.ActiveP());
		UpdatePlayerInfo (MageMatch.InactiveP());
	}

	public static void FlipGradient(){
		GameObject go = GameObject.Find ("green-gradient");
		//		Vector3 scale = go.transform.localScale;
		//		go.transform.localScale.Set (scale.x * -1, scale.y, scale.z);
		go.transform.Rotate(0,0,180);
	}

	public static void UpdatePlayerInfo(Player player){
		GameObject pinfo;
		if (player.id == 1) {
			pinfo = GameObject.Find ("Player1_Info");
		} else {
			pinfo = GameObject.Find ("Player2_Info");
		}

		if (player.id == MageMatch.ActiveP().id) {
			pinfo.GetComponent<Image> ().color = new Color (0, 1, 0, .4f);
		} else {
			pinfo.GetComponent<Image> ().color = new Color (1, 1, 1, .4f);
		}

		// TODO not great
		Text nameText    = pinfo.transform.Find ("Text_Name").GetComponent<Text>();
		Text matchesText = pinfo.transform.Find ("Text_Matches").GetComponent<Text>();
		Text p1APText    = pinfo.transform.Find ("Text_AP").GetComponent<Text>();
		Text healthText  = pinfo.transform.Find ("Health_Outline").Find ("Text_Health").GetComponent<Text>();
		Image healthBar  = pinfo.transform.Find ("Health_Outline").Find("Healthbar").GetComponent<Image>();

		nameText.text = "P" + player.id + " - " + player.name;
		p1APText.text = "AP left: " + player.AP;
		matchesText.text = "Matches: " + player.matches;

		// health bar text and width
		healthText.text = player.health + "/" + player.loadout.maxHealth;
		Vector3 healthScale = healthBar.rectTransform.localScale;
		float healthRatio = (float)player.health / (float)player.loadout.maxHealth;
		healthScale.x = healthRatio;
		healthBar.rectTransform.localScale = healthScale;

		// health bar coloring; green -> yellow -> red
		float thresh = .6f; // point where health bar is yellow (0.6 = 60% health)
		float r = (((Mathf.Clamp(healthRatio, thresh, 1) - thresh)/(1 - thresh)) * -1 + 1);
		float g = Mathf.Clamp(healthRatio, 0, thresh) / thresh;
		healthBar.color = new Color (r, g, 0);
	}

	public static void ShowLoadout(Player player){
		Transform pload;
		if(player.id == 1)
			pload = GameObject.Find ("Player1_Loadout").transform;
		else
			pload = GameObject.Find ("Player2_Loadout").transform;
		Text loadoutText = pload.Find ("Text_LoadoutName").GetComponent<Text>();
		loadoutText.text = player.loadout.characterName + " - " + player.loadout.techniqueName;

		for (int i = 0; i < 4; i++){
			Transform t = pload.Find ("Button_Spell" + i);
			Spell currentSpell = player.loadout.GetSpell (i);

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
						minitile.sprite = uires.miniFire;
						break;
					case Tile.Element.Water:
						minitile.sprite = uires.miniWater;
						break;
					case Tile.Element.Earth:
						minitile.sprite = uires.miniEarth;
						break;
					case Tile.Element.Air:
						minitile.sprite = uires.miniAir;
						break;
					case Tile.Element.Muscle:
						minitile.sprite = uires.miniMuscle;
						break;
					}
				} else {
					// turn image transparent
					minitile.color = new Color(1,1,1,0);
				}

			}
		}
	}

	static Button GetButton(Player player, int index){
		Transform pload;
		if(player.id == 1)
			pload = GameObject.Find ("Player1_Loadout").transform;
		else
			pload = GameObject.Find ("Player2_Loadout").transform;
		return pload.Find ("Button_Spell" + index).GetComponent<Button>();
	}

	public static void ActivateSpellButton(Player player, int index){
		Button button = GetButton (player, index);
		button.interactable = true;
	}

	public static void DeactivateSpellButton(Player player, int index){
		Button button = GetButton (player, index);
		button.interactable = false;
	}

	public static void DeactivateAllSpellButtons(Player player){
		for (int i = 0; i < 4; i++) {
			Button button = GetButton (player, i);
			button.interactable = false;
		}
	}

	public static int GetLoadoutNum(int p){
		if (p == 1) {
			return DD1.value;
		} else {
			return DD2.value;
		}
	}

	public static void UpdateCommishMeter(){
		mm.StartAnim(SlideMoodMarker());
	}

	static IEnumerator SlideMoodMarker(){
		RectTransform moodmarker = GameObject.Find ("MoodMarker").GetComponent<RectTransform> ();
		float slideRatio = (float)(Commish.GetMood() + 100) / 200f;
		float meterwidth = GameObject.Find ("MoodMeter").GetComponent<RectTransform> ().rect.width;
		yield return moodmarker.DOAnchorPosX(slideRatio * meterwidth, .2f, false);
	}
}