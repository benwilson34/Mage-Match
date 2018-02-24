using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Character {

    public enum Ch { Sample = 0, Enfuego, Gravekeeper, Valeria };
    public Ch ch;

    public string characterName;

    public static int METER_MAX = 1000; // may need to be changed later
    protected int _meter = 0;

    public static int HEALTH_WARNING_AMT = 150; // audio/visual warning at 150 hp
    protected int _maxHealth;
    protected ObjectEffects _objFX; // needed here?

    protected static int DECK_BASIC_COUNT = 50; 
    protected int[] _basicDeck; // portions of 50 total
    protected Spell[] _spells;
    protected MageMatch _mm;
    protected HexManager _hexMan;
    protected int _playerId;
    protected List<string> _runes;
    //protected string genHexTag;

    protected bool playedFullMeterSound = false;

    public Character(MageMatch mm, Ch ch, int playerId) {
        this._mm = mm;
        this.ch = ch;
        this._playerId = playerId;
        _hexMan = mm.hexMan;
        _runes = new List<string>();

        mm.onEffectContReady += OnEffectContLoad;
        mm.onEventContReady += OnEventContLoad;

        CharacterInfo info = CharacterInfo.GetCharacterInfoObj(ch);
        characterName = info.name;
        _maxHealth = info.health;
        SetDeckElements(info.deck);
        InitSpells(info);
    }

    public virtual void InitEvents() {
        _mm.eventCont.AddDrawEvent(OnDraw, EventController.Type.Player, EventController.Status.Begin);
        _mm.eventCont.AddDropEvent(OnDrop, EventController.Type.Player, EventController.Status.Begin);
        _mm.eventCont.AddSwapEvent(OnSwap, EventController.Type.Player, EventController.Status.Begin);
        _mm.eventCont.spellCast += OnSpellCast;
        _mm.eventCont.playerHealthChange += OnPlayerHealthChange;
        _mm.eventCont.tileRemove += OnTileRemove;
    }

    // override to init character with event callbacks (for their passive, probably)
    public virtual void OnEventContLoad() {}

    // override to init character with effect callbacks (for their passive, probably)
    public virtual void OnEffectContLoad() {}


    // ----------  METER  ----------

    public int GetMeter() { return _meter; }

    public void ChangeMeter(int amount) {
        _meter += amount;
        _meter = Mathf.Clamp(_meter, 0, METER_MAX); // TODO clamp amount before event
        _mm.eventCont.PlayerMeterChange(_playerId, amount, _meter);

        if (!playedFullMeterSound && _meter == METER_MAX) {
            _mm.audioCont.FullMeter();
            playedFullMeterSound = true;
        }
    }

    public IEnumerator OnDraw(int id, string tag, bool playerAction, bool dealt) {
        if(id == _playerId && !dealt)
            ChangeMeter(10);
        yield return null;
    }

    public IEnumerator OnDrop(int id, bool playerAction, string tag, int col) {
        if(id == _playerId)
            ChangeMeter(10);
        yield return null;
    }

    public IEnumerator OnSwap(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        if(id == _playerId)
            ChangeMeter(10);
        yield return null;
    }

    public void OnSpellCast(int id, Spell spell) {
        if (id == _playerId) {
            if (spell is SignatureSpell)
                playedFullMeterSound = false;
            else
                ChangeMeter(10);    
        }
    }

    public void OnPlayerHealthChange(int id, int amount, int newHealth, bool dealt) {
        if (dealt && id != _playerId) // if the other player was dealt dmg (not great)
            ChangeMeter(-amount);

        if (amount > 0 && id == _playerId) // healing
            ChangeMeter(amount / 2);
    }

    public void OnTileRemove(int id, TileBehav tb) {
        if (id == _playerId)
            ChangeMeter(5);
    }


    public int GetMaxHealth() { return _maxHealth; }

    
    // ----------  SPELLS  ----------

    protected void InitSpells(CharacterInfo info) {
        _spells = new Spell[5];
        Debug.Log("Core spell has a cost of " + info.core.cost);
        _spells[0] = new CoreSpell(0, info.core.title, CoreSpell, info.core.cost);
        _spells[0].Init(_mm);
        _spells[0].info = CharacterInfo.GetSpellInfo(info.core, true);

        _spells[1] = new Spell(1, info.spell1.title, info.spell1.prereq, Spell1, info.spell1.cost);
        _spells[1].Init(_mm);
        _spells[1].info = CharacterInfo.GetSpellInfo(info.spell1, true);

        _spells[2] = new Spell(2, info.spell2.title, info.spell2.prereq, Spell2, info.spell2.cost);
        _spells[2].Init(_mm);
        _spells[2].info = CharacterInfo.GetSpellInfo(info.spell2, true);

        _spells[3] = new Spell(3, info.spell3.title, info.spell3.prereq, Spell3, info.spell3.cost);
        _spells[3].Init(_mm);
        _spells[3].info = CharacterInfo.GetSpellInfo(info.spell3, true);

        _spells[4] = new SignatureSpell(4, info.signature.title, info.signature.prereq, SignatureSpell, info.signature.cost, info.signature.meterCost);
        _spells[4].Init(_mm);
        _spells[4].info = CharacterInfo.GetSpellInfo(info.signature, true);

    }

    protected abstract IEnumerator CoreSpell(TileSeq seq);
    protected abstract IEnumerator Spell1(TileSeq seq);
    protected abstract IEnumerator Spell2(TileSeq seq);
    protected abstract IEnumerator Spell3(TileSeq seq);
    protected abstract IEnumerator SignatureSpell(TileSeq seq);

    public Spell GetSpell(int index) { return _spells[index]; }

    public List<Spell> GetSpells() {
        return new List<Spell>(_spells);
    }

    public List<TileSeq> GetTileSeqList() {
        List<TileSeq> outlist = new List<TileSeq>();
        foreach (Spell s in _spells)
            outlist.Add(s.GetTileSeq());
        return outlist;
    }

    
    // ----------  DECK  ----------

    protected void SetDeckElements(int[] fweam) {
        _basicDeck = fweam;
    }

    // eventually we will need to generate a deck so this won't be needed.
    public Tile.Element GetDeckBasicTile() {
        int rand = Random.Range(0, DECK_BASIC_COUNT);

        for (int e = 1; e <= 5; e++) {
            int threshold = 0;
            for (int i = 0; i < e; i++)
                threshold += _basicDeck[i];
            if (rand < threshold)
                return (Tile.Element)e;
        }
        MMDebug.MMLog.LogError("CHARACTER: Generating a tile from the deck didn't work?");
        return Tile.Element.None;
    }

    public string GenerateHexTag() {
        int total = 50 + (_runes.Count * 10);
        int rand = Random.Range(0, total);

        if (rand < 50)
            return "p" + _playerId + "-B-" + GetDeckBasicTile().ToString().Substring(0, 1); // + "-" ?
        else {
            int index = Mathf.CeilToInt((rand - 50) / 10f); 
            return _runes[index]; // + "-" ?
        }
    }

    public Player ThisPlayer() {
        return _mm.GetPlayer(_playerId);
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
        _objFX = mm.hexFX;
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
