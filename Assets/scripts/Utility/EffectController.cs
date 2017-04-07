using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO events for beginning-of-turn effects and for passive/trigger effects
public class EffectController {

    private MageMatch mm;
    private List<Effect> beginTurnEffects, endTurnEffects;
    private List<MatchEffect> matchEffects;
    private List<SwapEffect> swapEffects;
    private Dictionary<string, int> tagDict;
    private int effectsResolving = 0;

    public EffectController(MageMatch mm) {
        this.mm = mm;
        beginTurnEffects = new List<Effect>();
        endTurnEffects = new List<Effect>();
        matchEffects = new List<MatchEffect>();
        swapEffects = new List<SwapEffect>();
        tagDict = new Dictionary<string, int>();
    }

    public void InitEvents() {
        mm.eventCont.AddTurnBeginEvent(OnTurnBegin, 3);
        mm.eventCont.AddTurnEndEvent(OnTurnEnd, 3);
        mm.eventCont.AddMatchEvent(OnMatch, 3);
        mm.eventCont.AddSwapEvent(OnSwap, 3);
    }

    #region EventCont calls
    public IEnumerator OnTurnBegin(int id) {
        yield return ResolveBeginTurnEffects();
    }

    public IEnumerator OnTurnEnd(int id) {
        yield return ResolveEndTurnEffects();
    }

    public IEnumerator OnMatch(int id, string[] seqs) {
        yield return ResolveMatchEffects(id);
    }

    public IEnumerator OnSwap(int id, int c1, int r1, int c2, int r2) {
        yield return ResolveSwapEffects(id, c1, r1, c2, r2);
        Debug.Log("EFFECTCONT: Just finished resolving swap effects.");
    }
    #endregion

    public string GenFullTag(string effType, string tag) {
        string fullTag = effType + "-";
        if (tagDict.ContainsKey(tag)) {
            tagDict[tag]++;
            fullTag += tag + "-" + tagDict[tag].ToString("D3");
        } else {
            tagDict.Add(tag, 1);
            fullTag += tag + "-001";
        }
        Debug.Log("EFFECTCONT: adding effect with tag " + fullTag);
        return fullTag;
    }

    #region TurnEffects
    public string AddBeginTurnEffect(Effect e, string tag) {
        // TODO insert at correct position for priority
        e.tag = GenFullTag("begt", tag);
        int i;
        for (i = 0; i < beginTurnEffects.Count; i++) {
            Effect listE = beginTurnEffects[i];
            if (listE.priority < e.priority)
                break;
        }
        beginTurnEffects.Insert(i, e);
        return e.tag;
    }

    public string AddEndTurnEffect(Effect e, string tag) {
        // TODO test somehow?
        e.tag = GenFullTag("endt", tag);
        int i;
        for (i = 0; i < endTurnEffects.Count; i++) {
            Effect listE = endTurnEffects[i];
            if (listE.priority < e.priority)
                break;    
        }
        endTurnEffects.Insert(i, e);
        return e.tag;
    }

    // TODO not the right way to do this
    public void RemoveTurnEffect(Effect e) {
        List<Effect> list;
        if (e.tag.Substring(0, 4) == "begt")
            list = beginTurnEffects;
        else
            list = endTurnEffects;

        int i;
        for (i = 0; i < list.Count; i++) {
            Effect listE = list[i];
            if (listE.tag == e.tag) {
                Debug.Log("EFFECT-CONT: found effect with tag " + e.tag);
                list.RemoveAt(i); // can be moved up?
                return;
            }
        }
        Debug.LogError("EFFECT-CONT: Missed the remove!");
    }

    // TODO generalize
    public Effect GetTurnEffect(string tag) {
        if (tag == "")
            return null;

        List<Effect> list;
        if (tag.Substring(0, 4) == "begt")
            list = beginTurnEffects;
        else
            list = endTurnEffects;

        int i;
        Effect e;
        for (i = 0; i < list.Count; i++) {
            e = list[i];
            if (e.tag == tag) {
                Debug.Log("EFFECT-CONT: found effect with tag " + e.tag);
                return e;
            }
        }

        return null;
    }

