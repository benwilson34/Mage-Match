using UnityEngine;
using System.Collections;

public class Spell {

	public string name;
	public int APcost;
	public int turns; // TODO

	private TileSeq seq;
	private TileSeq boardSeq;

	public delegate void MySpellEffect();
	private MySpellEffect effect;
	
	public Spell(string name, string seq, int APcost, MySpellEffect effect){
		this.name = name;
		this.seq = new TileSeq (seq);
		this.APcost = APcost;
		this.effect = effect;
	}

	public void Cast(){
		effect ();
	}

	public Tile.Element GetElementAt(int index){
		return seq.GetElementAt (index);
	}

	public TileSeq GetTileSeq(){
		return seq;
	}

	public void SetBoardSeq(TileSeq boardSeq){
		this.boardSeq = boardSeq;
	}

	public TileSeq GetBoardSeq(){
		return boardSeq;
	}

}
