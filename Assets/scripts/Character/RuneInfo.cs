using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneInfo {

    public string title, category;
    public string[] keywords;
    public int deckCount;
    public string desc;

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

    public static RuneInfo GetRuneInfo(string rune) {
        string json = (Resources.Load("json/runes") as TextAsset).text;
        MMDebug.MMLog.Log("RuneInfo", "black", "json for "+rune+": " + json);

        JObject o = JObject.Parse(json);
        MMDebug.MMLog.Log("RuneInfo", "black", "parsed: " + o[rune].ToString());

        if (o[rune] == null) {
            MMDebug.MMLog.LogError("RuneInfo: Couldn't find info for \"" + rune + "\"");
        }

        //json = o[rune].ToObject<string>();
        //MMDebug.MMLog.Log("RuneInfo", "black", "json for "+rune+": " + json);

        RuneInfo info = new RuneInfo();
        
        // I feel like this isn't the proper way to do this
        JsonConvert.PopulateObject(o[rune].ToString(), info);
        info.title = rune;

        return info;
    }
}