using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System;

public class Stats {

    public int turns = 0;

    private int commishMatches, commishDrops;

    private MageMatch mm;
    private StringBuilder report;

    private class PlayerStat {
        public string name;
        public string character;
        public string loadout;
        public int draws, drops, swaps, matches, cascades, tilesRemoved, spellsCast, timeouts;
        public int dmgDealt, dmgTaken, healingDone;
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

        mm.eventCont.turnBegin += OnTurnBegin;
        mm.eventCont.turnEnd += OnTurnEnd;
        mm.eventCont.timeout += OnTimeout;
        mm.eventCont.commishDrop += OnCommishDrop;
        mm.eventCont.commishMatch += OnCommishMatch;

        mm.eventCont.draw += OnDraw;
        mm.eventCont.drop += OnDrop;
        mm.eventCont.swap += OnSwap;
        mm.eventCont.spellCast += OnSpellCast;

        mm.eventCont.match += OnMatch;
        mm.eventCont.cascade += OnCascade;
        mm.eventCont.tileRemove += OnTileRemove;
        mm.eventCont.playerHealthChange += OnPlayerHealthChange;
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

    public void OnTurnBegin(int id) {
        report.AppendLine("\nT" + turns + " - ");
    }

    public void OnTurnEnd(int id) {
        turns++;
        report.AppendLine("\nC" + turns + " - ");
    }

    public void OnTimeout(int id) {
        report.AppendLine("...the turn timer timed out!");
        GetPS(id).timeouts++;
    }

    public void OnCommishDrop(Tile.Element elem, int col) {
        commishDrops++;
        report.AppendLine("C-drop " + Tile.ElementToChar(elem) + " col" + col);
    }

    public void OnCommishMatch(int count) {
        commishMatches += count;
        report.AppendLine("...Commish match.");
    }

    #region GameAction subscriptions

    public void OnDraw(int id, Tile.Element elem, bool dealt) {
        if(dealt)
            report.AppendLine("Deal p"+id+" " + Tile.ElementToChar(elem));
        else
            report.AppendLine("Draw " + Tile.ElementToChar(elem));
        GetPS(id).draws++;
    }

    public void OnDrop(int id, Tile.Element elem, int col) {
        if (!mm.menu) {
            report.AppendLine("Drop " + Tile.ElementToChar(elem) + " col" + col);
            GetPS(id).drops++;
        } else
            report.AppendLine("menu Drop col" + col);
    }

    public void OnSwap(int id, int c1, int r1, int c2, int r2) {
        if (!mm.menu) {
            report.AppendLine("Swap (" + c1 + "," + r1 + ")(" + c2 + "," + r2 + ")");
            GetPS(id).swaps++;
        } else
            report.AppendLine("menu Swap (" + c1 + "," + r1 + ")(" + c2 + "," + r2 + ")");
    }

    public void OnSpellCast(int id, Spell spell) {
        report.AppendLine("Spell " + spell.name);
        GetPS(id).spellsCast++;
    }

    #endregion

    public void OnMatch(int id, int count) {
        report.AppendLine("...match");
        GetPS(id).matches += count;
    }

    public void OnCascade(int id, int chain) {
        //Debug.Log("STATS: OnCascade called. chain = " + chain);
        PlayerStat ps = GetPS(id);
        ps.cascades++;
        if (ps.longestCascade < chain)
            ps.longestCascade = chain;
    }

    public void OnTileRemove(int id, TileBehav tb) {
        if (!mm.IsCommishTurn()) {
            if (!mm.menu)
                GetPS(id).tilesRemoved++;
            else
                report.AppendLine("menu Remove (" + tb.tile.col + "," + tb.tile.row + ")");
        }
    }

    public void OnPlayerHealthChange(int id, int amount, bool dealt, bool sent) {
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
