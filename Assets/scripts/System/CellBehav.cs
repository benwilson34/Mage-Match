using UnityEngine;
using System.Collections;

public class CellBehav : MonoBehaviour {

	public int col, row;

    public bool HasSamePos(CellBehav cb) { return cb.col == col && cb.row == row; }

    public string PrintCoord() { return string.Format("({0},{1})", col, row); }

    public bool EqualsCoord(CellBehav cb) { return cb.col == this.col && cb.row == this.row; }

}
