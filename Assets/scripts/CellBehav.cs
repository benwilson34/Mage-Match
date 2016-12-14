using UnityEngine;
using System.Collections;

public class CellBehav : MonoBehaviour {

	public int col, row;

    public bool HasSamePos(CellBehav cb) { return cb.col == col && cb.row == row; }
	// TODO hovers/arrows?
//	void OnMouseDown(){
//		InputController.CorrectMouseDown ();
//	}
//
//	void OnMouseUp(){
//		InputController.CorrectMouseUp ();
//	}
}
