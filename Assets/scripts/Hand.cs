using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Hand {

    private const int MAX_HAND_SIZE = 7;

    private List<Hex> hexes = null;
    private Transform handPos = null;
    private HandSlot[] slots;
    private GameObject placeholderPF;
    private TileBehav placeholder = null;
    private HandSlot placeholderSlot;
    private Player p;
    private MageMatch mm;

    public Hand(MageMatch mm, Player p) {
        hexes = new List<Hex>();
        slots = new HandSlot[MAX_HAND_SIZE];
        placeholderPF = Resources.Load("prefabs/ui/placeholder") as GameObject;
        this.mm = mm;
        this.p = p;
        SetHandPos();
    }

    void SetHandPos() {
        int place = 0;
        if (p.id == mm.myID)
            place = 1;
        else
            place = 2;

        handPos = GameObject.Find("handslot" + place).transform;

        //if (mm.gameSettings.hideOpponentHand && p.id != mm.myID) {
        //    //MMDebug.MMLog.Log("HAND", "black", "hiding opponent hand..........");
        //    handPos.position = new Vector3(handPos.position.x + 5, handPos.position.y);
        //}

        for (int i = 0; i < MAX_HAND_SIZE; i++) {
            slots[i] = handPos.Find("slot" + i).GetComponent<HandSlot>();
        }
    }

    public Vector3 GetHandPos() { return handPos.position; }

    public void Add(Hex hex) {
        hex.transform.SetParent(handPos); // , false)?
        hexes.Add(hex);

        int i;
        for (i = 0; i < MAX_HAND_SIZE; i++) {
            HandSlot slot = slots[i];
            if (!slot.IsFull()) {
                slot.SetHex(hex);
                hex.transform.position = slot.transform.position;
                mm.animCont.PlayAnim(mm.animCont._Draw(hex));
                mm.audioCont.HexDraw(hex.GetComponent<AudioSource>());
                MMLog.Log("HAND", "black", "Added hex with tag " + hex.hextag);
                break;
            }
        } 
    }

    public void Remove(Hex hex) {
        // TODO iterate thru, then clear corresponding handSlot
        for (int i = 0; i < MAX_HAND_SIZE; i++) {
            Hex slotHex = slots[i].GetHex();
            if (slotHex != null && slotHex.hextag.Equals(hex.hextag)) {
                slots[i].ClearHex();
                break;
            }
        }
        hexes.Remove(hex);
    }

    public void Empty() {
        while (hexes.Count > 0) {
            GameObject.Destroy(hexes[0].gameObject);
            hexes.RemoveAt(0);
        }
    }

    public TileBehav GetTile(int i) { return (TileBehav)hexes[i]; }

    public GameObject GetTile(Tile.Element elem) {
        for (int i = 0; i < hexes.Count; i++) {
            if (((TileBehav)hexes[i]).tile.element == elem)
                return hexes[i].gameObject;
        }
        return null;
    }

    public Hex GetHex(string tag) {
        foreach (Hex obj in hexes) {
            //MMLog.Log("HAND", "black", "Trying \"" + tag + "\" against \"" + obj.tag + "\"");
            if (tag.Equals(obj.hextag))
                return obj;
        }
        MMLog.LogError("HAND: Failed to find hex with tag \"" + tag + "\"");
        return null;
    }

    public List<string> Debug_GetAllTags() {
        List<string> tags = new List<string>();
        foreach (Hex h in hexes) {
            tags.Add(h.hextag);
        }
        tags.Sort();
        return tags;
    }
    public bool Debug_CheckTags(List<string> theirTags) {
        List<string> myTags = Debug_GetAllTags();
        for (int i = 0; i < theirTags.Count; i++) {
            string ttag = theirTags[i];
            if (theirTags[i] != myTags[i]) {
                // that's bad
                MMLog.LogError("Hand desync!!\nmine =" + string.Concat(myTags) + 
                    "\ntheirs=" + string.Concat(theirTags));
                return false;
            }
        }
        return true;
    }

    public int Count() { return hexes.Count; }

    public bool IsFull() { return hexes.Count == MAX_HAND_SIZE; }

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
        for (int i = 0; i < MAX_HAND_SIZE; i++) {
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
        for (int i = newSlotInd; i < MAX_HAND_SIZE; i++) {
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
        if (placeholderSlot.GetHex().hextag.Equals("placeholder")) // i dislike this deeply
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
