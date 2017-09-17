using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Hand {

    private List<Hex> objs = null;
    private Transform handPos = null;
    private HandSlot[] slots;
    private GameObject placeholderPF;
    private TileBehav placeholder = null;
    private HandSlot placeholderSlot;
    private Player p;
    private MageMatch mm;

    private const int maxHandSize = 7;

    public Hand(MageMatch mm, Player p) {
        objs = new List<Hex>();
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

        //if (mm.gameSettings.hideOpponentHand && p.id != mm.myID) {
        //    //MMDebug.MMLog.Log("HAND", "black", "hiding opponent hand..........");
        //    handPos.position = new Vector3(handPos.position.x + 5, handPos.position.y);
        //}

        for (int i = 0; i < maxHandSize; i++) {
            slots[i] = handPos.Find("slot" + i).GetComponent<HandSlot>();
        }
    }

    public Vector3 GetHandPos() { return handPos.position; }

    public void Add(Hex hex) {
        hex.transform.SetParent(handPos); // , false)?
        objs.Add(hex);

        int i;
        for (i = 0; i < maxHandSize; i++) {
            HandSlot slot = slots[i];
            if (!slot.IsFull()) {
                slot.SetHex(hex);
                mm.animCont.PlayAnim(mm.animCont._Move(hex, slot.transform.position));
                MMLog.Log("HAND", "black", "Added hex with tag " + hex.tag);
                break;
            }
        } 
    }

    public void Remove(Hex hex) {
        // TODO iterate thru, then clear corresponding handSlot
        for (int i = 0; i < maxHandSize; i++) {
            Hex slotHex = slots[i].GetHex();
            if (slotHex != null && slotHex.tag.Equals(hex.tag)) {
                slots[i].ClearHex();
                break;
            }
        }
        objs.Remove(hex);
    }

    public void Empty() {
        while (objs.Count > 0) {
            GameObject.Destroy(objs[0].gameObject);
            objs.RemoveAt(0);
        }
    }

    public TileBehav GetTile(int i) { return (TileBehav)objs[i]; }

    public GameObject GetTile(Tile.Element elem) {
        for (int i = 0; i < objs.Count; i++) {
            if (((TileBehav)objs[i]).tile.element == elem)
                return objs[i].gameObject;
        }
        return null;
    }

    public Hex GetHex(string tag) {
        foreach (Hex obj in objs) {
            MMLog.Log("HAND", "black", "Trying \"" + tag + "\" against \"" + obj.tag + "\"");
            if (tag.Equals(obj.tag))
                return obj;
        }
        MMLog.Log("HAND", "black", "Failed to find hex with tag \"" + tag + "\"");
        return null;
    }

    public int Count() { return objs.Count; }

    public bool IsFull() { return objs.Count == maxHandSize; }

    public HandSlot GetHandSlot(int ind) { return slots[ind]; }

    public int NumFullSlots() {
        string str = "";
        int total = 0;
        foreach (HandSlot slot in slots) {
            if (slot.IsFull()) {
                total++;
                Hex hex = slot.GetHex();
                if (hex is TileBehav)
                    str += "[" + ((TileBehav)hex).tile.ThisElementToChar() + "] ";
                else
                    str += "[?] ";
            } else
                str += "[ ] ";
        }
        MMDebug.MMLog.Log("HAND", "black", str);
        return total;
    }


    // -------------------- rearrangement -----------------------
    // maybe kinda depends on tile tagging system?

    // On MouseDown 
    public void GrabHex(Hex hex) {
        //NumFullSlots();
        for (int i = 0; i < maxHandSize; i++) {
            HandSlot slot = slots[i];
            Hex slotHex = slot.GetHex();
            if (slotHex != null && hex.EqualsTag(slotHex)) {
                //MMDebug.MMLog.Log("HAND", "black", "Found it at index=" + i);

                placeholder = GameObject.Instantiate(placeholderPF).GetComponent<TileBehav>();
                placeholder.transform.position = slot.transform.position;

                slot.SetHex(placeholder);
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
                Hex hex = slots[s - 1].GetHex();
                slots[s].SetHex(hex);
                mm.animCont.PlayAnim(mm.animCont._Move(hex, slots[s].transform.position));
            }
        } else { // upward
            for (int s = swapSlot.handIndex; s < newSlotInd; s++) {
                Hex hex = slots[s + 1].GetHex();
                slots[s].SetHex(hex);
                mm.animCont.PlayAnim(mm.animCont._Move(hex, slots[s].transform.position));
            }
        }

        newSlot.SetHex(placeholder);
        if (placeholderSlot.GetHex().tag.Equals("placeholder")) // i dislike this deeply
            placeholderSlot.ClearHex();
        placeholderSlot = newSlot;

        //MMDebug.MMLog.Log("HAND", "black", "After:");
        //NumFullSlots();
    }

    // On MouseUp
    public void ReleaseTile(Hex hex) {
        //MMDebug.MMLog.Log("HAND", "black", "ReleaseTile called! PlaceholderSlot="+placeholderSlot.handIndex);
        mm.animCont.PlayAnim(mm.animCont._Move(hex, placeholderSlot.transform.position));

        placeholderSlot.SetHex(hex);
        ClearPlaceholder();
        //NumFullSlots();
    }

    public void ClearPlaceholder() {
        GameObject.Destroy(placeholder.gameObject);
        placeholder = null; //?
    }
}
