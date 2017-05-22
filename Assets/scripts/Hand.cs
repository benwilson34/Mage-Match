using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand {

    private List<TileBehav> tiles = null;
    private Transform handSlot = null;
    private Player p;
    private MageMatch mm;
    private const int maxHandSize = 7;

    public Hand(MageMatch mm, Player p) {
        tiles = new List<TileBehav>();
        this.mm = mm;
        this.p = p;
        SetHandSlot();
    }

    void SetHandSlot() {
        if (mm.gameSettings.localPlayerOnLeft) {
            if (p.id == mm.myID)
                handSlot = GameObject.Find("handslot1").transform;
            else
                handSlot = GameObject.Find("handslot2").transform;
        } else {
            if (p.id == 1)
                handSlot = GameObject.Find("handslot1").transform;
            else
                handSlot = GameObject.Find("handslot2").transform;
        }
    }

    public void Add(TileBehav tb) { tiles.Add(tb); }

    public void Remove(TileBehav tb) { tiles.Remove(tb); }

    public void Empty() {
        while (tiles.Count > 0) {
            GameObject.Destroy(tiles[0].gameObject);
            tiles.RemoveAt(0);
        }
    }

    public Vector3 GetHandPos() { return handSlot.position; }

    public TileBehav GetTile(int i) { return tiles[i]; }

    public GameObject GetTile(Tile.Element elem) {
        for (int i = 0; i < tiles.Count; i++) {
            if (tiles[i].tile.element == elem)
                return tiles[i].gameObject;
        }
        return null;
    }

    public int Count() { return tiles.Count; }

    public void Align(float duration, bool linear) {
        mm.animCont.PlayAnim(mm.animCont._AlignHand(p, duration, linear));
    }

    public bool IsFull() { return tiles.Count == maxHandSize; }

    public Transform GetHandSlot() { return handSlot; }
}
