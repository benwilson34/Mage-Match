using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

// TODO events for beginning-of-turn effects and for passive/trigger effects
public class EffectController {

    private MageMatch mm;
    private List<Effect> beginTurnEffects, endTurnEffects;
    private List<MatchEffect> matchEffects;
    private List<DropEffect> dropEffects;
    private List<SwapEffect> swapEffects;
    private List<HealthEffect> healthEffects;
    private Dictionary<string, int> tagDict;
    private int effectsResolving = 0, beginTurnRes = 0, endTurnRes = 0; // TODO match+swap effs

    private int c = 0;
    private Effect.Type currentType = Effect.Type.None;

    public EffectController(MageMatch mm) {
        this.mm = mm;
        beginTurnEffects = new List<Effect>();
        endTurnEffects = new List<Effect>();
        matchEffects = new List<MatchEffect>();
        dropEffects = new List<DropEffect>();
        swapEffects = new List<SwapEffect>();
        healthEffects = new List<HealthEffect>();
        tagDict = new Dictionary<string, int>();
    }

    public void InitEvents() {
        mm.eventCont.AddTurnBeginEvent(OnTurnBegin, EventController.Type.EventEffects);
        mm.eventCont.AddTurnEndEvent(OnTurnEnd, EventController.Type.EventEffects);
        //mm.eventCont.AddMatchEvent(OnMatch, EventController.Type.EventEffects);
        mm.eventCont.AddDropEvent(OnDrop, EventController.Type.EventEffects, EventController.Status.End);
        mm.eventCont.AddSwapEvent(OnSwap, EventController.Type.EventEffects, EventController.Status.End); // begin or end?
    }

    #region EventCont calls
    public IEnumerator OnTurnBegin(int id) {
        yield return ResolveBeginTurnEffects();
        MMLog.Log_EffectCont("Just finished resolving TURN BEGIN effects.");
    }

    public IEnumerator OnTurnEnd(int id) {
        yield return ResolveEndTurnEffects();
        MMLog.Log_EffectCont("Just finished resolving TURN END effects.");
    }

    //public IEnumerator OnMatch(int id, string[] seqs) {
    //    yield return ResolveMatchEffects(id);
    //    MMLog.Log_EffectCont("Just finished resolving MATCH effects.");
    //}

    public IEnumerator OnDrop(int id, bool playerAction, string tag, int col) {
        yield return ResolveDropEffects(id, playerAction, tag, col);
        MMLog.Log_EffectCont("Just finished resolving DROP effects.");
    }

