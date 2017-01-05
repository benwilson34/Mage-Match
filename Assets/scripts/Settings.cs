using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// TODO move to UIController.cs
public class Settings : MonoBehaviour {

    private MageMatch mm;
	private GameObject settingsMenu;
	private SpellEffects spellfx;

	void Start () {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        settingsMenu = GameObject.Find("SettingsMenu");
		settingsMenu.SetActive(mm.menu); //?
		spellfx = new SpellEffects();
	}

	public void ToggleMenu(){
		mm.menu = !mm.menu;
		Text menuButtonText = GameObject.Find ("MenuButtonText").GetComponent<Text> ();
		if (mm.menu) {
			menuButtonText.text = "Close Menu";
		} else {
			menuButtonText.text = "Menu";
		}
		settingsMenu.SetActive(mm.menu);
	}

	// TODO methods for two edit dropdowns
	public Tile.Element GetClickElement(){
		Dropdown dd = GameObject.Find ("Dropdown_DropColor").GetComponent<Dropdown> ();
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

	public void GetClickEffect(TileBehav tb){
		Dropdown dd = GameObject.Find ("Dropdown_ClickEffect").GetComponent<Dropdown> ();
		MageMatch mm = GameObject.Find ("board").GetComponent<MageMatch> ();

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
//			tb.SetEnchantment (EnchantEffects.Cherrybomb);
			spellfx.Ench_SetCherrybomb(tb);
			break;
		case 4: // burning
			spellfx.Ench_SetBurning (tb);
			break;
		case 5: // zombify
            spellfx.Ench_SetZombify(tb, false);
			break;
		}
	}
}
