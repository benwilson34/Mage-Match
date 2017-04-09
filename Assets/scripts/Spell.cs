using UnityEngine;
using System.Collections;

public class Spell {

	public string name;
	public int APcost;
    public int index;
    public bool core;
    public string effectTag = "";

	private TileSeq seq;
	private TileSeq boardSeq;
    private int cooldown;

	public delegate IEnumerator MySpellEffect();
	private MySpellEffect effect;
	
	public Spell(int index, string name, string seq, int APcost, MySpellEffect effect) : this(index, name, 0, APcost, effect){
        this.seq = new TileSeq (seq);
        core = false;
        //Debug.Log(">>>>>>>>SPELL: Normal constructor called. core=" + core);
    }

    public Spell(int index, string name, int cooldown, int APcost, MySpellEffect effect) { // core spell
        this.index = index;
		this.name = name;
        this.cooldown = cooldown;
		this.APcost = APcost;
		this.effect = effect;
        this.seq = new TileSeq(); // empty seq...be wary of errors...
        this.boardSeq = new TileSeq(); // empty seq...be wary of errors...
        core = true;
        //Debug.Log(">>>>>>>>SPELL: Core constructor called. core="+core);
    }

	public IEnumerator Cast(){
        if (core) {
            MageMatch mm = GameObject.Find("board").GetComponent<MageMatch>();
            TurnEffect te = new TurnEffect(cooldown, null, null, null);
            te.priority = 1; //?
            effectTag = mm.effectCont.AddEndTurnEffect(te, name.Substring(0, 5) + "-C");
        }
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
