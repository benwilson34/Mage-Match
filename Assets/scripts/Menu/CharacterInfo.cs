using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfo {

    static JSONObject chars;

	public static void Init() {
        string file = Resources.Load<TextAsset>("characters").text;
        chars = new JSONObject(file);
	}

    static JSONObject GetCharacter(Character.Ch ch) {
        MMDebug.MMLog.Log("CharInfo", "pink", "Getting info for " + ch);
        return chars.list[(int)ch];
    }

    public static string GetCharacterInfo(Character.Ch ch) {
        string s = "";
        JSONObject c = GetCharacter(ch);
        Dictionary<string, string> passive = c.list[2].ToDictionary(); // not great for this one
        Dictionary<string, string> core = c.list[3].ToDictionary();
        Dictionary<string, string> sig = c.list[4].ToDictionary();
        s += string.Format("{0}!! Passive - {1}: {2}\nSignature: {3} - {4}/{5} - {6}\n", c.list[0], passive["title"], passive["desc"], sig["title"], sig["prereq"], sig["type"], sig["desc"]);
        s += string.Format("Core Spell: {0} - {1}-turn cooldown/{2} - {3}", core["title"], core["cooldown"], core["type"], core["desc"]);
        // TODO deck and health
        //JSONObject deck = loadout.list[2];
        //Dictionary<string, string> spell1 = loadout.list[3].ToDictionary();
        //Dictionary<string, string> spell2 = loadout.list[4].ToDictionary();
        //s += string.Format("Loadout {0} - {1}: {2} health - ", which, loadout.list[0], loadout.list[1]);
        //s += string.Format("{0}F/{1}W/{2}E/{3}A/{4}M - \n", deck.list[0], deck.list[1], deck.list[2], deck.list[3], deck.list[4]);

        return s;
    }

    public static string GetSpellInfo(Character.Ch ch, int index) {
        string s = "";
        JSONObject c = GetCharacter(ch);
        Dictionary<string, string> spell = c.list[5 + index].ToDictionary();
        s += string.Format("{0} - {1}/{2} - {3}\n", 
            spell["title"], 
            spell.ContainsKey("prereq") ? spell["prereq"] : "", 
            spell["type"], 
            spell["desc"]
        );

        return s;
    }
}