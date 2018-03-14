using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck {

    private MageMatch _mm;
    private Player _player;
    private Queue<string> _deckQ;
    private List<string> _removeList;

    public Deck(MageMatch mm, Player p) {
        _mm = mm;
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
            string cat = runeInfo.category.Substring(0, 1);
            for (int i = 0; i < runeInfo.deckCount; i++) {
                list.Add("p" + _player.id + "-" + cat + "-" + runeInfo.title);
            }
        }

        Shuffle(list.ToArray());
        PrintDeck();
    }

    void Shuffle(string[] coll) {
        Debug.Log("DECK: Shuffling " + coll.Length + " hexes...");
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
            s += Hex.TagTitle(hextag) + ", ";
        }
        if (_deckQ.Count > 0)
            s = s.Substring(0, s.Length - 2);
        s += "]";

        Debug.Log("DECK: " + s);
    }

    public int GetDeckCount() { return _deckQ.Count; }

    public string GetNextHextag() {
        if (_deckQ.Count == 0) {
            Shuffle(_removeList.ToArray());
            _removeList.Clear();
            _mm.uiCont.UpdateRemovedCount(0);
        }

        Debug.Log("DECK: Next hex is " + _deckQ.Peek());
        string nextHex = _deckQ.Dequeue();

        _mm.uiCont.UpdateDeckCount(_deckQ.Count);

        PrintDeck();
        return nextHex; // + "-" ?
    }

    public void AddHextagToRemoveList(string hextag) {
        _removeList.Add(hextag);
        _mm.uiCont.UpdateRemovedCount(_removeList.Count);
    }

    public int GetRemoveListCount() { return _removeList.Count; }
}
