using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using MMDebug;

public static class Report {

    public static int Turns { get { return _turns; } }
    private static int _turns = 1;

    private static MageMatch _mm;
    private static StringBuilder _report;
    private static int _commishDrops;

    private class PlayerStat {
        public string name;
        public string character;
        public int draws, drops, swaps, tilesRemoved, spellsCast, timeouts, discards;
        public int dmgDealt, dmgTaken, healingDone;
    }

    private static PlayerStat _ps1, _ps2;

    public static void Init(MageMatch mm) {
        _mm = mm;
        _mm.AddEventContLoadEvent(OnEventContLoaded);
        _mm.AddPlayersLoadEvent(OnPlayersLoaded);
    }

    public static void OnEventContLoaded() {
        EventController.AddTurnBeginEvent(OnTurnBegin, MMEvent.Behav.Report);
        EventController.AddTurnEndEvent(OnTurnEnd, MMEvent.Behav.Stats);
        EventController.timeout += OnTimeout;
        //EventController.commishDrop += OnCommishDrop;
        //EventController.commishMatch += OnCommishMatch;

        EventController.AddHandChangeEvent(OnHandChange, MMEvent.Behav.Stats, MMEvent.Moment.Begin);
        EventController.AddDropEvent(OnDrop, MMEvent.Behav.Stats, MMEvent.Moment.Begin);
        EventController.AddSwapEvent(OnSwap, MMEvent.Behav.Stats, MMEvent.Moment.Begin);
        EventController.AddSpellCastEvent(OnSpellCast, MMEvent.Behav.Stats, MMEvent.Moment.Begin);
        //EventController.AddDiscardEvent(OnDiscard, EventController.Type.Stats);

        //EventController.AddMatchEvent(OnMatch, EventController.Type.Stats);
        //mm.eventCont.cascade += OnCascade;
        EventController.tileRemove += OnTileRemove;
        EventController.playerHealthChange += OnPlayerHealthChange;
    }

    public static void OnPlayersLoaded() {
        Player p = _mm.GetPlayer(1);
        _ps1 = new PlayerStat() {
            name = p.Name,
            character = p.Character.ch.ToString()
        };
        p = _mm.GetPlayer(2);
        _ps2 = new PlayerStat() {
            name = p.Name,
            character = p.Character.ch.ToString()
        };

        InitReport();
    }

    static void InitReport() {
        _report = new StringBuilder();
        _report.AppendLine("  # MODE " + _mm.gameMode.ToString());
        // TODO need to get both players' loadouts as well
        ReportPlayer(_mm.GetPlayer(1));
        ReportPlayer(_mm.GetPlayer(2));
        _report.AppendLine("  # SETUP");
    }

    static void ReportPlayer(Player p) {
        _report.AppendLine("  # PLAYER " + p.ID);
        _report.AppendLine("  # name " + p.Name);
        _report.AppendLine("  # character " + p.Character.ch.ToString());
        _report.AppendLine("  # loadout " + 
            string.Join(" ", _mm.gameSettings.GetLoadout(p.ID)));
    }

    static PlayerStat GetPS(int id) {
        if (id == 1)
            return _ps1;
        else
            return _ps2;
    }

    static int GetOpponentID(int id) {
        if (id == 1)
            return 2;
        else
            return 1;
    }

    public static IEnumerator OnTurnBegin(int id) {
        _report.AppendLine("\n  # TURN " + _turns + " (p" + id + ")");
        yield return null;
    }

    public static IEnumerator OnTurnEnd(int id) {
        _report.AppendLine("\n  # COMMISH TURN");
        _turns++;
        yield return null;
    }

