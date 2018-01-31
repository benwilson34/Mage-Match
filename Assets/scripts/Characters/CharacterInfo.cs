//using Newtonsoft.Json;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CharacterInfo {

//    //public class SpellInfo {
//    //    public string title, prereq, cooldown, type, desc;
//    //    public int cost, meterCost;
//    //}

//    //public string name, keyElements;
//    //public int health;
//    //public int[] deck;
//    //public SpellInfo passive, coreSpell, spell1, spell2, spell3, signature, altSpell;



//    public class Passive {
//        public string title;
//        public string desc;
//    }

//    public class Core {
//        public string title;
//        public int cost;
//        public int cooldown;
//        public string type;
//        public string desc;
//    }

//    public class Spell1 {
//        public string title;
//        public string prereq;
//        public int cost;
//        public string type;
//        public string desc;
//    }

//    public class Spell2 {
//        public string title;
//        public string prereq;
//        public int cost;
//        public string type;
//        public string desc;
//    }

//    public class Spell3 {
//        public string title;
//        public string prereq;
//        public int cost;
//        public string type;
//        public string desc;
//    }

//    public class Signature {
//        public string title;
//        public string prereq;
//        public int cost;
//        public int meterCost;
//        public string type;
//        public string desc;
//    }

//    public class AltSpell {
//        public string title;
//        public int cost;
//        public int cooldown;
//        public string type;
//        public string desc;
//    }


//    public string name;
//    public string keyElements;
//    public int health;
//    public int[] deck;
//    public Passive passive;
//    public Core core;
//    public Spell1 spell1;
//    public Spell2 spell2;
//    public Spell3 spell3;
//    public Signature signature;
//    public AltSpell altSpell;


//    //static JSONObject chars;

//    //public static void Init() {
//    //       string file = Resources.Load<TextAsset>("characters").text;
//    //       //chars = new JSONObject(file);


//    //}

//    //static JSONObject GetCharacter(Character.Ch ch) {
//    //    Debug.Log("CharInfo: Getting info for " + ch);
//    //    return chars.list[(int)ch];
//    //}

//    public static string GetCharacterInfo(Character.Ch ch) {
//        string s = "";
//        CharacterInfo c = GetCharacterInfoObj(ch);
//        s += string.Format("{0} | elements: {1} | health: {2}", c.name, c.keyElements, c.health);
//        s += string.Format("{0}F/{1}W/{2}E/{3}A/{4}M - \n", c.deck[0], c.deck[1], c.deck[2], c.deck[3], c.deck[4]);

//        s += string.Format("Passive - {1}: {2}\nSignature: {3} - {4}/{5} - {6}\n", c.passive.title, c.passive.desc, c.signature.title, c.signature.prereq, c.signature.type, c.signature.desc);
//        s += string.Format("Core Spell: {0} - {1}-turn cooldown/{2} - {3}", c.core.title, c.core.cooldown, c.core.type, c.core.desc);

//        s += GetSpellInfo(c.spell1, false) + "\n" + GetSpellInfo(c.spell2, false) + "\n" + GetSpellInfo(c.spell3, false);

//        return s;
//    }

//    //public static string GetSpellInfo(Character.Ch ch, int index, bool formatted = false) {
//    //    JSONObject c = GetCharacter(ch);
//    //    Dictionary<string, string> spell = c.list[5 + index].ToDictionary();

//    //    return GetSpellInfoJSON(spell, formatted);
//    //}

//    public static string GetSpellInfo(object spell, bool formatted) {
//        if (spell is Core) {
//            Core s = (Core)spell;
//            return GetSpellInfoJSON(s.title, "", s.type, s.desc, formatted);
//        } else if (spell is AltSpell) {
//            AltSpell s = (AltSpell)spell;
//            return GetSpellInfoJSON(s.title, "", s.type, s.desc, formatted);
//        } else if (spell is Spell1) {
//            Spell1 s = (Spell1)spell;
//            return GetSpellInfoJSON(s.title, s.prereq, s.type, s.desc, formatted);
//        } else if (spell is Spell2) {
//            Spell2 s = (Spell2)spell;
//            return GetSpellInfoJSON(s.title, s.prereq, s.type, s.desc, formatted);
//        } else if (spell is Spell3) {
//            Spell3 s = (Spell3)spell;
//            return GetSpellInfoJSON(s.title, s.prereq, s.type, s.desc, formatted);
//        } else if (spell is Signature) {
//            Signature s = (Signature)spell;
//            return GetSpellInfoJSON(s.title, s.prereq, s.type, s.desc, formatted);
//        } else return null;
//    }

