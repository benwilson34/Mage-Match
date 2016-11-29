using UnityEngine;
using System.Collections;

public class TurnEffect { // TODO enact effect right away! skip current turn count!

	public delegate void MyTurnEffect(int id); // research delegates please?
	public delegate void MyCancelEffect(int id, TileBehav tb); // research delegates please?

	private int turnsLeft;
	private int playerID;
	private MyTurnEffect turnEffect, endEffect;
	private MyCancelEffect cancelEffect;
//	private bool enchantment = false;
	private TileBehav enchantee;

	public TurnEffect(int id, int turns, MyTurnEffect turnEffect, MyTurnEffect endEffect, MyCancelEffect cancelEffect){
		this.playerID = id;
		this.turnsLeft = turns;
		this.turnEffect = turnEffect;
		this.endEffect = endEffect;
		this.cancelEffect = cancelEffect;
	}

	public void SetAsEnchantment(TileBehav tb){
		this.enchantee = tb;
	}

	public bool IsEnchantment(){
		return enchantee != null;
	}

	public TileBehav GetEnchantee(){
		return enchantee;
	}

	public bool ResolveEffect(){
		turnsLeft--;
		if (turnsLeft > 0) {
			turnEffect (playerID);
			return false;
		} else {
			EndEffect ();
			return true;
		}
	}

	public void EndEffect(){
		endEffect (playerID);
	}

	public void CancelEffect(TileBehav tb){
		cancelEffect (playerID, tb);
	}

	public int TurnsRemaining(){
		return turnsLeft;
	}
}
