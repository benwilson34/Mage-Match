using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class TileManager { // should maybe inherit MonoBehaviour? or maybe static?

    public int removing = 0;
    public Sprite flipSprite;

    private Dictionary<string, int>[] tagDicts;
    private MageMatch mm;
    private HexGrid hexGrid;

    private GameObject firePF, waterPF, earthPF, airPF, muscPF; // tile prefabs
    private GameObject stonePF, emberPF, tombstonePF; // token prefabs

    public TileManager(MageMatch mm) {
        this.mm = mm;
        hexGrid = mm.hexGrid;
        tagDicts = new Dictionary<string, int>[3];
        for (int i = 0; i < 3; i++) {
            tagDicts[i] = new Dictionary<string, int>();
        }
        LoadPrefabs();
    }

    void LoadPrefabs() {
        firePF = Resources.Load("prefabs/tile_fire") as GameObject;
        waterPF = Resources.Load("prefabs/tile_water") as GameObject;
        earthPF = Resources.Load("prefabs/tile_earth") as GameObject;
        airPF = Resources.Load("prefabs/tile_air") as GameObject;
        muscPF = Resources.Load("prefabs/tile_muscle") as GameObject;

        stonePF = Resources.Load("prefabs/token_stone") as GameObject;
        emberPF = Resources.Load("prefabs/token_ember") as GameObject;
        tombstonePF = Resources.Load("prefabs/token_tombstone") as GameObject;

        flipSprite = Resources.Load<Sprite>("sprites/hex-back");

        //TODO consumables
        // load only what could be used by either player??
    }

    string GenFullTag(int id, string cat, string type) {
        string fullTag = "p" + id + "-" + cat + "-" + type + "-";
        Dictionary<string, int> dict = tagDicts[id - 1];
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
                go = GameObject.Instantiate(firePF, position, Quaternion.identity);
                tag = "F";
                break;
            case Tile.Element.Water:
                go = GameObject.Instantiate(waterPF, position, Quaternion.identity);
                tag = "W";
                break;
            case Tile.Element.Earth:
                go = GameObject.Instantiate(earthPF, position, Quaternion.identity);
                tag = "E";
                break;
            case Tile.Element.Air:
                go = GameObject.Instantiate(airPF, position, Quaternion.identity);
                tag = "A";
                break;
            case Tile.Element.Muscle:
                go = GameObject.Instantiate(muscPF, position, Quaternion.identity);
                tag = "M";
                break;
            default:
                MMLog.LogError("TILEMAN: Tried to init a tile with elem None!");
                return null;
        }

        TileBehav tb = go.GetComponent<TileBehav>();
        tb.tag = GenFullTag(id, "B", tag); // B for Basic tile
        tb.gameObject.name = tb.tag;
        return tb;
    }

    // maybe return TB?
    public Hex GenerateToken(int id, string name) {
        GameObject go;
        string tag = "";
        switch (name) {
            case "stone":
                go = GameObject.Instantiate(stonePF);
                break;
            case "ember":
                go = GameObject.Instantiate(emberPF);
                break;
            case "tombstone":
                go = GameObject.Instantiate(tombstonePF);
                break;
            default:
                MMLog.LogError("TILEMAN: Tried to init a token with bad name=" + name);
                return null;
        }

        TileBehav tb = go.GetComponent<TileBehav>();
        tb.tag = GenFullTag(id, "T", tag); // T for Token
        tb.gameObject.name = tb.tag;
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
        hex.tag = GenFullTag(id, "C", tag); // C for consumable
        hex.gameObject.name = hex.tag;
        return hex;
    }

    public void RemoveSeq(TileSeq seq) { // TODO messy stuff
        //Debug.Log ("MAGEMATCH: RemoveSeq() about to remove " + boardCheck.PrintSeq(seq, true));
        Tile tile;
        for (int i = 0; i < seq.sequence.Count;) {
            tile = seq.sequence[0];
            if (hexGrid == null)
                MMLog.LogError("TILEMAN: hexGrid is null");
            if (hexGrid.IsCellFilled(tile.col, tile.row))
                RemoveTile(tile, true);
            else
                Debug.Log("RemoveSeq(): The tile at (" + tile.col + ", " + tile.row + ") is already gone.");
            seq.sequence.Remove(tile);
        }
    }

    // TODO should be IEnums? Then just start the anim at the end?
    public void RemoveTile(Tile tile, bool resolveEnchant) {
        mm.StartCoroutine(_RemoveTile(tile.col, tile.row, resolveEnchant));
    }

    public void RemoveTile(int col, int row, bool resolveEnchant) {
        mm.StartCoroutine(_RemoveTile(col, row, resolveEnchant));
    }

    public IEnumerator _RemoveTile(int col, int row, bool resolveEnchant) {
        //		Debug.Log ("Removing (" + col + ", " + row + ")");
        removing++;

        TileBehav tb = hexGrid.GetTileBehavAt(col, row);

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
        hexGrid.ClearTileBehavAt(col, row); // move up?

        yield return mm.animCont._RemoveTile(tb); // just start it, don't yield?
        GameObject.Destroy(tb.gameObject);
        mm.eventCont.TileRemove(tb); //? not needed for checking but idk

        removing--;
    }
}
