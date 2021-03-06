﻿using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfo {

    public class SpellInfo {
        public string title, prereq, desc;
        public string[] keywords;
        public int cost = 1, meterCost = 1000;
    }

    public string name, keyElements;
    public int health;
    public int[] deck;
    public SpellInfo passive, core, spell1, spell2, spell3, signature;
    public SpellInfo[] altSpells;

    public static string GetSpellInfoString(SpellInfo spell, bool formatted) {
        // title, prereq, type, desc
        string format = "{0} - {1}/{2} - {3}\n";
        if (formatted) {
            format  = "<size=40>{0}</size>\n";
            format += "<size=25>{1}</size>\n";
            format += "<size=25><i>{2}</i></size>\n";
            format += "{3}";
        }

        return string.Format(format,
            spell.title,
            spell.prereq,
            spell.keywords != null ? string.Join(", ", spell.keywords) : "",
            spell.desc
        );
    }

    public static CharacterInfo GetCharacterInfo(Character.Ch ch) {
        TextAsset ta = Resources.Load("json/Characters/" + ch.ToString()) as TextAsset;

        string json = ta.text;
        //Debug.Log("Got json: " + json);
        CharacterInfo info = new CharacterInfo();
        JsonConvert.PopulateObject(json, info);
        //Debug.Log("Got info for " + info.name + " from " + ch.ToString() + ".json");
        //if (info.core != null)
        //    Debug.Log(">>>>> coreSpell is not null. title=" + info.core.title);
        return info;
    }

    public static string GetCharacterDesc(Character.Ch ch) {
        string s = "";
        CharacterInfo c = GetCharacterInfo(ch);
        s += string.Format("{0} | elements: {1} | health: {2}\n", c.name, c.keyElements, c.health);
        s += string.Format("{0}F/{1}W/{2}E/{3}A/{4}M - \n", c.deck[0], c.deck[1], c.deck[2], c.deck[3], c.deck[4]);

        s += string.Format("Passive - {0}: {1}\nSignature: {2} - {3}/{4} - {5}\n", c.passive.title, c.passive.desc, c.signature.title, c.signature.prereq, c.signature.keywords, c.signature.desc);
        s += string.Format("Core Spell: {0} - {1} - {2}\n", c.core.title, c.core.keywords, c.core.desc);

        s += GetSpellInfoString(c.spell1, false) + "\n" + GetSpellInfoString(c.spell2, false) + "\n" + GetSpellInfoString(c.spell3, false);

        return s;
    }
}