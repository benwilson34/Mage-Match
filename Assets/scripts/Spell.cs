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

    protected MageMatch mm;
	protected TileSeq seq;

	public delegate IEnumerator MySpellEffect(TileSeq prereq);
	protected MySpellEffect effect;
	
	public Spell(int index, string name, string seq, MySpellEffect effect, int APcost = 1) 
        : this(index, name, effect, APcost) {
        this.seq = new TileSeq (seq);
        SymmetryCheck(seq);
    }

    public Spell(int index, string name, MySpellEffect effect, int APcost = 1) {
        //MageMatch mm = GameObject.Find("board").GetComponent<MageMatch>();
        this.index = index;
        this.name = name;
        this.effect = effect;
        this.APcost = APcost;
    }

    public void Init(MageMatch mm) {
        this.mm = mm;
    }

    void SymmetryCheck(string seq) {
        string first = seq.Substring(0, seq.Length / 2);
        char[] arr = seq.ToCharArray();
        Array.Reverse(arr);
        string temp = new string(arr);
        string second = temp.Substring(0, temp.Length / 2);
        isSymmetric = first.Equals(second);
    }

    public virtual IEnumerator Cast(TileSeq prereq){
		return effect (prereq); //yield??
	}

	public Tile.Element GetElementAt(int index){
		return seq.GetElementAt (index);
	}

	public TileSeq GetTileSeq() { return seq; }

    public virtual int GetLength() { return seq.GetSeqLength(); }

    public string PrintSeq() { return seq.SeqAsString(); }

}



public class CooldownSpell : Spell {

    public string effectTag = "";

    private int cooldown;

    public CooldownSpell(int index, string name, int cooldown, MySpellEffect effect, int APcost = 1) 
        : base(index, name, effect, APcost) { // core spell
        this.cooldown = cooldown;
        this.seq = new TileSeq(); // empty seq...be wary of errors...
    }

    public override IEnumerator Cast(TileSeq prereq) {
        TurnEffect te = new TurnEffect(cooldown, Effect.Type.Cooldown, null, null);
        effectTag = mm.effectCont.AddEndTurnEffect(te, name.Substring(0, 5) + "-C");

        return effect(prereq); // yield??
    }

    // TODO maybe override boardSeq stuff since it's not needed?

    public bool IsReadyToCast() {
        return mm.effectCont.GetTurnEffect(effectTag) == null;
    }
}



public class SignatureSpell : Spell {

    public int meterCost;

    public SignatureSpell(int index, string name, string seq, MySpellEffect effect, int APcost = 1, int meterCost = 100) : base(index, name, seq, effect, APcost) {
        this.meterCost = meterCost;
    }

    public bool IsReadyToCast() {
        MMLog.Log("SIGSPELL", "black", "Checking " + mm.ActiveP().name + "'s sig spell.");
        return mm.ActiveP().character.GetMeter() >= meterCost;
    }
}



public class CoreSpell : Spell {

    public Tile.Element currentElem = Tile.Element.None;

    public CoreSpell(int index, string name, MySpellEffect effect, int APcost = 1) 
        : base(index, name, effect, APcost) {
        seq = new TileSeq(); // empty seq...be wary of errors...
    }

    public override int GetLength() {
        return 5; // max core spell length
    }
}
