using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSlot : MonoBehaviour {

    public int handPosition = 1, handIndex = 0;

    private Hex _hex = null;

    public void SetHex(Hex hex) { this._hex = hex; }

    public Hex GetHex() { return _hex; }

    public void ClearHex() { _hex = null; }

    public bool IsFull() { return _hex != null; }

}
