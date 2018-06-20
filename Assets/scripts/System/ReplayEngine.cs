using System.Collections;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

public class ReplayEngine {

    private static MageMatch _mm;
    private static string[] _fileLines;
    private static int _linePointer = 0;
    private static bool _readLine = false; // not needed currently

    public static void Init(MageMatch mm) {
        _mm = mm;
    }

    public static void Load(string replayName) {
        string filepath = string.Format("{0}/{1}/MageMatch_{2}_Report.txt", Application.persistentDataPath, replayName, replayName);
        _fileLines = File.ReadAllLines(filepath);

        // TODO will need to grab player/character info from header but I'll skip it for now
        _linePointer = 3;
    }

    public static IEnumerator StartReplay() {
        do {
            // line pointer needs to always be on the NEXT line, that's why this looks weird
            _linePointer++;
            yield return HandleCommand(_fileLines[_linePointer-1]);

            // handle turn switching somehow?
            yield return new WaitUntil(() => _mm.GetState() != MageMatch.State.TurnSwitching);

            if (_mm.debugSettings.animateReplay) // could just be AnimationController.WaitForSeconds()
                yield return new WaitForSeconds(.25f);
        } while (_linePointer < _fileLines.Length);

        MMDebug.MMLog.LogWarning("REPLAY: Done replaying!");
    }

    static IEnumerator HandleCommand(string cmd) {
        int id = _mm.ActiveP.ID;
        string[] tokens = cmd.Split(' ');

        switch (tokens[0]) {
            case "$":
                HandleScriptCommand(tokens);
                break;

            case "DRAW":
                yield return _mm._Draw(id, 1, EventController.HandChangeState.PlayerDraw);
                break;
            case "DROP":
                Hex hex = _mm.ActiveP.Hand.GetHex(tokens[1]);
                _mm.ActiveP.Hand.Remove(hex);
                int col = -1;
                if(tokens.Length == 3) // tile, not charm
                    col = ParseTokenInt(tokens[2]);
                yield return _mm._Drop(hex, col, EventController.DropState.PlayerDrop);
                break;
            case "SWAP":
                int[] coordA = ParseCoord(tokens[1]), coordB = ParseCoord(tokens[2]);
                yield return _mm._SwapTiles(coordA[0], coordA[1], coordB[0], coordB[1], EventController.SwapState.PlayerSwap);
                break;
            case "CAST":
                int spellNum = ParseTokenInt(tokens[1]);
                yield return _mm._CastSpell(spellNum);
                break;

            default:
                MMDebug.MMLog.LogWarning("REPLAY: Read \"" + cmd + "\"");
                _readLine = false;
                yield break;
        }

        _readLine = true;
        yield return null;
    }

    static void HandleScriptCommand(string[] tokens) {
        if (tokens[1] == "DEBUG") {
            HandleDebugCommand(tokens);
        } else {
            MMDebug.MMLog.LogError("REPLAY: Read \"" + string.Join(" ", tokens) + "\"");
            _readLine = false;
        }
    }

    static void HandleDebugCommand(string[] tokens) {
        int[] coord;
        int id, amt;
        switch (tokens[2]) {
            case "INSERT":
                coord = ParseCoord(tokens[4]);
                _mm.debugTools.Insert(tokens[3], coord[0], coord[1]);
                break;
            case "DESTROY":
                coord = ParseCoord(tokens[3]);
                _mm.debugTools.Destroy(coord[0], coord[1]);
                break;
            case "ENCHANT":
                coord = ParseCoord(tokens[4]);
                _mm.debugTools.Enchant(coord[0], coord[1], tokens[3]);
                break;
            case "CLEAR":
                coord = ParseCoord(tokens[3]);
                _mm.debugTools.Clear(coord[0], coord[1]);
                break;
            case "ADDTOHAND":
                id = Hex.TagPlayer(tokens[3]);
                _mm.debugTools.AddToHand(id, tokens[3]);
                break;
            case "DISCARD":
                _mm.debugTools.Discard(tokens[3]);
                break;
            case "HEALTH":
                id = ParseTokenInt(tokens[3]);
                amt = int.Parse(tokens[4]);
                _mm.debugTools.ChangeHealth(id, amt);
                break;
            case "METER":
                id = ParseTokenInt(tokens[3]);
                amt = int.Parse(tokens[4]);
                _mm.debugTools.ChangeMeter(id, amt);
                break;
        }
    }

