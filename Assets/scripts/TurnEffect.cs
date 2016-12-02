// REPLACED BY EFFECT.CS. SLATED FOR DELETION 12/1/2016

//using UnityEngine;
//using System.Collections;

//public class TurnEffect { // TODO enact effect right away! skip current turn count!

//	public delegate void Effect(int id, TileBehav tb); // research delegates please?

//	private int turnsLeft;
//	private int playerID;
//	private Effect turnEffect, endEffect, cancelEffect;
////	private bool enchantment = false;
//	private TileBehav enchantee;

//	public TurnEffect(int id, int turns, Effect turnEffect, Effect endEffect, Effect cancelEffect){
//		playerID = id;
//		turnsLeft = turns;
//		this.turnEffect = turnEffect;
//		this.endEffect = endEffect;
//		this.cancelEffect = cancelEffect;
//	}

//	public void SetAsEnchantment(TileBehav tb){
//		enchantee = tb;
//	}

//	public bool IsEnchantment(){
//		return enchantee != null;
//	}

//	public TileBehav GetEnchantee(){
//		return enchantee;
//	}

//    public void TriggerEffect() {
//        turnEffect(playerID, enchantee);
//    }

//	public bool ResolveEffect(){
//		turnsLeft--;
//		if (turnsLeft != 0) {
//			turnEffect (playerID, enchantee);
//			return false;
//		} else {
//			EndEffect ();
//			return true;
//		}
//	}

//	public void EndEffect(){
//		endEffect (playerID, enchantee);
//	}

//	public void CancelEffect(){
//		cancelEffect (playerID, enchantee);
//	}

//	public int TurnsRemaining(){
//		return turnsLeft;
//	}
//}
