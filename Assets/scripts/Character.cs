using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Character {

    public string characterName;
    public string loadoutName;

    protected int maxHealth;
    protected SpellEffects spellfx;
    protected int dfire, dwater, dearth, dair, dmuscle; // portions of 100 total
    protected Spell[] spells;
    public int meter, meterMax = 100; // protected?

    public Character() { //?
        spellfx = new SpellEffects();
    }

    public void SetDeckElements(int f, int w, int e, int a, int m) {
        dfire = f;
        dwater = w;
        dearth = e;
        dair = a;
        dmuscle = m;
    }

    public void ChangeMeter(int amount) {
        meter += amount;
        meter = Mathf.Clamp(meter, 0, 100);
    }

    public int GetMaxHealth() { return maxHealth; }

    public Spell GetSpell(int index) { return spells[index]; }

    public List<TileSeq> GetTileSeqList() {
        List<TileSeq> outlist = new List<TileSeq>();
        foreach (Spell s in spells)
            outlist.Add(s.GetTileSeq());
        return outlist;
    }

    public Tile.Element GetTileElement() {
        int rand = Random.Range(0, 100);
        if (rand < dfire)
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

public class CharTest : Character {
    public CharTest() {
        spellfx = new SpellEffects();
        spells = new Spell[4];

        characterName = "Sample";
        loadoutName = "Test Loadout";
        maxHealth = 1000;

        SetDeckElements(20, 20, 20, 20, 20);

        spells[0] = new Spell(0, "Cherrybomb", "FFA", 1, spellfx.Cherrybomb);
        spells[1] = new Spell(1, "Stalagmite", "FFA", 1, spellfx.Stalagmite);
        spells[2] = new Spell(2, "Stone Test", "FAF", 1, spellfx.StoneTest);
        spells[3] = new Spell(3, "Zombie Synergy", "AFA", 1, spellfx.ZombieSynergy);
    }
}

public class Enfuego : Character {

    public Enfuego(int loadout) {
        spellfx = new SpellEffects();
        characterName = "Enfuego";
        spells = new Spell[4];

        if (loadout == 1)
            EnfuegoA();
        else
            EnfuegoB();
    }

    void EnfuegoA() { // Enfuego A - Supah Hot Fire
        loadoutName = "Supah Hot Fire";
        maxHealth = 1000;

        SetDeckElements(50, 0, 0, 20, 30);

        spells[0] = new Spell(0, "White-Hot Combo Kick", "MFFM", 1, spellfx.WhiteHotComboKick);
        spells[1] = new Spell(1, "Baila!", "FMF", 1, spellfx.Baila);
        spells[2] = new Spell(2, "Incinerate", "FAFF", 1, spellfx.Incinerate);
        spells[3] = new Spell(3, "Phoenix Fire", "AFM", 1, spellfx.PhoenixFire);
    }

    // FOCUS
    void EnfuegoB() { // Enfuego B - Hot Feet
        loadoutName = "Hot Feet";
        maxHealth = 1100;

        SetDeckElements(50, 0, 15, 0, 35);

        spells[0] = new Spell(0, "White-Hot Combo Kick", "MFFM", 1, spellfx.WhiteHotComboKick);
        spells[1] = new Spell(1, "Hot Body", "FEFM", 1, spellfx.HotBody);
        spells[2] = new Spell(2, "Hot and Bothered", "FMF", 1, spellfx.HotAndBothered);
        spells[3] = new Spell(3, "Pivot", "MEF", 0, spellfx.Pivot);
    }
}

public class Rocky : Character {
    
    // TODO passive

    public Rocky(int loadout) {
        spellfx = new SpellEffects();
        characterName = "Rocky";
        spells = new Spell[4];

        if (loadout == 1)
            RockyA();
        else
            RockyB();
    }

    // TODO
    void RockyA() { // Rocky A - Tectonic Titan 
        loadoutName = "Tectonic Titan";
        maxHealth = 1100;

        SetDeckElements(5, 0, 45, 30, 20);

        spells[0] = new Spell(0, "Magnitude 10", "EEMEE", 1, spellfx.Magnitude10);
        spells[1] = new Spell(1, "Sinkhole", "EAAE", 1, spellfx.Deal496Dmg); //
        spells[2] = new Spell(2, "Boulder Barrage", "MMEE", 1, spellfx.Deal496Dmg); //
        spells[3] = new Spell(3, "Stalagmite", "AEE", 1, spellfx.Stalagmite);
    }

    // TODO
    void RockyB() { // Rocky B - Continental Champion
        loadoutName = "Continental Champion";
        maxHealth = 1300;

        SetDeckElements(0, 25, 40, 10, 25);

        spells[0] = new Spell(0, "Magnitude 10", "EEMEE", 1, spellfx.Magnitude10);
        spells[1] = new Spell(1, "Living Flesh Armor", "EWWE", 1, spellfx.Deal496Dmg); //
        spells[2] = new Spell(2, "Figure-Four Leglock", "MEEM", 1, spellfx.Deal496Dmg); //
        spells[3] = new Spell(3, "Stalagmite", "AEE", 1, spellfx.Stalagmite);
    }
}

public class Gravekeeper : Character {

    public Gravekeeper(int loadout) {
        spellfx = new SpellEffects();
        spells = new Spell[4];
        characterName = "The Gravekeeper";
        if (loadout == 1)
            GravekeeperA();
        else
            GravekeeperB();
    }

    // FOCUS
    void GravekeeperA() { // The Gravekeeper A - Business in the Front
        loadoutName = "Business in the Front";
        maxHealth = 1150;

        SetDeckElements(0, 20, 40, 0, 40);

        spells[0] = new Spell(0, "Raise Zombie", "EMME", 1, spellfx.RaiseZombie);
        spells[1] = new Spell(1, "Zombie Synergy", "MEE", 1, spellfx.ZombieSynergy);
        spells[2] = new Spell(2, "Human Resources", "MEME", 1, spellfx.HumanResources);
        spells[3] = new Spell(3, "Company Luncheon", "EMWM", 1, spellfx.CompanyLuncheon);
    }

    void GravekeeperB() { // The Gravekeeper B - Party in the Back
        loadoutName = "Party in the Back";
        maxHealth = 1050;

        SetDeckElements(25, 0, 35, 0, 40);

        spells[0] = new Spell(0, "Raise Zombie", "EMME", 1, spellfx.Deal496Dmg); //
        spells[1] = new Spell(1, "R.S.V.Z.", "MEM", 1, spellfx.Deal496Dmg); //
        spells[2] = new Spell(2, "The Oogie Boogie", "MFE", 1, spellfx.Deal496Dmg); //
        spells[3] = new Spell(3, "Bottle Rocket Mishap", "EMFM", 1, spellfx.Deal496Dmg); //
    }
}

// TODO non-static
public static class CharacterLoader {

    public static Character Load(int num) {
        switch (num) {
            case 0:
                return new CharTest();
            case 1:
                return new Enfuego(1);
            case 2:
                return new Enfuego(2);
            case 3:
                return new Gravekeeper(1);
            case 4:
                return new Gravekeeper(2);
            case 5:
                return new Rocky(1);
            case 6:
                return new Rocky(2);
            default:
                Debug.Log("Loadout number must be 1 through 6.");
                return null;
        }
    }

}
