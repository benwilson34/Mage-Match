﻿using UnityEngine;
using System.Collections;

public class Spell {

	public string name;
	public int APcost;
    public int index;

	private TileSeq seq;
	private TileSeq boardSeq;

	public delegate IEnumerator MySpellEffect();
	private MySpellEffect effect;
	
	public Spell(int index, string name, string seq, int APcost, MySpellEffect effect){
        this.index = index;
		this.name = name;
		this.seq = new TileSeq (seq);
		this.APcost = APcost;
		this.effect = effect;
	}

	public IEnumerator Cast(){
        Debug.Log("SPELL: Casting...");
        if(effect == null)
            Debug.Log("SPELL: Effect is null!");

		return effect ();
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
