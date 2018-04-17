using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using MMDebug;

public class Stats {

    public int turns = 1;

    private MageMatch _mm;
    private StringBuilder _report;
    private int _commishDrops;

    private class PlayerStat {
        public string name;
        public string character;
        public int draws, drops, swaps, tilesRemoved, spellsCast, timeouts, discards;
        public int dmgDealt, dmgTaken, healingDone;
    }

    private PlayerStat _ps1, _ps2;

    public Stats(Player p1, Player p2) {
        _mm = GameObject.Find("board").GetComponent<MageMatch>();
        _ps1 = new PlayerStat() {
            name = p1.name,
            character = p1.character.ch.ToString()
        };
        _ps2 = new PlayerStat() {
            name = p2.name,
            character = p2.character.ch.ToString()
        };

        InitReport();

        _mm.eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.Report);
        _mm.eventCont.AddTurnEndEvent(OnTurnEnd, EventController.Type.Stats);
        _mm.eventCont.timeout += OnTimeout;
        //_mm.eventCont.commishDrop += OnCommishDrop;
        //_mm.eventCont.commishMatch += OnCommishMatch;

        _mm.eventCont.AddDrawEvent(OnDraw, EventController.Type.Stats, EventController.Status.Begin);
        _mm.eventCont.AddDropEvent(OnDrop, EventController.Type.Stats, EventController.Status.Begin);
        _mm.eventCont.AddSwapEvent(OnSwap, EventController.Type.Stats, EventController.Status.Begin);
        _mm.eventCont.AddSpellCastEvent(OnSpellCast, EventController.Type.Stats, EventController.Status.Begin);
        //_mm.eventCont.AddDiscardEvent(OnDiscard, EventController.Type.Stats);

