using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class Player {

    public const int MAX_AP = 7;

    public int id; // auto
    public string name; // auto
    public int AP; // private w/ methods
    public Character character;
    public Hand hand;
    public Deck deck;

    private const int INIT_AP = 4;

    private MageMatch _mm;
    //private MatchEffect _matchEffect;

    public Player(int playerId) {
        AP = 0;
        _mm = GameObject.Find("board").GetComponent<MageMatch>();
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
    }

    public void InitEvents() {
        character.InitEvents();
        _mm.eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.Player);
    }

    // TODO this should really just happen in MM.TurnSystem unless priority is important...
    public IEnumerator OnTurnBegin(int id) {
        if (id == this.id) { // only the active player
            InitAP();

            yield return _mm._Deal(id);
        }
        yield return null;
    }

    public IEnumerator DrawHexes(int count, bool playerAction, bool dealt) {
        yield return _mm._Draw(id, count, playerAction, dealt);
    }

    public IEnumerator DiscardRandom(int count) {
        for (int i = 0; i < count && hand.Count() > 0; i++) {
            yield return _mm.syncManager.SyncRand(id, Random.Range(0, hand.Count()));
            int rand = _mm.syncManager.GetRand();
            Hex hex = hand.GetTile(rand);
            yield return Discard(hex);
        }
    }

    public IEnumerator Discard(Hex hex) {
        _mm.eventCont.Discard(id, hex.hextag);

        _mm.audioCont.HexDiscard();
        yield return _mm.animCont._DiscardTile(hex.transform);
        hand.Remove(hex);
        GameObject.Destroy(hex.gameObject); //should maybe go thru TileMan
    }

    public IEnumerator Discard(string tag) {
        MMLog.Log_Player("Discarding " + tag);
        yield return Discard(hand.GetHex(tag));

    }

    public bool IsHexMine(Hex hex) {
        return hex.transform.parent.position.Equals(hand.GetHandPos()); // kinda weird...hand function? compare tags
    }

    public bool ThisIsLocal() { return _mm.myID == id; }

    public void InitAP() { AP = INIT_AP; }

    public void ApplySpellCosts(Spell spell) {
        bool applyAPcost = true;
        if (_mm.IsDebugMode()) {
            applyAPcost = _mm.debugSettings.applyAPcost;
        }
        if (applyAPcost) {
            MMLog.Log_Player("Applying AP cost...which is " + spell.APcost);
            AP -= spell.APcost;
        }


        if (spell is SignatureSpell) {
            int meterCost = ((SignatureSpell)spell).meterCost;
            MMLog.Log_Player("Applying meter cost...which is " + meterCost);
            character.ChangeMeter(-meterCost);
        }

        _mm.eventCont.GameAction(false);
    }
}
