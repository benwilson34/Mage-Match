using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneInfoLoader {

    public class RuneInfo { // could just be struct?
        public string tagTitle, title, category;
        public string[] keywords;
        public int deckCount;
        public string desc;
    }

    private static List<RuneInfo> _p1runes, _p2runes; // for in-game rune info
    private static List<RuneInfo> _allRunes; // all runes
    // TODO need something for character select screen (since user will still get tooltips)
    private static Dictionary<string, RuneInfo> allRuneInfo; // for Runebuilding


    //public static string GetCharacterInfo(Character.Ch ch) {
    //    string s = "";
    //    CharacterInfo c = GetCharacterInfoObj(ch);
    //    s += string.Format("{0} | elements: {1} | health: {2}", c.name, c.keyElements, c.health);
    //    s += string.Format("{0}F/{1}W/{2}E/{3}A/{4}M - \n", c.deck[0], c.deck[1], c.deck[2], c.deck[3], c.deck[4]);

    //    s += string.Format("Passive - {0}: {1}\nSignature: {2} - {3}/{4} - {5}\n", c.passive.title, c.passive.desc, c.signature.title, c.signature.prereq, c.signature.type, c.signature.desc);
    //    s += string.Format("Core Spell: {0} - {1}-turn cooldown/{2} - {3}", c.core.title, c.core.cooldown, c.core.type, c.core.desc);

    //    s += GetSpellInfo(c.spell1, false) + "\n" + GetSpellInfo(c.spell2, false) + "\n" + GetSpellInfo(c.spell3, false);

    //    return s;
    //}

    //public static string GetSpellInfo(SpellInfo spell, bool formatted) {
    //    return GetSpellInfoJSON(spell.title, spell.prereq, spell.type, spell.desc, formatted);
    //}

    //public static string GetSpellInfoJSON(string title, string prereq, string type, string desc, bool formatted) {
    //    // title, prereq, type, desc
    //    string format = "{0} - {1}/{2} - {3}\n";
    //    if (formatted)
    //        format = "<b>{0}</b>\n{1}/{2}\n\n{3}\n";

    //    return string.Format(format,
    //        title,
    //        prereq,
    //        type,
    //        desc
    //    );
    //}

    public static void InitInGameRuneInfo(GameSettings settings) {
        JObject o = GetRuneJObject();
        JObject neutralRunes = o["Neutral"].ToObject<JObject>();

        for (int id = 1; id <= 2; id++) {
            List<RuneInfo> runeList = new List<RuneInfo>();
            JObject charRunes = null;
            if(o[settings.GetChar(id).ToString()] != null)
                charRunes = o[settings.GetChar(id).ToString()].Value<JObject>();

            foreach (string rune in settings.GetLoadout(id)) {
                if (neutralRunes[rune] != null) {
                    runeList.Add(GetRuneInfo(neutralRunes, rune));
                } else if (charRunes[rune] != null) {
                    runeList.Add(GetRuneInfo(charRunes, rune));
                } else {
                    MMDebug.MMLog.LogError("RuneInfoLoader: Couldn't find info for \"" + rune + "\"");
                }
            }
            SetInGameInfoList(runeList, id);
        }

        if (settings.trainingMode)
            InitRuneList();
    }

    static void InitRuneList() {
        JObject o = GetRuneJObject();

        _allRunes = new List<RuneInfo>();
        foreach (JProperty cat in o.Properties()) {
            foreach (JProperty prop in ((JObject)cat.Value).Properties()) {
                //string cat = prop.Value.ToObject<JObject>()["category"].ToString().Substring(0, 1);
                //runes.Add(cat + "-" + prop.Name);
                var info = new RuneInfo();
                // I feel like this isn't the proper way to do this
                JsonConvert.PopulateObject(prop.Value.ToString(), info);
                if(info.title == null)
                    info.title = prop.Name;

                _allRunes.Add(info);
            }
        }
    }

    static void SetInGameInfoList(List<RuneInfo> list, int id) {
        if (id == 1)
            _p1runes = list;
        else
            _p2runes = list;
    }

    static JObject GetRuneJObject() {
        string json = (Resources.Load("json/runes") as TextAsset).text;
        //MMDebug.MMLog.Log("RuneInfo", "black", "json for "+rune+": " + json);

        JObject o = JObject.Parse(json);
        //MMDebug.MMLog.Log("RuneInfo", "black", "parsed: " + o[rune].ToString());
        return o;
    }

    static RuneInfo GetRuneInfo(JObject obj, string rune) {
        RuneInfo info = new RuneInfo();
        
        // I feel like this isn't the proper way to do this
        JsonConvert.PopulateObject(obj[rune].ToString(), info);
        info.tagTitle = rune;

        if (info.title == null)
            info.title = rune;

        return info;
    }

    public static RuneInfo GetPlayerRuneInfo(int id, string rune) {
        List<RuneInfo> runeList;
        if (id == 1)
            runeList = _p1runes;
        else
            runeList = _p2runes;

        foreach (RuneInfo info in runeList) {
            if (info.tagTitle == rune)
                return info;
        }
        MMDebug.MMLog.LogError("RuneInfoLoader: Couldn't get info for \"" + rune + "\"");
        return null;
    }

    //public static RuneInfo GetRuneInfo(string rune) {
    //    JObject o = GetRuneJObject();
    //    if (o[rune] == null) {
    //        MMDebug.MMLog.LogError("RuneInfo: Couldn't find info for \"" + rune + "\"");
    //    }

    //    //json = o[rune].ToObject<string>();
    //    //MMDebug.MMLog.Log("RuneInfo", "black", "json for "+rune+": " + json);

    //    RuneInfo info = new RuneInfo();
        
    //    // I feel like this isn't the proper way to do this
    //    JsonConvert.PopulateObject(o[rune].ToString(), info);
    //    info.tagTitle = rune;
    //    if (info.title == null)
    //        info.title = rune;

    //    return info;
    //}

    // This is only for DebugTools right now, but will be adapted in time

    public static List<string> GetTileList() {
        return GetCatList("Tile");
    }

    public static List<string> GetCharmList() {
        return GetCatList("Charm");
    }

    static List<string> GetCatList(string category) {
        var list = new List<string>();
        foreach (var rune in _allRunes) {
            if (rune.category == category)
                list.Add(category.Substring(0, 1) + "-" + rune.tagTitle);
        }
        return list;
    }
}