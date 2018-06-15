using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class MagicAl : Character {

    private BitArray _passive_spellsCastThisTurn;

    private enum MagicAlSpell { Jab, Cross, Hook, StingerStance, Flutterfly, SkyUppercut, StormForceFootwork };
    private Spell[] _altMatchSpells;

    private bool _sig_active = false;
    private string _sig_turnEffectTag, _sig_healthEffectTag;
    private int _sig_spellsCastThisTurn = 0;

    public MagicAl(MageMatch mm, int id) : base(mm, Ch.MagicAl, id) {
        const int numSpells = 7;
        _passive_spellsCastThisTurn = new BitArray(numSpells, false);

        // load alternate match spells
        CharacterInfo info = CharacterInfo.GetCharacterInfo(Ch.MagicAl);
        _altMatchSpells = new Spell[3];
        _altMatchSpells[0] = _spells[0]; // Jab
        _altMatchSpells[1] = new MatchSpell(0, "Cross", Cross);
        _altMatchSpells[1].info = CharacterInfo.GetSpellInfoString(info.altSpells[0], true);
        _altMatchSpells[2] = new MatchSpell(0, "Hook", Hook);
        _altMatchSpells[2].info = CharacterInfo.GetSpellInfoString(info.altSpells[1], true);
    }


    #region ---------- PASSIVE ----------

    public override void OnEffectControllerLoad() {
        TurnEffect te = new TurnEndEffect(_playerId, "MagicAl_Passive", Effect.Behav.Damage, Passive_OnTurnEnd);
        EffectManager.AddEventEffect(te);
    }

    public IEnumerator Passive_OnTurnEnd(int id) {
        int uniqueCount = 0;
        foreach (bool b in _passive_spellsCastThisTurn)
            if (b) uniqueCount++;

        if (uniqueCount > 0) {
            const int damagePerSpell = 10;
            DealDamage(uniqueCount * damagePerSpell);
        }

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

        yield return Targeting.WaitForTileTarget(returnCount);
        var tbs = Targeting.GetTargetTBs();
        foreach (var tb in tbs) {
            yield return CommonEffects.BounceToHand(tb, Opponent);
        }

        SetSpellCast(MagicAlSpell.Jab);
        ChangeMatchSpell(MagicAlSpell.Cross);

        yield return null;
    }

    // alt core spell - Cross
    public IEnumerator Cross(TileSeq seq) {
        //AudioController.Trigger(SFX.GravekeeperSFX.PartyInTheBack);

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

        yield return Opponent.Hand._DiscardRandom(discardCount);

        SetSpellCast(MagicAlSpell.Cross);
        ChangeMatchSpell(MagicAlSpell.Hook);

        yield return null;
    }

    // alt core spell - Hook
    public IEnumerator Hook(TileSeq seq) {
        //AudioController.Trigger(SFX.GravekeeperSFX.PartyInTheBack);

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
            _mm.uiCont.ShowAlertText("Empty swapping isn't supported yet. Sorry!");
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
        if(_sig_active)
            _mm.StartCoroutine(_OnSpellCast());
    }

    // Stinger Stance
    protected override IEnumerator Spell1(TileSeq prereq) {
        //AudioController.Trigger(SFX.GravekeeperSFX.OogieBoogie);

        TurnEffect effect = new TurnEndEffect(_playerId, "StingerStance", Effect.Behav.Damage, Stinger_OnTurnEnd) { turnsLeft = 1 };
        EffectManager.AddEventEffect(effect);

        SetSpellCast(MagicAlSpell.StingerStance);
        ChangeMatchSpell(MagicAlSpell.Jab);

        yield return null;
    }
    IEnumerator Stinger_OnTurnEnd(int id) {
        int dmg = 40;
        const int damagePerHex = 15;
        dmg += Opponent.Hand.Count * damagePerHex;
        DealDamage(dmg);
        yield return null;
    }

    // Flutterfly
    protected override IEnumerator Spell2(TileSeq prereq) {
        //AudioController.Trigger(SFX.GravekeeperSFX.PartyCrashers);

        yield return Prompt.WaitForSwap();
        if (!Prompt.WasSuccessful)
            yield break;

        var bounceTB = Prompt.GetSwapTBs()[1];
        yield return CommonEffects.BounceToHand(bounceTB, Opponent);

        HealthModEffect he = new HealthModEffect(_playerId, "Flutterfly", Flutterfly_Buff, HealthModEffect.Type.DealingPercent) { turnsLeft = 3, countLeft = 1 };
        EffectManager.AddHealthMod(he);

        SetSpellCast(MagicAlSpell.Flutterfly);
        ChangeMatchSpell(MagicAlSpell.Cross);

        yield return null;
    }
    float Flutterfly_Buff(Player p, int dmg) {
        const float morePercent = .08f;
        return 1 + morePercent; // +8% dmg
    }

    // Sky Uppercut
    protected override IEnumerator Spell3(TileSeq prereq) {
        //AudioController.Trigger(SFX.GravekeeperSFX.UndeadUnion);

        yield return Targeting.WaitForCellTarget(1);
        var cbs = Targeting.GetTargetCBs();
        if (cbs.Count != 1)
            yield break;

        var cb = cbs[0];
        var tbs = HexGrid.GetTilesInCol(cb.col);

        yield return CommonEffects.ShootIntoAirAndRearrange(tbs);

        const int dmg = 110;
        DealDamage(dmg);

        SetSpellCast(MagicAlSpell.SkyUppercut);
        ChangeMatchSpell(MagicAlSpell.Hook);

        yield return null;
    }

    // Storm Force Footwork
    protected override IEnumerator SignatureSpell(TileSeq prereq) {
        //AudioController.Trigger(SFX.GravekeeperSFX.SigBell1);

        const int turnCount = 5;

        _sig_active = true;
        _sig_spellsCastThisTurn = 0;
        TurnEffect te = new TurnEndEffect(_playerId, "MagicAl_SFFCount", Effect.Behav.TickDown, null) { turnsLeft = turnCount, onEndEffect = Sig_OnEffectEnd };
        _sig_turnEffectTag = te.tag;
        EffectManager.AddEventEffect(te);

        HealthModEffect he = new HealthModEffect(_playerId, "MagicAl_SFFBuff", Sig_Buff, HealthModEffect.Type.ReceivingPercent) { turnsLeft = turnCount };
        _sig_healthEffectTag = he.tag;
        EffectManager.AddHealthMod(he);

        SetSpellCast(MagicAlSpell.StormForceFootwork);

        yield return null;
    }
    IEnumerator Sig_OnEffectEnd() {
        _sig_active = false;
        yield return null;
    }
    float Sig_Buff(Player p, int dmg) {
        const float lessPercent = .25f;
        return 1 - lessPercent; // -25% damage
    }

    IEnumerator _OnSpellCast() {
        _sig_spellsCastThisTurn++;

        const int APgainAmt = 3;
        if (_sig_spellsCastThisTurn == APgainAmt)
            ThisPlayer.IncreaseAP();

        const int massiveEffectAmt = 6, massiveDmg = 280;
        if (_sig_spellsCastThisTurn == massiveEffectAmt) {
            EffectManager.RemoveEventEffect(_sig_turnEffectTag); // safe?
            EffectManager.RemoveHealthMod(_sig_healthEffectTag); // safe?

            DealDamage(massiveDmg);

            var tbs = HexGrid.GetPlacedTiles();
            int bounceCount = 7;
            bounceCount = Mathf.Max(tbs.Count, bounceCount);
            if (bounceCount == 0)
                yield break;

            int[] rands = CommonEffects.GetRandomInds(tbs.Count, bounceCount);
            yield return _mm.syncManager.SyncRands(_playerId, rands);
            rands = _mm.syncManager.GetRands(bounceCount);

            IEnumerator lastBounceAnim = null;
            Player opp = Opponent;
            for (int i = 0; i < bounceCount; i++) {
                lastBounceAnim = CommonEffects.BounceToHand(tbs[rands[i]], opp);
                if (i != bounceCount - 1) {
                    _mm.StartCoroutine(lastBounceAnim);
                    yield return new WaitForSeconds(.1f);
                } else {
                    yield return lastBounceAnim;
                }
            }
        }

        yield return null;
    }
    #endregion

}
