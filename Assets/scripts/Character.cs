using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Character {

    public string characterName;
    public string loadoutName;
    public int meter, meterMax = 100; // protected?

    protected int maxHealth;
    protected SpellEffects spellfx;
    protected int dfire, dwater, dearth, dair, dmuscle; // portions of 100 total
    protected Spell[] spells;
    protected MageMatch mm;
    protected int playerID;

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

    public static Character Load(MageMatch mm, int id) {
        switch (mm.uiCont.GetLoadoutNum(id)) {
            case 0:
                return new CharTest();
            case 1:
                return new Enfuego(mm, id, 1);
            case 2:
                return new Enfuego(mm, id, 2);
            case 3:
                return new Gravekeeper(mm, id, 1);
            case 4:
                return new Gravekeeper(mm, id, 2);
            case 5:
                return new Rocky(mm, id, 1);
            case 6:
                return new Rocky(mm, id, 2);
            default:
                Debug.Log("Loadout number must be 1 through 6.");
                return null;
        }
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
        spells[1] = new Spell(1, "Massive damage", "FFA", 1, spellfx.Deal496Dmg);
        spells[2] = new Spell(2, "Stone Test", "FAF", 1, spellfx.StoneTest);
        spells[3] = new Spell(3, "Massive damage", "AFA", 1, spellfx.Deal496Dmg);
    }
}