    public static void OnTimeout(int id) {
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
    public static IEnumerator OnHandChange(HandChangeEventArgs args) {
        bool countAsDraw = true;
        if (args.state == EventController.HandChangeState.TurnBeginDeal)
            ReportLine("  # DEAL " + args.hextag);
        else if (args.state == EventController.HandChangeState.PlayerDraw)
            ReportLine("DRAW " + args.hextag);
        else if (args.state == EventController.HandChangeState.Discard) {
            GetPS(args.id).discards++;
        }

        if (countAsDraw)
            GetPS(args.id).draws++;
        yield return null;
    }

    public static IEnumerator OnDrop(DropEventArgs args) {
        if (args.state == EventController.DropState.PlayerDrop) {
            string s = "DROP " + args.hex.hextag;
            if (!Hex.IsCharm(args.hex.hextag))
                s += " col" + args.col;
            ReportLine(s);
            GetPS(args.id).drops++;
        } else if (args.state == EventController.DropState.CommishDrop) {
            _commishDrops++;
        }
 
        yield return null;
    }

    public static IEnumerator OnSwap(SwapEventArgs args) {
        if (args.state == EventController.SwapState.PlayerSwap) {
            ReportLine(string.Format("SWAP ({0},{1}) ({2},{3})", args.c1, args.r1, args.c2, args.r2));
            GetPS(args.id).swaps++;
        }
        yield return null;
    }

    public static IEnumerator OnSpellCast(int id, Spell spell, TileSeq prereq) {
        ReportLine("CAST spell" + spell.index, false);
        ReportLine("$ SELECT " + prereq.SeqAsString(true, true), false);
        ReportLine("  # " + spell.name, false);
        _mm.uiCont.newsfeed.UpdateNewsfeed("CAST " + spell.name);
        GetPS(id).spellsCast++;
        yield return null;
    }
    #endregion


    public static void OnTileTarget(TileBehav tb) {
        ReportLine("...targeted " + tb.PrintCoord() + " " + tb.hextag);
    }

    public static void OnTileAreaTarget(TileBehav tb) {

    }

    public static void OnDragTarget(List<TileBehav> tbs) {

    }

    public static void OnTileRemove(int id, TileBehav tb) {
        GetPS(id).tilesRemoved++;
    }

    public static void OnPlayerHealthChange(int id, int amount, int newHealth, bool dealt) {
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


    public static void ReportLine(string str, bool showInNewsfeed = true) {
        _report.AppendLine(str);
        _mm.debugTools.UpdateReport(_report.ToString());

        if (showInNewsfeed)
            _mm.uiCont.newsfeed.UpdateNewsfeed(str);
    }

    public static string GetReportText() { return _report.ToString(); }


    public static void SaveFiles(string customName = "") {
        DateTime dt = DateTime.Now;
        string timestamp = dt.Year + "-" + dt.Month + "-" + dt.Day + "_";
        timestamp += dt.Hour + "" + dt.Minute + "-" + dt.Second;

        string dirPath = Application.persistentDataPath + "/Replays";
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        dirPath = dirPath + "/" + timestamp;
        if (customName.Length > 0)
            dirPath += "_" + customName;

        MMLog.Log("Stats", "black", "dataPath is " + dirPath + "...");
        dirPath = Directory.CreateDirectory(dirPath).FullName;
        MMLog.Log("Stats", "black", "saving files to " + dirPath + "...");

        SaveReportTXT(dirPath, timestamp);
        SaveStatsCSV(dirPath, timestamp);
        MMLog.SaveReportTXT(dirPath, timestamp);
    }

    static void SaveReportTXT(string path, string timestamp) {
        string filename = timestamp + "_Report";
        filename = @"/" + filename + ".txt";

        File.WriteAllText(path + filename, "  # " + timestamp + "\n" + GetReportText());
    }

    public static void SaveStatsCSV(string path, string timestamp) {
        DateTime dt = DateTime.Now;
        string filename = timestamp + "_Stats";
        filename = @"/" + filename + ".csv";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Turns complete," + _turns);
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

        // TODO write num of each spell cast from EffectController.tagDict
        File.WriteAllText(path + filename, sb.ToString());
    }
}