    public IEnumerator ResolveBeginTurnEffects() {
        effectsResolving++;
        Effect e;
        for (int i = 0; i < beginTurnEffects.Count; i++) { //foreach
            e = beginTurnEffects[i];
            bool remove = e.TurnsRemaining() == 1; // test?

            yield return e.ResolveEffect();

            if (remove) { // if it's the last pass of the effect (turnsLeft == 0)
                beginTurnEffects.Remove(e);
                if (e is Enchantment)
                    ((Enchantment)e).GetEnchantee().ClearEnchantment();
                i--;
            } else {
                Debug.Log("MAGEMATCH: Beginning-of-turn effect " + i + " has " + e.TurnsRemaining() + " turns left.");
            }
        }
        effectsResolving--;
    }

    public IEnumerator ResolveEndTurnEffects() {
        effectsResolving++;
        Effect e;
        for (int i = 0; i < endTurnEffects.Count; i++) { // foreach
            e = endTurnEffects[i];
            Debug.Log("EFFECTCONTROLLER: " + e.tag + " (p" + e.priority + ") has " + e.TurnsRemaining() + " turns left (including this one).");
            bool remove = e.TurnsRemaining() == 0; // 1?

            yield return e.ResolveEffect();

            if (remove) { // if it's the last pass of the effect (turnsLeft == 0)
                endTurnEffects.Remove(e);
                if (e is Enchantment)
                    ((Enchantment)e).GetEnchantee().ClearEnchantment();
                i--;
            } 
        }
        effectsResolving--;
    }
    #endregion


    #region MatchEffects
    public void AddMatchEffect(MatchEffect me, string tag) {
        me.tag = GenFullTag("matc", tag);
        matchEffects.Add(me);
    }

    IEnumerator ResolveMatchEffects(int id) {
        MatchEffect me;
        for (int i = 0; i < matchEffects.Count; i++) { // foreach
            me = matchEffects[i];
            if (me.playerID == id) {
                bool remove = me.TurnsRemaining() == 1; // should be count?
                yield return me.TriggerEffect();
                if (remove) {
                    matchEffects.Remove(me); // maybe not best...
                    i--;
                }
            }
        }
    }

    public void RemoveMatchEffect(string tag) { // TODO test
        MatchEffect me = matchEffects[0];
        for (int i = 0; i < matchEffects.Count; i++, me = matchEffects[i]) {
            if (me.tag == tag)
                break;
        }
        matchEffects.Remove(me);
    }
    #endregion


    #region SwapEffects
    public void AddSwapEffect(SwapEffect se, string tag) {
        se.tag = GenFullTag("swap", tag);
        swapEffects.Add(se);
    }

    public IEnumerator ResolveSwapEffects(int id, int c1, int r1, int c2, int r2) {
        SwapEffect se;
        for (int i = 0; i < swapEffects.Count; i++) { // foreach
            se = swapEffects[i];
            Debug.Log("EFFECTCONT: Checking swapEff with tag " + se.tag);
            if (se.playerID == id) {
                bool remove = se.TurnsRemaining() == 1; // should be count?
                yield return se.TriggerEffect(c1, r1, c2, r2);
                if (remove) {
                    RemoveSwapEffect(se.tag); // just removeAt...?
                    i--;
                }
            }
        }
    }

    public void RemoveSwapEffect(string tag) { // TODO test
        SwapEffect se = swapEffects[0];
        for (int i = 0; i < swapEffects.Count; se = swapEffects[i], i++) {
            if (se.tag == tag)
                break;
        }
        swapEffects.Remove(se);
    }
    #endregion


    public bool isResolving() { return effectsResolving > 0; }

    public object[] GetLists() {
        return new object[4] {
            beginTurnEffects,
            endTurnEffects,
            matchEffects,
            swapEffects
        };
    }

}
