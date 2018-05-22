using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck {

    private MageMatch _mm;
    private Player _player;
    private Queue<string> _deckQ;
    private List<string> _removeList;
    private string _nextHexTag;

    public Deck(MageMatch mm, Player p) {
        _mm = mm;
        _player = p;
        _removeList = new List<string>();
    }

    string[] GetInitHextags() {
        MMDebug.MMLog.Log("DECK", "black", "Init player " + _player.id);
        var list = new List<string>();
        var info = CharacterInfo.GetCharacterInfo(_player.character.ch);
        for(int d = 0; d < 5; d++) {
            Tile.Element elem = (Tile.Element)(d + 1);
            for (int i = 0; i < info.deck[d]; i++) {
                list.Add("p"+_player.id+"-B-"+elem.ToString().Substring(0, 1));
            }
        }

        foreach (string rune in _mm.gameSettings.GetLoadout(_player.id)) {
            var runeInfo = RuneInfoLoader.GetPlayerRuneInfo(_player.id, rune);
            string cat = runeInfo.category.Substring(0, 1);
            for (int i = 0; i < runeInfo.deckCount; i++) {
                list.Add("p" + _player.id + "-" + cat + "-" + runeInfo.tagTitle);
            }
        }

        return list.ToArray();
    }

    public IEnumerator Shuffle(string[] hextags = null) {
        if (hextags == null)
            hextags = GetInitHextags();

        int id;
        if (_mm.IsDebugMode())
            id = 1;
        else
            id = _player.id;

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

    public int GetDeckCount() { return _deckQ.Count; }

    public IEnumerator ReadyNextHextag() {
        if (_deckQ.Count == 0) {
            yield return Shuffle(_removeList.ToArray());
            _removeList.Clear();
            _mm.uiCont.UpdateRemovedCount(_player.id, 0);
        }

        Debug.Log("DECK: Next hex is " + _deckQ.Peek());
        string nextHex = _deckQ.Dequeue();

        _mm.uiCont.UpdateDeckCount(_player.id, _deckQ.Count);

        PrintDeck();
        _nextHexTag = nextHex; // + "-" ?
    }

    public string GetNextHextag() { return _nextHexTag; }

    public void AddHextagToRemoveList(string hextag) {
        _removeList.Add(hextag);
        _mm.uiCont.UpdateRemovedCount(_player.id, _removeList.Count);
    }

    public int GetRemoveListCount() { return _removeList.Count; }
}
