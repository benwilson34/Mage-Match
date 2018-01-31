using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Character {

    public enum Ch { Sample = 0, Enfuego, Gravekeeper, Valeria };
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
    protected HexManager hexMan;
    protected int playerId;
    protected List<string> runes;
    //protected string genHexTag;

    protected bool playedFullMeterSound = false;

    public Character(MageMatch mm, Ch ch, int playerId) {
        this.mm = mm;
        this.ch = ch;
        this.playerId = playerId;
        hexMan = mm.hexMan;
        runes = new List<string>();

        mm.onEffectContReady += OnEffectContLoad;
        mm.onEventContReady += OnEventContLoad;

        CharacterInfo info = CharacterInfo.GetCharacterInfoObj(ch);
        characterName = info.name;
        maxHealth = info.health;
        SetDeckElements(info.deck);
        InitSpells(info);
    }

    public virtual void InitEvents() {
        mm.eventCont.AddDrawEvent(OnDraw, EventController.Type.Player, EventController.Status.Begin);
        mm.eventCont.AddDropEvent(OnDrop, EventController.Type.Player, EventController.Status.Begin);
        mm.eventCont.AddSwapEvent(OnSwap, EventController.Type.Player, EventController.Status.Begin);
        mm.eventCont.spellCast += OnSpellCast;
        mm.eventCont.playerHealthChange += OnPlayerHealthChange;
        mm.eventCont.tileRemove += OnTileRemove;
    }

    // override to init character with event callbacks (for their passive, probably)
    public virtual void OnEventContLoad() {}

    // override to init character with effect callbacks (for their passive, probably)
    public virtual void OnEffectContLoad() {}


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

    
    // ----------  SPELLS  ----------

    protected void InitSpells(CharacterInfo info) {
        spells = new Spell[5];
        Debug.Log("Core spell has a cost of " + info.core.cost);
        spells[0] = new CoreSpell(0, info.core.title, CoreSpell, info.core.cost);
        spells[0].Init(mm);
        spells[0].info = CharacterInfo.GetSpellInfo(info.core, true);

        spells[1] = new Spell(1, info.spell1.title, info.spell1.prereq, Spell1, info.spell1.cost);
        spells[1].Init(mm);
        spells[1].info = CharacterInfo.GetSpellInfo(info.spell1, true);

        spells[2] = new Spell(2, info.spell2.title, info.spell2.prereq, Spell2, info.spell2.cost);
        spells[2].Init(mm);
        spells[2].info = CharacterInfo.GetSpellInfo(info.spell2, true);

        spells[3] = new Spell(3, info.spell3.title, info.spell3.prereq, Spell3, info.spell3.cost);
        spells[3].Init(mm);
        spells[3].info = CharacterInfo.GetSpellInfo(info.spell3, true);

        spells[4] = new SignatureSpell(4, info.signature.title, info.signature.prereq, SignatureSpell, info.signature.cost, info.signature.meterCost);
        spells[4].Init(mm);
        spells[4].info = CharacterInfo.GetSpellInfo(info.signature, true);

    }

    protected abstract IEnumerator CoreSpell(TileSeq seq);
    protected abstract IEnumerator Spell1(TileSeq seq);
    protected abstract IEnumerator Spell2(TileSeq seq);
    protected abstract IEnumerator Spell3(TileSeq seq);
    protected abstract IEnumerator SignatureSpell(TileSeq seq);

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

    
    // ----------  DECK  ----------

    protected void SetDeckElements(int[] fweam) {
        dfire = fweam[0];
        dwater = fweam[1];
        dearth = fweam[2];
        dair = fweam[3];
        dmuscle = fweam[4];
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
            case Ch.Sample:
                return new SampleChar(mm, id);
            case Ch.Enfuego:
                return new Enfuego(mm, id);
            case Ch.Gravekeeper:
                return new Gravekeeper(mm, id);
            case Ch.Valeria:
                return new Valeria(mm, id);

            default:
                Debug.LogError("That character is not currently implemented.");
                return null;
        }
    }
}



public class SampleChar : Character {
    public SampleChar(MageMatch mm, int id) : base(mm, Ch.Sample, id) {
        objFX = mm.hexFX;
    }

    protected override IEnumerator CoreSpell(TileSeq seq) {
        yield return null;
    }

    protected override IEnumerator Spell1(TileSeq seq) {
        yield return null;
    }

    protected override IEnumerator Spell2(TileSeq seq) {
        yield return null;
    }

    protected override IEnumerator Spell3(TileSeq seq) {
        yield return null;
    }

    protected override IEnumerator SignatureSpell(TileSeq seq) {
        yield return null;
    }
}
