using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

// TODO events for beginning-of-turn effects and for passive/trigger effects
public class EffectController {

    private static MageMatch _mm;
    private static List<Effect> _beginTurnEffects, _endTurnEffects;
    //private List<MatchEffect> _matchEffects;
    private static List<DropEffect> _dropEffects;
    private static List<SwapEffect> _swapEffects;
    private static List<HealthModEffect> _healthEffects;
    private static Dictionary<string, int> _tagDict;
    private static int _effectsResolving = 0, _beginTurnRes = 0, _endTurnRes = 0; // TODO match+swap effs

    private static int c = 0;
    private static Effect.Type currentType = Effect.Type.None;

    public static void Init(MageMatch mm) {
        _mm = mm;
        _beginTurnEffects = new List<Effect>();
        _endTurnEffects = new List<Effect>();
        //_matchEffects = new List<MatchEffect>();
        _dropEffects = new List<DropEffect>();
        _swapEffects = new List<SwapEffect>();
        _healthEffects = new List<HealthModEffect>();
        _tagDict = new Dictionary<string, int>();

        _mm.AddEventContLoadEvent(OnEventContLoaded);
    }

    public static void OnEventContLoaded() {
        EventController.AddTurnBeginEvent(OnTurnBegin, EventController.Type.EventEffects);
        EventController.AddTurnEndEvent(OnTurnEnd, EventController.Type.EventEffects);
        //mm.eventCont.AddMatchEvent(OnMatch, EventController.Type.EventEffects);
        EventController.AddDropEvent(OnDrop, EventController.Type.EventEffects, EventController.Status.End);
        EventController.AddSwapEvent(OnSwap, EventController.Type.EventEffects, EventController.Status.End); // begin or end?
    }

    #region EventCont calls
    public static IEnumerator OnTurnBegin(int id) {
        yield return ResolveBeginTurnEffects();
        MMLog.Log_EffectCont("Just finished resolving TURN BEGIN effects.");
    }

    public static IEnumerator OnTurnEnd(int id) {
        yield return ResolveEndTurnEffects();
        MMLog.Log_EffectCont("Just finished resolving TURN END effects.");
    }

    //public IEnumerator OnMatch(int id, string[] seqs) {
    //    yield return ResolveMatchEffects(id);
    //    MMLog.Log_EffectCont("Just finished resolving MATCH effects.");
    //}

    public static IEnumerator OnDrop(int id, bool playerAction, string tag, int col) {
        yield return ResolveDropEffects(id, playerAction, tag, col);
        MMLog.Log_EffectCont("Just finished resolving DROP effects.");
    }

    public static IEnumerator OnSwap(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        yield return ResolveSwapEffects(id, c1, r1, c2, r2);
        MMLog.Log_EffectCont("Just finished resolving SWAP effects.");
    }
    #endregion

    public static string GenFullTag(string effType, string tag) {
        string fullTag = effType + "-";
        if (_tagDict.ContainsKey(tag)) {
            _tagDict[tag]++;
            fullTag += tag + "-" + _tagDict[tag].ToString("D3");
        } else {
            _tagDict.Add(tag, 1);
            fullTag += tag + "-001";
        }
        MMLog.Log_EffectCont("...adding effect with tag " + fullTag);
        return fullTag;
    }

    #region TurnEffects
    public static string AddBeginTurnEffect(Effect e, string tag) {
        // TODO insert at correct position for priority
        e.tag = GenFullTag("begt", tag);
        int i;
        for (i = 0; i < _beginTurnEffects.Count; i++) {
            Effect listE = _beginTurnEffects[i];
            if ((int)listE.type < (int)e.type)
                break;
        }
        _beginTurnEffects.Insert(i, e);
        if ((int)e.type > (int)currentType && _beginTurnRes > 0)
            c++; // correct list if this effect is getting put in the front
        return e.tag;
    }

    public static string AddEndTurnEffect(Effect e, string tag) {
        // TODO test somehow?
        e.tag = GenFullTag("endt", tag);
        int i;
        for (i = 0; i < _endTurnEffects.Count; i++) {
            Effect listE = _endTurnEffects[i];
            if ((int)listE.type < (int)e.type)
                break;    
        }
        _endTurnEffects.Insert(i, e);
        if ((int)e.type > (int)currentType && _endTurnRes > 0)
            c++; // correct list if this effect is getting put in the front
        return e.tag;
    }