//    public static string GetSpellInfoJSON(string title, string prereq, string type, string desc, bool formatted) {
//        // title, prereq, type, desc
//        string format = "{0} - {1}/{2} - {3}\n";
//        if (formatted)
//            format = "<b>{0}</b>\n{1}/{2}\n\n{3}\n";

//        return string.Format(format, 
//            title, 
//            prereq, 
//            type, 
//            desc
//        );
//    }

//    public static CharacterInfo GetCharacterInfoObj(Character.Ch ch) {
//        //CharacterInfo info = new CharacterInfo();
//        string file = ch.ToString().ToLower();
//        string json = Resources.Load<TextAsset>("json/" + file).text;
//        CharacterInfo info = new CharacterInfo();
//        JsonConvert.PopulateObject(json, info);
//        //CharacterInfo info = JsonUtility.FromJson<CharacterInfo>(json);
//        Debug.Log("Got info for " + info.name + " from " + file + ".json");
//        if(info.core != null)
//            Debug.Log(">>>>> coreSpell is not null. title=" + info.core.title);
//        return info;
//    }
//}




using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfo {

    public class SpellInfo {
        public string title, prereq, cooldown, type, desc;
        public int cost, meterCost;
    }

    public string name, keyElements;
    public int health;
    public int[] deck;
    public SpellInfo passive, core, spell1, spell2, spell3, signature, altSpell;

    public static string GetCharacterInfo(Character.Ch ch) {
        string s = "";
        CharacterInfo c = GetCharacterInfoObj(ch);
        s += string.Format("{0} | elements: {1} | health: {2}", c.name, c.keyElements, c.health);
        s += string.Format("{0}F/{1}W/{2}E/{3}A/{4}M - \n", c.deck[0], c.deck[1], c.deck[2], c.deck[3], c.deck[4]);

        s += string.Format("Passive - {0}: {1}\nSignature: {2} - {3}/{4} - {5}\n", c.passive.title, c.passive.desc, c.signature.title, c.signature.prereq, c.signature.type, c.signature.desc);
        s += string.Format("Core Spell: {0} - {1}-turn cooldown/{2} - {3}", c.core.title, c.core.cooldown, c.core.type, c.core.desc);

        s += GetSpellInfo(c.spell1, false) + "\n" + GetSpellInfo(c.spell2, false) + "\n" + GetSpellInfo(c.spell3, false);

        return s;
    }

    public static string GetSpellInfo(SpellInfo spell, bool formatted) {
        return GetSpellInfoJSON(spell.title, spell.prereq, spell.type, spell.desc, formatted);
    }

    public static string GetSpellInfoJSON(string title, string prereq, string type, string desc, bool formatted) {
        // title, prereq, type, desc
        string format = "{0} - {1}/{2} - {3}\n";
        if (formatted)
            format = "<b>{0}</b>\n{1}/{2}\n\n{3}\n";

        return string.Format(format,
            title,
            prereq,
            type,
            desc
        );
    }

    public static CharacterInfo GetCharacterInfoObj(Character.Ch ch) {
        //CharacterInfo info = new CharacterInfo();
        //CharacterInfo info = JsonUtility.FromJson<CharacterInfo>(json);

        string json = Resources.Load<TextAsset>("json/" + ch.ToString()).text;
        CharacterInfo info = new CharacterInfo();
        JsonConvert.PopulateObject(json, info);
        Debug.Log("Got info for " + info.name + " from " + ch.ToString() + ".json");
        if (info.core != null)
            Debug.Log(">>>>> coreSpell is not null. title=" + info.core.title);
        return info;
    }
}