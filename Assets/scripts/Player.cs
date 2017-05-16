using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player {

    public int id;
    public string name;
    public Character character;
    public int health;
    public int AP;
    public Transform handSlot;
    public List<TileBehav> hand; // private
    public int handSize = 7;

    private Spell currentSpell;
    private MageMatch mm;
    private MatchEffect matchEffect;

    private float buff_dmgMult = 1;
    private int buff_dmgBonus;
    private int debuff_dmgExtra;

    public Player(int playerNum) {
        AP = 0;
        hand = new List<TileBehav>();
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        id = playerNum;
        SetHandSlot();

        switch (playerNum) {
            case 1:
                name = mm.gameSettings.p1name;
                if (name == "")
                    name = "player 1";
                //handSlot = GameObject.Find("handslot1").transform;
                break;
            case 2:
                name = mm.gameSettings.p2name;
                if (name == "")
                    name = "player 2";
                //handSlot = GameObject.Find("handslot2").transform;
                break;
            default:
                Debug.LogError("PLAYER: Tried to instantiate player with id not 1 or 2!");
                break;
        }

        character = Character.Load(mm, id);
        health = character.GetMaxHealth();
    }

    void SetHandSlot() {
        if (mm.gameSettings.localPlayerOnLeft) {
            if (id == mm.myID)
                handSlot = GameObject.Find("handslot1").transform;
            else
                handSlot = GameObject.Find("handslot2").transform;
        } else {
            if (id == 1)
                handSlot = GameObject.Find("handslot1").transform;
            else
                handSlot = GameObject.Find("handslot2").transform;
        }
    }

    public void InitEvents() {
        character.InitEvents();
        mm.eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.Player);
        mm.eventCont.AddMatchEvent(OnMatch, EventController.Type.Player);
    }

    public IEnumerator OnTurnBegin(int id) {
        if (id == this.id) {
            InitAP();

            if(ThisIsLocal()) // i think?
                DealTile();
        }
        yield return null;
    }

    public IEnumerator OnMatch(int id, string[] seqs) {
        if (id == this.id) { // important!!
            foreach (string seq in seqs) {
                int len = seq.Length;
                int dmg = 0;
                switch (len) {
                    case 3:
                        dmg = Random.Range(30, 50); // diff 20
                        break;
                    case 4:
                        dmg = Random.Range(60, 85); // diff 25
                        break;
                    case 5:
                        dmg = Random.Range(95, 125); // diff 30
                        break;
                }

                //Debug.Log("PLAYER: Player "+id+" about to sync dmg=" + dmg);
                yield return mm.syncManager.SyncRand(id, dmg);
                DealDamage(mm.syncManager.GetRand()); // do the actual damage
            }
        }
    }

    public void DealDamage(int amount) {
        int buffAmount = (int)(amount*buff_dmgMult) + buff_dmgBonus;
        mm.GetOpponent(id).TakeDamage(buffAmount);
    }

    public void TakeDamage(int amount) {
        if (amount > 0) {
            int debuffAmount = amount + debuff_dmgExtra;
            ChangeHealth(-debuffAmount, true);
        } else
            Debug.LogError("PLAYER: Tried to take zero or negative damage...something is wrong.");
    }

    public void Heal(int amount) {
        ChangeHealth(amount, false);
    }

    public void ChangeHealth(int amount, bool dealt) {
        string str = "PLAYER: >>>>>";
        if (amount < 0) { // damage
            if (dealt)
                str += mm.GetOpponent(id).name + " dealt " + (-1 * amount) + " damage; ";
            else
                str += name + " took " + (-1 * amount) + " damage; ";
        } else { // healing
            str += name + " healed for " + amount + " health; ";
        }
        str += name + "'s health changed from " + health + " to " + (health + amount);
        Debug.Log(str);

        health += amount;
        health = Mathf.Clamp(health, 0, character.GetMaxHealth()); // clamp amount before event
        mm.eventCont.PlayerHealthChange(id, amount, dealt);

        if (health == 0)
            mm.EndTheGame();
    }

    // bool again?
    public void DealTile() {
        DrawTiles(1, Tile.Element.None, false, true, false);
    }

    public void DrawTiles(Tile.Element elem) {
        DrawTiles(1, elem, true, false, false);
    }

    public void DrawTiles(int numTiles, Tile.Element elem, bool playerAction, bool dealt, bool linear) {
        for (int i = 0; i < numTiles && !IsHandFull(); i++) {
            GameObject go;
            if (elem == Tile.Element.None)
                go = mm.GenerateTile(character.GetTileElement());
            else
                go = mm.GenerateTile(elem);

            go.transform.position = Camera.main.ScreenToWorldPoint(mm.uiCont.GetPinfo(id).position);
            //if (id == 1)
            //    go.transform.position = new Vector3(-5, 2);
            //else if (id == 2)
            //    go.transform.position = new Vector3(5, 2);

            go.transform.SetParent(handSlot, false);

            TileBehav tb = go.GetComponent<TileBehav>();
            hand.Add(tb);

            mm.eventCont.Draw(id, playerAction, dealt, tb.tile.element);
            if (playerAction)
                mm.eventCont.GameAction(true); //?
        }
        AlignHand(.1f, linear);
    }

    public void AlignHand(float duration, bool linear) {
        if (mm.gameSettings.hideOpponentHand && !ThisIsLocal())
            return;
        mm.animCont.PlayAnim(mm.animCont._AlignHand(this, duration, linear));
    }

    public int DiscardRandom(int count) {
        int tilesInHand = hand.Count;
        int i;
        for (i = 0; i < count; i++) {
            if (tilesInHand > 0) {
                int rand = Random.Range(0, tilesInHand);
                GameObject go = hand[rand].gameObject;
                hand.RemoveAt(rand);
                GameObject.Destroy(go);
            }
        }
        return i;
    }

    // delete?
    //public void FlipHand() {
    //    foreach (TileBehav tb in hand) {
    //        tb.FlipTile();
    //    }
    //}

    public void EmptyHand() {
        while (hand.Count > 0) {
            GameObject.Destroy(hand[0].gameObject);
            hand.RemoveAt(0);
        }
    }

    public bool IsHandFull() { return hand.Count == handSize; }

    public GameObject GetTileFromHand(Tile.Element elem) {
        for (int i = 0; i < hand.Count; i++) {
            if (hand[i].tile.element == elem)
                return hand[i].gameObject;
        }
        return null;
    }

    public bool IsTileMine(TileBehav tb) {
        return tb.transform.parent.position.Equals(handSlot.position);
    }

    public bool ThisIsLocal() { return mm.myID == id; }

    public void InitAP() { AP = 3; }

    public void SetCurrentSpell(int index) {
        currentSpell = character.GetSpell(index);
    }

    public void ApplySpellCosts() {
        Debug.Log("PLAYER: Applying AP cost...which is " + currentSpell.APcost);
        AP -= currentSpell.APcost;
        if (currentSpell is SignatureSpell) {
            int meterCost = ((SignatureSpell)currentSpell).meterCost;
            Debug.Log("PLAYER: Applying meter cost...which is " + meterCost);
            character.ChangeMeter(-meterCost);
        }
        mm.eventCont.GameAction(false);
    }

    public TileSeq GetCurrentBoardSeq() {
        return currentSpell.GetBoardSeq();
    }

    // buff/debuff stuff could be a switch if it's too unwieldy
    public void ChangeBuff_DmgMult(float d) {
        Debug.Log("PLAYER: " + name + " had dmg multiply buff changed to " + d);
        buff_dmgMult = d;
    }

    public void ChangeBuff_DmgBonus(int amount) {
        Debug.Log("PLAYER: " + name + " had dmg bonus buff changed to +" + amount);
        buff_dmgBonus = amount;
    }

    public void ChangeDebuff_DmgExtra(int amount) {
        Debug.Log("PLAYER: " + name + " had dmg extra debuff changed to +" + amount);
        debuff_dmgExtra = amount;
    }
}
