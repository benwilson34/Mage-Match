using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
			Commissioner ();
			break;
		case 1:
			EnfuegoA ();
			break;
		case 2:
			EnfuegoB ();
			break;
		case 3:
			RockyA ();
			break;
		case 4:
			RockyB ();
			break;
		default:
			Debug.Log ("Loadouts must be 1, 2, 3, or 4.");
			break;
		}
	}

	public static void Init(){
		spellfx = new SpellEffects ();
		SpellEffects.Init ();
	}

	void Commissioner(){
		characterName = "Commissioner";
//		techniqueName = "default";
//		maxHealth = 1;

		dfire = 20;
		dwater = 20;
		dearth = 20;
		dair = 20;
		dmuscle = 20;

		spells [0] = new Spell ("Commissioner turn", "", 0, SpellEffects.Comm_Place5RandomTiles);
	}

	void EnfuegoA(){ // Enfuego A - kickboxing
		characterName = "Enfuego";
		techniqueName = "Kickboxer";
		maxHealth = 1150;

		dfire = 40;
		dmuscle = 40;
		dair = 20;

		spells[0] = new Spell ("White-Hot Combo Kick", "MFFM", 1, spellfx.WhiteHotComboKick);
		spells[1] = new Spell ("Lightning Palm", "AFAF", 1, spellfx.LightningPalm);
		spells[2] = new Spell ("Caught You 'Mirin", "MFMF", 1, spellfx.CaughtYouMirin);
		spells[3] = new Spell ("Hot Body", "FMF", 1, spellfx.HotBody);
	}

	void EnfuegoB(){ // Enfuego B - fire mage
		characterName = "Enfuego";
		techniqueName = "Fire Mage";
		maxHealth = 1050;

		dfire = 45;
		dair = 25;
		dmuscle = 15;
		dwater = 8;
		dearth = 7;

		spells[0] = new Spell ("White-Hot Combo Kick", "MFFM", 1, spellfx.WhiteHotComboKick);
		spells[1] = new Spell ("Cherrybomb", "EFWF", 1, spellfx.Cherrybomb);
		spells[2] = new Spell ("Incinerate", "FAFF", 1, spellfx.Deal496Dmg);
		spells[3] = new Spell ("Hot Body", "FMF", 1, spellfx.HotBody);
	}

	void RockyA(){ // Rocky A - earth mage
		characterName = "Rocky";
		techniqueName = "Earth Mage";
		maxHealth = 1100;

		dearth = 45;
		dair = 30;
		dmuscle = 20;
		dfire = 5;

		spells[0] = new Spell ("Magnitude 10", "EEMEE", 1, spellfx.Magnitude10);
		spells[1] = new Spell ("Sinkhole", "EAAE", 1, spellfx.Deal496Dmg);
		spells[2] = new Spell ("Boulder Barrage", "MMEE", 1, spellfx.Deal496Dmg);
		spells[3] = new Spell ("Stalagmite", "AEE", 1, spellfx.Deal496Dmg);
	}

	void RockyB(){ // Rocky B - MMA master
		characterName = "Rocky";
		techniqueName = "MMA Thug";
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

	public Tile.Element GetTileElement (){
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
