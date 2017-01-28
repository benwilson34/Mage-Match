﻿using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System;

// eventually, this will be where the output file stuff happens?
public class Stats {

    public int turns = 0;

    private int commishMatches;

    private MageMatch mm;
    private StringBuilder report;

    struct PlayerStat {
        public string name;
        public string character;
        public string loadout;
        public int draws, drops, swaps, matches, cascades, tilesRemoved, spellsCast, timeouts;
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

        InitReport();

        mm.eventCont.turnChange += OnTurnChange;
        mm.eventCont.commishMatch += OnCommishMatch;
        mm.eventCont.commishTurnDone += OnCommishTurnDone;

        mm.eventCont.draw += OnDraw;
        mm.eventCont.drop += OnDrop;
        mm.eventCont.swap += OnSwap;
        mm.eventCont.match += OnMatch;
        mm.eventCont.cascade += OnCascade;
        mm.eventCont.tileRemove += OnTileRemove;
        mm.eventCont.spellCast += OnSpellCast;
        mm.eventCont.timeout += OnTimeout;
    }

    void InitReport() {
        report = new StringBuilder();
        report.Append(ps1.name + " (" + ps1.character + " - " + ps1.loadout + ") vs ");
        report.AppendLine(ps2.name + " (" + ps2.character + " - " + ps2.loadout + ")");
        int rand = UnityEngine.Random.state.GetHashCode(); //?
        report.AppendLine("random seed - " + rand);
        report.AppendLine("...setup - deal p1 4, deal p2 4");
        report.AppendLine("T1 - deal p1 1");
    }

    public void OnTurnChange(int id) {
        turns++;
        report.AppendLine("\nC" + turns + " - ");
    }

    public void OnCommishMatch(int count) {
        commishMatches += count;
    }

    public void OnCommishTurnDone(int id) {
        report.AppendLine("\nT" + turns + " - deal p" + id + " 1");
    }


    public void OnDraw(int id) {
        report.AppendLine("Draw");
        if (id == 1)
            ps1.draws++;
        else
            ps2.draws++;
    }

    public void OnDrop(int id, int col) {
        if (!mm.menu) {
            report.AppendLine("Drop col" + col);
            if (!mm.IsCommishTurn()) {
                if (id == 1)
                    ps1.drops++;
                else
                    ps2.drops++;
            }
        } else
            report.AppendLine("menu Drop col" + col);
    }

    public void OnSwap(int id, int c1, int r1, int c2, int r2) {
        if (!mm.menu) {
            report.AppendLine("Swap (" + c1 + "," + r1 + ")(" + c2 + "," + r2 + ")");
            if (id == 1)
                ps1.swaps++;
            else
                ps2.swaps++;
        } else
            report.AppendLine("menu Swap (" + c1 + "," + r1 + ")(" + c2 + "," + r2 + ")");
    }

    public void OnMatch(int id, int count) {
        report.AppendLine("...match");
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
        if (!mm.IsCommishTurn() && !mm.menu) {
            if (id == 1)
                ps1.tilesRemoved++;
            else
                ps2.tilesRemoved++;
        }
    }

    public void OnSpellCast(int id, Spell spell) {
        report.AppendLine("Spell " + spell.name);
        if (id == 1)
            ps1.spellsCast++;
        else
            ps2.spellsCast++;
    }

    public void OnTimeout(int id) {
        report.AppendLine("...the turn timer timed out!");
        if (id == 1)
            ps1.timeouts++;
        else
            ps2.timeouts++;
    }

    public void SaveStatsCSV() {
        DateTime dt = DateTime.Now;
        string filePath = "MageMatch_" + dt.Year + "-" + dt.Month + "-" + dt.Day + "_";
        filePath += dt.Hour + "-" + dt.Minute + "-" + dt.Second + "_Stats";
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
            sb.AppendLine("Spells cast," + ps.spellsCast);
            sb.AppendLine("Turns timed out," + ps.timeouts).AppendLine("");
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    public void SaveReportTXT() {
        DateTime dt = DateTime.Now;
        string filePath = "MageMatch_" + dt.Year + "-" + dt.Month + "-" + dt.Day + "_";
        filePath += dt.Hour + "-" + dt.Minute + "-" + dt.Second + "_Report";
        filePath = @"/" + filePath + ".txt";

        File.WriteAllText(filePath, report.ToString());
    }

}
