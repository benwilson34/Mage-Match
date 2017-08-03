using UnityEngine;
using System.Collections;
using MMDebug;
using System;

public class Spell {

	public string name;
	public int APcost;
    public int index;
    public bool isSymmetric;

    protected MageMatch mm;
	protected TileSeq seq;
	protected TileSeq boardSeq;

	public delegate IEnumerator MySpellEffect();
	protected MySpellEffect effect;
	
	public Spell(int index, string name, string seq, int APcost, MySpellEffect effect) 
        : this(index, name, APcost, effect) {
        this.seq = new TileSeq (seq);
        //this.boardSeq = new TileSeq(); // empty seq...be wary of errors...
        SymmetryCheck(seq);
    }

    public Spell(int index, string name, int APcost, MySpellEffect effect) {
        //MageMatch mm = GameObject.Find("board").GetComponent<MageMatch>();
        this.index = index;
        this.name = name;
        this.APcost = APcost;
        this.effect = effect;
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

    public virtual IEnumerator Cast(){
		return effect (); //yield??
	}

	public Tile.Element GetElementAt(int index){
		return seq.GetElementAt (index);
	}

	public TileSeq GetTileSeq() { return seq; }

    public virtual int GetLength() { return seq.GetSeqLength(); }

	public void SetBoardSeq(TileSeq boardSeq){
		this.boardSeq = boardSeq;
	}

	public TileSeq GetBoardSeq(){
		return boardSeq;
	}

    public string PrintSeq() { return seq.SeqAsString(); }

}



public class CooldownSpell : Spell {

    public string effectTag = "";

    private int cooldown;

    public CooldownSpell(int index, string name, int cooldown, int APcost, MySpellEffect effect) : base(index, name, APcost, effect) { // core spell
        this.cooldown = cooldown;
        this.seq = new TileSeq(); // empty seq...be wary of errors...
        this.boardSeq = new TileSeq(); // empty seq...be wary of errors...
    }

    public override IEnumerator Cast() {
        TurnEffect te = new TurnEffect(cooldown, Effect.Type.Cooldown, null, null);
        effectTag = mm.effectCont.AddEndTurnEffect(te, name.Substring(0, 5) + "-C");

        return effect(); // yield??
    }

    // TODO maybe override boardSeq stuff since it's not needed?

    public bool IsReadyToCast() {
        return mm.effectCont.GetTurnEffect(effectTag) == null;
    }
}



public class SignatureSpell : Spell {

    public int meterCost;

    public SignatureSpell(int index, string name, string seq, int APcost, int meterCost, MySpellEffect effect) : base(index, name, seq, APcost, effect) {
        this.meterCost = meterCost;
    }

    public bool IsReadyToCast() {
        MMLog.Log("SIGSPELL", "black", "Checking " + mm.ActiveP().name + "'s sig spell.");
        return mm.ActiveP().character.meter >= meterCost;
    }
}



public class CoreSpell : Spell {

    public delegate IEnumerator CoreSpellEffect(TileSeq seq);

    public Tile.Element currentElem = Tile.Element.None;

    private CoreSpellEffect coreEffect;

    public CoreSpell(int index, string name, int APcost, CoreSpellEffect coreEffect) 
        :base(index, name, APcost, null) {
        this.coreEffect = coreEffect;
        seq = new TileSeq(); // empty seq...be wary of errors...
        boardSeq = new TileSeq(); // empty seq...be wary of errors...
    }

    public IEnumerator CastCore(TileSeq seq) { // not a huge fan of this
        return coreEffect(seq); // yield?
    }

    public override int GetLength() {
        return 5; // max core spell length
    }
}
