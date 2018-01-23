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
        Debug.Log("CharInfo: Getting info for " + ch);
        return chars.list[(int)ch];
    }

    public static string GetCharacterInfo(Character.Ch ch) {
        string s = "";
        JSONObject c = GetCharacter(ch);
        s += string.Format("{0} | elements: {1} | health: {2}", c.list[0], c.list[1], c.list[2]);
        JSONObject deck = c.list[3];
        s += string.Format("{0}F/{1}W/{2}E/{3}A/{4}M - \n", deck.list[0], deck.list[1], deck.list[2], deck.list[3], deck.list[4]);

        Dictionary<string, string> passive = c.list[4].ToDictionary(); // not great for this one
        Dictionary<string, string> core = c.list[5].ToDictionary();
        Dictionary<string, string> sig = c.list[9].ToDictionary();
        s += string.Format("{0}!! Passive - {1}: {2}\nSignature: {3} - {4}/{5} - {6}\n", c.list[0], passive["title"], passive["desc"], sig["title"], sig["prereq"], sig["type"], sig["desc"]);
        s += string.Format("Core Spell: {0} - {1}-turn cooldown/{2} - {3}", core["title"], core["cooldown"], core["type"], core["desc"]);

        Dictionary<string, string> spell1 = c.list[6].ToDictionary();
        Dictionary<string, string> spell2 = c.list[7].ToDictionary();
        Dictionary<string, string> spell3 = c.list[8].ToDictionary();
        s += GetSpellInfoJSON(spell1, false) + "\n" + GetSpellInfoJSON(spell2, false) + "\n" + GetSpellInfoJSON(spell3, false);

        return s;
    }

    public static string GetSpellInfo(Character.Ch ch, int index, bool formatted = false) {
        JSONObject c = GetCharacter(ch);
        Dictionary<string, string> spell = c.list[5 + index].ToDictionary();

        return GetSpellInfoJSON(spell, formatted);
    }

    static string GetSpellInfoJSON(Dictionary<string, string> spell, bool formatted) {
        // title, prereq, type, desc
        string format = "{0} - {1}/{2} - {3}\n";
        if (formatted)
            format = "<b>{0}</b>\n{1}/{2}\n\n{3}\n";

        return string.Format(format, 
            spell["title"], 
            spell.ContainsKey("prereq") ? spell["prereq"] : "", 
            spell["type"], 
            spell["desc"]
        );
    }
}