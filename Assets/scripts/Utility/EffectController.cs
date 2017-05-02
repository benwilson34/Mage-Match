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
    private int effectsResolving = 0, beginTurnRes = 0, endTurnRes = 0; // TODO match+swap effs

    private int c = 0;
    private Effect.Type currentType = Effect.Type.None;

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
        Debug.Log("EFFECTCONT: Just finished resolving TURN BEGIN effects.");
    }

    public IEnumerator OnTurnEnd(int id) {
        yield return ResolveEndTurnEffects();
        Debug.Log("EFFECTCONT: Just finished resolving TURN END effects.");
    }

    public IEnumerator OnMatch(int id, string[] seqs) {
        yield return ResolveMatchEffects(id);
        Debug.Log("EFFECTCONT: Just finished resolving MATCH effects.");
    }

    public IEnumerator OnSwap(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        yield return ResolveSwapEffects(id, c1, r1, c2, r2);
        Debug.Log("EFFECTCONT: Just finished resolving SWAP effects.");
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
            if ((int)listE.type < (int)e.type)
                break;
        }
        beginTurnEffects.Insert(i, e);
        if ((int)e.type > (int)currentType && beginTurnRes > 0)
            c++; // correct list if this effect is getting put in the front
        return e.tag;
    }

    public string AddEndTurnEffect(Effect e, string tag) {
        // TODO test somehow?
        e.tag = GenFullTag("endt", tag);
        int i;
        for (i = 0; i < endTurnEffects.Count; i++) {
            Effect listE = endTurnEffects[i];
            if ((int)listE.type < (int)e.type)
                break;    
        }
        endTurnEffects.Insert(i, e);
        if ((int)e.type > (int)currentType && endTurnRes > 0)
            c++; // correct list if this effect is getting put in the front
        return e.tag;
    }

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
        Debug.LogError("EFFECT-CONT: Missed the remove! tag="+ e.tag);
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
        beginTurnRes++;
        for (c = 0; c < beginTurnEffects.Count; c++) { //foreach
            Effect e = beginTurnEffects[c];
            Effect.Type t = currentType = e.type;
            Debug.Log("EFFECTCONT: " + e.tag + " (type " + t + ", p" + (int)t + ") has " + e.TurnsLeft() + " turns left (including this one).");
            yield return e.Turn();

            if (e.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                Debug.Log("EFFECTCONT: Removing " + e.tag + "...");
                beginTurnEffects.RemoveAt(c);
                if (e is Enchantment)
                    ((Enchantment)e).GetEnchantee().ClearEnchantment();
                c--;
            }
        }
        beginTurnRes--;
    }

    public IEnumerator ResolveEndTurnEffects() {
        endTurnRes++;
        Effect e;
        for (c = 0; c < endTurnEffects.Count; c++) { // foreach
            e = endTurnEffects[c];
            Effect.Type t = currentType = e.type;
            Debug.Log("EFFECTCONT: " + e.tag + " (type " + t + ", p" + (int)t + ") has " + e.TurnsLeft() + " turns left (including this one).");
            yield return e.Turn();

            if (e.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                Debug.Log("EFFECTCONT: Removing " + e.tag + "...");
                endTurnEffects.RemoveAt(c);
                if (e is Enchantment)
                    ((Enchantment)e).GetEnchantee().ClearEnchantment();
                c--;
            } 
        }

        // TODO generalize somehow?
        for (int i = 0; i < matchEffects.Count; i++) { // foreach
            MatchEffect me = matchEffects[i];
            Effect.Type t = me.type;
            Debug.Log("EFFECTCONT: " + me.tag + " (type " + t + ", p" + (int)t + ") has " + me.TurnsLeft() + " turns left (including this one).");
            yield return me.Turn();

            if (me.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                Debug.Log("EFFECTCONT: Removing " + me.tag + "...");
                matchEffects.RemoveAt(i);
                i--;
            }
        }
        for (int i = 0; i < swapEffects.Count; i++) { // foreach
            SwapEffect se = swapEffects[i];
            Effect.Type t = se.type;
            Debug.Log("EFFECTCONT: " + se.tag + " (type " + t + ", p" + (int)t + ") has " + se.TurnsLeft() + " turns left (including this one).");
            yield return se.Turn();

            if (se.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                Debug.Log("EFFECTCONT: Removing " + se.tag + "...");
                swapEffects.RemoveAt(i);
                i--;
            }
        }

        endTurnRes--;
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
                yield return me.TriggerEffect();

                if (me.NeedRemove()) {
                    Debug.Log("EFFECTCONT: Removing " + me.tag + "...");
                    matchEffects.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    //public void RemoveMatchEffect(string tag) { // TODO test
    //    MatchEffect me = matchEffects[0];
    //    for (int i = 0; i < matchEffects.Count; i++, me = matchEffects[i]) {
    //        if (me.tag == tag)
    //            break;
    //    }
    //    matchEffects.Remove(me);
    //}
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
            if (se.playerID == id) {
                Debug.Log("EFFECTCONT: Checking swapEff with tag " + se.tag + "; count=" + se.countLeft);

                yield return se.TriggerEffect(c1, r1, c2, r2);

                if (se.NeedRemove()) {
                    Debug.Log("EFFECTCONT: Removing " + se.tag + "...");
                    swapEffects.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    //public void RemoveSwapEffect(string tag) { // TODO test
    //    SwapEffect se = swapEffects[0];
    //    for (int i = 0; i < swapEffects.Count; se = swapEffects[i], i++) {
    //        if (se.tag == tag)
    //            break;
    //    }
    //    swapEffects.Remove(se);
    //}
    #endregion


    public bool IsResolving() { return effectsResolving + beginTurnRes + endTurnRes > 0; }

    public object[] GetLists() {
        return new object[4] {
            beginTurnEffects,
            endTurnEffects,
            matchEffects,
            swapEffects
        };
    }

}
