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
        _firePF = Resources.Load("prefabs/tile_fire") as GameObject;
        _waterPF = Resources.Load("prefabs/tile_water") as GameObject;
        _earthPF = Resources.Load("prefabs/tile_earth") as GameObject;
        _airPF = Resources.Load("prefabs/tile_air") as GameObject;
        _muscPF = Resources.Load("prefabs/tile_muscle") as GameObject;

        _stonePF = Resources.Load("prefabs/token_stone") as GameObject;
        _emberPF = Resources.Load("prefabs/token_ember") as GameObject;
        _tombstonePF = Resources.Load("prefabs/token_tombstone") as GameObject;

        flipSprite = Resources.Load<Sprite>("sprites/hex-back");

        //TODO consumables
        // load only what could be used by either player??
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

    public Hex GenerateRandomHex(Player p) {
        string genTag = p.character.GenerateHexTag();
        return GenerateHex(p.id, genTag);
    }

    //public HandObject GetRandomHex() { return genHex; }

    public Hex GenerateHex(int id, string genTag) {
        MMLog.Log("TILEMAN", "black", "Generating tile from genTag \"" + genTag + "\"");
        string type = Hex.TagType(genTag);
        switch (Hex.TagCat(genTag)) {
            case "B":
                return GenerateTile(id, Tile.CharToElement(type[0]));
            case "T":
                return GenerateToken(id, type);
            case "C":
                return GenerateConsumable(id, type);
            default:
                MMLog.LogError("TILEMAN: Failed " + genTag + " with cat="+Hex.TagCat(genTag));
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
        string tag;
        switch (element) {
            case Tile.Element.Fire:
                go = GameObject.Instantiate(_firePF, position, Quaternion.identity);
                tag = "F";
                break;
            case Tile.Element.Water:
                go = GameObject.Instantiate(_waterPF, position, Quaternion.identity);
                tag = "W";
                break;
            case Tile.Element.Earth:
                go = GameObject.Instantiate(_earthPF, position, Quaternion.identity);
                tag = "E";
                break;
            case Tile.Element.Air:
                go = GameObject.Instantiate(_airPF, position, Quaternion.identity);
                tag = "A";
                break;
            case Tile.Element.Muscle:
                go = GameObject.Instantiate(_muscPF, position, Quaternion.identity);
                tag = "M";
                break;
            default:
                MMLog.LogError("TILEMAN: Tried to init a tile with elem None!");
                return null;
        }

        TileBehav tb = go.GetComponent<TileBehav>();
        tb.hextag = GenFullTag(id, "B", tag); // B for Basic tile
        tb.gameObject.name = tb.hextag;
        return tb;
    }

    // maybe return TB?
    public Hex GenerateToken(int id, string name) {
        GameObject go;
        string tag = "";
        switch (name) {
            case "stone":
                go = GameObject.Instantiate(_stonePF);
                break;
            case "ember":
                go = GameObject.Instantiate(_emberPF);
                break;
            case "tombstone":
                go = GameObject.Instantiate(_tombstonePF);
                break;
            default:
                MMLog.LogError("TILEMAN: Tried to init a token with bad name=" + name);
                return null;
        }

        TileBehav tb = go.GetComponent<TileBehav>();
        tb.hextag = GenFullTag(id, "T", tag); // T for Token
        tb.gameObject.name = tb.hextag;
        return tb;
    }

    public Hex GenerateConsumable(int id, string name) {
        GameObject go;
        string tag = "";
        switch (name) {
            default:
                MMLog.LogError("TILEMAN: Tried to init a consumable with bad name=" + name);
                return null;
        }

        Hex hex = go.GetComponent<Hex>();
        hex.hextag = GenFullTag(id, "C", tag); // C for consumable
        hex.gameObject.name = hex.hextag;
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
            MMLog.LogError("MAGEMATCH: RemoveTile tried to access a tile that's gone!");
            removing--;
            yield break;
        }
        if (!tb.ableDestroy) {
            MMLog.LogWarning("MAGEMATCH: RemoveTile tried to remove an indestructable tile!");
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
        GameObject.Destroy(tb.gameObject);
        _mm.eventCont.TileRemove(tb); //? not needed for checking but idk

        removing--;
    }
}
