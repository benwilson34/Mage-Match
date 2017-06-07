using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSlot : MonoBehaviour {

    public int handPosition = 1, handIndex = 0;

    private TileBehav tile = null;

    public void SetTile(TileBehav tb) { tile = tb; }

    public TileBehav GetTile() { return tile; }

    public void ClearTile() { tile = null; }

    public bool IsFull() { return tile != null; }

}