    public static void RemoveTurnEffect(Effect e) {
        RemoveTurnEffect(e.tag);
    }

    public static void RemoveTurnEffect(string tag) {
        List<Effect> list;
        if (tag.Substring(0, 4) == "begt")
            list = _beginTurnEffects;
        else
            list = _endTurnEffects;

        int i;
        for (i = 0; i < list.Count; i++) {
            Effect listE = list[i];
            if (listE.tag == tag) {
                MMLog.Log_EffectCont("found effect with tag " + tag);
                list.RemoveAt(i); // can be moved up?
                return;
            }
        }
        MMLog.LogError("EffectController: Missed the remove! tag="+ tag);
    }

    // TODO generalize
    public static Effect GetTurnEffect(string tag) {
        if (tag == "")
            return null;

        List<Effect> list;
        if (tag.Substring(0, 4) == "begt")
            list = _beginTurnEffects;
        else
            list = _endTurnEffects;

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

    public static IEnumerator ResolveBeginTurnEffects() {
        _beginTurnRes++;
        for (c = 0; c < _beginTurnEffects.Count; c++) { //foreach
            Effect e = _beginTurnEffects[c];
            Effect.Type t = currentType = e.type;
            MMLog.Log_EffectCont(e.tag + " (type " + t + ", p" + (int)t + ") has " + e.TurnsLeft() + " turns left (including this one).");
            yield return e.Turn();

            if (e.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                MMLog.Log_EffectCont("Removing " + e.tag + "...");
                _beginTurnEffects.RemoveAt(c);
                if (e is Enchantment)
                    ((Enchantment)e).GetEnchantee().ClearEnchantment(false);
                c--;
            }
        }
        _beginTurnRes--;
    }

    public static IEnumerator ResolveEndTurnEffects() {
        _endTurnRes++;
        Effect e;
        for (c = 0; c < _endTurnEffects.Count; c++) { // foreach
            e = _endTurnEffects[c];
            Effect.Type t = currentType = e.type;
            MMLog.Log_EffectCont(e.tag + " (type " + t + ", p" + (int)t + ") has " + e.TurnsLeft() + " turns left (including this one).");
            yield return e.Turn();

            if (e.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                MMLog.Log_EffectCont("Removing " + e.tag + "...");
                _endTurnEffects.RemoveAt(c);
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
        //for (int i = 0; i < _matchEffects.Count; i++) { // foreach
        //    MatchEffect me = _matchEffects[i];
        //    Effect.Type t = me.type;
        //    MMLog.Log_EffectCont(me.tag + " (type " + t + ", p" + (int)t + ") has " + me.TurnsLeft() + " turns left (including this one).");
        //    yield return me.Turn();

        //    if (me.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
        //        MMLog.Log_EffectCont("Removing " + me.tag + "...");
        //        _matchEffects.RemoveAt(i);
        //        i--;
        //    }
        //}

        for (int i = 0; i < _dropEffects.Count; i++) { // foreach
            DropEffect de = _dropEffects[i];
            Effect.Type t = de.type;
            MMLog.Log_EffectCont(de.tag + " (type " + t + ", p" + (int)t + ") has " + de.TurnsLeft() + " turns left (including this one).");
            yield return de.Turn();

            if (de.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                MMLog.Log_EffectCont("Removing " + de.tag + "...");
                _dropEffects.RemoveAt(i);
                i--;
            }
        }
        for (int i = 0; i < _swapEffects.Count; i++) { // foreach
            SwapEffect se = _swapEffects[i];
            Effect.Type t = se.type;
            MMLog.Log_EffectCont(se.tag + " (type " + t + ", p" + (int)t + ") has " + se.TurnsLeft() + " turns left (including this one).");
            yield return se.Turn();

            if (se.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                MMLog.Log_EffectCont("Removing " + se.tag + "...");
                _swapEffects.RemoveAt(i);
                i--;
            }
        }
        for (int i = 0; i < _healthEffects.Count; i++) { // foreach
            HealthModEffect he = _healthEffects[i];
            Effect.Type t = he.type;
            MMLog.Log_EffectCont(he.tag + " (type " + t + ", p" + (int)t + ") has " + he.TurnsLeft() + " turns left (including this one).");
            yield return he.Turn();

            if (he.NeedRemove()) { // if it's the last pass of the effect (turnsLeft == 0)
                MMLog.Log_EffectCont("Removing " + he.tag + "...");
                _healthEffects.RemoveAt(i);
                i--;
            }
        }

        _endTurnRes--;
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
    public static void AddDropEffect(DropEffect de, string tag) {
        de.tag = GenFullTag("drop", tag);
        _dropEffects.Add(de);
    }

    public static IEnumerator ResolveDropEffects(int id, bool playerAction, string tag, int col) {
        DropEffect de;
        for (int i = 0; i < _dropEffects.Count; i++) { // foreach
            de = _dropEffects[i];
            if (de.isGlobal || de.playerID == id) {
                MMLog.Log_EffectCont("Checking dropEff with tag " + de.tag + "; count=" + de.countLeft);

                yield return de.TriggerEffect(playerAction, tag, col);

                if (de.NeedRemove()) {
                    MMLog.Log_EffectCont("Removing " + de.tag + "...");
                    _dropEffects.RemoveAt(i);
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
    public static void AddSwapEffect(SwapEffect se, string tag) {
        se.tag = GenFullTag("swap", tag);
        _swapEffects.Add(se);
    }

    public static IEnumerator ResolveSwapEffects(int id, int c1, int r1, int c2, int r2) {
        SwapEffect se;
        for (int i = 0; i < _swapEffects.Count; i++) { // foreach
            se = _swapEffects[i];
            if (se.isGlobal || se.playerID == id) {
                MMLog.Log_EffectCont("Checking swapEff with tag " + se.tag + "; count=" + se.countLeft);

                yield return se.TriggerEffect(c1, r1, c2, r2);

                if (se.NeedRemove()) {
                    MMLog.Log_EffectCont("Removing " + se.tag + "...");
                    _swapEffects.RemoveAt(i);
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
    public static void AddHealthEffect(HealthModEffect he, string tag) {
        he.tag = GenFullTag("hlth", tag);
        _healthEffects.Add(he);
    }

    private static int healthResult_add = 0;
    private static float healthResult_mult = 1;

    public static void ResolveHealthEffects(int id, int dmg, bool dealing) {
        HealthModEffect he;
        healthResult_add = 0;
        healthResult_mult = 1;

        Player p = _mm.GetPlayer(id);

        for (int i = 0; i < _healthEffects.Count; i++) { // foreach
            he = _healthEffects[i];
            if (he.playerID == id && he.isDealing == dealing) {
                MMLog.Log_EffectCont("Checking healthEff with tag " + he.tag + "; count=" + he.countLeft);

                if (he.isAdditive)
                    healthResult_add += (int)he.GetResult(p, dmg);
                else
                    healthResult_mult *= he.GetResult(p, dmg);

                if (he.NeedRemove()) {
                    MMLog.Log_EffectCont("Removing " + he.tag + "...");
                    _healthEffects.RemoveAt(i);
                    i--;
                }
            }
        }

        //yield return null;
    }

    public static int GetHEResult_Additive() { return healthResult_add; }
    public static float GetHEResult_Mult() { return healthResult_mult; }

    //public void RemoveSwapEffect(string tag) { // TODO test
    //    SwapEffect se = swapEffects[0];
    //    for (int i = 0; i < swapEffects.Count; se = swapEffects[i], i++) {
    //        if (se.tag == tag)
    //            break;
    //    }
    //    swapEffects.Remove(se);
    //}
    #endregion

    public static bool IsResolving() { return _effectsResolving + _beginTurnRes + _endTurnRes > 0; }

    public static object[] GetLists() {
        return new object[4] {
            _beginTurnEffects,
            _endTurnEffects,
            _dropEffects,
            _swapEffects
        };
    }

}
