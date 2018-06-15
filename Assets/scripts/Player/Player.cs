using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class Player {

    public const int MAX_AP = 7;

    public int ID { get { return _id; } }
    private int _id;

    public string Name { get { return _name; } }
    private string _name;

    public Character Character { get { return _character; } }
    private Character _character;

    public Deck Deck { get { return _deck; } }
    private Deck _deck;

    public Hand Hand { get { return _hand; } }
    private Hand _hand;

    private MageMatch _mm;
    private int _ap, _initAP = 1;
    public int AP { get { return _ap; } }
    public bool IsOutOfAP { get { return _ap == 0; } }

    public Player(MageMatch mm, int playerId) {
        _ap = 0;
        _mm = mm;
        _id = playerId;
        _hand = new Hand(_mm, this);

        switch (playerId) {
            case 1:
                _name = _mm.gameSettings.p1name;
                break;
            case 2:
                _name = _mm.gameSettings.p2name;
                break;
            default:
                MMLog.LogError("PLAYER: Tried to instantiate player with id not 1 or 2!");
                break;
        }

        _character = Character.Load(_mm, ID);
        _deck = new Deck(_mm, this);

        //_mm.AddEventContLoadEvent(OnEventContLoaded);
    }

    //public void OnEventContLoaded() {
    //    character.InitEvents();
    //    EventController.AddTurnBeginEvent(OnTurnBegin, EventController.Type.Player);
    //}

    // TODO this should really just happen in MM.TurnSystem unless priority is important...
    public IEnumerator OnTurnBegin() {
        InitAP();
        yield return _mm._Deal(_id);
    }

    // just for the convenience of calling from effects
    public IEnumerator DrawHexes(int count) {
        yield return _mm._Draw(ID, count, EventController.HandChangeState.DrawFromEffect);
    }

    //public bool IsHexMine(Hex hex) {
    //    return hex.transform.parent.position.Equals(hand.GetHandPos()); // kinda weird...hand function? compare tags
    //}

    //public bool ThisIsLocal() { return _mm.myID == ID; }

    void InitAP() {
        _ap += _initAP;
        _mm.uiCont.UpdateAP(this);

        if (_initAP < MAX_AP)
            _initAP++;
    }

    public void IncreaseAP(int amount = 1) {
        ChangeAP(amount);
        AudioController.Trigger(SFX.Other.APGain);
    }

    public void DecreaseAP(int amount = 1) {
        ChangeAP(-1 * amount);
    }

    void ChangeAP(int amount) {
        _ap += amount;
        // TODO clamp
        _mm.uiCont.UpdateAP(this);
    }
}
