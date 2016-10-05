using UnityEngine;
using System.Collections;

public class TurnEffect { // TODO enact effect right away! skip current turn count!

	public delegate void MyTurnEffect(int id); // research delegates please?

	private int turnsLeft;
	private int playerID;
	private MyTurnEffect turnEffect;
	private MyTurnEffect endEffect;

	public TurnEffect(int id, int turns, MyTurnEffect turnEffect, MyTurnEffect endEffect){
		this.playerID = id;
		this.turnsLeft = turns;
		this.turnEffect = turnEffect;
		this.endEffect = endEffect;
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

	public int TurnsRemaining(){
		return turnsLeft;
	}
}
