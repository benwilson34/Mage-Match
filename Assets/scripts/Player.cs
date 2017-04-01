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
    private int buff_dmgExtra;

    public Player(int playerNum) {
        AP = 0;
        hand = new List<TileBehav>();
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        id = playerNum;

        switch (playerNum) {
            case 1:
                name = mm.gameSettings.p1name;
                if (name == "")
                    name = "player 1";
                handSlot = GameObject.Find("handslot1").transform;
                break;
            case 2:
                name = mm.gameSettings.p2name;
                if (name == "")
                    name = "player 2";
                handSlot = GameObject.Find("handslot2").transform;
                break;
            default:
                Debug.LogError("PLAYER: Tried to instantiate player with id not 1 or 2!");
                break;
        }

        character = Character.Load(mm, id);
        health = character.GetMaxHealth();
    }

    public void InitEvents() {
        mm.eventCont.turnBegin += OnTurnBegin;
        mm.eventCont.match += OnMatch;
    }

    public void OnTurnBegin(int id) {
        if (id == this.id) {
            InitAP();

            if(ThisIsLocal()) // i think?
                DealTile();

            mm.uiCont.UpdatePlayerInfo(); // not needed once events have priority...?
        }
    }

    public void OnMatch(int id, string[] seqs) {
        if (id == this.id) {
            foreach (string seq in seqs) {
                int len = seq.Length;
                switch (len) {
                    case 3:
                        DealDamage(Random.Range(30, 50), false); // diff 20
                        break;
                    case 4:
                        DealDamage(Random.Range(60, 85), false); // diff 25
                        break;
                    case 5:
                        DealDamage(Random.Range(95, 125), false); // diff 30
                        break;
                }
            }
        }
    }

    public void DealDamage(int amount, bool sent) {
        Debug.Log("PLAYER: >>>Dealing " + (-amount) + "dmg to p" + id);
        if (ThisIsLocal() || sent) {
            mm.GetOpponent(id).ChangeHealth(-amount, true, sent);
            character.ChangeMeter(amount / 3); // TODO tentative
        }
    }

    public void ChangeHealth(int amount) {
        ChangeHealth(amount, false, false);
    }

    public void ChangeHealth(int amount, bool dealt, bool sent) {
        if (ThisIsLocal() || dealt) {
            int newAmount = 0;
            if (amount < 0) { // damage
                newAmount = (int)(amount * buff_dmgMult) - buff_dmgExtra;
            } else { } // healing?

            Debug.Log("PLAYER: " + mm.GetOpponent(id).name + " dealt " + (-1 * newAmount) + " damage; " + name + "'s health changed from " + health + " to " + (health + newAmount));
            health += newAmount;
            health = Mathf.Clamp(health, 0, character.GetMaxHealth());
            mm.eventCont.PlayerHealthChange(id, amount, dealt, sent);

            if (health == 0)
                mm.EndTheGame();
        }
    }

    public bool DealTile() {
        return DrawTiles(1, Tile.Element.None, true, false) != null;
    }

    public Tile.Element[] DrawTiles(Tile.Element elem) {
        return DrawTiles(1, elem, false, false);
    }

    // this return type isn't really necessary anymore due to the drawEvent
    public Tile.Element[] DrawTiles(int numTiles, Tile.Element elem, bool dealt, bool linear) {
        Tile.Element[] tileElems = new Tile.Element[numTiles];
        for (int i = 0; i < numTiles && !IsHandFull(); i++) {
            GameObject go;
            if (elem == Tile.Element.None)
                go = mm.GenerateTile(character.GetTileElement());
            else
                go = mm.GenerateTile(elem);

            if (id == 1)
                go.transform.position = new Vector3(-5, 2);
            else if (id == 2)
                go.transform.position = new Vector3(5, 2);

            go.transform.SetParent(handSlot, false);

            TileBehav tb = go.GetComponent<TileBehav>();
            tileElems[i] = tb.tile.element;
            hand.Add(tb);

            mm.eventCont.Draw(id, tb.tile.element, dealt);
            if (!dealt)
                mm.eventCont.GameAction(true); //?
        }
        AlignHand(.1f, linear);

        return tileElems;
    }

    public void AlignHand(float duration, bool linear) {
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
    public void FlipHand() {
        foreach (TileBehav tb in hand) {
            tb.FlipTile();
        }
    }

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

    public void InitAP() {
        AP = 3;
    }

    public bool CastSpell(int index) { // TODO
        Spell spell = character.GetSpell(index);
        if (AP >= spell.APcost) {
            currentSpell = spell;
            mm.eventCont.SpellCast(currentSpell); // here?
            mm.StartCoroutine(spell.Cast());
            return true;
        } else
            return false;
    }

    public void ApplyAPCost() {
        Debug.Log("PLAYER: Applying AP cost...which is " + currentSpell.APcost);
        AP -= currentSpell.APcost;
        mm.eventCont.GameAction(false);
    }

    public TileSeq GetCurrentBoardSeq() {
        return currentSpell.GetBoardSeq();
    }

    public void ChangeBuff_DmgMult(float d) {
        Debug.Log("PLAYER: " + name + " had dmg multiply buff changed to " + d);
        buff_dmgMult = d;
    }

    public void ChangeBuff_DmgExtra(int amount) {
        Debug.Log("PLAYER: " + name + " had dmg bonus buff changed to +" + amount);
        buff_dmgExtra = amount;
    }
}
