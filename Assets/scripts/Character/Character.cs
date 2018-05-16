using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public abstract class Character {

    public enum Ch { Neutral = 0, Enfuego, Gravekeeper, Valeria };
    public Ch ch;

    public string characterName;

    public static int METER_MAX = 1000; // may need to be changed later
    protected int _meter = 0;

    public static int HEALTH_WARNING_AMT = 150; // audio/visual warning at 150 hp
    protected int _healthMax;
    protected int _health;

     
    protected int[] _basicDeck; // portions of 50 total
    protected Spell[] _spells;

    protected MageMatch _mm;
    protected HexManager _hexMan;
    protected ObjectEffects _objFX; // needed here?
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

        _mm.AddEffectContLoadEvent(OnEffectContLoad);
        _mm.AddEventContLoadEvent(InitEvents);
        _mm.AddEventContLoadEvent(OnEventContLoad);

        CharacterInfo info = CharacterInfo.GetCharacterInfo(ch);
        characterName = info.name;
        _healthMax = info.health;
        _health = _healthMax;
        //SetDeckElements(info.deck);
        InitSpells(info);
    }

    public virtual void InitEvents() {
        _mm.eventCont.AddDrawEvent(OnDraw, EventController.Type.Player, EventController.Status.Begin);
        _mm.eventCont.AddDropEvent(OnDrop, EventController.Type.Player, EventController.Status.Begin);
        _mm.eventCont.AddSwapEvent(OnSwap, EventController.Type.Player, EventController.Status.Begin);
        _mm.eventCont.AddSpellCastEvent(OnSpellCast, EventController.Type.Player, EventController.Status.Begin);
        _mm.eventCont.playerHealthChange += OnPlayerHealthChange;
        _mm.eventCont.tileRemove += OnTileRemove;
    }

    // override to init character with event callbacks (for their passive, probably)
    public virtual void OnEventContLoad() {}

    // override to init character with effect callbacks (for their passive, probably)
    public virtual void OnEffectContLoad() {}


    // ----------  HEALTH  ----------

    public int GetHealth() { return _health; }

    public int GetMaxHealth() { return _healthMax; }

    public void DealDamage(int amount) {
        //int bonus = CalcBonus(); // TODO displaying the bonus amt in UI would be cool
        _mm.effectCont.ResolveHealthEffects(_playerId, amount, true);
        int bonus = _mm.effectCont.GetHEResult_Additive();
        float mult = _mm.effectCont.GetHEResult_Mult();
        int buffAmount = (int)((amount + bonus) * mult);
        if (bonus != 0 || mult != 1)
            MMLog.Log_Player("BUFF: bonus=" + bonus + ", mult=" + mult + "; amount changed from " + amount + " to " + buffAmount);
        _mm.GetOpponent(_playerId).character.TakeDamage(buffAmount, true);
    }

    public void TakeDamage(int amount, bool dealt) {
        if (amount <= 0) {
            MMLog.LogError("PLAYER: Tried to take zero or negative damage...something is wrong.");
            return;
        }
        _mm.effectCont.ResolveHealthEffects(_playerId, amount, false); // buff/debuff
        int bonus = _mm.effectCont.GetHEResult_Additive();
        float mult = _mm.effectCont.GetHEResult_Mult();
        int debuffAmount = (int)(amount * mult) + bonus;
        if (bonus != 0 || mult != 1)
            MMLog.Log_Player("DEBUFF: bonus=" + bonus + ", mult=" + mult + "; amount changed from " + amount + " to " + debuffAmount);
        ChangeHealth(-debuffAmount, dealt);
    }

    public void SelfDamage(int amount) { TakeDamage(-amount, false); }

    public void Heal(int amount) {
        ChangeHealth(amount, false);
    }

    void ChangeHealth(int amount, bool dealt) {
        string str = ">>>>>";
        Player p = ThisPlayer();
        if (amount < 0) { // damage
            if (dealt)
                str += _mm.GetOpponent(_playerId).name + " dealt " + (-1 * amount) + " damage; ";
            else
                str += p.name + " took " + (-1 * amount) + " damage; ";
        } else { // healing
            str += p.name + " healed for " + amount + " health; ";
        }
        str += p.name + "'s health changed from " + _health + " to " + (_health + amount);
        MMLog.Log("CHAR", "green", str);

        _health += amount;
        _health = Mathf.Clamp(_health, 0, _healthMax); // clamp amount before event
        _mm.eventCont.PlayerHealthChange(_playerId, amount, _health, dealt);

        if (_health == 0)
            _mm.EndTheGame();
    }


    // ----------  METER  ----------

    public int GetMeter() { return _meter; }

    public void ChangeMeter(int amount) {
        _meter += amount;
        _meter = Mathf.Clamp(_meter, 0, METER_MAX); // TODO clamp amount before event
        _mm.eventCont.PlayerMeterChange(_playerId, amount, _meter);

        if (!playedFullMeterSound && _meter == METER_MAX) {
            _mm.audioCont.Trigger(AudioController.OtherSoundEffect.FullMeter);
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

    public IEnumerator OnSpellCast(int id, Spell spell, TileSeq prereq) {
        if (id == _playerId) {
            if (spell is SignatureSpell)
                playedFullMeterSound = false;
            else
                ChangeMeter(10);    
        }
        yield return null;
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

    
    // ----------  SPELLS  ----------

    protected void InitSpells(CharacterInfo info) {
        _spells = new Spell[5];
        _spells[0] = new CoreSpell(0, info.core.title, CoreSpell, info.core.cost);
        _spells[0].Init(_mm);
        _spells[0].info = CharacterInfo.GetSpellInfoString(info.core, true);

        _spells[1] = new Spell(1, info.spell1.title, info.spell1.prereq, Spell1, info.spell1.cost);
        _spells[1].Init(_mm);
        _spells[1].info = CharacterInfo.GetSpellInfoString(info.spell1, true);

        _spells[2] = new Spell(2, info.spell2.title, info.spell2.prereq, Spell2, info.spell2.cost);
        _spells[2].Init(_mm);
        _spells[2].info = CharacterInfo.GetSpellInfoString(info.spell2, true);

        _spells[3] = new Spell(3, info.spell3.title, info.spell3.prereq, Spell3, info.spell3.cost);
        _spells[3].Init(_mm);
        _spells[3].info = CharacterInfo.GetSpellInfoString(info.spell3, true);

        _spells[4] = new SignatureSpell(4, info.signature.title, info.signature.prereq, SignatureSpell, info.signature.cost, info.signature.meterCost);
        _spells[4].Init(_mm);
        _spells[4].info = CharacterInfo.GetSpellInfoString(info.signature, true);

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

    //public List<TileSeq> GetTileSeqList() {
    //    List<TileSeq> outlist = new List<TileSeq>();
    //    foreach (Spell s in _spells)
    //        outlist.Add(s.GetTileSeq());
    //    return outlist;
    //}

    public Player ThisPlayer() {
        return _mm.GetPlayer(_playerId);
    }

    //public string GetHexTag() { return genHexTag; }

    public static Character Load(MageMatch mm, int id) {
        Ch myChar = mm.gameSettings.GetChar(id);
        switch (myChar) {
            case Ch.Neutral:
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
    public SampleChar(MageMatch mm, int id) : base(mm, Ch.Neutral, id) {
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
