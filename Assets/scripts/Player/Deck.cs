using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck {

    private static int DECK_BASIC_COUNT = 50;

    private Player _player;
    private Queue<string> _deckQ;
    private List<string> _removeList;

    public Deck(Player p) {
        _player = p;
        _removeList = new List<string>();
        Init();
    }

    public void Init() {
        MMDebug.MMLog.Log("DECK", "black", "Init player " + _player.id);
        var list = new List<string>();
        var info = CharacterInfo.GetCharacterInfoObj(_player.character.ch);
        for(int d = 0; d < 5; d++) {
            Tile.Element elem = (Tile.Element)(d + 1);
            for (int i = 0; i < info.deck[d]; i++) {
                list.Add("p"+_player.id+"-B-"+elem.ToString().Substring(0, 1));
            }
        }

        foreach (string rune in info.runes) {
            var runeInfo = RuneInfo.GetRuneInfo(rune);
            if (runeInfo.type == "Consumable") {
                for (int i = 0; i < runeInfo.deckCount; i++) {
                    list.Add("p" + _player.id + "-C-" + runeInfo.title.Substring(0, 5)); // how long?
                }
            }
        }

        Shuffle(list.ToArray());
        PrintDeck();
    }

    void Shuffle(string[] coll = null) {
        if (coll == null)
            coll = _removeList.ToArray();

        // Knuth shuffle algorithm, courtesy of Wikipedia :)
        for (int t = 0; t < coll.Length; t++) {
            string tmp = coll[t];
            int r = Random.Range(t, coll.Length);
            coll[t] = coll[r];
            coll[r] = tmp;
        }

        _deckQ = new Queue<string>(coll);
    }

    void PrintDeck() {
        string s = "[";
        foreach (string hextag in _deckQ.ToArray()) {
            s += hextag.Substring(5, hextag.Length - 5) + ", ";
        }
        s = s.Substring(0, s.Length - 2) + "]";

        Debug.Log("DECK: " + s);
    }

    public string GetNextHextag() {
        Debug.Log("DECK: Next hex is " + _deckQ.Peek());
        string nextHex = _deckQ.Dequeue();
        PrintDeck();
        return nextHex; // + "-" ?
    }

    public void AddHextagToRemoveList(string hextag) {
        _removeList.Add(hextag);
    }
}
