using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System;

// eventually, this will be where the output file stuff happens?
public class Stats {

    public int turns = 0;

    private int commishMatches;

    private MageMatch mm;

    struct PlayerStat {
        public string name;
        public string character;
        public string loadout;
        public int draws, drops, swaps, matches, cascades, tilesRemoved, spellsCast;
        public int longestCascade;
    }

    private PlayerStat ps1, ps2;

    public Stats(Player p1, Player p2) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        ps1 = new PlayerStat() {
            name = p1.name,
            character = p1.character.characterName,
            loadout = p1.character.loadoutName
        };
        ps2 = new PlayerStat() {
            name = p2.name,
            character = p2.character.characterName,
            loadout = p2.character.loadoutName
        };

        // TODO reorder methods and subscriptions to make more sense (visually)
        mm.eventCont.match += OnMatch;
        mm.eventCont.turnChange += OnTurnChange;
        mm.eventCont.commishMatch += OnCommishMatch;
        mm.eventCont.spellCast += OnSpellCast;
        mm.eventCont.draw += OnDraw;
        mm.eventCont.tileRemove += OnTileRemove;
        mm.eventCont.cascade += OnCascade;
    }

    public void OnMatch(int id, int count) {
        if (id == 1)
            ps1.matches += count;
        else
            ps2.matches += count;
    }

    public void OnCascade(int id, int chain) {
        Debug.Log("STATS: OnCascade called. chain = " + chain);
        if (id == 1)
            OnCascade(ref ps1, chain);
        else
            OnCascade(ref ps2, chain);
    }
    void OnCascade(ref PlayerStat ps, int chain) {
        ps.cascades++;
        if (ps.longestCascade < chain)
            ps.longestCascade = chain;
    }

    public void OnDraw(int id) {
        if (id == 1)
            ps1.draws++;
        else
            ps2.draws++;
    }

    public void OnTileRemove(int id, TileBehav tb) {
        if (id == 1)
            ps1.tilesRemoved++;
        else
            ps2.tilesRemoved++;
    }

    //public void OnDrop(int id) {
    //    if (id == 1)
    //        ps1.drops++;
    //    else
    //        ps2.drops++;
    //}

    public void OnTurnChange(int id) {
        turns++;
        //Debug.Log("STATS: Turns incremented to " + turns);
    }

    public void OnCommishMatch(int count) {
        commishMatches += count;
    }

    public void OnSpellCast(int id, Spell spell) {
        if (id == 1)
            ps1.spellsCast++;
        else
            ps2.spellsCast++;
    }

    public void SaveCSV() {
        DateTime dt = DateTime.Now;
        string filePath = "MageMatchStats-" + dt.Year + "-" + dt.Month + "-" + dt.Day;
        filePath = @"/" + filePath + ".csv";
        string delimiter = ",";

        string[][] output = new string[][]{
             new string[]{ "Turns complete", turns.ToString() },
             new string[]{ "Commish matches", commishMatches.ToString() },
             new string[]{ "" },
             new string[]{ "Player 1" },
             new string[]{ ps1.name, ps1.character, ps1.loadout },
             new string[]{ "Matches", ps1.matches.ToString() },
             new string[]{ "Cascades", ps1.cascades.ToString(), "...longest", ps1.longestCascade.ToString() },
             new string[]{ "Spells cast", ps1.spellsCast.ToString() },
             new string[]{ "Tiles drawn", ps1.draws.ToString() },
             new string[]{ "Tiles removed", ps1.tilesRemoved.ToString() },
             new string[]{ "" },
             new string[]{ "Player 2" },
             new string[]{ ps2.name, ps2.character, ps2.loadout },
             new string[]{ "Matches", ps2.matches.ToString() },
             new string[]{ "Cascades", ps2.cascades.ToString(), "...longest", ps2.longestCascade.ToString() },
             new string[]{ "Spells cast", ps2.spellsCast.ToString() },
             new string[]{ "Tiles drawn", ps2.draws.ToString() },
             new string[]{ "Tiles removed", ps2.tilesRemoved.ToString() }
         };
        int length = output.GetLength(0);
        StringBuilder sb = new StringBuilder();
        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));

        File.WriteAllText(filePath, sb.ToString());
    }

}
