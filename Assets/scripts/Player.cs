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

    private const int INIT_AP = 4;

    private MageMatch _mm;
    //private MatchEffect _matchEffect;

    public Player(int playerNum) {
        AP = 0;
        _mm = GameObject.Find("board").GetComponent<MageMatch>();
        id = playerNum;
        hand = new Hand(_mm, this);

        switch (playerNum) {
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
    }

    public void InitEvents() {
        character.InitEvents();
        _mm.eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.Player);
    }

    // TODO this should really just happen in MM.TurnSystem unless priority is important...
    public IEnumerator OnTurnBegin(int id) {
        if (id == this.id) {
            InitAP();

            if(ThisIsLocal()) // i think?
                yield return DealTile();
        }
        yield return null;
    }

    public IEnumerator DealTile() {
        yield return DrawTiles(1, "", false, true);
    }

    public IEnumerator DealTile(string genTag) {
        yield return DrawTiles(1, genTag, false, true);
    }

    public IEnumerator DrawTiles(int numTiles, string genTag, bool playerAction, bool dealt) {
        MMLog.Log_Player("p" + id + " drawing with genTag=" + genTag);
        for (int i = 0; i < numTiles && !hand.IsFull(); i++) {
            Hex hex;
            if (genTag.Equals("")) {
                hex = _mm.hexMan.GenerateRandomHex(this);
            } else
                hex = _mm.hexMan.GenerateHex(id, genTag);

            if (!ThisIsLocal())
                hex.Flip();

            //hex.transform.position = Camera.main.ScreenToWorldPoint(mm.uiCont.GetPinfo(id).position);

            yield return _mm.eventCont.Draw(EventController.Status.Begin, id, hex.hextag, playerAction, dealt);

            hand.Add(hex);
            // I feel like the draw anim should go here

            yield return _mm.eventCont.Draw(EventController.Status.End, id, hex.hextag, playerAction, dealt);

            if (playerAction)
                _mm.eventCont.GameAction(true); //?
        }
        MMLog.Log_Player(">>>" + hand.NumFullSlots() + " slots filled...");
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
