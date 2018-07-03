using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;
using System;

// TODO events for beginning-of-turn effects and for passive/trigger effects
public static class EffectManager {

    private static MageMatch _mm;
    private static List<EventEffect> _turnBeginEffects, _turnEndEffects;
    private static List<EventEffect> _handChangeEffects;
    private static List<EventEffect> _dropEffects;
    private static List<EventEffect> _swapEffects;
    private static List<HealthModEffect> _healthModEffects;
    private static List<TileEffect> _tileEffects;
    private static Dictionary<string, int> _tagDict;
    private static int _effectsResolving = 0, _beginTurnRes = 0, _endTurnRes = 0; // TODO match+swap effs

    private static int c = 0;
    //private static Effect.Behav currentType = Effect.Behav.None;


    #region ---------- INIT ----------

    public static void Init(MageMatch mm) {
        _mm = mm;
        _turnBeginEffects = new List<EventEffect>();
        _turnEndEffects = new List<EventEffect>();
        _handChangeEffects = new List<EventEffect>();
        _dropEffects = new List<EventEffect>();
        _swapEffects = new List<EventEffect>();
        _healthModEffects = new List<HealthModEffect>();
        _tileEffects = new List<TileEffect>();
        _tagDict = new Dictionary<string, int>();

        _mm.AddEventContLoadEvent(OnEventContLoaded);
    }

    public static void OnEventContLoaded() {
        EventController.AddTurnBeginEvent(ResolveTurnBeginEffects, MMEvent.Behav.EventEffects);
        EventController.AddTurnEndEvent(OnTurnEnd, MMEvent.Behav.EventEffects);
        EventController.AddHandChangeEvent(ResolveHandChangeEffects, MMEvent.Behav.EventEffects, MMEvent.Moment.End);
        EventController.AddDropEvent(ResolveDropEffects, MMEvent.Behav.EventEffects, MMEvent.Moment.End);
        EventController.AddSwapEvent(ResolveSwapEffects, MMEvent.Behav.EventEffects, MMEvent.Moment.End); // begin or end?
    }

    // this also resolves TurnEndEffects
    public static IEnumerator OnTurnEnd(int id) {
        foreach (MMEvent.Type type in Enum.GetValues(typeof(MMEvent.Type))) {
            if (type == MMEvent.Type.SpellCast) // skip this one
                continue;

            bool isEndTurnEffect = type == MMEvent.Type.TurnEnd;
            var effectList = GetEventEffectList(type);
            for (int i = 0; i < effectList.Count; i++) {
                var effect = effectList[i];
                if (isEndTurnEffect)
                    yield return ((TurnEffect)effect).OnTurnEffect(id);
                effect.DecTurnsLeft();
                if (effect.NeedRemove) {
                    yield return effect.OnEndEffect();
                    effectList.RemoveAt(i);
                    i--;
                }
            }
        }

        for (int i = 0; i < _healthModEffects.Count; i++) {
            var effect = _healthModEffects[i];
            effect.DecTurnsLeft();
            if (effect.NeedRemove) {
                yield return effect.OnEndEffect();
                _healthModEffects.RemoveAt(i);
                i--;
            }
        }

        yield return null;
    }
    #endregion


    #region ---------- EVENTEFFECT RESOLUTION ----------

    public static IEnumerator ResolveTurnBeginEffects(int id) {
        _beginTurnRes++;
        for (c = 0; c < _turnBeginEffects.Count; c++) { //foreach
            TurnEffect e = (TurnEffect)_turnBeginEffects[c];
            //Effect.Behav t = currentType = e.behav;
            //MMLog.Log_EffectCont(e.tag + " (type " + t + ", p" + (int)t + ") has " + e.turnsLeft + " turns left (including this one).");
            yield return e.OnTurnEffect(id);
            e.DecTurnsLeft();

            if (e.NeedRemove) { // can this be done internally to the module?
                MMLog.Log_EffectCont("Removing " + e.tag + "...");
                yield return e.OnEndEffect();
                _turnBeginEffects.RemoveAt(c);
                c--;
            }
        }
        MMLog.Log_EffectCont("Just finished resolving TURN BEGIN effects.");
        _beginTurnRes--;
    }

    public static IEnumerator ResolveHandChangeEffects(HandChangeEventArgs args) {
        for (int i = 0; i < _handChangeEffects.Count; i++) { // foreach
            var hce = (HandChangeEffect)_handChangeEffects[i];
            MMLog.Log_EffectCont("Checking handChangeEff with tag " + hce.tag + "; count=" + hce.countLeft);

            yield return hce.OnHandChange(args);

            if (hce.NeedRemove) {
                yield return hce.OnEndEffect();
                MMLog.Log_EffectCont("Removing " + hce.tag + "...");
                _handChangeEffects.RemoveAt(i);
                i--;
            }
        }
    }

    public static IEnumerator ResolveDropEffects(DropEventArgs args) {
        for (int i = 0; i < _dropEffects.Count; i++) { // foreach
            var de = (DropEffect)_dropEffects[i];
            MMLog.Log_EffectCont("Checking dropEff with tag " + de.tag + "; count=" + de.countLeft);

            yield return de.OnDrop(args);

            if (de.NeedRemove) {
                yield return de.OnEndEffect();
                MMLog.Log_EffectCont("Removing " + de.tag + "...");
                _dropEffects.RemoveAt(i);
                i--;
            }
        }
        MMLog.Log_EffectCont("Just finished resolving DROP effects.");
    }

