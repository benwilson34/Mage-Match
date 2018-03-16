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

    private GameObject _b_fire, _b_water, _b_earth, _b_air, _b_musc; // basic tile prefabs
    private GameObject _stonePF, _emberPF, _t_tombstone; // other tile prefabs
    private GameObject _tq_brushfire, _tq_whiteWater;
    private GameObject _td_muscleMass;
    private GameObject _c_sample;
    private GameObject _c_HRform, _c_partySnacks, _c_proteinPills, _c_leeches;
    private GameObject _c_bandages, _c_waterLily, _c_shuffleGem;
    private GameObject _c_danceShoes, _c_burningBracers, _c_molotov, _c_fiveAlarmBell;

    public HexManager(MageMatch mm) {
        _mm = mm;
        _hexGrid = mm.hexGrid;
        _tagDicts = new Dictionary<string, int>[3];
        for (int i = 0; i < 3; i++) {
            _tagDicts[i] = new Dictionary<string, int>();
        }
        LoadPrefabs();
    }

    void LoadPrefabs() {
        _b_fire = Resources.Load("prefabs/hexes/b_fire") as GameObject;
        _b_water = Resources.Load("prefabs/hexes/b_water") as GameObject;
        _b_earth = Resources.Load("prefabs/hexes/b_earth") as GameObject;
        _b_air = Resources.Load("prefabs/hexes/b_air") as GameObject;
        _b_musc = Resources.Load("prefabs/hexes/b_muscle") as GameObject;

        // TODO load only the tokens + consumables that either player will use??
        //_stonePF = Resources.Load("prefabs/hexes/token_stone") as GameObject;
        //_emberPF = Resources.Load("prefabs/hexes/token_ember") as GameObject;
        _t_tombstone = Resources.Load("prefabs/hexes/token_tombstone") as GameObject;


        _c_sample = Resources.Load("prefabs/hexes/c_sample") as GameObject;

        _c_HRform = Resources.Load("prefabs/hexes/c_HRform") as GameObject;
        _c_partySnacks = Resources.Load("prefabs/hexes/c_partySnacks") as GameObject;
        _c_proteinPills = Resources.Load("prefabs/hexes/c_proteinPills") as GameObject;
        _c_leeches = Resources.Load("prefabs/hexes/c_leeches") as GameObject;
        _td_muscleMass = Resources.Load("prefabs/hexes/td_muscleMass") as GameObject;

        _c_bandages = Resources.Load("prefabs/hexes/c_bandages") as GameObject;
        _c_waterLily = Resources.Load("prefabs/hexes/c_waterLily") as GameObject;
        _tq_whiteWater = Resources.Load("prefabs/hexes/tq_whiteWater") as GameObject;
        _c_shuffleGem = Resources.Load("prefabs/hexes/c_shuffleGem") as GameObject;

        _c_danceShoes = Resources.Load("prefabs/hexes/c_danceShoes") as GameObject;
        _c_burningBracers = Resources.Load("prefabs/hexes/c_burningBracers") as GameObject;
        _c_molotov = Resources.Load("prefabs/hexes/c_molotov") as GameObject;
        //_c_fiveAlarmBell = Resources.Load("prefabs/hexes/c_fiveAlarmBell") as GameObject;
        _tq_brushfire = Resources.Load("prefabs/hexes/tq_brushfire") as GameObject;


        flipSprite = Resources.Load<Sprite>("sprites/hex-back");
    }

    
    // ---------- GENERATION ----------

    public Hex GenerateHex(int id, string genTag) {
        MMLog.Log("HEXMAN", "black", "Generating tile from genTag \"" + genTag + "\"");
        string type = Hex.TagTitle(genTag);
        switch (Hex.TagCat(genTag)) {
            case Hex.Category.BasicTile:
                return GenerateBasicTile(id, Tile.CharToElement(type[0]));
            case Hex.Category.Tile:
                return GenerateTile(id, type);
            case Hex.Category.Charm:
                return GenerateCharm(id, type);
            default:
                MMLog.LogError("HEXMAN: Failed " + genTag + " with cat="+Hex.TagCat(genTag));
                return null;
        }
    }

    // maybe return TB?
    public Hex GenerateBasicTile(int id, Tile.Element element) {
        return GenerateBasicTile(id, element, GameObject.Find("tileSpawn").transform.position);
    }

    // maybe return TB?
    public Hex GenerateBasicTile(int id, Tile.Element element, Vector3 position) {
        GameObject go;
        string type;
        switch (element) {
            case Tile.Element.Fire:
                go = GameObject.Instantiate(_b_fire, position, Quaternion.identity);
                type = "F";
                break;
            case Tile.Element.Water:
                go = GameObject.Instantiate(_b_water, position, Quaternion.identity);
                type = "W";
                break;
            case Tile.Element.Earth:
                go = GameObject.Instantiate(_b_earth, position, Quaternion.identity);
                type = "E";
                break;
            case Tile.Element.Air:
                go = GameObject.Instantiate(_b_air, position, Quaternion.identity);
                type = "A";
                break;
            case Tile.Element.Muscle:
                go = GameObject.Instantiate(_b_musc, position, Quaternion.identity);
                type = "M";
                break;
            default:
                MMLog.LogError("HEXMAN: Tried to init a tile with elem None!");
                return null;
        }

        TileBehav tb = go.GetComponent<TileBehav>();
        tb.hextag = GenFullTag(id, "B", type); // B for Basic tile
        tb.Init(_mm);
        tb.gameObject.name = tb.hextag;
        return tb;
    }

    // maybe return TB?
    public Hex GenerateTile(int id, string type) {
        GameObject go;
        switch (type) {
            //case "Stone":
            //    go = GameObject.Instantiate(_stonePF);
            //    break;
            //case "Ember":
            //    go = GameObject.Instantiate(_emberPF);
            //    break;
            case "Tombstone":
                go = GameObject.Instantiate(_t_tombstone);
                break;

            case "Brushfire":
                go = GameObject.Instantiate(_tq_brushfire);
                break;
            case "White Water":
                go = GameObject.Instantiate(_tq_whiteWater);
                break;

            case "Muscle Mass":
                go = GameObject.Instantiate(_td_muscleMass);
                break;

            default:
                MMLog.LogError("HEXMAN: Tried to init a token with bad name=" + type);
                return null;
        }

        TileBehav tb = go.GetComponent<TileBehav>();
        tb.hextag = GenFullTag(id, "T", type); // T for Tile
        tb.Init(_mm);
        tb.gameObject.name = tb.hextag;
        return tb;
    }

    public Hex GenerateCharm(int id, string type) {
        GameObject go;
        switch (type) {
            case "Sample Charm":
                go = GameObject.Instantiate(_c_sample);
                break;

            case "HR Form":
                go = GameObject.Instantiate(_c_HRform);
                break;
            case "Party Snacks":
                go = GameObject.Instantiate(_c_partySnacks);
                break;
            case "Protein Pills":
                go = GameObject.Instantiate(_c_proteinPills);
                break;
            case "Leeches!":
                go = GameObject.Instantiate(_c_leeches);
                break;

            case "Bandages":
                go = GameObject.Instantiate(_c_bandages);
                break;
            case "Water Lily":
                go = GameObject.Instantiate(_c_waterLily);
                break;
            case "Shuffle Gem":
                go = GameObject.Instantiate(_c_shuffleGem);
                break;

            case "Dance Shoes":
                go = GameObject.Instantiate(_c_danceShoes);
                break;
            case "Burning Bracers":
                go = GameObject.Instantiate(_c_burningBracers);
                break;
            case "Molotov":
                go = GameObject.Instantiate(_c_molotov);
                break;
            case "Five-Alarm Bell":
                go = GameObject.Instantiate(_c_fiveAlarmBell);
                break;

            default:
                MMLog.LogError("HEXMAN: Tried to init a consumable with bad type=" + type);
                return null;
        }

        Hex hex = go.GetComponent<Hex>();
        hex.hextag = GenFullTag(id, "C", type); // C for Charm
        hex.gameObject.name = hex.hextag;
        hex.Init(_mm);
        return hex;
    }

    string GenFullTag(int id, string cat, string type) {
        string fullTag = "p" + id + "-" + cat + "-" + type + "-";
        if (id == 0)
            id = 3;
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


    // ---------- REMOVAL ----------

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
        if (id != 0 && hex.putBackIntoDeck) { // don't do this for tiles added by the Commish
            MMLog.Log("HEXMAN", "orange", "Removing " + hextag + " and adding it to their remove list.");
            _mm.GetPlayer(id).deck.AddHextagToRemoveList(hextag);
        }
    }
}
