﻿using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System;

public class Stats {

    public int turns = 1;

    private int commishMatches, commishDrops;

    private MageMatch mm;
    private StringBuilder report;

    private class PlayerStat {
        public string name;
        public string character;
        public int draws, drops, swaps, matches, match3s, match4s, match5s, cascades, tilesRemoved, spellsCast, timeouts, discards;
        public int dmgDealt, dmgTaken, healingDone;
        public int longestCascade;
    }

    private PlayerStat ps1, ps2;

    public Stats(Player p1, Player p2) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        ps1 = new PlayerStat() {
            name = p1.name,
            character = p1.character.characterName
        };
        ps2 = new PlayerStat() {
            name = p2.name,
            character = p2.character.characterName
        };

        InitReport();

        mm.eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.Stats);
        mm.eventCont.AddTurnEndEvent(OnTurnEnd, EventController.Type.Stats);
        mm.eventCont.timeout += OnTimeout;
        mm.eventCont.commishDrop += OnCommishDrop;
        mm.eventCont.commishMatch += OnCommishMatch;

        mm.eventCont.AddDrawEvent(OnDraw, EventController.Type.Stats);
        mm.eventCont.AddDropEvent(OnDrop, EventController.Type.Stats);
        mm.eventCont.AddSwapEvent(OnSwap, EventController.Type.Stats);
        mm.eventCont.spellCast += OnSpellCast;
        mm.eventCont.AddDiscardEvent(OnDiscard, EventController.Type.Stats);

        mm.eventCont.AddMatchEvent(OnMatch, EventController.Type.Stats);
        //mm.eventCont.cascade += OnCascade;
        mm.eventCont.tileRemove += OnTileRemove;
        mm.eventCont.playerHealthChange += OnPlayerHealthChange;
    }

    void InitReport() {
        report = new StringBuilder();
        report.Append(ps1.name + " (" + ps1.character + ") vs ");
        report.AppendLine(ps2.name + " (" + ps2.character + ")");
        int rand = UnityEngine.Random.state.GetHashCode(); //?
        report.AppendLine("random seed - " + rand);
        report.AppendLine("...setup - deal p1 4, deal p2 4"); //?
        report.AppendLine("T1 - ");
    }

    PlayerStat GetPS(int id) {
        if (id == 1)
            return ps1;
        else
            return ps2;
    }

    int GetOpponentID(int id) {
        if (id == 1)
            return 2;
        else
            return 1;
    }

    public IEnumerator OnTurnBegin(int id) {
        report.Append("\nT" + turns + " - ");
        yield return null;
    }

    public IEnumerator OnTurnEnd(int id) {
        report.AppendLine("\nC" + turns + " - ");
        turns++;
        yield return null;
    }

    public void OnTimeout(int id) {
        report.AppendLine("...the turn timer timed out!");
        GetPS(id).timeouts++;
    }

    public void OnCommishDrop(Tile.Element elem, int col) {
        commishDrops++;
        report.AppendLine("C-drop " + Tile.ElementToChar(elem) + " col" + col);
    }

    public void OnCommishMatch(string[] seqs) {
        commishMatches += seqs.Length;
        report.AppendLine("...Commish made "+seqs.Length+" match(es)");
    }

    #region GameAction subscriptions
    // TODO for all, different report lines for playerAction or not...
    public IEnumerator OnDraw(int id, string tag, bool playerAction, bool dealt) {
        if(dealt)
            Report("Deal p" + id + " " + tag);
        else
            Report("Draw " + tag);
        GetPS(id).draws++;
        yield return null;
    }

    public IEnumerator OnDiscard(int id, string tag) {
        Report("p" + id + " discards " + tag);
        GetPS(id).discards++;
        yield return null;
    }

    public IEnumerator OnDrop(int id, bool playerAction, string tag, int col) {
        if (playerAction) {
            Report("Drop " + tag + " col" + col);
            GetPS(id).drops++;
        } else if (mm.uiCont.IsDebugMenuOpen()) //?
            Report("menu Drop col" + col);
        yield return null;
    }

    public IEnumerator OnSwap(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        if (!mm.uiCont.IsDebugMenuOpen()) { //?
            Report("Swap (" + c1 + "," + r1 + ")(" + c2 + "," + r2 + ")");
            if(playerAction)
                GetPS(id).swaps++;
        } else
            Report("menu Swap (" + c1 + "," + r1 + ")(" + c2 + "," + r2 + ")");
        yield return null;
    }

    public void OnSpellCast(int id, Spell spell) {
        Report("Spell " + spell.name);
        GetPS(id).spellsCast++;
    }
    #endregion

    public IEnumerator OnMatch(int id, string[] seqs) {
        report.AppendLine("...made " + seqs.Length + " match(es)");
        GetPS(id).matches += seqs.Length;
        foreach (string seq in seqs) {
            int len = seq.Length;
            if (len == 3)
                GetPS(id).match3s++;
            else if(len == 4)
                GetPS(id).match4s++;
            else
                GetPS(id).match5s++;
        }
        yield return null;
    }

    //public void OnCascade(int id, int chain) {
    //    report.AppendLine("...cascade of " + chain + " matches");
    //    PlayerStat ps = GetPS(id);
    //    ps.cascades++;
    //    if (ps.longestCascade < chain)
    //        ps.longestCascade = chain;
    //}

    public void OnTileRemove(int id, TileBehav tb) {
        if (!mm.IsCommishTurn()) {
            if (!mm.uiCont.IsDebugMenuOpen()) //?
                GetPS(id).tilesRemoved++;
            else
                report.AppendLine("menu Remove (" + tb.tile.col + "," + tb.tile.row + ")");
        }
    }

    public void OnPlayerHealthChange(int id, int amount, bool dealt) {
        if (dealt) {
            GetPS(GetOpponentID(id)).dmgDealt -= amount;
            report.AppendLine("...p" + GetOpponentID(id) + " dealt " + (-amount) + " dmg...");
        }
        if (amount < 0)
            GetPS(id).dmgTaken -= amount;
        else
            GetPS(id).healingDone += amount;
        report.AppendLine("...p"+id+" changes health by " + amount);
    }


    public void SaveStatsCSV() {
        DateTime dt = DateTime.Now;
        string filePath = "MageMatch_" + dt.Year + "-" + dt.Month + "-" + dt.Day + "_";
        filePath += dt.Hour + "-" + dt.Minute + "-" + dt.Second + "_Stats";
        filePath = @"/" + filePath + ".csv";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Turns complete," + turns);
        sb.AppendLine("Commish drops," + commishDrops).AppendLine("");
        sb.AppendLine("Commish matches," + commishMatches).AppendLine("");
        for (int id = 1; id <= 2; id++) {
            PlayerStat ps = GetPS(id);
            sb.AppendLine("Player " + id);
            sb.AppendLine(ps.name + "," + ps.character);
            sb.AppendLine("Tiles drawn," + ps.draws);
            sb.AppendLine("Tiles dropped," + ps.drops);
            sb.AppendLine("Tiles swapped," + ps.swaps);
            sb.AppendLine("Matches," + ps.matches + ",...match-3s," + ps.match3s);
            sb.AppendLine(",,...match-4s," + ps.match4s);
            sb.AppendLine(",,...match-5s," + ps.match5s);
            sb.AppendLine("Cascades," + ps.cascades + ",...longest," + ps.longestCascade);
            sb.AppendLine("Tiles removed," + ps.tilesRemoved);
            sb.AppendLine("Spells cast," + ps.spellsCast);
            sb.AppendLine("Turns timed out," + ps.timeouts).AppendLine("");
        }

        // TODO write num of each spell cast from EffectCont.tagDict
        File.WriteAllText(filePath, sb.ToString());
    }

    void Report(string str) {
        report.AppendLine(str);
        mm.uiCont.newsfeed.UpdateNewsfeed(str);
    }

    public string GetReportText() { return report.ToString(); }

    public void SaveReportTXT() {
        DateTime dt = DateTime.Now;
        string filePath = "MageMatch_" + dt.Year + "-" + dt.Month + "-" + dt.Day + "_";
        filePath += dt.Hour + "-" + dt.Minute + "-" + dt.Second + "_Report";
        filePath = @"/" + filePath + ".txt";

        File.WriteAllText(filePath, GetReportText());
    }

}
