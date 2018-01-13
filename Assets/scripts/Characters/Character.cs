using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Character {

    // NOTE: keep in same order as JSON list!!
    public enum Ch { Test = 0, Enfuego, Gravekeeper, Rocky };
    public Ch ch;

    public string characterName;

    public static int METER_MAX = 1000; // may need to be changed later
    protected int meter = 0;

    public static int HEALTH_WARNING_AMT = 150; // audio/visual warning at 150 hp
    protected int maxHealth;
    protected ObjectEffects objFX; // needed here?
    protected int dfire, dwater, dearth, dair, dmuscle; // portions of 50 total
    protected Spell[] spells;
    protected MageMatch mm;
    protected HexManager tileMan;
    protected int playerId;
    protected List<string> runes;
    //protected string genHexTag;

    protected bool playedFullMeterSound = false;

    public Character(MageMatch mm, Ch ch, int playerId) {
        this.mm = mm;
        this.ch = ch;
        this.playerId = playerId;
        tileMan = mm.tileMan;
        runes = new List<string>();

        mm.onEffectContReady += OnEffectContLoad;
        mm.onEventContReady += OnEventContLoad;
    } //?

    public virtual void InitEvents() {
        mm.eventCont.AddDrawEvent(OnDraw, EventController.Type.Player, EventController.Status.Begin);
        mm.eventCont.AddDropEvent(OnDrop, EventController.Type.Player, EventController.Status.Begin);
        mm.eventCont.AddSwapEvent(OnSwap, EventController.Type.Player, EventController.Status.Begin);
        mm.eventCont.spellCast += OnSpellCast;
        mm.eventCont.playerHealthChange += OnPlayerHealthChange;
        mm.eventCont.tileRemove += OnTileRemove;
    }

    // override to init character with event callbacks (for their passive, probably)
    public virtual void OnEventContLoad() {
        
    }

    // override to init character with effect callbacks (for their passive, probably)
    public virtual void OnEffectContLoad() {

    }

    public void InitSpells() {
        Spell sp;
        for (int i = 0; i < 5; i++) {
            sp = spells[i];
            sp.Init(mm);
            sp.info = CharacterInfo.GetSpellInfo(ch, i, true);
        }
    }

    public void SetDeckElements(int f, int w, int e, int a, int m) {
        dfire = f;
        dwater = w;
        dearth = e;
        dair = a;
        dmuscle = m;
    }


    // ----------  METER  ----------

    public int GetMeter() { return meter; }

    public void ChangeMeter(int amount) {
        meter += amount;
        meter = Mathf.Clamp(meter, 0, METER_MAX); // TODO clamp amount before event
        mm.eventCont.PlayerMeterChange(playerId, amount, meter);

        if (!playedFullMeterSound && meter == METER_MAX) {
            mm.audioCont.FullMeter();
            playedFullMeterSound = true;
        }
    }

    public IEnumerator OnDraw(int id, string tag, bool playerAction, bool dealt) {
        if(id == playerId && !dealt)
            ChangeMeter(10);
        yield return null;
    }

    public IEnumerator OnDrop(int id, bool playerAction, string tag, int col) {
        if(id == playerId)
            ChangeMeter(10);
        yield return null;
    }

    public IEnumerator OnSwap(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        if(id == playerId)
            ChangeMeter(10);
        yield return null;
    }

    public void OnSpellCast(int id, Spell spell) {
        if (id == playerId) {
            if (spell is SignatureSpell)
                playedFullMeterSound = false;
            else
                ChangeMeter(10);    
        }
    }

    public void OnPlayerHealthChange(int id, int amount, int newHealth, bool dealt) {
        if (dealt && id != playerId) // if the other player was dealt dmg (not great)
            ChangeMeter(-amount);

        if (amount > 0 && id == playerId) // healing
            ChangeMeter(amount / 2);
    }

    public void OnTileRemove(int id, TileBehav tb) {
        if (id == playerId)
            ChangeMeter(5);
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
            return "p" + playerId + "-B-" + GetDeckBasicTile().ToString().Substring(0, 1); // + "-" ?
        else {
            int index = Mathf.CeilToInt((rand - 50) / 10f); 
            return runes[index]; // + "-" ?
        }
    }

    public Player ThisPlayer() {
        return mm.GetPlayer(playerId);
    }

    //public string GetHexTag() { return genHexTag; }

    public static Character Load(MageMatch mm, int id) {
        Ch myChar = mm.gameSettings.GetLocalChar(id);
        switch (myChar) {
            case Ch.Test:
                return new CharTest(mm, id);
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
    public CharTest(MageMatch mm, int id) : base(mm, Ch.Test, id) {
        objFX = mm.hexFX;

        characterName = "Sample";
        maxHealth = 1000;

        SetDeckElements(20, 20, 20, 20, 20);

        spells = new Spell[5];
        spells[0] = new Spell(0, "Cherrybomb", "FFA", objFX.Cherrybomb);
        spells[1] = new Spell(1, "Massive damage", "FFA", objFX.Deal496Dmg);
        spells[2] = new Spell(2, "Massive damage", "FAF", objFX.Deal496Dmg);
        spells[3] = new Spell(3, "Massive damage", "AFA", objFX.Deal496Dmg);
        spells[4] = new Spell(4, "Massive damage", "AFA", objFX.Deal496Dmg);
        InitSpells();
    }
}