    static int[] ParseCoord(string str) {
        //MMDebug.MMLog.LogWarning("REPLAY: Parsing " + str);
        int[] coord = new int[2];
        coord[0] = int.Parse(str.Substring(1, 1));
        coord[1] = int.Parse(str.Substring(3, 1));
        return coord;
    }

    static public int ParseTokenInt(string token) {
        return int.Parse(Regex.Match(token, @"\d+").Value);
    }

    public static TileSeq GetSpellSelection() {
        string cmd = _fileLines[_linePointer];
        string[] tokens = cmd.Split(' ');
        if (tokens[1] != "SELECT") {
            MMDebug.MMLog.LogError("REPLAY: Looking for SELECT command and found: " + cmd);
            return null;
        }

        TileSeq seq = new TileSeq();
        for (int i = 3; i < tokens.Length - 1; i++) {
            int[] coord = ParseCoord(tokens[i]);
            seq.sequence.Add(HexGrid.GetTileAt(coord[0], coord[1]));
        }
        _linePointer += 2; // this is to compensate for the spell name comment
        return seq;
    }

    public static int[] GetSyncedRands() {
        string cmd = _fileLines[_linePointer];
        string[] tokens = cmd.Split(' ');
        if (tokens[1] != "SYNC") {
            MMDebug.MMLog.LogError("REPLAY: Looking for SYNC command and found: " + cmd);
            return null;
        }

        int count = ParseTokenInt(tokens[2]);
        MMDebug.MMLog.LogWarning("REPLAY: Getting " + count + " rands");

        int[] rands = new int[count];
        for (int i = 0, r = 3; i < count; i++, r++) {
            //MMDebug.MMLog.LogWarning("REPLAY: Token = " + tokens[r]);
            rands[i] = int.Parse(tokens[r]);
        }
        _linePointer++;
        return rands;
    }

    public static void GetPrompt() {
        string cmd = _fileLines[_linePointer];
        string[] tokens = cmd.Split(' ');
        if (tokens[2] == "DROP") {
            Hex hex = _mm.ActiveP.Hand.GetHex(tokens[3]);
            int col = -1;
            if (tokens.Length == 5)
                col = ParseTokenInt(tokens[4]);
            Prompt.SetDrop(hex, col);
        } else if (tokens[2] == "SWAP") {
            int[] coordA = ParseCoord(tokens[3]), coordB = ParseCoord(tokens[4]);
            Prompt.SetSwaps(coordA[0], coordA[1], coordB[0], coordB[1]);
        } else {  // KEEP QUICKDRAW
            Prompt.SetQuickdrawHand();
        }
        _linePointer++;
    }

    public static void GetTargets() {
        string cmd = _fileLines[_linePointer];
        string[] tokens = cmd.Split(' ');
        do {
            int[] coord = ParseCoord(tokens[3]);
            if (tokens[2] == "TILE") {
                TileBehav tb = HexGrid.GetTileBehavAt(coord[0], coord[1]);
                Targeting.OnTBTarget(tb);
            } else {       // CELL
                CellBehav cb = HexGrid.GetCellBehavAt(coord[0], coord[1]);
                Targeting.OnCBTarget(cb);
            }

            _linePointer++;
            cmd = _fileLines[_linePointer];
            tokens = cmd.Split(' ');
        } while (tokens[1] == "TARGET");
    }
}
