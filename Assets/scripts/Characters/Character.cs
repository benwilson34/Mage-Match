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
    protected SpellEffects spellfx;
    protected int dfire, dwater, dearth, dair, dmuscle; // portions of 100 total
    protected Spell[] spells;
    protected MageMatch mm;
    protected int playerID;

    public Character(MageMatch mm) {
        this.mm = mm;
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

    public static Character Load(MageMatch mm, int id) {
        Ch myChar = mm.gameSettings.GetLocalChar(id);
        int loadout = mm.gameSettings.GetLocalLoadout(id);
        switch (myChar) {
            case Ch.Test:
                return new CharTest(mm);
            case Ch.Enfuego:
                return new Enfuego(mm, id, loadout);
            case Ch.Gravekeeper:
                return new Gravekeeper(mm, id, loadout);

            default:
                Debug.LogError("Loadout number must be 1 through 6.");
                return null;
        }

        //switch (mm.uiCont.GetLoadoutNum(id)) {
        //    case 0:
        //        return new CharTest(mm);
        //    case 1:
        //        return new Enfuego(mm, id, 1);
        //    case 2:
        //        return new Enfuego(mm, id, 2);
        //    case 3:
        //        return new Gravekeeper(mm, id, 1);
        //    case 4:
        //        return new Gravekeeper(mm, id, 2);
        //    case 5:
        //        return new Rocky(mm, id, 1);
        //    case 6:
        //        return new Rocky(mm, id, 2);
        //    default:
        //        Debug.Log("Loadout number must be 1 through 6.");
        //        return null;
        //}
    }
}

public class CharTest : Character {
    public CharTest(MageMatch mm) : base(mm) {
        spellfx = mm.spellfx;
        spells = new Spell[4];

        characterName = "Sample";
        loadoutName = "Test Loadout";
        maxHealth = 1000;

        SetDeckElements(20, 20, 20, 20, 20);

        spells[0] = new Spell(0, "Cherrybomb", "FFA", 1, spellfx.Cherrybomb);
        spells[1] = new Spell(1, "Massive damage", "FFA", 1, spellfx.Deal496Dmg);
        spells[2] = new Spell(2, "Stone Test", "FAF", 1, spellfx.StoneTest);
        spells[3] = new Spell(3, "Massive damage", "AFA", 1, spellfx.Deal496Dmg);
        InitSpells();
    }
}
