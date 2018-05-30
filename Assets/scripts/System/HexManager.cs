using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class HexManager { // should maybe inherit MonoBehaviour? or maybe static?

    public int removing = 0;
    public Sprite flipSprite;

    private MageMatch _mm;
    private HexGrid _hexGrid;
    private Dictionary<string, int>[] _tagDicts;
    private Dictionary<string, GameObject> _prefabs;

    public HexManager(MageMatch mm) {
        _mm = mm;
        _hexGrid = mm.hexGrid;
        _tagDicts = new Dictionary<string, int>[3];
        for (int i = 0; i < 3; i++) {
            _tagDicts[i] = new Dictionary<string, int>();
        }
        LoadPrefabs(_mm.gameSettings);
    }

    void LoadPrefabs(GameSettings settings) {
        _prefabs = new Dictionary<string, GameObject>();

        AddPrefab("Basic", "Fire");
        AddPrefab("Basic", "Water");
        AddPrefab("Basic", "Earth");
        AddPrefab("Basic", "Air");
        AddPrefab("Basic", "Muscle");

        // TODO load only the tokens + consumables that either player will use??
        //_stonePF = Resources.Load("prefabs/hexes/token_stone") as GameObject;
        //_emberPF = Resources.Load("prefabs/hexes/token_ember") as GameObject;
        AddPrefab("Tile", "Tombstone");

        AddPrefab("Tile", "Twinfire");
        AddPrefab("Tile", "Raindrops");
        AddPrefab("Tile", "MuscleMass");
        AddPrefab("Tile", "Brushfire");
        AddPrefab("Tile", "WhiteWater");
        AddPrefab("Tile", "Stimulant");
        AddPrefab("Charm", "Flamethrower");
        AddPrefab("Charm", "Geode");
        AddPrefab("Charm", "PuffPendant");
        AddPrefab("Charm", "BrawnBroth");

        AddPrefab("Charm", "ShuffleGem");
        AddPrefab("Charm", "Molotov");
        AddPrefab("Charm", "Leeches");
        AddPrefab("Charm", "ProteinPills");
        AddPrefab("Charm", "RollingBone");
        AddPrefab("Charm", "ComebackBrace");
        AddPrefab("Tile", "EvilDoll");
        AddPrefab("Tile", "Lifestealer");

        AddPrefab("Charm", "LighterFluid");
        AddPrefab("Charm", "DanceShoes");
        AddPrefab("Charm", "BurningBracers");
        AddPrefab("Tile", "CausticCastanet");

        AddPrefab("Charm", "Bandages");
        AddPrefab("Tile", "WaterLily");

        AddPrefab("Charm", "HRForm");
        AddPrefab("Charm", "PartySnacks");

        flipSprite = Resources.Load<Sprite>("sprites/hex-back");
    }

    void AddPrefab(string cat, string title) {
        if (!_prefabs.ContainsKey(title)) {
            _prefabs.Add(title, Resources.Load("prefabs/hexes/"+cat+"/"+title) as GameObject);
        }
    }

    GameObject GetRunePrefab() {
        // TODO 
        return null;
    }


    #region ---------- GENERATION ----------

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

    public TileBehav GenerateBasicTile(int id, Tile.Element element) {
        return GenerateBasicTile(id, element, GameObject.Find("tileSpawn").transform.position);
    }

    public TileBehav GenerateBasicTile(int id, Tile.Element element, Vector3 position) {
        if (element == Tile.Element.None) {
            MMLog.LogError("HEXMAN: Tried to init a basic tile with element None!");
            return null;
        }

        string elemStr = element.ToString();
        //Debug.Log("Generating " + elemStr);
        GameObject go = GameObject.Instantiate(_prefabs[elemStr], position, Quaternion.identity);

        TileBehav tb = go.GetComponent<TileBehav>();
        tb.hextag = GenFullTag(id, "B", elemStr); // B for Basic tile
        tb.Init(_mm);
        tb.gameObject.name = tb.hextag;
        return tb;
    }

    public TileBehav GenerateTile(int id, string type) {
        if (!_prefabs.ContainsKey(type)) {
            MMLog.LogError("HEXMAN: Tried to init a tile with bad name=" + type);
            return null;
        }

        GameObject go = GameObject.Instantiate(_prefabs[type]);

        TileBehav tb = go.GetComponent<TileBehav>();
        tb.hextag = GenFullTag(id, "T", type); // T for Tile
        tb.Init(_mm);
        tb.gameObject.name = tb.hextag;
        return tb;
    }

    public Charm GenerateCharm(int id, string type) {
        if (!_prefabs.ContainsKey(type)) {
            MMLog.LogError("HEXMAN: Tried to init a charm with bad type=" + type);
            return null;
        }

        GameObject go = GameObject.Instantiate(_prefabs[type]);

        Charm charm = go.GetComponent<Charm>();
        charm.hextag = GenFullTag(id, "C", type); // C for Charm
        charm.gameObject.name = charm.hextag;
        charm.Init(_mm);
        return charm;
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
    #endregion


    #region ---------- REMOVAL ----------

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
            _mm.GetPlayer(id).deck.AddHextagToGraveyard(hextag);
        }
    }
    #endregion
}
