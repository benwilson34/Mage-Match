using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand {

    private List<TileBehav> tiles = null;
    private Transform handPos = null;
    private HandSlot[] slots;
    private GameObject placeholderPF;
    private TileBehav placeholder = null;
    private HandSlot placeholderSlot;
    private Player p;
    private MageMatch mm;

    private const int maxHandSize = 7;

    public Hand(MageMatch mm, Player p) {
        tiles = new List<TileBehav>();
        slots = new HandSlot[maxHandSize];
        placeholderPF = Resources.Load("prefabs/ui/placeholder") as GameObject;
        this.mm = mm;
        this.p = p;
        SetHandPos();
    }

    void SetHandPos() {
        int place = 0;
        if (mm.gameSettings.localPlayerOnLeft) {
            if (p.id == mm.myID)
                place = 1;
            else
                place = 2;
        } else {
            place = p.id;
        }

        handPos = GameObject.Find("handslot" + place).transform;

        if (mm.gameSettings.hideOpponentHand && p.id != mm.myID) {
            //MMDebug.MMLog.Log("HAND", "black", "hiding opponent hand..........");
            handPos.position = new Vector3(handPos.position.x + 5, handPos.position.y);
        }

        for (int i = 0; i < maxHandSize; i++) {
            slots[i] = handPos.Find("slot" + i).GetComponent<HandSlot>();
        }
    }

    public Vector3 GetHandPos() { return handPos.position; }

    public void Add(TileBehav tb) {
        tb.transform.SetParent(handPos); // , false)?
        tiles.Add(tb);

        int i;
        for (i = 0; i < maxHandSize; i++) {
            HandSlot slot = slots[i];
            if (!slot.IsFull()) {
                slot.SetTile(tb);
                mm.animCont.PlayAnim(mm.animCont._Move(tb, slot.transform.position));
                break;
            }
        } 
    }

    public void Remove(TileBehav tb) {
        // TODO iterate thru, then clear corresponding handSlot
        for (int i = 0; i < maxHandSize; i++) {
            TileBehav slotTile = slots[i].GetTile();
            if (slotTile != null && slotTile.Equals(tb)) {
                slots[i].ClearTile();
                break;
            }
        }
        tiles.Remove(tb);
    }

    public void Empty() {
        while (tiles.Count > 0) {
            GameObject.Destroy(tiles[0].gameObject);
            tiles.RemoveAt(0);
        }
    }

    public TileBehav GetTile(int i) { return tiles[i]; }

    public GameObject GetTile(Tile.Element elem) {
        for (int i = 0; i < tiles.Count; i++) {
            if (tiles[i].tile.element == elem)
                return tiles[i].gameObject;
        }
        return null;
    }

    public int Count() { return tiles.Count; }

    public bool IsFull() { return tiles.Count == maxHandSize; }

    public HandSlot GetHandSlot(int ind) { return slots[ind]; }

    public int NumFullSlots() {
        string str = "";
        int total = 0;
        foreach (HandSlot slot in slots) {
            if (slot.IsFull()) {
                total++;
                str += "[" + slot.GetTile().tile.ThisElementToChar() + "] ";
            } else
                str += "[ ] ";
        }
        MMDebug.MMLog.Log("HAND", "black", str);
        return total;
    }


    // -------------------- rearrangement -----------------------
    // maybe kinda depends on tile tagging system?

    // On MouseDown 
    public void GrabTile(TileBehav tb) {
        //NumFullSlots();
        for (int i = 0; i < maxHandSize; i++) {
            HandSlot slot = slots[i];
            TileBehav slotTB = slot.GetTile();
            if (slotTB != null && slotTB.Equals(tb)) { // TODO tags instead!
                //MMDebug.MMLog.Log("HAND", "black", "Found it at index=" + i);

                placeholder = GameObject.Instantiate(placeholderPF).GetComponent<TileBehav>();
                placeholder.transform.position = slot.transform.position;

                slot.SetTile(placeholder);
                placeholderSlot = slot;
                break;
            }
        }
    }

    // called from OnMouseDrag
    public void Rearrange(HandSlot newSlot) {
        if (newSlot.handIndex == placeholderSlot.handIndex)
            return;

        //MMDebug.MMLog.Log("HAND", "black", "Before:");
        //NumFullSlots();

        // first, find the slot to swap towards
        HandSlot swapSlot = null;
        int newSlotInd = newSlot.handIndex;
        // downward
        for (int i = newSlotInd; i < maxHandSize; i++) {
            if (!slots[i].IsFull() || i == placeholderSlot.handIndex) {
                swapSlot = slots[i];
                break;
            }
        }
        if (swapSlot == null) {
            // upward
            for (int i = newSlotInd; i >= 0; i--) {
                if (!slots[i].IsFull() || i == placeholderSlot.handIndex) {
                    swapSlot = slots[i];
                    break;
                }
            }
        }

        //MMDebug.MMLog.Log("HAND", "black", "Swap index=" + swapSlot.handIndex);

        // downward swap
        if (swapSlot.handIndex > newSlotInd) {
            for (int s = swapSlot.handIndex; s > newSlotInd; s--) {
                TileBehav tb = slots[s - 1].GetTile();
                slots[s].SetTile(tb);
                mm.animCont.PlayAnim(mm.animCont._Move(tb, slots[s].transform.position));
            }
        } else { // upward
            for (int s = swapSlot.handIndex; s < newSlotInd; s++) {
                TileBehav tb = slots[s + 1].GetTile();
                slots[s].SetTile(tb);
                mm.animCont.PlayAnim(mm.animCont._Move(tb, slots[s].transform.position));
            }
        }

        newSlot.SetTile(placeholder);
        if (placeholderSlot.GetTile().tile.element == Tile.Element.None) // i dislike this deeply
            placeholderSlot.ClearTile();
        placeholderSlot = newSlot;

        //MMDebug.MMLog.Log("HAND", "black", "After:");
        //NumFullSlots();
    }

    // On MouseUp
    public void ReleaseTile(TileBehav tb) {
        //MMDebug.MMLog.Log("HAND", "black", "ReleaseTile called! PlaceholderSlot="+placeholderSlot.handIndex);
        mm.animCont.PlayAnim(mm.animCont._Move(tb, placeholderSlot.transform.position));

        placeholderSlot.SetTile(tb);
        ClearPlaceholder();
        //NumFullSlots();
    }

    public void ClearPlaceholder() {
        GameObject.Destroy(placeholder.gameObject);
        placeholder = null; //?
    }
}
