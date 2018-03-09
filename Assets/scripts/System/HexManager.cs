using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class HexManager { // should maybe inherit MonoBehaviour? or maybe static?

    public int removing = 0;
    public Sprite flipSprite;

    private Dictionary<string, int>[] _tagDicts;
    private MageMatch _mm;
    private HexGrid _hexGrid;

    private GameObject _firePF, _waterPF, _earthPF, _airPF, _muscPF; // basic tile prefabs
    private GameObject _stonePF, _emberPF, _tombstonePF; // token prefabs
    private GameObject _cons_sample;
    private GameObject _cons_HRform, _cons_partySnacks, _cons_proteinPills, _cons_leeches;
    private GameObject _cons_bandages, _cons_waterLily;
    private GameObject _cons_danceShoes, _cons_burningBracers, _cons_molotov, _cons_fiveAlarmBell;

    public HexManager(MageMatch mm) {
        this._mm = mm;
        _hexGrid = mm.hexGrid;
        _tagDicts = new Dictionary<string, int>[3];
        for (int i = 0; i < 3; i++) {
            _tagDicts[i] = new Dictionary<string, int>();
        }
        LoadPrefabs();
    }

    void LoadPrefabs() {
        _firePF = Resources.Load("prefabs/hexes/basic_fire") as GameObject;
        _waterPF = Resources.Load("prefabs/hexes/basic_water") as GameObject;
        _earthPF = Resources.Load("prefabs/hexes/basic_earth") as GameObject;
        _airPF = Resources.Load("prefabs/hexes/basic_air") as GameObject;
        _muscPF = Resources.Load("prefabs/hexes/basic_muscle") as GameObject;

        // TODO load only the tokens + consumables that either player will use??
        _stonePF = Resources.Load("prefabs/hexes/token_stone") as GameObject;
        _emberPF = Resources.Load("prefabs/hexes/token_ember") as GameObject;
        _tombstonePF = Resources.Load("prefabs/hexes/token_tombstone") as GameObject;


        _cons_sample = Resources.Load("prefabs/hexes/cons_sample") as GameObject;

        _cons_HRform = Resources.Load("prefabs/hexes/cons_HRform") as GameObject;
        _cons_partySnacks = Resources.Load("prefabs/hexes/cons_partySnacks") as GameObject;
        _cons_proteinPills = Resources.Load("prefabs/hexes/cons_proteinPills") as GameObject;
        _cons_leeches = Resources.Load("prefabs/hexes/cons_leeches") as GameObject;

        _cons_bandages = Resources.Load("prefabs/hexes/cons_bandages") as GameObject;
        _cons_waterLily = Resources.Load("prefabs/hexes/cons_waterLily") as GameObject;

        _cons_danceShoes = Resources.Load("prefabs/hexes/cons_danceShoes") as GameObject;
        _cons_burningBracers = Resources.Load("prefabs/hexes/cons_burningBracers") as GameObject;
        _cons_molotov = Resources.Load("prefabs/hexes/cons_molotov") as GameObject;
        _cons_fiveAlarmBell = Resources.Load("prefabs/hexes/cons_fiveAlarmBell") as GameObject;


        flipSprite = Resources.Load<Sprite>("sprites/hex-back");
    }

    string GenFullTag(int id, string cat, string type) {
        string fullTag = "p" + id + "-" + cat + "-" + type + "-";
        Dictionary<string, int> dict = _tagDicts[id - 1];
        if (dict.ContainsKey(type)) {
            dict[type]++;
            fullTag += dict[type].ToString("D3");
        } else {
            dict.Add(type, 1);
            fullTag += "001";
        }
        MMLog.Log("TILEMAN", "black", "...generated HandObject with tag " + fullTag);
        return fullTag;
    }

    //public Hex GenerateRandomHex(Player p) {
    //    //string genTag = p.character.GenerateHexTag();
    //    string genTag = p.deck.GetNextHextag();
    //    return GenerateHex(p.id, genTag);
    //}

    //public HandObject GetRandomHex() { return genHex; }

    public Hex GenerateHex(int id, string genTag) {
        MMLog.Log("HEXMAN", "black", "Generating tile from genTag \"" + genTag + "\"");
        string type = Hex.TagType(genTag);
        switch (Hex.TagCat(genTag)) {
            case "B":
                return GenerateTile(id, Tile.CharToElement(type[0]));
            case "T":
                return GenerateToken(id, type);
            case "C":
                return GenerateConsumable(id, type);
            default:
                MMLog.LogError("HEXMAN: Failed " + genTag + " with cat="+Hex.TagCat(genTag));
                return null;
        }
    }

    // maybe return TB?
    public Hex GenerateTile(int id, Tile.Element element) {
        return GenerateTile(id, element, GameObject.Find("tileSpawn").transform.position);
    }

    // maybe return TB?
    public Hex GenerateTile(int id, Tile.Element element, Vector3 position) {
        GameObject go;
        string type;
        switch (element) {
            case Tile.Element.Fire:
                go = GameObject.Instantiate(_firePF, position, Quaternion.identity);
                type = "F";
                break;
            case Tile.Element.Water:
                go = GameObject.Instantiate(_waterPF, position, Quaternion.identity);
                type = "W";
                break;
            case Tile.Element.Earth:
                go = GameObject.Instantiate(_earthPF, position, Quaternion.identity);
                type = "E";
                break;
            case Tile.Element.Air:
                go = GameObject.Instantiate(_airPF, position, Quaternion.identity);
                type = "A";
                break;
            case Tile.Element.Muscle:
                go = GameObject.Instantiate(_muscPF, position, Quaternion.identity);
                type = "M";
                break;
            default:
                MMLog.LogError("HEXMAN: Tried to init a tile with elem None!");
                return null;
        }

        TileBehav tb = go.GetComponent<TileBehav>();
        tb.hextag = GenFullTag(id, "B", type); // B for Basic tile
        tb.gameObject.name = tb.hextag;
        return tb;
    }

    // maybe return TB?
    public Hex GenerateToken(int id, string name) {
        GameObject go;
        string type = "";
        switch (name) {
            case "stone":
                go = GameObject.Instantiate(_stonePF);
                type = "Stone";
                break;
            case "ember":
                go = GameObject.Instantiate(_emberPF);
                type = "Ember";
                break;
            case "tombstone":
                go = GameObject.Instantiate(_tombstonePF);
                type = "Tombstone";
                break;
            default:
                MMLog.LogError("HEXMAN: Tried to init a token with bad name=" + name);
                return null;
        }

        TileBehav tb = go.GetComponent<TileBehav>();
        tb.hextag = GenFullTag(id, "T", type); // T for Token
        tb.gameObject.name = tb.hextag;
        return tb;
    }

    public Hex GenerateConsumable(int id, string name) {
        GameObject go;
        switch (name) {
            case "Sample Consumable":
                go = GameObject.Instantiate(_cons_sample);
                break;

            case "HR Form":
                go = GameObject.Instantiate(_cons_HRform);
                break;
            case "Party Snacks":
                go = GameObject.Instantiate(_cons_partySnacks);
                break;
            case "Protein Pills":
                go = GameObject.Instantiate(_cons_proteinPills);
                break;
            case "Leeches!":
                go = GameObject.Instantiate(_cons_leeches);
                break;

            case "Bandages":
                go = GameObject.Instantiate(_cons_bandages);
                break;
            case "Water Lily":
                go = GameObject.Instantiate(_cons_waterLily);
                break;

            case "Dance Shoes":
                go = GameObject.Instantiate(_cons_danceShoes);
                break;
            case "Burning Bracers":
                go = GameObject.Instantiate(_cons_burningBracers);
                break;
            case "Molotov":
                go = GameObject.Instantiate(_cons_molotov);
                break;
            case "Five-Alarm Bell":
                go = GameObject.Instantiate(_cons_fiveAlarmBell);
                break;

            default:
                MMLog.LogError("HEXMAN: Tried to init a consumable with bad name=" + name);
                return null;
        }

        Hex hex = go.GetComponent<Hex>();
        hex.hextag = GenFullTag(id, "C", name); // C for consumable
        hex.gameObject.name = hex.hextag;
        hex.Init();
        return hex;
    }

    public void RemoveTileSeq(TileSeq seq) {
        _mm.StartCoroutine(_RemoveSeq(seq, false));
    }

    public void SetInvokedSeq(TileSeq seq) {
        TileBehav tb;
        TileSeq seqCopy = seq.Copy();
        for (int i = 0; i < seqCopy.sequence.Count;) {
            Tile tile = seqCopy.sequence[0];
            tb = _hexGrid.GetTileBehavAt(tile.col, tile.row);
            tb.wasInvoked = true;

            _mm.StartCoroutine(_mm.animCont._InvokeTile(tb));

            seqCopy.sequence.RemoveAt(0);
        }
    }

    public void RemoveInvokedSeq(TileSeq seq) {
        _mm.StartCoroutine(_RemoveSeq(seq, true));
    }

    public IEnumerator _RemoveSeq(TileSeq seq, bool isInvoked) {
        if (_hexGrid == null) {
            MMLog.LogError("HEXMAN: hexGrid is null");
            yield break;
        }

        Debug.Log("HEXMAN: RemoveSeq() about to remove " + _mm.boardCheck.PrintSeq(seq, true));
        Tile tile;
        for (int i = 0; i < seq.sequence.Count;) {
            tile = seq.sequence[0];
            if (isInvoked || _hexGrid.IsCellFilled(tile.col, tile.row)) {
                TileBehav tb = _hexGrid.GetTileBehavAt(tile.col, tile.row);
                if (seq.sequence.Count == 1) // if it's the last one, wait for it
                    yield return _RemoveTile(tb, false, isInvoked);
                else
                    _mm.StartCoroutine(_RemoveTile(tb, false, isInvoked));
            } else
                Debug.Log("RemoveSeq(): The tile at (" + tile.col + ", " + tile.row + ") is already gone.");
            seq.sequence.RemoveAt(0);
        }
    }

    // TODO should be IEnums? Then just start the anim at the end?
    public void RemoveTile(Tile tile, bool resolveEnchant) {
        TileBehav tb = _hexGrid.GetTileBehavAt(tile.col, tile.row);
        _mm.StartCoroutine(_RemoveTile(tb, resolveEnchant));
    }

    public void RemoveTile(int col, int row, bool resolveEnchant) {
        TileBehav tb = _hexGrid.GetTileBehavAt(col, row);
        _mm.StartCoroutine(_RemoveTile(tb, resolveEnchant));
    }

    public IEnumerator _RemoveTile(TileBehav tb, bool resolveEnchant, bool isInvoked = false) {
        //		Debug.Log ("Removing (" + col + ", " + row + ")");
        removing++;

        if (tb == null) {
            MMLog.LogError("HEXMAN: RemoveTile tried to access a tile that's gone!");
            removing--;
            yield break;
        }
        if (!tb.ableDestroy) {
            MMLog.LogWarning("HEXMAN: RemoveTile tried to remove an indestructable tile!");
            removing--;
            yield break;
        }

        if (tb.HasEnchantment()) {
            if (resolveEnchant) {
                MMLog.Log_MageMatch("About to resolve enchant on tile " + tb.PrintCoord());
                tb.ResolveEnchantment();
            }
            tb.ClearEnchantment(); // TODO
            tb.ClearTileEffects(); //?
        }
        _hexGrid.ClearTileBehavAt(tb.tile.col, tb.tile.row); // move up?

        if (isInvoked)
            yield return _mm.animCont._InvokeTileRemove(tb); // just start it, don't yield?
        else
            yield return _mm.animCont._DestroyTile(tb); // just start it, don't yield?

        RemoveHex(tb);

        _mm.eventCont.TileRemove(tb); //? not needed for checking but idk

        removing--;
    }

    public void RemoveHex(Hex hex) {
        string hextag = hex.hextag;
        GameObject.Destroy(hex.gameObject);

        // add to appropriate player's discard list
        int id = Hex.TagPlayer(hextag);
        if (id != 3) { // don't do this for tiles added by the Commish
            MMLog.Log("HEXMAN", "orange", "Removing " + hextag + " and adding it to their remove list.");
            _mm.GetPlayer(id).deck.AddHextagToRemoveList(hextag);
        }
    }
}
