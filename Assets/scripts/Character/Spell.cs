using UnityEngine;
using System.Collections;
using MMDebug;
using System;

public class Spell {

	public string name;
	public int APcost;
    public string info;
    public int index;
    public bool isSymmetric;

    protected MageMatch _mm;
	protected TileSeq _seq;

	public delegate IEnumerator MySpellEffect(TileSeq prereq);
	protected MySpellEffect _effect;
	
	public Spell(int index, string name, string seq, MySpellEffect effect, int APcost = 1) 
        : this(index, name, effect, APcost) {
        _seq = new TileSeq (seq);
        SymmetryCheck(seq);
    }

    public Spell(int index, string name, MySpellEffect effect, int APcost = 1) {
        //MageMatch mm = GameObject.Find("board").GetComponent<MageMatch>();
        this.index = index;
        this.name = name;
        this._effect = effect;
        this.APcost = APcost;
    }

    public void Init(MageMatch mm) {
        this._mm = mm;
    }

    void SymmetryCheck(string seq) {
        char[] arr = seq.ToCharArray();
        Array.Reverse(arr);
        string reverse = new string(arr);
        isSymmetric = seq.Equals(reverse); // same forward and backward

        //MMLog.Log("SPELL", "green", "Spell " + index + "...checking "+seq+" and "+reverse);
        //if (isSymmetric)
        //    MMLog.Log("SPELL", "green", "Spell " + index + " is symmetric!");
    }

    public virtual IEnumerator Cast(TileSeq prereq){
		return _effect (prereq); //yield??
	}

	public Tile.Element GetElementAt(int index){
		return _seq.GetElementAt(index);
	}

	public TileSeq GetTileSeq() { return _seq; }

    public virtual int GetLength() { return _seq.GetSeqLength(); }

    public string PrintSeq() { return _seq.SeqAsString(); }

}



public class MatchSpell : Spell {

    public Tile.Element currentElem = Tile.Element.None;

    public MatchSpell(int index, string name, MySpellEffect effect, int APcost = 1) 
        : base(index, name, effect, APcost) {
        _seq = new TileSeq(); // empty seq...be wary of errors...
    }

    public MatchSpell(MatchSpell copySpell) : this(copySpell.index, copySpell.name, copySpell._effect) { }

    public override int GetLength() {
        return 5; // max match spell length
    }
}



public class SignatureSpell : Spell {

    public int meterCost;

    public SignatureSpell(int index, string name, string seq, MySpellEffect effect, int APcost = 1, int meterCost = 1000) : base(index, name, seq, effect, APcost) {
        this.meterCost = meterCost;
    }

    public bool IsReadyToCast() {
        MMLog.Log("SIGSPELL", "black", "Checking " + _mm.ActiveP().name + "'s sig spell.");
        return _mm.ActiveP().character.GetMeter() >= meterCost;
    }
}