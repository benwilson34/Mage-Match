using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileSeq {

	public List<Tile> sequence; // TODO private?

	public TileSeq(){
		sequence = new List<Tile> ();
	}

	public TileSeq (string seq){
		char[] chars = seq.ToCharArray ();
		this.sequence = new List<Tile> ();
		foreach (char c in chars) {
			this.sequence.Add (new Tile (c));
		}
	}

	public TileSeq (Tile tile){
		sequence = new List<Tile> ();
//		Tile t = new Tile (tile.color);
		Tile t = new Tile (tile.element);
		t.SetPos (tile.col, tile.row);
		sequence.Add (t);
	}

	public int GetSeqLength(){
		return sequence.Count;
	}

	public Tile.Element GetElementAt(int index){
		if (index < sequence.Count)
			return sequence [index].element;
		else
			return Tile.Element.None;
	}

	public void SetPosAt(int index, int x, int y){
		sequence [index].SetPos(x, y);
	}

	public string SeqAsString(){
		string seqString = "";
		foreach (Tile t in sequence)
			seqString += "" + t.ThisElementToChar();
		return seqString;
	}

	public bool MatchesTileSeq(TileSeq compSeq){
		bool result = false;
		if (this.SeqAsString().Equals(compSeq.SeqAsString()))
			result = true;
		return result;
	}
}
