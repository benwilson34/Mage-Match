using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Character {

    // NOTE: keep in same order as JSON list!!
    public enum Ch { Test = 0, Enfuego, Gravekeeper };

    public string characterName;
    public string loadoutName;
    public int meter = 0, meterMax = 100; // protected?

    protected int maxHealth;
    protected ObjectEffects objFX; // needed here?
    protected int dfire, dwater, dearth, dair, dmuscle; // portions of 100 total
    protected Spell[] spells;
    protected MageMatch mm;
    protected TileManager tileMan;
    protected int playerID;
    protected List<string> runes;
    //protected string genHexTag;

    public Character(MageMatch mm) {
        this.mm = mm;
        tileMan = mm.tileMan;
        runes = new List<string>();
    } //?

    public virtual void InitEvents() {
        mm.eventCont.playerHealthChange += OnPlayerHealthChange;
    }

    public void InitSpells() {
        foreach (Spell sp in spells)
            sp.Init(mm);
    }

    public void OnPlayerHealthChange(int id, int amount, bool dealt) {
        if (dealt && id != playerID) // if the other player was dealt dmg (not great)
            ChangeMeter((-amount) / 3);
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
        meter = Mathf.Clamp(meter, 0, meterMax); // TODO clamp amount before event
        mm.eventCont.PlayerMeterChange(playerID, amount);
    }

    public int GetMaxHealth() { return maxHealth; }

    public Spell GetSpell(int index) { return spells[index]; }

    public List<Spell> GetSpells() {
        return new List<Spell>(spells);
    }

    public List<TileSeq> GetTileSeqList() {
        List<TileSeq> outlist = new List<TileSeq>();
        foreach (Spell s in spells)
            outlist.Add(s.GetTileSeq());
        return outlist;
    }

    public Tile.Element GetDeckBasicTile() {
        int rand = Random.Range(0, 50);
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

    public string GenerateHexTag() {
        int total = 50 + (runes.Count * 10);
        int rand = Random.Range(0, total);

        if (rand < 50)
            return "p" + playerID + "-B-" + GetDeckBasicTile().ToString().Substring(0, 1); // + "-" ?
        else {
            int index = Mathf.CeilToInt((rand - 50) / 10f); 
            return runes[index]; // + "-" ?
        }
    }

    //public string GetHexTag() { return genHexTag; }

    public static Character Load(MageMatch mm, int id) {
        Ch myChar = mm.gameSettings.GetLocalChar(id);
        switch (myChar) {
            case Ch.Test:
                return new CharTest(mm);
            case Ch.Enfuego:
                return new Enfuego(mm, id);
            case Ch.Gravekeeper:
                return new Gravekeeper(mm, id);

            default:
                Debug.LogError("Loadout number must be 1 through 6.");
                return null;
        }
    }
}



public class CharTest : Character {
    public CharTest(MageMatch mm) : base(mm) {
        objFX = mm.objFX;
        spells = new Spell[4];

        characterName = "Sample";
        loadoutName = "Test Loadout";
        maxHealth = 1000;

        SetDeckElements(20, 20, 20, 20, 20);

        spells[0] = new Spell(0, "Cherrybomb", "FFA", 1, objFX.Cherrybomb);
        spells[1] = new Spell(1, "Massive damage", "FFA", 1, objFX.Deal496Dmg);
        spells[2] = new Spell(2, "Stone Test", "FAF", 1, objFX.Deal496Dmg);
        spells[3] = new Spell(3, "Massive damage", "AFA", 1, objFX.Deal496Dmg);
        InitSpells();
    }
}
