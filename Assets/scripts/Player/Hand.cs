﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class Hand {

    public const int MAX_HAND_SIZE = 7;

    public int Count { get { return _hexes.Count; } }
    public bool IsEmpty { get { return _hexes.Count == 0; } }
    public bool IsFull { get { return _hexes.Count == MAX_HAND_SIZE; } }

    private List<Hex> _hexes = null;
    private Transform _handPos = null;
    private HandSlot[] _slots;
    private GameObject _placeholderPF;
    private TileBehav _placeholder = null;
    private HandSlot _placeholderSlot;
    private Player _p;
    private MageMatch _mm;

    public Hand(MageMatch mm, Player p) {
        _hexes = new List<Hex>();
        _slots = new HandSlot[MAX_HAND_SIZE];
        _placeholderPF = Resources.Load("prefabs/ui/placeholder") as GameObject;
        this._mm = mm;
        this._p = p;
        SetHandPos();
    }

    void SetHandPos() { // TODO this needs to check which side of the screen the player is on
        int place = 0;
        if (_p.ID == _mm.myID)
            place = 1;
        else
            place = 2;

        _handPos = GameObject.Find("handslot" + place).transform;

        for (int i = 0; i < MAX_HAND_SIZE; i++) {
            _slots[i] = _handPos.Find("slot" + i).GetComponent<HandSlot>();
        }
    }

    //public Vector3 GetHandPos() { return _handPos.position; }

    public bool IsHexMine(Hex hex) {
        return IsHexMine(hex.hextag);
    }

    public bool IsHexMine(string hextag) {
        foreach (Hex handHex in _hexes) {
            if (hextag == handHex.hextag)
                return true;
        }
        return false;
    }

    public void Add(Hex hex) {
        // TODO add HandChange Event

        if (_mm.gameMode != MageMatch.GameMode.TrainingTwoChars && !_mm.IsMe(_p.ID))
            hex.Flip();

        hex.transform.SetParent(_handPos); // , false)?
        hex.state = Hex.State.Hand;
        _hexes.Add(hex);

        int i;
        for (i = 0; i < MAX_HAND_SIZE; i++) {
            HandSlot slot = _slots[i];
            if (!slot.IsFull()) {
                slot.SetHex(hex);
                hex.transform.position = slot.transform.position;
                AnimationController.PlayAnim(AnimationController._Draw(hex));
                AudioController.Trigger(SFX.Hex.Draw);
                MMLog.Log("HAND", "black", "Added hex with tag " + hex.hextag);
                break;
            }
        } 
    }

    public void Remove(Hex hex) {
        // iterate thru, then clear corresponding handSlot
        for (int i = 0; i < MAX_HAND_SIZE; i++) {
            Hex slotHex = _slots[i].GetHex();
            if (slotHex != null && slotHex.hextag.Equals(hex.hextag)) {
                _slots[i].ClearHex();
                break;
            }
        }
        _hexes.Remove(hex);
    }

    public void Empty() {
        while (_hexes.Count > 0) {
            GameObject.Destroy(_hexes[0].gameObject);
            _hexes.RemoveAt(0);
        }
    }

    // needed?
    public TileBehav GetTile(int i) {
        MMLog.Log("HAND", "black", "Found an object in hand -> " + _hexes[i].ToString());
        return (TileBehav)_hexes[i];
    }

    public Hex GetHex(string tag) {
        foreach (Hex obj in _hexes) {
            //MMLog.Log("HAND", "black", "Trying \"" + tag + "\" against \"" + obj.tag + "\"");
            if (tag.Equals(obj.hextag))
                return obj;
        }
        MMLog.LogError("HAND: Failed to find hex with tag \"" + tag + "\"");
        return null;
    }

    public Hex GetHexAt(int i) { return _hexes[i]; }

    public List<Hex> GetAllHexes() { return _hexes; }

    public void FlipAllHexes(bool flipped) {
        foreach (var hex in _hexes)
            hex.Flip(flipped);
    }


    #region ---------- DISCARD ----------
    public void Discard(Hex hex) {
        _mm.StartCoroutine(_Discard(hex));
    }

    public IEnumerator _Discard(string tag) {
        MMLog.Log_Player("Discarding " + tag);
        yield return _Discard(GetHex(tag));
    }

    public IEnumerator _Discard(Hex hex) {
        yield return EventController.HandChange(MMEvent.Moment.Begin, _p.ID, hex.hextag, EventController.HandChangeState.Discard);

        AudioController.Trigger(SFX.Hex.Discard);
        Remove(hex);
        yield return AnimationController._DiscardTile(hex.transform);
        GameObject.Destroy(hex.gameObject); //should maybe go thru TileMan

        yield return EventController.HandChange(MMEvent.Moment.End, _p.ID, hex.hextag, EventController.HandChangeState.Discard);
    }

    public IEnumerator _DiscardRandom(int count = 1) {
        for (int i = 0; i < count && _hexes.Count > 0; i++) {
            yield return _mm.syncManager.SyncRand(_p.ID, Random.Range(0, _hexes.Count));
            int rand = _mm.syncManager.GetRand();
            Hex hex = _hexes[rand];
            yield return _Discard(hex);
        }
    }
    #endregion


    public List<string> Debug_GetAllTags() {
        List<string> tags = new List<string>();
        foreach (Hex h in _hexes) {
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
                MMLog.LogError("Hand desync!!\nmine =" + string.Join(", ", myTags.ToArray()) +
                    "\ntheirs=" + string.Join(", ", theirTags.ToArray()));
                return false;
            }
        }
        return true;
    }

    public HandSlot GetHandSlot(int ind) { return _slots[ind]; }

    public Vector3 GetEmptyHandSlotPos() {
        foreach (HandSlot slot in _slots) {
            if (!slot.IsFull())
                return slot.transform.position;
        }
        return Vector3.zero;
    }

    public int NumFullSlots() {
        string str = "";
        int total = 0;
        foreach (HandSlot slot in _slots) {
            if (slot.IsFull()) {
                total++;
                Hex hex = slot.GetHex();
                if (hex is TileBehav)
                    str += "[" + ((TileBehav)hex).tile.ElementsToString() + "] ";
                else
                    str += "[?] ";
            } else
                str += "[ ] ";
        }
        MMDebug.MMLog.Log("HAND", "black", str);
        return total;
    }


    #region ---------- REARRANGEMENT ----------

    // On MouseDown 
    public void GrabHex(Hex hex) {
        //NumFullSlots();
        for (int i = 0; i < MAX_HAND_SIZE; i++) {
            HandSlot slot = _slots[i];
            Hex slotHex = slot.GetHex();
            if (slotHex != null && hex.EqualsTag(slotHex)) {
                //MMDebug.MMLog.Log("HAND", "black", "Found it at index=" + i);

                _placeholder = GameObject.Instantiate(_placeholderPF).GetComponent<TileBehav>();
                _placeholder.transform.position = slot.transform.position;

                slot.SetHex(_placeholder);
                _placeholderSlot = slot;

                AudioController.Trigger(SFX.Hex.Pickup);
                break;
            }
        }
    }

    // called from OnMouseDrag
    public void Rearrange(HandSlot newSlot) {
        if (newSlot.handIndex == _placeholderSlot.handIndex)
            return;

        //MMDebug.MMLog.Log("HAND", "black", "Before:");
        //NumFullSlots();

        // first, find the slot to swap towards
        HandSlot swapSlot = null;
        int newSlotInd = newSlot.handIndex;
        // downward
        for (int i = newSlotInd; i < MAX_HAND_SIZE; i++) {
            if (!_slots[i].IsFull() || i == _placeholderSlot.handIndex) {
                swapSlot = _slots[i];
                break;
            }
        }
        if (swapSlot == null) {
            // upward
            for (int i = newSlotInd; i >= 0; i--) {
                if (!_slots[i].IsFull() || i == _placeholderSlot.handIndex) {
                    swapSlot = _slots[i];
                    break;
                }
            }
        }

        //MMDebug.MMLog.Log("HAND", "black", "Swap index=" + swapSlot.handIndex);

        // downward swap
        if (swapSlot.handIndex > newSlotInd) {
            for (int s = swapSlot.handIndex; s > newSlotInd; s--) {
                Hex hex = _slots[s - 1].GetHex();
                _slots[s].SetHex(hex);
                AnimationController.PlayAnim(AnimationController._Move(hex, _slots[s].transform.position));
            }
        } else { // upward
            for (int s = swapSlot.handIndex; s < newSlotInd; s++) {
                Hex hex = _slots[s + 1].GetHex();
                _slots[s].SetHex(hex);
                AnimationController.PlayAnim(AnimationController._Move(hex, _slots[s].transform.position));
            }
        }

        newSlot.SetHex(_placeholder);
        if (_placeholderSlot.GetHex().hextag.Equals("placeholder")) // i dislike this deeply
            _placeholderSlot.ClearHex();
        _placeholderSlot = newSlot;

        //AudioController.Trigger(AudioController.HexSoundEffect.Pickup);

        //MMDebug.MMLog.Log("HAND", "black", "After:");
        //NumFullSlots();
    }

    // On MouseUp
    public void ReleaseTile(Hex hex) {
        //MMDebug.MMLog.Log("HAND", "black", "ReleaseTile called! PlaceholderSlot="+placeholderSlot.handIndex);
        AnimationController.PlayAnim(AnimationController._Move(hex, _placeholderSlot.transform.position));

        _placeholderSlot.SetHex(hex);
        ClearPlaceholder();

        AudioController.Trigger(SFX.Hex.Pickup);

        //NumFullSlots();
    }

    public void ClearPlaceholder() {
        GameObject.Destroy(_placeholder.gameObject);
        _placeholder = null; //?
    }
    #endregion
}