    public static IEnumerator ResolveSwapEffects(SwapEventArgs args) {
        for (int i = 0; i < _swapEffects.Count; i++) { // foreach
            var se = (SwapEffect)_swapEffects[i];
            MMLog.Log_EffectCont("Checking swapEff with tag " + se.tag + "; count=" + se.countLeft);

            yield return se.OnSwap(args);

            if (se.NeedRemove) {
                yield return se.OnEndEffect();
                MMLog.Log_EffectCont("Removing " + se.tag + "...");
                _swapEffects.RemoveAt(i);
                i--;
            }
        }
        MMLog.Log_EffectCont("Just finished resolving SWAP effects.");
    }
    #endregion


    static string GenFullTag(int id, MMEvent.Type eventType, string title) {
        return GenFullTag(id, eventType.ToString(), title);
    }

    static string GenFullTag(int id, string cat, string title) {
        string fullTag = id + "-" + cat + "-";
        if (_tagDict.ContainsKey(title)) {
            _tagDict[title]++;
            fullTag += title + "-" + _tagDict[title].ToString("D3");
        } else {
            _tagDict.Add(title, 1);
            fullTag += title + "-001";
        }
        MMLog.Log_EffectCont("...adding effect with tag " + fullTag);
        return fullTag;
    }


    #region ---------- EVENTEFFECTS ----------

    static List<EventEffect> GetEventEffectList(MMEvent.Type type) {
        switch (type) {
            case MMEvent.Type.TurnBegin:
                return _turnBeginEffects;
            case MMEvent.Type.TurnEnd:
                return _turnEndEffects;
            case MMEvent.Type.HandChange:
                return _handChangeEffects;
            case MMEvent.Type.Drop:
                return _dropEffects;
            case MMEvent.Type.Swap:
                return _swapEffects;
            default:
                return null;
        }
    }

    public static string AddEventEffect(EventEffect e) {
        // insert at correct position for priority
        e.tag = GenFullTag(e.playerId, e.eventType, e.title);
        var effectList = GetEventEffectList(e.eventType);
        int i;
        for (i = 0; i < effectList.Count; i++) {
            var listModule = effectList[i];
            if ((int)listModule.eventType < (int)e.eventType)
                break;
        }
        effectList.Insert(i, e);

        //if ((int)e.eventType > (int)currentType && _beginTurnRes > 0)
        //    c++; // correct list if this effect is getting put in the front
        return e.tag;
    }

    public static void RemoveEventEffect(string tag) {
        var effectList = GetEventEffectList(Effect.TagType(tag));
        for (int i = 0; i < effectList.Count; i++) {
            var listEff = effectList[i];
            if (listEff.tag == tag) {
                effectList.RemoveAt(i);
                return;
            }
        }
    }

    public static string AddTileEffect(TileEffect e) {
        e.tag = GenFullTag(e.playerId, "TileEffect", e.title);
        _tileEffects.Add(e);

        return e.tag;
    }

    public static void RemoveTileEffect(TileEffect e) {
        Debug.Log("Looking for " + e.tag);
        for (int i = 0; i < _tileEffects.Count; i++) {
            var listEff = _tileEffects[i];
            Debug.Log("Found " + listEff.tag);
            if (listEff.tag == e.tag) {
                listEff.ClearEffects();
                _tileEffects.RemoveAt(i);
                return;
            }
        }
    }
    #endregion


    #region ---------- HEALTHEFFECTS ----------
    public static void AddHealthMod(HealthModEffect he) {
        he.tag = GenFullTag(he.playerId, "HealthMod", he.title);
        _healthModEffects.Add(he);
    }

    public static void RemoveHealthMod(string tag) {
        for (int i = 0; i < _healthModEffects.Count; i++) {
            var listEff = _healthModEffects[i];
            if (listEff.tag == tag) {
                _healthModEffects.RemoveAt(i);
                return;
            }
        }
    }

    // TODO CalculateHealthEffects method

    public static void ResolveHealthEffects(int id, int dmg, bool dealing, out int result_add, out float result_mult) {
        result_add = 0;
        result_mult = 1;

        Player p = _mm.GetPlayer(id);

        for (int i = 0; i < _healthModEffects.Count; i++) { // foreach
            var he = _healthModEffects[i];
            if (he.affectingPlayer == id && he.IsDealing == dealing) {
                MMLog.Log_EffectCont("Checking healthEff with tag " + he.tag + "; count=" + he.countLeft);

                if (he.IsAdditive)
                    result_add += (int)he.GetResult(p, dmg);
                else
                    result_mult *= he.GetResult(p, dmg);

                if (he.NeedRemove) {
                    MMLog.Log_EffectCont("Removing " + he.tag + "...");
                    _healthModEffects.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    #endregion


    public static bool IsResolving() { return _effectsResolving + _beginTurnRes + _endTurnRes > 0; }

    public static LastingEffect[][] GetLists() {
        return new LastingEffect[7][] {
            _turnBeginEffects.ToArray(),
            _turnEndEffects.ToArray(),
            _handChangeEffects.ToArray(),
            _dropEffects.ToArray(),
            _swapEffects.ToArray(),
            _healthModEffects.ToArray(),
            _tileEffects.ToArray()
        };
    }

}
