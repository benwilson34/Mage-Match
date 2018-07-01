using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;

public class ReplayEngine {

    private static MageMatch _mm;
    private static string[] _fileLines;
    private static int _linePointer = 0;
    //private static bool _readLine = false; // not needed currently

    public static void Init(MageMatch mm) {
        _mm = mm;
        Load(_mm.debugSettings.replayFile);
        _mm.AddEventContLoadEvent(OnEventContLoaded);
    }

    public static void OnEventContLoaded() {
        EventController.AddTurnEndEvent(OnTurnEnd, MMEvent.Behav.FirstStep);
    }

    static IEnumerator OnTurnEnd(int id) {
        // i don't like this but it will work for now
        //_linePointer += 2;
        yield return null;
    }

    static void Load(string filepath) {
        if (File.Exists(filepath)) {
            Debug.LogWarning("Found the file.");
        } else {
            Debug.LogWarning("Couldn't find " + filepath);
        }
        _fileLines = File.ReadAllLines(filepath);

        // parse gamemode
        _mm.gameMode = (MageMatch.GameMode)Enum.Parse(
            typeof(MageMatch.GameMode), Split(_fileLines[1])[2]);

        // parse player name, ch, loadout FOR players 1 & 2
        SetPlayerSettings(1);
        SetPlayerSettings(2);

        _linePointer = 11; // set the pointer to right after SETUP
    }

    static void SetPlayerSettings(int id) {
        var index = id == 1 ? 3 : 7; // starting line in the file
        var name = _fileLines[index].Substring(9); // not super safe but oh well
        var ch = (Character.Ch)Enum.Parse(
            typeof(Character.Ch), Split(_fileLines[index + 1])[2]);
        _mm.gameSettings.SetPlayerInfo(id, name, ch);

        var loadout = new List<string>(Split(_fileLines[index + 2]));
        loadout.RemoveRange(0, 2);
        _mm.gameSettings.SetPlayerLoadout(id, loadout.ToArray());
    }

    static string[] Split(string line) {
        return line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    }

    static string Unsplit(string[] tokens) { return string.Join(" ", tokens); }

    public static IEnumerator StartReplay() {
        do {
            // handle turn switching somehow?
            yield return new WaitUntil(() => !_mm.switchingTurn);

            yield return HandleCommand(GetNextTokens());

            if (_mm.debugSettings.animateReplay) // could just be AnimationController.WaitForSeconds()
                yield return new WaitForSeconds(.25f);
        } while (!EndOfFile);

        if (_mm.debugTools.DebugMenuOpen)
            _mm.debugTools.ToggleDebugActiveState();

        MMDebug.MMLog.LogWarning("REPLAY: Done replaying!");
    }

    static string[] GetNextTokens() {
        string[] tokens = new string[0];

        // skip empty lines and "comments"
        while (tokens.Length == 0 || tokens[0] == "#") {
            if (EndOfFile) {
                return new string[0];
            }
            Debug.LogWarning("REPLAY: Read " + _fileLines[_linePointer]);
            tokens = Split(_fileLines[_linePointer]);
            _linePointer++;
        } 
        return tokens;
    }

    static bool EndOfFile { get { return _linePointer == _fileLines.Length; } }

    static IEnumerator HandleCommand(string[] tokens) {
        int id = _mm.ActiveP.ID;

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
                MMDebug.MMLog.LogError("REPLAY: Read \"" + Unsplit(tokens) + "\"");
                //_readLine = false;
                yield break;
        }

        //_readLine = true;
        yield return null;
    }

    static void HandleScriptCommand(string[] tokens) {
        if (tokens[1] == "DEBUG") {
            HandleDebugCommand(tokens);
        } else {
            MMDebug.MMLog.LogError("REPLAY: Read \"" + string.Join(" ", tokens) + "\"");
        }
    }

    static void HandleDebugCommand(string[] tokens) {
        int[] coord;
        int id, amt;
        switch (tokens[2]) {
            case "start":
                _mm.debugTools.ToggleDebugActiveState();
                break;
            case "end":
                _mm.debugTools.ToggleDebugActiveState();
                break;

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
        //string cmd = _fileLines[_linePointer];
        string[] tokens = GetNextTokens();
        if (tokens[1] != "SELECT") {
            MMDebug.MMLog.LogError("REPLAY: Looking for SELECT command and found: " + Unsplit(tokens));
            return null;
        }

        TileSeq seq = new TileSeq();
        for (int i = 3; i < tokens.Length; i++) {
            int[] coord = ParseCoord(tokens[i]);
            seq.sequence.Add(HexGrid.GetTileAt(coord[0], coord[1]));
        }

        Debug.LogWarning("REPLAY: Selecting " + seq.SeqAsString(true, true));
        //_linePointer += 2; // this is to compensate for the spell name comment
        return seq;
    }

    public static int[] GetSyncedRands() {
        //string cmd = _fileLines[_linePointer];
        string[] tokens = GetNextTokens();
        if (tokens[1] != "SYNC") {
            MMDebug.MMLog.LogError("REPLAY: Looking for SYNC command and found: " + Unsplit(tokens));
            return null;
        }

        int count = ParseTokenInt(tokens[2]);
        MMDebug.MMLog.LogWarning("REPLAY: Getting " + count + " rands");

        int[] rands = new int[count];
        for (int i = 0, r = 3; i < count; i++, r++) {
            //MMDebug.MMLog.LogWarning("REPLAY: Token = " + tokens[r]);
            rands[i] = int.Parse(tokens[r]);
        }
        //_linePointer++;
        return rands;
    }

    public static void GetPrompt() {
        //string cmd = _fileLines[_linePointer];
        string[] tokens = GetNextTokens();
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
            Debug.LogWarning("REPLAY: Keeping Quickdraw.");
            Prompt.SetQuickdrawHand();
        }
        //_linePointer++;
    }

    public static void GetTargets() {
        //string cmd = _fileLines[_linePointer];
        do {
            string[] tokens = GetNextTokens();
            Debug.LogWarning("REPLAY: Parsed " + Unsplit(tokens));
            int[] coord = ParseCoord(tokens[3]);
            if (tokens[2] == "TILE") {
                TileBehav tb = HexGrid.GetTileBehavAt(coord[0], coord[1]);
                Targeting.OnTBTarget(tb);
            } else {       // CELL
                CellBehav cb = HexGrid.GetCellBehavAt(coord[0], coord[1]);
                Targeting.OnCBTarget(cb);
            }

            //_linePointer++;
            //cmd = _fileLines[_linePointer];
            //tokens = GetNextTokens();
        } while (NextLineIsTarget());
    }

    static bool NextLineIsTarget() {
        return !EndOfFile && _fileLines[_linePointer].Contains(" TARGET ");
    }
}
