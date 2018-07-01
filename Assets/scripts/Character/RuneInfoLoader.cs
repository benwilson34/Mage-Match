using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rune {
    public enum NeutralRune {  };
}

public class RuneInfoLoader {

    public class RuneInfo { // could just be struct?
        public string tagTitle, title, category;
        public string[] keywords;
        public int deckCount;
        public int cost = -1;
        public string desc;
    }

    private static Dictionary<string, RuneInfo> _p1runes, _p2runes; // for in-game rune info
    //private static List<RuneInfo> _allRunes; // all runes
    // TODO need something for character select screen (since user will still get tooltips)
    private static Dictionary<Character.Ch, Dictionary<string, RuneInfo>> _allRuneInfo; // for Runebuilding

    private static bool _loadedAllRunes = false;

    public static void InitInGameRuneInfo(GameSettings settings) {
        if (settings.trainingMode)
            InitAllRuneInfo();
        else
            InitCharacterRunesOnly(settings);
    }

    static void InitCharacterRunesOnly(GameSettings settings) {
        JObject neutralRunes = GetCharacterJObject(Character.Ch.Neutral);

        for (int id = 1; id <= 2; id++) {
            Dictionary<string, RuneInfo> runeList = new Dictionary<string, RuneInfo>();
            JObject charRunes = GetCharacterJObject(settings.GetChar(id));
            //if(o[settings.GetChar(id).ToString()] != null)
            //    charRunes = o[settings.GetChar(id).ToString()].Value<JObject>();

            foreach (string rune in settings.GetLoadout(id)) {
                if (neutralRunes[rune] != null) {
                    runeList.Add(rune, GetRuneInfo(rune, (JObject)neutralRunes[rune]));
                } else if (charRunes[rune] != null) {
                    runeList.Add(rune, GetRuneInfo(rune, (JObject)charRunes[rune]));
                } else {
                    MMDebug.MMLog.LogError("RuneInfoLoader: Couldn't find info for \"" + rune + "\"");
                }
            }
            SetInGameInfoList(runeList, id);
        }
    }

    public static void InitAllRuneInfo() {
        //_allRunes = new List<RuneInfo>();
        _loadedAllRunes = true;
        _allRuneInfo = new Dictionary<Character.Ch, Dictionary<string, RuneInfo>>();
        //foreach (JProperty charGroup in o.Properties()) {
        foreach (Character.Ch ch in Enum.GetValues(typeof(Character.Ch))) {
            var charDict = new Dictionary<string, RuneInfo>();
            foreach (JProperty rune in GetCharacterJObject(ch).Properties()) {
                //_allRunes.Add(GetRuneInfo(rune.Name, (JObject)rune.Value));
                charDict.Add(rune.Name, GetRuneInfo(rune.Name, (JObject)rune.Value));
            }
            _allRuneInfo.Add(ch, charDict);
        }
    }

    static void SetInGameInfoList(Dictionary<string, RuneInfo> info, int id) {
        if (id == 1)
            _p1runes = info;
        else
            _p2runes = info;
    }

    static JObject GetCharacterJObject(Character.Ch ch) {
        string json = (Resources.Load("json/Runes/" + ch.ToString() + "_runes") as TextAsset).text;
        return JObject.Parse(json);
    }

    static RuneInfo GetRuneInfo(string rune, JObject runeObj) {
        RuneInfo info = new RuneInfo();

        // I feel like this isn't the proper way to do this
        JsonConvert.PopulateObject(runeObj.ToString(), info);
        info.tagTitle = rune;

        if (info.title == null)
            info.title = rune;

        if (info.cost == -1)
            info.cost = 1;

        return info;
    }

    public static RuneInfo GetRuneInfo(Character.Ch ch, string rune) {
        if (_allRuneInfo[ch].ContainsKey(rune))
            return _allRuneInfo[ch][rune];
        else if (_allRuneInfo[Character.Ch.Neutral].ContainsKey(rune))
            return _allRuneInfo[Character.Ch.Neutral][rune];
        else {
            Debug.LogError("Couldn't load rune info for \"" + rune + "\"!");
            return null;
        }
    }

    static RuneInfo GetRuneInfo(string rune) {
        // very inefficient, use only if utterly necessary
        foreach (var key in _allRuneInfo.Keys) {
            if (_allRuneInfo[key].ContainsKey(rune))
                return _allRuneInfo[key][rune];
        }
        Debug.LogError("Couldn't load rune info for \"" + rune + "\"!");
        return null;
    }

    public static RuneInfo GetPlayerRuneInfo(int id, string rune) {
        if (_loadedAllRunes) // for training mode
            return GetRuneInfo(rune);

        Dictionary<string, RuneInfo> info = id == 1 ? _p1runes : _p2runes;

        if (info.ContainsKey(rune)) {
            return info[rune];
        } else {
            MMDebug.MMLog.LogError("RuneInfoLoader: Couldn't get info for \"" + rune + "\"");
            return null;
        }
    }

    public static List<string> GetTileList() {
        return GetCatList("Tile");
    }

    public static List<string> GetCharmList() {
        return GetCatList("Charm");
    }

    static List<string> GetCatList(string category) {
        var list = new List<string>();
        foreach (var charDict in _allRuneInfo.Values) {
            foreach (var rune in charDict.Values) {
                if (rune.category == category)
                    list.Add(category.Substring(0, 1) + "-" + rune.tagTitle);
            }
        }
        return list;
    }

    public static List<RuneInfo> GetRuneList(Character.Ch ch) {
        List<RuneInfo> runeList = new List<RuneInfo>();
        runeList.AddRange(_allRuneInfo[ch].Values);
        runeList.AddRange(_allRuneInfo[Character.Ch.Neutral].Values);
        return runeList;
    }

    public static Sprite GetRuneSprite(Character.Ch ch, string rune) {
        RuneInfo info = GetRuneInfo(ch, rune);
        return Resources.Load<Sprite>("sprites/hexes/" + info.category + "/" + info.tagTitle);
    }
}