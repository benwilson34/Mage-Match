using UnityEngine;
using System.Collections;

public class CellBehav : MonoBehaviour {

	public int col, row;

    public bool HasSamePos(CellBehav cb) { return cb.col == col && cb.row == row; }

}
