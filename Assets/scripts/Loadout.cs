using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Abstract class??
public class Loadout {

	public string characterName;
	public string techniqueName;
	public int maxHealth;

	private static SpellEffects spellfx;
	private int dfire, dwater, dearth, dair, dmuscle; // portions of 100 total
	private Spell[] spells;

	public Loadout(int preset){
		spells = new Spell[4];
		switch (preset) {
		case 0:
			TestLoadout ();
			break;
		case 1:
			EnfuegoA ();
			break;
		case 2:
			EnfuegoB ();
			break;
		case 3:
			GravekeeperA ();
			break;
		case 4:
			GravekeeperB ();
			break;
		case 5:
			RockyA ();
			break;
		case 6:
			RockyB ();
			break;
		default:
			Debug.Log ("Loadout number must be 1 through 6.");
			break;
		}
	}

	public static void Init(){
		spellfx = new SpellEffects ();
		SpellEffects.Init ();
	}

	void TestLoadout(){
		characterName = "Sample";
		techniqueName = "Test Loadout";
		maxHealth = 1000;

		dfire = 20;
		dwater = 20;
		dearth = 20;
		dair = 20;
		dmuscle = 20;

		spells[0] = new Spell ("Cherrybomb", "FAM", 1, spellfx.Cherrybomb);
		spells[2] = new Spell ("Spell two", "FAF", 1, spellfx.Deal496Dmg);
		spells[1] = new Spell ("Spell three", "FMF", 1, spellfx.Deal496Dmg);
		spells[3] = new Spell ("Spell four", "AFM", 1, spellfx.Deal496Dmg);
	}

	void EnfuegoA(){ // Enfuego A - Supah Hot Fire
		characterName = "Enfuego";
		techniqueName = "Supah Hot Fire";
		maxHealth = 1000;

		dfire = 50;
		dair = 20;
		dmuscle = 30;

		spells[0] = new Spell ("White-Hot Combo Kick", "MFFM", 1, spellfx.WhiteHotComboKick);
		spells[2] = new Spell ("Incinerate", "FAFF", 1, spellfx.Incinerate);
		spells[1] = new Spell ("Baila!", "FMF", 1, spellfx.Baila);
		spells[3] = new Spell ("Phoenix Fire", "AFM", 1, spellfx.PhoenixFire);
	}

	// FOCUS
	void EnfuegoB(){ // Enfuego B - Hot Feet
		characterName = "Enfuego";
		techniqueName = "Hot Feet";
		maxHealth = 1100;

		dfire = 50;
		dearth = 15;
		dmuscle = 35;

		spells[0] = new Spell ("White-Hot Combo Kick", "MFFM", 1, spellfx.WhiteHotComboKick);
		spells[1] = new Spell ("Hot Body", "FEFM", 1, spellfx.HotBody);
		spells[2] = new Spell ("Hot and Bothered", "FMF", 1, spellfx.HotAndBothered);
		spells[3] = new Spell ("Pivot", "MEF", 0, spellfx.Pivot);
	}

	// FOCUS
	void GravekeeperA(){ // The Gravekeeper A - Business in the Front
		characterName = "The Gravekeeper";
		techniqueName = "Business in the Front";
		maxHealth = 1150;

		dwater = 20;
		dearth = 40;
		dmuscle = 40;

		spells[0] = new Spell ("Raise Zombie", "EMME", 1, spellfx.Deal496Dmg);
		spells[1] = new Spell ("Zombie Synergy", "MEE", 1, spellfx.Deal496Dmg);
		spells[2] = new Spell ("Human Resources", "MEME", 1, spellfx.Deal496Dmg);
		spells[3] = new Spell ("Company Luncheon", "EMWM", 1, spellfx.Deal496Dmg);
	}
	
	void GravekeeperB(){ // The Gravekeeper A - Party in the Back
		characterName = "The Gravekeeper";
		techniqueName = "Party in the Back";
		maxHealth = 1050;

		dfire = 25;
		dearth = 35;
		dmuscle = 40;

		spells[0] = new Spell ("Raise Zombie", "EMME", 1, spellfx.Deal496Dmg);
		spells[1] = new Spell ("R.S.V.Z.", "MEM", 1, spellfx.Deal496Dmg);
		spells[2] = new Spell ("The Oogie Boogie", "MFE", 1, spellfx.Deal496Dmg);
		spells[3] = new Spell ("Bottle Rocket Mishap", "EMFM", 1, spellfx.Deal496Dmg);
	}

	// TODO
	void RockyA(){ // Rocky A - Tectonic Titan 
		characterName = "Rocky";
		techniqueName = "Tectonic Titan";
		maxHealth = 1100;

		dearth = 45;
		dair = 30;
		dmuscle = 20;
		dfire = 5;

		spells[0] = new Spell ("Magnitude 10", "EEMEE", 1, spellfx.Magnitude10);
		spells[1] = new Spell ("Sinkhole", "EAAE", 1, spellfx.Deal496Dmg);
		spells[2] = new Spell ("Boulder Barrage", "MMEE", 1, spellfx.Deal496Dmg);
		spells[3] = new Spell ("Stalagmite", "AEE", 1, spellfx.Stalagmite);
	}

	// TODO
	void RockyB(){ // Rocky B - Continental Champion
		characterName = "Rocky";
		techniqueName = "Continental Champion";
		maxHealth = 1300;

		dearth = 40;
		dwater = 25;
		dmuscle = 25;
		dair = 10;

		spells[0] = new Spell ("Magnitude 10", "EEMEE", 1, spellfx.Magnitude10);
		spells[1] = new Spell ("Living Flesh Armor", "EWWE", 1, spellfx.Deal496Dmg);
		spells[2] = new Spell ("Figure-Four Leglock", "MEEM", 1, spellfx.Deal496Dmg);
		spells[3] = new Spell ("Stalagmite", "AEE", 1, spellfx.Deal496Dmg);
	}

	public Spell GetSpell(int index){
		return spells [index];
	}

	public List<TileSeq> GetTileSeqList(){
		List<TileSeq> outlist = new List<TileSeq> ();
		foreach (Spell s in spells)
			outlist.Add (s.GetTileSeq ());
		return outlist;
	}

	public Tile.Element GetTileElement(){
		int rand = Random.Range (0, 100);
		if      (rand < dfire)
			return Tile.Element.Fire;
		else if (rand < dfire + dwater)
			return Tile.Element.Water;
		else if (rand < dfire + dwater + dearth)
			return Tile.Element.Earth;
		else if (rand < dfire + dwater + dearth + dair)
			return Tile.Element.Air;
		else
			return Tile.Element.Muscle;
	}
}
