using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Player {

	public string name;
	public int id;

	public int health;
	public int maxAP = 3; // Action points per round
	public int AP;
	public List<TileBehav> hand;
	public int handSize = 5;
	public int tilesPlaced, tilesSwapped, matches;
	public Loadout loadout;

	// TODO buffs and debuffs
	private float buff_dmg = 1;

	private Transform handSlot;
	private const float vert = 0.866025f; // sqrt(3) / 2 ... it's the height of an equilateral triangle, used to offset the horiz position on the board

	public Player (int playerNum) {
		AP = 0;
		hand = new List<TileBehav>();

		switch (playerNum) {
		case 0: 
			SetName ("Commissioner");
			id = 0;
			loadout = new Loadout (0);
			break;
		case 1: 
			SetName ("Raucous Pinefuck");
			id = 1;
			handSlot = GameObject.Find ("handslot1").transform;
			loadout = new Loadout (UIController.GetLoadoutNum(1));
			break;
		case 2: 
			SetName ("Stevey St. Evans");
			id = 2;
			handSlot = GameObject.Find ("handslot2").transform;
			loadout = new Loadout (UIController.GetLoadoutNum(2));
			break;
		default:
			break;
		}

		health = loadout.maxHealth;
	}

	public void SetName(string name){
		this.name = name;	
	}

	public void ChangeHealth(int amount){
		if(amount < 0) { // damage
			amount = (int)(amount * buff_dmg);
			amount = -1 * Mathf.Min (Mathf.Abs(amount), health); // prevent negative health
		} else // healing
			amount = Mathf.Min (amount, loadout.maxHealth - health);
		Debug.Log (name + " had health changed from " + health + " to " + (health + amount) + ".");
		health += amount;
		if (health == 0)
			MageMatch.EndTheGame ();
	}

	public void AlignHand(float duration, bool linear){
//		TileBehav tb;
//		for(int i = 0; i < hand.Count; i++){
//			tb = hand[i];
////			Debug.Log ("AlignHand hand[" + i + "] = " + tb.transform.name + ", position is (" + handSlot.position.x + ", " + handSlot.position.y + ")");
//			if (id == 1) {
//				tb.transform.position = new Vector3 (handSlot.position.x - i, handSlot.position.y);
//				if (i >= 3) {
//					tb.transform.position = new Vector3 (handSlot.position.x - (i - 3) - .5f, handSlot.position.y - vert);
//				}
//			} else {
//				tb.transform.position = new Vector3 (handSlot.position.x + i, handSlot.position.y);
//				if (i >= 3) {
//					tb.transform.position = new Vector3 (handSlot.position.x + (i - 3) + .5f, handSlot.position.y - vert);
//				}
//			}
//		}
		GameObject.Find("board").GetComponent<MageMatch>().StartAnim(AlignHand_Anim(duration, linear));
	}

	public IEnumerator AlignHand_Anim(float dur, bool linear){
		TileBehav tb;
		Tween tween;
		for(int i = 0; i < hand.Count; i++){
			tb = hand[i];
//			Debug.Log ("AlignHand hand[" + i + "] = " + tb.transform.name + ", position is (" + handSlot.position.x + ", " + handSlot.position.y + ")");
			if (id == 1) {
				if (i < 3)
					tween = tb.transform.DOMove (new Vector3 (handSlot.position.x - i, handSlot.position.y), dur, false);
				else 
					tween = tb.transform.DOMove (new Vector3 (handSlot.position.x - (i - 3) - .5f, handSlot.position.y - vert), dur, false);
			} else {
				if (i < 3)
					tween = tb.transform.DOMove (new Vector3 (handSlot.position.x + i, handSlot.position.y), dur, false);
				else
					tween = tb.transform.DOMove (new Vector3 (handSlot.position.x + (i - 3) + .5f, handSlot.position.y - vert), dur, false);
			}
			if (linear || i == hand.Count - 1)
				yield return tween.WaitForCompletion ();
		}
	}

	public void FlipHand(){
		foreach (TileBehav tb in hand) {
			tb.FlipTile ();
		}
	}

	public void InitAP(){
		AP = maxAP;
	}

	public bool CastSpell(int index){ // TODO
		Spell spell = loadout.GetSpell (index);
		if (AP >= spell.APcost) {
			spell.Cast ();
			AP -= spell.APcost;
			if (AP == 0)
				FlipHand ();
			return true;
		} else 
			return false;
	}

	public void ChangeBuff_Dmg(float d){
		Debug.Log (name + " changed dmg buff to " + d);
		buff_dmg = d;
	}
}
