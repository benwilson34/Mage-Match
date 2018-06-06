using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class MagicAl : Character {

    private BitArray _passive_spellsCastThisTurn;

    private enum MagicAlSpell { Jab, Cross, Hook, StingerStance, Flutterfly, SkyUppercut, StormForceFootwork };
    private Spell[] _altMatchSpells;

    private bool _sig_active = false;
    private int _sig_spellsCastThisTurn = 0;

    public MagicAl(MageMatch mm, int id) : base(mm, Ch.MagicAl, id) {
        _objFX = mm.hexFX;

        const int numSpells = 7;
        _passive_spellsCastThisTurn = new BitArray(numSpells, false);

        // load alternate match spells
        CharacterInfo info = CharacterInfo.GetCharacterInfo(Ch.MagicAl);
        _altMatchSpells = new Spell[3];
        _altMatchSpells[0] = _spells[0]; // Jab
        _altMatchSpells[1] = new MatchSpell(0, "Cross", Cross);
        _altMatchSpells[1].info = CharacterInfo.GetSpellInfoString(info.altSpells[1], true);
        _altMatchSpells[2] = new MatchSpell(0, "Hook", Hook);
        _altMatchSpells[2].info = CharacterInfo.GetSpellInfoString(info.altSpells[2], true);
    }


    #region ---------- PASSIVE ----------

    public override void OnEffectControllerLoad() {
        TurnEffect te = new TurnEffect(_playerId, Effect.Type.Damage, Passive_OnTurnEnd);
        EffectController.AddEndTurnEffect(te, "MagicAl_Passive");
    }

    public IEnumerator Passive_OnTurnEnd(int id) {
        int uniqueCount = 0;
        foreach (bool b in _passive_spellsCastThisTurn)
            if (b) uniqueCount++;

        const int damagePerSpell = 10;
        DealDamage(uniqueCount * damagePerSpell);

        _passive_spellsCastThisTurn.SetAll(false);
        yield return null;
    }
    #endregion


    #region ---------- SPELLS ----------

    // Jab
    protected override IEnumerator MatchSpell(TileSeq seq) {
        int dmg = 0, returnCount = 0;
        switch (seq.GetSeqLength()) {
            case 3:
                dmg = 20;
                returnCount = 1;
                break;
            case 4:
                dmg = 40;
                returnCount = 2;
                break;
            case 5:
                dmg = 70;
                returnCount = 3;
                break;
        }

        DealDamage(dmg);

        //List<TileBehav> tbs = _hexGrid.GetPlacedTiles(seq);
        yield return Targeting.WaitForTileTarget(returnCount);
        var tbs = Targeting.GetTargetTBs();
        foreach (var tb in tbs) {
            // TODO add to hand?
        }

        SetSpellCast(MagicAlSpell.Jab);
        ChangeMatchSpell(MagicAlSpell.Cross);

        yield return null;
    }

    // alt core spell - Cross
    public IEnumerator Cross(TileSeq seq) {
        //AudioController.Trigger(AudioController.GravekeeperSFX.PartyInTheBack);

        int dmg = 0, discardCount = 0;
        switch (seq.GetSeqLength()) {
            case 3:
                dmg = 40;
                break;
            case 4:
                dmg = 60;
                discardCount = 1;
                break;
            case 5:
                dmg = 90;
                discardCount = 2;
                break;
        }

        DealDamage(dmg);

        yield return Opponent.hand._DiscardRandom(discardCount);
        //for (int i = 0; i < discardCount; i++) {
        //}

        SetSpellCast(MagicAlSpell.Cross);
        ChangeMatchSpell(MagicAlSpell.Hook);

        yield return null;
    }

    // alt core spell - Hook
    public IEnumerator Hook(TileSeq seq) {
        //AudioController.Trigger(AudioController.GravekeeperSFX.PartyInTheBack);

        int dmg = 0, swapCount = 0;
        switch (seq.GetSeqLength()) {
            case 3:
                dmg = 10;
                swapCount = 1;
                break;
            case 4:
                dmg = 40;
                swapCount = 2;
                break;
            case 5:
                dmg = 80;
                swapCount = 3;
                break;
        }

        DealDamage(dmg);

        for (int i = 0; i < swapCount; i++) {
            // TODO allow swapping into open space
            yield return Prompt.WaitForSwap();
            if (Prompt.WasSuccessful)
                Prompt.ContinueSwap();
        }

        SetSpellCast(MagicAlSpell.Hook);
        ChangeMatchSpell(MagicAlSpell.Jab);

        yield return null;
    }

    void ChangeMatchSpell(MagicAlSpell spell) {
        int index = (int)spell;
        if (index >= 3) {
            MMLog.LogError("MAGIC AL: Tried to switch match spell to something besides the first three!");
            return;
        }

        _spells[0] = _altMatchSpells[index];
        MMLog.Log_Gravekeeper("MAGIC AL: Switching core spell..." + _spells[0].info);
        _mm.uiCont.GetButtonCont(_playerId, 0).SpellChanged();
    }

    void SetSpellCast(MagicAlSpell spell) {
        _passive_spellsCastThisTurn[(int)spell] = true;
        _mm.StartCoroutine(_OnSpellCast());
    }

    // Stinger Stance
    protected override IEnumerator Spell1(TileSeq prereq) {
        //AudioController.Trigger(AudioController.GravekeeperSFX.OogieBoogie);

        TurnEffect te = new TurnEffect(_playerId, Effect.Type.Damage, Stinger_OnTurnEnd);
        EffectController.AddEndTurnEffect(te, "Stinger");

        SetSpellCast(MagicAlSpell.StingerStance);
        ChangeMatchSpell(MagicAlSpell.Jab);

        yield return null;
    }
    IEnumerator Stinger_OnTurnEnd(int id) {
        int dmg = 40;
        const int damagePerHex = 15;
        dmg += Opponent.hand.Count * damagePerHex;
        DealDamage(dmg);
        yield return null;
    }

    // Flutterfly
    protected override IEnumerator Spell2(TileSeq prereq) {
        //AudioController.Trigger(AudioController.GravekeeperSFX.PartyCrashers);

        yield return Prompt.WaitForSwap();
        if (!Prompt.WasSuccessful)
            yield break;

        var bounceTB = Prompt.GetSwapTBs()[1];
        // TODO add bounceTB to hand

        // TODO stacking buff that gives next instance of damage +8%, lasts three turns

        SetSpellCast(MagicAlSpell.Flutterfly);
        ChangeMatchSpell(MagicAlSpell.Cross);

        yield return null;
    }

    // Sky Uppercut
    protected override IEnumerator Spell3(TileSeq prereq) {
        //AudioController.Trigger(AudioController.GravekeeperSFX.UndeadUnion);

        yield return Targeting.WaitForCellTarget(1);
        var cbs = Targeting.GetTargetCBs();
        if (cbs.Count != 1)
            yield break;

        var cb = cbs[0];
        var tbs = HexGrid.GetTilesInCol(cb.col);

        // TODO blast tbs up off of board and drop back down in random cols

        const int dmg = 110;
        DealDamage(dmg);

        SetSpellCast(MagicAlSpell.SkyUppercut);
        ChangeMatchSpell(MagicAlSpell.Hook);

        yield return null;
    }

    // Storm Force Footwork
    protected override IEnumerator SignatureSpell(TileSeq prereq) {
        //AudioController.Trigger(AudioController.GravekeeperSFX.SigBell1);

        const int turnCount = 5;

        _sig_active = true;
        _sig_spellsCastThisTurn = 0;
        TurnEffect te = new TurnEffect(_playerId, Effect.Type.None, null, Sig_OnEffectEnd, turnCount);
        EffectController.AddEndTurnEffect(te, "MagicAl_SFFCount");

        HealthModEffect he = new HealthModEffect(_playerId, Sig_Buff, false, false, turnCount);
        EffectController.AddHealthEffect(he, "MagicAl_SFFBuff");

        SetSpellCast(MagicAlSpell.StormForceFootwork);

        yield return null;
    }
    IEnumerator Sig_OnEffectEnd(int id) {
        _sig_active = false;
        yield return null;
    }
    float Sig_Buff(Player p, int dmg) {
        return .75f; // -25% damage
    }

    IEnumerator _OnSpellCast() {
        _sig_spellsCastThisTurn++;

        const int APgainAmt = 3;
        if (_sig_spellsCastThisTurn == APgainAmt)
            ThisPlayer.IncreaseAP();

        const int massiveEffectAmt = 6, massiveDmg = 280;
        if (_sig_spellsCastThisTurn == massiveEffectAmt) {
            // TODO remove turnEffect and buff

            DealDamage(massiveDmg);

            var tbs = HexGrid.GetPlacedTiles();
            const int bounceCount = 7;
            for (int i = 0; i < bounceCount; i++) {
                // TODO blast random tiles into opponent hand
            }
        }

        yield return null;
    }
    #endregion

}
