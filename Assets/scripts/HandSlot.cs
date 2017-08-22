using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSlot : MonoBehaviour {

    public int handPosition = 1, handIndex = 0;

    private HandObject hex = null;

    public void SetHex(HandObject hex) { this.hex = hex; }

    public HandObject GetHex() { return hex; }

    public void ClearHex() { hex = null; }

    public bool IsFull() { return hex != null; }

}
