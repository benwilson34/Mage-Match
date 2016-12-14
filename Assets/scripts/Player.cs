using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Player {

	public string name;
	public int id;
	public int health;
	public int AP;
	public List<TileBehav> hand; // private
	public int handSize = 5;
	public int tilesPlaced, tilesSwapped, matches;
    public Character character;

	private Spell currentSpell;
	private MageMatch mm;
	private MatchEffect matchEffect;

	private float buff_dmgMult = 1;
	private int buff_dmgExtra;

	public Transform handSlot;
	private const float vert = 0.866025f; // sqrt(3) / 2 ... it's the height of an equilateral triangle, used to offset the horiz position on the board

	public Player (int playerNum) {
		AP = 0;
		hand = new List<TileBehav>();
		mm = GameObject.Find ("board").GetComponent<MageMatch> ();

		switch (playerNum) {
		case 1: 
			SetName ("Maxsimilous Forza");
			id = 1;
			handSlot = GameObject.Find ("handslot1").transform;
			//loadout = new Loadout (UIController.GetLoadoutNum(1));
            character = CharacterLoader.Load(UIController.GetLoadoutNum(1));
			break;
		case 2: 
			SetName ("Quincy Shungle");
			id = 2;
			handSlot = GameObject.Find ("handslot2").transform;
            character = CharacterLoader.Load(UIController.GetLoadoutNum(2));
			break;
		default:
			break;
		}

		health = character.GetMaxHealth();
	}

	public void SetName(string name){
		this.name = name;	
	}

	public void DealDamage(int amount){
		MageMatch.GetOpponent (id).ChangeHealth (-amount);
	}

	public void ChangeHealth(int amount){ // TODO clamp instead?
		if(amount < 0) { // damage
			if (buff_dmgExtra > 0)
				Debug.Log ("PLAYER: Wow, " + name + " is taking " + buff_dmgExtra + " extra damage!");
			amount = (int)(amount * buff_dmgMult) + buff_dmgExtra;
			amount = -1 * Mathf.Min (Mathf.Abs(amount), health); // prevent negative health
		} else // healing
			amount = Mathf.Min (amount, character.GetMaxHealth() - health);
		Debug.Log ("PLAYER: " + name + " had health changed from " + health + " to " + (health + amount) + ".");
		health += amount;
		if (health == 0)
			MageMatch.EndTheGame ();
	}

	public void DrawTiles(int numTiles){
		for (int i = 0; i < numTiles && hand.Count < handSize; i++) {
			GameObject go = mm.GenerateTile (character.GetTileElement());
			if (id == 1)
				go.transform.position = new Vector3 (-5, 2);
			else if (id == 2)
				go.transform.position = new Vector3 (5, 2);

			go.transform.SetParent (handSlot, false);

			TileBehav tb = go.GetComponent<TileBehav> ();
			hand.Add (tb);
		}
		AudioController.PickupSound (mm.GetComponent<AudioSource> ());
		AlignHand (.12f, true);
	}

	public void AlignHand(float duration, bool linear){
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

	public int DiscardRandom(int count){
		int tilesInHand = hand.Count;
		int i;
		for(i = 0; i < count; i++){
			if (tilesInHand > 0) {
				int rand = Random.Range (0, tilesInHand);
				GameObject go = hand[rand].gameObject;
				hand.RemoveAt (rand);
				GameObject.Destroy(go);
			}
		}
		return i;
	}

	public void FlipHand(){
		foreach (TileBehav tb in hand) {
			tb.FlipTile ();
		}
	}

    public void EmptyHand() {
        while (hand.Count > 0) {
            GameObject.Destroy(hand[0].gameObject);
            hand.RemoveAt(0);
        }
    }

	public void InitAP(){
		AP = 3;
	}

	public bool CastSpell(int index){ // TODO
		Spell spell = character.GetSpell (index);
		if (AP >= spell.APcost) {
			currentSpell = spell;
			spell.Cast ();
			return true;
		} else 
			return false;
	}

	public void ApplyAPCost(){
		AP -= currentSpell.APcost;
		if (AP == 0)
			FlipHand ();
	}

	public TileSeq GetCurrentBoardSeq(){
		return currentSpell.GetBoardSeq ();
	}

	public void SetMatchEffect(MatchEffect effect){
        // TODO bool for success? able to overwrite?
		matchEffect = effect;
	}

    public void ClearMatchEffect() {
        matchEffect = null;
    }

	public void ResolveMatchEffect(){
        if (matchEffect != null && matchEffect.ResolveEffect())
            matchEffect = null;
	}

	public void ChangeBuff_DmgMult(float d){
		Debug.Log ("PLAYER: " + name + " had dmg multiply buff changed to " + d);
		buff_dmgMult = d;
	}

	public void ChangeBuff_DmgExtra(int amount){
		Debug.Log ("PLAYER: " + name + " had dmg bonus buff changed to +" + amount);
		buff_dmgExtra = amount;
	}
}
