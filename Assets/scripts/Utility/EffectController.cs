using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO events for beginning-of-turn effects and for passive/trigger effects
public class EffectController {

    private List<Effect> beginTurnEffects, endTurnEffects;

    public EffectController() {
        beginTurnEffects = new List<Effect>();
        endTurnEffects = new List<Effect>();
    }

    public void InitEvents(EventController eventCont) {
        eventCont.turnBegin += OnTurnBegin;
        eventCont.turnEnd += OnTurnEnd;
    }

    #region EventCont calls
    public void OnTurnBegin(int id) {
        ResolveBeginTurnEffects();
    }

    public void OnTurnEnd(int id) {
        ResolveEndTurnEffects();
    }
    #endregion

    public void AddBeginTurnEffect(Effect e) {
        // TODO insert at correct position for priority
        beginTurnEffects.Add(e);
    }

    public void AddEndTurnEffect(Effect e) {
        // TODO test somehow?
        int i;
        for (i = 0; i < endTurnEffects.Count; i++) {
            Effect listE = endTurnEffects[i];
            if (listE.priority < e.priority)
                break;    
        }
        endTurnEffects.Insert(i, e);
    }

    // TODO not the right way to do this
    public void RemoveEndTurnEffect(Effect e) {
        endTurnEffects.Remove(e);
    }

    public void ResolveBeginTurnEffects() {
        Effect e;
        for (int i = 0; i < beginTurnEffects.Count; i++) {
            e = beginTurnEffects[i];
            if (e.ResolveEffect()) { // if it's the last pass of the effect (turnsLeft == 0)
                beginTurnEffects.Remove(e);
                if (e is Enchantment)
                    ((Enchantment)e).GetEnchantee().ClearEnchantment();
                i--;
            } else {
                Debug.Log("MAGEMATCH: Beginning-of-turn effect " + i + " has " + e.TurnsRemaining() + " turns left.");
            }
        }
    }

    public void ResolveEndTurnEffects() { // TODO test priority
        Effect e;
        for (int i = 0; i < endTurnEffects.Count; i++) {
            e = endTurnEffects[i];
            if (e.ResolveEffect()) { // if it's the last pass of the effect (turnsLeft == 0)
                endTurnEffects.Remove(e);
                if (e is Enchantment)
                    ((Enchantment)e).GetEnchantee().ClearEnchantment();
                i--;
            } else {
                Debug.Log("EFFECTCONTROLLER: End-of-turn effect " + i + " with priority " + e.priority + " has " + e.TurnsRemaining() + " turns left.");
            }
        }
    }
}
