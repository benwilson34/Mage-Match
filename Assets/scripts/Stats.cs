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

        mm.eventCont.turnChange += OnTurnChange;
        mm.eventCont.commishMatch += OnCommishMatch;

        mm.eventCont.draw += OnDraw;
        mm.eventCont.drop += OnDrop;
        mm.eventCont.swap += OnSwap;
        mm.eventCont.match += OnMatch;
        mm.eventCont.cascade += OnCascade;
        mm.eventCont.tileRemove += OnTileRemove;
        mm.eventCont.spellCast += OnSpellCast;
    }

    public void OnTurnChange(int id) {
        turns++;
    }

    public void OnCommishMatch(int count) {
        commishMatches += count;
    }

    public void OnDraw(int id) {
        if (id == 1)
            ps1.draws++;
        else
            ps2.draws++;
    }

    public void OnDrop(int id, int col) {
        if (id == 1)
            ps1.drops++;
        else
            ps2.drops++;
    }

    public void OnSwap(int id, int c1, int r1, int c2, int r2) {
        if (id == 1)
            ps1.swaps++;
        else
            ps2.swaps++;
    }

    public void OnMatch(int id, int count) {
        if (id == 1)
            ps1.matches += count;
        else
            ps2.matches += count;
    }

    public void OnCascade(int id, int chain) {
        //Debug.Log("STATS: OnCascade called. chain = " + chain);
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

    public void OnTileRemove(int id, TileBehav tb) {
        if (id == 1)
            ps1.tilesRemoved++;
        else
            ps2.tilesRemoved++;
    }

    public void OnSpellCast(int id, Spell spell) {
        if (id == 1)
            ps1.spellsCast++;
        else
            ps2.spellsCast++;
    }

    public void SaveCSV() {
        DateTime dt = DateTime.Now;
        string filePath = "MageMatchStats_" + dt.Year + "-" + dt.Month + "-" + dt.Day;
        filePath += "_" + dt.Hour + "-" + dt.Minute + "-" + dt.Second;
        filePath = @"/" + filePath + ".csv";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Turns complete," + turns);
        sb.AppendLine("Commish matches," + commishMatches).AppendLine("");
        for (int i = 1; i <= 2; i++) {
            PlayerStat ps;
            if (i == 1) ps = ps1;
            else ps = ps2;
            sb.AppendLine("Player " + i);
            sb.AppendLine(ps.name + "," + ps.character + "," + ps.loadout);
            sb.AppendLine("Tiles drawn," + ps.draws);
            sb.AppendLine("Tiles dropped," + ps.drops);
            sb.AppendLine("Tiles swapped," + ps.swaps);
            sb.AppendLine("Matches," + ps.matches);
            sb.AppendLine("Cascades," + ps.cascades + ",...longest," + ps.longestCascade);
            sb.AppendLine("Tiles removed," + ps.tilesRemoved);
            sb.AppendLine("Spells cast," + ps.spellsCast).AppendLine("");
        }

        File.WriteAllText(filePath, sb.ToString());
    }

}
