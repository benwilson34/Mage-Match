using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class Player {

    public const int MAX_AP = 7;

    public int id; // auto
    public string name; // auto
    public Character character;
    public Hand hand;
    public Deck deck;

    private MageMatch _mm;
    private int _ap, _initAP = 1;
    //private MatchEffect _matchEffect;

    public Player(MageMatch mm, int playerId) {
        _ap = 0;
        _mm = mm;
        id = playerId;
        hand = new Hand(_mm, this);

        switch (playerId) {
            case 1:
                name = _mm.gameSettings.p1name;
                break;
            case 2:
                name = _mm.gameSettings.p2name;
                break;
            default:
                MMLog.LogError("PLAYER: Tried to instantiate player with id not 1 or 2!");
                break;
        }

        character = Character.Load(_mm, id);
        deck = new Deck(_mm, this);

        //_mm.AddEventContLoadEvent(OnEventContLoaded);
    }

    //public void OnEventContLoaded() {
    //    character.InitEvents();
    //    _mm.eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.Player);
    //}

    // TODO this should really just happen in MM.TurnSystem unless priority is important...
    public IEnumerator OnTurnBegin() {
        InitAP();
        yield return _mm._Deal(id);
    }

    public IEnumerator DrawHexes(int count, bool playerAction, bool dealt) {
        yield return _mm._Draw(id, count, playerAction, dealt);
    }

    //public bool IsHexMine(Hex hex) {
    //    return hex.transform.parent.position.Equals(hand.GetHandPos()); // kinda weird...hand function? compare tags
    //}

    public bool ThisIsLocal() { return _mm.myID == id; }

    void InitAP() {
        _ap = _initAP;
        _mm.uiCont.UpdateAP(this);

        if (_initAP < MAX_AP)
            _initAP++;
    }

    public void IncreaseAP(int amount = 1) {
        ChangeAP(amount);
        AudioController.Trigger(AudioController.OtherSoundEffect.APGain);
    }

    public void DecreaseAP(int amount = 1) {
        ChangeAP(-1 * amount);
    }

    void ChangeAP(int amount) {
        _ap += amount;
        // TODO clamp
        _mm.uiCont.UpdateAP(this);
    }

    public int GetAP() { return _ap; }

    public bool OutOfAP() { return _ap == 0; }

    public void ApplySpellCosts(Spell spell) {
        bool applyAPcost = true;
        if (_mm.IsDebugMode()) {
            applyAPcost = _mm.debugSettings.applyAPcost;
        }
        if (applyAPcost) {
            MMLog.Log_Player("Applying AP cost...which is " + spell.APcost);
            _ap -= spell.APcost;
            _mm.uiCont.UpdateAP(this);
        }


        if (spell is SignatureSpell) {
            int meterCost = ((SignatureSpell)spell).meterCost;
            MMLog.Log_Player("Applying meter cost...which is " + meterCost);
            character.ChangeMeter(-meterCost);
        }

        _mm.eventCont.GameAction(0);
    }
}
