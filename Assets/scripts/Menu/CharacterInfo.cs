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
        return chars.list[(int)ch];
    }

    public static string[] GetLoadoutNames(Character.Ch ch) {
        string[] loadouts = new string[2];
        JSONObject character = GetCharacter(ch);
        loadouts[0] = character.GetField("loadout-A").GetField("nickname").ToString();
        loadouts[1] = character.GetField("loadout-B").GetField("nickname").ToString();
        return loadouts;
    }

    public static string GetCharacterInfo(Character.Ch ch) {
        string s = "";
        JSONObject c = GetCharacter(ch);
        Dictionary<string, string> passive = c.list[2].ToDictionary(); // not great for this one
        Dictionary<string, string> core = c.list[3].ToDictionary();
        Dictionary<string, string> sig = c.list[4].ToDictionary();
        s += string.Format("{0}!! Passive - {1}: {2}\nSignature: {3} - {4}/{5} - {6}\n", c.list[0], passive["title"], passive["desc"], sig["title"], sig["prereq"], sig["type"], sig["desc"]);
        s += string.Format("Core Spell: {0} - {1}-turn cooldown/{2} - {3}", core["title"], core["cooldown"], core["type"], core["desc"]);
        return s;
    }

    public static string GetLoadoutInfo(Character.Ch ch, int loadoutInd) {
        string s = "";
        //JSONObject c = characters.list[0].list[index];
        char which = 'A';
        if (loadoutInd == 1)
            which = 'B';

        JSONObject c = GetCharacter(ch);
        JSONObject loadout = c.list[loadoutInd + 5]; //hmm
        JSONObject deck = loadout.list[2];
        Dictionary<string, string> spell1 = loadout.list[3].ToDictionary();
        Dictionary<string, string> spell2 = loadout.list[4].ToDictionary();
        s += string.Format("Loadout {0} - {1}: {2} health - ", which, loadout.list[0], loadout.list[1]);
        s += string.Format("{0}F/{1}W/{2}E/{3}A/{4}M - \n", deck.list[0], deck.list[1], deck.list[2], deck.list[3], deck.list[4]);
        s += string.Format("   1. {0} - {1}/{2} - {3}\n", spell1["title"], spell1["prereq"], spell1["type"], spell1["desc"]);
        s += string.Format("   2. {0} - {1}/{2} - {3}\n", spell2["title"], spell2["prereq"], spell2["type"], spell2["desc"]);
        return s;
    }
}