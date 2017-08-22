using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public class Player {

    public int id;
    public string name;
    public Character character;
    public int health;
    public int AP;
    //public Transform handSlot;
    public Hand hand;
    public int handSize = 7;

    private Spell currentSpell;
    private MageMatch mm;
    private MatchEffect matchEffect;

    private float buff_dmgMult = 1;
    private int buff_dmgBonus;
    private int debuff_dmgExtra;

    public Player(int playerNum) {
        AP = 0;
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        id = playerNum;
        hand = new Hand(mm, this);

        switch (playerNum) {
            case 1:
                name = mm.gameSettings.p1name;
                break;
            case 2:
                name = mm.gameSettings.p2name;
                break;
            default:
                MMLog.LogError("PLAYER: Tried to instantiate player with id not 1 or 2!");
                break;
        }

        character = Character.Load(mm, id);
        health = character.GetMaxHealth();
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
                yield return DealTile();
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

                //Debug.MMLog.Log_Player("PLAYER: Player "+id+" about to sync dmg=" + dmg);
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
            MMLog.LogError("PLAYER: Tried to take zero or negative damage...something is wrong.");
    }

    public void Heal(int amount) {
        ChangeHealth(amount, false);
    }

    public void ChangeHealth(int amount, bool dealt) {
        string str = ">>>>>";
        if (amount < 0) { // damage
            if (dealt)
                str += mm.GetOpponent(id).name + " dealt " + (-1 * amount) + " damage; ";
            else
                str += name + " took " + (-1 * amount) + " damage; ";
        } else { // healing
            str += name + " healed for " + amount + " health; ";
        }
        str += name + "'s health changed from " + health + " to " + (health + amount);
        MMLog.Log_Player(str);

        health += amount;
        health = Mathf.Clamp(health, 0, character.GetMaxHealth()); // clamp amount before event
        mm.eventCont.PlayerHealthChange(id, amount, dealt);

        if (health == 0)
            mm.EndTheGame();
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
            HandObject hex;
            if (genTag.Equals("")) {
                hex = mm.tileMan.GenerateRandomHex(this);
            } else
                hex = mm.tileMan.GenerateHex(id, genTag);

            hex.transform.position = Camera.main.ScreenToWorldPoint(mm.uiCont.GetPinfo(id).position);
            hand.Add(hex);

            yield return mm.eventCont.Draw(id, hex.tag, playerAction, dealt);
            if (playerAction)
                mm.eventCont.GameAction(true); //?
        }
        MMLog.Log_Player(">>>" + hand.NumFullSlots() + " slots filled...");
    }

    public IEnumerator DiscardRandom(int count) {
        for (int i = 0; i < count && hand.Count() > 0; i++) {
            yield return mm.syncManager.SyncRand(id, Random.Range(0, hand.Count()));
            int rand = mm.syncManager.GetRand();
            HandObject hex = hand.GetTile(rand);
            yield return Discard(hex);
        }
    }

    public IEnumerator Discard(HandObject hex) {
        mm.eventCont.Discard(id, hex.tag);

        yield return mm.animCont._DiscardTile(hex.transform);
        hand.Remove(hex);
        GameObject.Destroy(hex.gameObject); //should maybe go thru TileMan
    }

    public IEnumerator Discard(string tag) {
        MMLog.Log_Player("Discarding " + tag);
        yield return Discard(hand.GetHex(tag));

    }

    public bool IsHexMine(HandObject hex) {
        return hex.transform.parent.position.Equals(hand.GetHandPos()); // kinda weird...hand function? compare tags
    }

    public bool ThisIsLocal() { return mm.myID == id; }

    public void InitAP() { AP = 4; }

    public void SetCurrentSpell(int index) {
        currentSpell = character.GetSpell(index);
    }

    public void ApplySpellCosts() {
        MMLog.Log_Player("Applying AP cost...which is " + currentSpell.APcost);
        AP -= currentSpell.APcost;
        if (currentSpell is SignatureSpell) {
            int meterCost = ((SignatureSpell)currentSpell).meterCost;
            MMLog.Log_Player("Applying meter cost...which is " + meterCost);
            character.ChangeMeter(-meterCost);
        }
        mm.eventCont.GameAction(false);
    }

    // buff/debuff stuff could be a switch if it's too unwieldy
    public void ChangeBuff_DmgMult(float d) {
        MMLog.Log_Player(name + " had dmg multiply buff changed to " + d);
        buff_dmgMult = d;
    }

    public void ChangeBuff_DmgBonus(int amount) {
        MMLog.Log_Player(name + " had dmg bonus buff changed to +" + amount);
        buff_dmgBonus = amount;
    }

    public void ChangeDebuff_DmgExtra(int amount) {
        MMLog.Log_Player(name + " had dmg extra debuff changed to +" + amount);
        debuff_dmgExtra = amount;
    }
}
