using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileSeq {

	public List<Tile> sequence; // TODO auto property?

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

    public bool IncludesTile(Tile t) {
        foreach (Tile seqt in sequence) {
            if (seqt.HasSamePos(t))
                return true;
        }
        return false;
    }

	public string SeqAsString(bool showLetters = true, bool showCoords = false){
		string letters = "", coords = "";
        foreach (Tile t in sequence) {
            letters += "" + t.ThisElementToChar();
            coords += t.PrintCoord();
        }
		return (showLetters ? letters + " " : "") + (showCoords ? coords : "");
	}

	public bool MatchesTileSeq(TileSeq compSeq){
		bool result = false;
		if (this.SeqAsString().Equals(compSeq.SeqAsString()))
			result = true;
		return result;
	}

    public TileSeq Copy() {
        TileSeq newSeq = new TileSeq();
        foreach (Tile t in sequence)
            newSeq.sequence.Add(t.Copy());
        return newSeq;
    }
}