    public IEnumerator OnSwap(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        yield return ResolveSwapEffects(id, c1, r1, c2, r2);
        MMLog.Log_EffectCont("Just finished resolving SWAP effects.");
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
        MMLog.Log_EffectCont("...adding effect with tag " + fullTag);
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
                MMLog.Log_EffectCont("found effect with tag " + e.tag);
                list.RemoveAt(i); // can be moved up?
                return;
            }
        }
        MMLog.LogError("EFFECTCONT: Missed the remove! tag="+ e.tag);
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
                MMLog.Log_EffectCont("found effect with tag " + e.tag);
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
            MMLog.Log_EffectCont(e.tag + " (type " + t + ", p" + (int)t + ") has " + e.TurnsLeft() + " turns left (including this one).");
            yield return e.Turn();

            if (e.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                MMLog.Log_EffectCont("Removing " + e.tag + "...");
                beginTurnEffects.RemoveAt(c);
                if (e is Enchantment)
                    ((Enchantment)e).GetEnchantee().ClearEnchantment(false);
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
            MMLog.Log_EffectCont(e.tag + " (type " + t + ", p" + (int)t + ") has " + e.TurnsLeft() + " turns left (including this one).");
            yield return e.Turn();

            if (e.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                MMLog.Log_EffectCont("Removing " + e.tag + "...");
                endTurnEffects.RemoveAt(c);
                if (e is Enchantment)
                    ((Enchantment)e).GetEnchantee().ClearEnchantment(false);
                else if (e is TileEffect) {
                    TileEffect te = (TileEffect)e;
                    te.GetEnchantee().RemoveTileEffect(te);
                }
                c--;
            } 
        }

        // TODO generalize somehow?
        for (int i = 0; i < matchEffects.Count; i++) { // foreach
            MatchEffect me = matchEffects[i];
            Effect.Type t = me.type;
            MMLog.Log_EffectCont(me.tag + " (type " + t + ", p" + (int)t + ") has " + me.TurnsLeft() + " turns left (including this one).");
            yield return me.Turn();

            if (me.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                MMLog.Log_EffectCont("Removing " + me.tag + "...");
                matchEffects.RemoveAt(i);
                i--;
            }
        }
        for (int i = 0; i < swapEffects.Count; i++) { // foreach
            SwapEffect se = swapEffects[i];
            Effect.Type t = se.type;
            MMLog.Log_EffectCont(se.tag + " (type " + t + ", p" + (int)t + ") has " + se.TurnsLeft() + " turns left (including this one).");
            yield return se.Turn();

            if (se.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                MMLog.Log_EffectCont("Removing " + se.tag + "...");
                swapEffects.RemoveAt(i);
                i--;
            }
        }
        for (int i = 0; i < healthEffects.Count; i++) { // foreach
            HealthEffect he = healthEffects[i];
            Effect.Type t = he.type;
            MMLog.Log_EffectCont(he.tag + " (type " + t + ", p" + (int)t + ") has " + he.TurnsLeft() + " turns left (including this one).");
            yield return he.Turn();

            if (he.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                MMLog.Log_EffectCont("Removing " + he.tag + "...");
                healthEffects.RemoveAt(i);
                i--;
            }
        }

        endTurnRes--;
    }
    #endregion


    //#region MatchEffects
    //public void AddMatchEffect(MatchEffect me, string tag) {
    //    me.tag = GenFullTag("matc", tag);
    //    matchEffects.Add(me);
    //}

    //IEnumerator ResolveMatchEffects(int id) {
    //    MatchEffect me;
    //    for (int i = 0; i < matchEffects.Count; i++) { // foreach
    //        me = matchEffects[i];
    //        if (me.playerID == id) {
    //            yield return me.TriggerEffect();

    //            if (me.NeedRemove()) {
    //                MMLog.Log_EffectCont("Removing " + me.tag + "...");
    //                matchEffects.RemoveAt(i);
    //                i--;
    //            }
    //        }
    //    }
    //}

    ////public void RemoveMatchEffect(string tag) { // TODO test
    ////    MatchEffect me = matchEffects[0];
    ////    for (int i = 0; i < matchEffects.Count; i++, me = matchEffects[i]) {
    ////        if (me.tag == tag)
    ////            break;
    ////    }
    ////    matchEffects.Remove(me);
    ////}
    //#endregion

    
    #region DropEffects
    public void AddDropEffect(DropEffect de, string tag) {
        de.tag = GenFullTag("drop", tag);
        dropEffects.Add(de);
    }

    public IEnumerator ResolveDropEffects(int id, bool playerAction, string tag, int col) {
        DropEffect de;
        for (int i = 0; i < dropEffects.Count; i++) { // foreach
            de = dropEffects[i];
            if (de.isGlobal || de.playerID == id) {
                MMLog.Log_EffectCont("Checking dropEff with tag " + de.tag + "; count=" + de.countLeft);

                yield return de.TriggerEffect(playerAction, tag, col);

                if (de.NeedRemove()) {
                    MMLog.Log_EffectCont("Removing " + de.tag + "...");
                    dropEffects.RemoveAt(i);
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


    #region SwapEffects
    public void AddSwapEffect(SwapEffect se, string tag) {
        se.tag = GenFullTag("swap", tag);
        swapEffects.Add(se);
    }

    public IEnumerator ResolveSwapEffects(int id, int c1, int r1, int c2, int r2) {
        SwapEffect se;
        for (int i = 0; i < swapEffects.Count; i++) { // foreach
            se = swapEffects[i];
            if (se.isGlobal || se.playerID == id) {
                MMLog.Log_EffectCont("Checking swapEff with tag " + se.tag + "; count=" + se.countLeft);

                yield return se.TriggerEffect(c1, r1, c2, r2);

                if (se.NeedRemove()) {
                    MMLog.Log_EffectCont("Removing " + se.tag + "...");
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


    #region HealthEffects
    public void AddHealthEffect(HealthEffect he, string tag) {
        he.tag = GenFullTag("hlth", tag);
        healthEffects.Add(he);
    }

    private int healthResult_add = 0;
    private float healthResult_mult = 1;

    public IEnumerator ResolveHealthEffects(int id, int dmg, bool dealing) {
        HealthEffect he;
        healthResult_add = 0;
        healthResult_mult = 1;

        Player p = mm.GetPlayer(id);

        for (int i = 0; i < healthEffects.Count; i++) { // foreach
            he = healthEffects[i];
            if (he.playerID == id && he.isBuff == dealing) {
                MMLog.Log_EffectCont("Checking healthEff with tag " + he.tag + "; count=" + he.countLeft);

                if (he.isAdditive)
                    healthResult_add += (int)he.GetResult(p, dmg);
                else
                    healthResult_mult *= he.GetResult(p, dmg);

                if (he.NeedRemove()) {
                    MMLog.Log_EffectCont("Removing " + he.tag + "...");
                    healthEffects.RemoveAt(i);
                    i--;
                }
            }
        }

        yield return null;
    }

    public int GetHEResult_Additive() { return healthResult_add; }
    public float GetHEResult_Mult() { return healthResult_mult; }

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