        //_mm.eventCont.AddMatchEvent(OnMatch, EventController.Type.Stats);
        //mm.eventCont.cascade += OnCascade;
        _mm.eventCont.tileRemove += OnTileRemove;
        _mm.eventCont.playerHealthChange += OnPlayerHealthChange;
    }

    void InitReport() {
        _report = new StringBuilder();
        _report.Append("  # " + _ps1.name + " (" + _ps1.character + ") vs ");
        _report.AppendLine(_ps2.name + " (" + _ps2.character + ")");
        _report.AppendLine("  # SETUP"); 
    }

    PlayerStat GetPS(int id) {
        if (id == 1)
            return _ps1;
        else
            return _ps2;
    }

    int GetOpponentID(int id) {
        if (id == 1)
            return 2;
        else
            return 1;
    }

    public IEnumerator OnTurnBegin(int id) {
        _report.AppendLine("\n  # TURN " + turns + " (p"+id+")");
        yield return null;
    }

    public IEnumerator OnTurnEnd(int id) {
        _report.AppendLine("\n  # COMMISH TURN");
        turns++;
        yield return null;
    }

    public void OnTimeout(int id) {
        //_report.AppendLine("...the turn timer timed out!");
        GetPS(id).timeouts++;
    }

    //public void OnCommishDrop(string hextag, int col) {
    //    _commishDrops++;
    //    _report.AppendLine("  # C-DROP " + hextag + " col" + col);
    //}

    //public void OnCommishMatch(string[] seqs) {
    //    _commishMatches += seqs.Length;
    //    _report.AppendLine("...Commish made "+seqs.Length+" match(es)");
    //}

    #region GameAction subscriptions
    public IEnumerator OnDraw(int id, string tag, bool playerAction, bool dealt) {
        if (dealt)
            Report("  # DEAL " + tag);
        else if (playerAction)
            Report("DRAW " + tag);
        GetPS(id).draws++;
        yield return null;
    }

    public IEnumerator OnDrop(int id, bool playerAction, string tag, int col) {
        if (playerAction) {
            string s = "DROP " + tag;
            if (!Hex.IsCharm(tag))
                s += " col" + col;
            Report(s);
            GetPS(id).drops++;
        }
        yield return null;
    }

    public IEnumerator OnSwap(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        if (playerAction) {
            Report(string.Format("SWAP ({0},{1}) ({2},{3})", c1, r1, c2, r2));
            GetPS(id).swaps++;
        }
        yield return null;
    }

    public IEnumerator OnSpellCast(int id, Spell spell, TileSeq prereq) {
        Report("CAST spell" + spell.index, false);
        Report("$ SELECT " + prereq.SeqAsString(true, true), false);
        Report("  # " + spell.name, false);
        _mm.uiCont.newsfeed.UpdateNewsfeed("CAST " + spell.name);
        GetPS(id).spellsCast++;
        yield return null;
    }
    #endregion


    public void OnTileTarget(TileBehav tb) {
        Report("...targeted " + tb.PrintCoord() + " " + tb.hextag);
    }

    public void OnTileAreaTarget(TileBehav tb) {
        
    }

    public void OnDragTarget(List<TileBehav> tbs) {
        
    }

    public void OnTileRemove(int id, TileBehav tb) {
        GetPS(id).tilesRemoved++;
    }

    public void OnPlayerHealthChange(int id, int amount, int newHealth, bool dealt) {
        if (dealt) {
            GetPS(GetOpponentID(id)).dmgDealt -= amount;
            //_report.AppendLine("...p" + GetOpponentID(id) + " dealt " + (-amount) + " dmg...");
        }
        if (amount < 0)
            GetPS(id).dmgTaken -= amount;
        else
            GetPS(id).healingDone += amount;
        //_report.AppendLine("...p"+id+" changes health by " + amount);
    }


    public void Report(string str, bool showInNewsfeed = true) {
        _report.AppendLine(str);
        _mm.debugTools.UpdateReport(_report.ToString());

        if (showInNewsfeed)
            _mm.uiCont.newsfeed.UpdateNewsfeed(str);
    }

    public string GetReportText() { return _report.ToString(); }


    public void SaveFiles() {
        DateTime dt = DateTime.Now;
        string timestamp = dt.Year + "-" + dt.Month + "-" + dt.Day + "_";
        timestamp += dt.Hour + "-" + dt.Minute + "-" + dt.Second;

        string dirPath = Application.persistentDataPath;
        MMLog.Log("Stats", "black", "dataPath is " + dirPath + "...");
        dirPath = Directory.CreateDirectory(dirPath + "/" + timestamp).FullName;
        MMLog.Log("Stats", "black", "saving files to " + dirPath + "...");

        SaveReportTXT(dirPath, timestamp);
        SaveStatsCSV(dirPath, timestamp);
        MMLog.SaveReportTXT(dirPath, timestamp);
    }

    void SaveReportTXT(string path, string timestamp) {
        string filename = "MageMatch_" + timestamp + "_Report";
        filename = @"/" + filename + ".txt";

        File.WriteAllText(path + filename, "  # " + timestamp + "\n" + GetReportText());
    }

    public void SaveStatsCSV(string path, string timestamp) {
        DateTime dt = DateTime.Now;
        string filename = "MageMatch_" + timestamp + "_Stats";
        filename = @"/" + filename + ".csv";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Turns complete," + turns);
        sb.AppendLine("Commish drops," + _commishDrops).AppendLine("");
        //sb.AppendLine("Commish matches," + _commishMatches).AppendLine("");
        for (int id = 1; id <= 2; id++) {
            PlayerStat ps = GetPS(id);
            sb.AppendLine("Player " + id);
            sb.AppendLine(ps.name + "," + ps.character);
            sb.AppendLine("Tiles drawn," + ps.draws);
            sb.AppendLine("Tiles dropped," + ps.drops);
            sb.AppendLine("Tiles swapped," + ps.swaps);
            //sb.AppendLine("Matches," + ps.matches + ",...match-3s," + ps.match3s);
            //sb.AppendLine(",,...match-4s," + ps.match4s);
            //sb.AppendLine(",,...match-5s," + ps.match5s);
            sb.AppendLine("Tiles removed," + ps.tilesRemoved);
            sb.AppendLine("Spells cast," + ps.spellsCast);
            sb.AppendLine("Turns timed out," + ps.timeouts).AppendLine("");
        }

        // TODO write num of each spell cast from EffectCont.tagDict
        File.WriteAllText(path + filename, sb.ToString());
    }
}
