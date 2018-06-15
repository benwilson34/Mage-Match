using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck {

    public int Count { get { return _deckQ.Count; } }

    private MageMatch _mm;
    private Player _player;
    private Queue<string> _deckQ;
    private List<string> _graveyard;

    public Deck(MageMatch mm, Player p) {
        _mm = mm;
        _player = p;
        _deckQ = new Queue<string>(GetInitHextags());

        _graveyard = new List<string>();
    }

    string[] GetInitHextags() {
        MMDebug.MMLog.Log("DECK", "black", "Init player " + _player.ID);
        var list = new List<string>();
        var info = CharacterInfo.GetCharacterInfo(_player.Character.ch);
        for(int d = 0; d < 5; d++) {
            Tile.Element elem = (Tile.Element)(d + 1);
            for (int i = 0; i < info.deck[d]; i++) {
                list.Add("p"+_player.ID+"-B-"+elem.ToString());
            }
        }

        foreach (string rune in _mm.gameSettings.GetLoadout(_player.ID)) {
            var runeInfo = RuneInfoLoader.GetPlayerRuneInfo(_player.ID, rune);
            string cat = runeInfo.category.Substring(0, 1);
            for (int i = 0; i < runeInfo.deckCount; i++) {
                list.Add("p" + _player.ID + "-" + cat + "-" + runeInfo.tagTitle);
            }
        }

        return list.ToArray();
    }

    public void AddHextag(string hextag) {
        _deckQ.Enqueue(hextag);
    }

    public IEnumerator Shuffle() {
        int id = _mm.IsDebugMode ? 1 : _player.ID;

        string[] hextags = _deckQ.ToArray();
        int count = hextags.Length;
        Debug.Log("DECK: Shuffling " + count + " hexes...");
        int[] rands = new int[count];
        for (int t = 0; t < count; t++) {
            rands[t] = Random.Range(t, count);
        }

        yield return _mm.syncManager.SyncRands(id, rands);
        rands = _mm.syncManager.GetRands(count);

        // Knuth shuffle algorithm, courtesy of Wikipedia :)
        for (int t = 0; t < count; t++) {
            string tmp = hextags[t];
            int r = rands[t];
            hextags[t] = hextags[r];
            hextags[r] = tmp;
        }

        _deckQ = new Queue<string>(hextags);
        PrintDeck();
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

    public string GetNextHextag() {
        if (_deckQ.Count == 0) {
            // TODO damage for trying to overdraw
        }

        Debug.Log("DECK: Next hex is " + _deckQ.Peek());
        string nextHex = _deckQ.Dequeue();

        _mm.uiCont.UpdateDeckCount(_player.ID, _deckQ.Count);

        PrintDeck();
        return nextHex;
    }

    public void AddHextagToGraveyard(string hextag) {
        _graveyard.Add(hextag);
        _mm.uiCont.UpdateRemovedCount(_player.ID, _graveyard.Count);
    }

    //public int GetRemoveListCount() { return _graveyard.Count; }
}
