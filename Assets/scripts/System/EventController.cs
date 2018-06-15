//using System; // maybe?
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class EventController {

    public static bool handlingEvents = false; // worth it?

    private static MageMatch _mm;

    public static void Init(MageMatch mm) {
        _mm = mm;
        _swap = new List<MMEvent>();
        _turnBegin = new List<MMEvent>();
        _turnEnd = new List<MMEvent>();
        _drop = new List<MMEvent>();
        _handChange = new List<MMEvent>();
        _spellCast = new List<MMEvent>();
    }

    // this could be better done with a Dictionary
    static List<MMEvent> GetEventList(MMEvent.Type type) {
        switch (type) {
            case MMEvent.Type.TurnBegin:
                return _turnBegin;
            case MMEvent.Type.TurnEnd:
                return _turnEnd;
            case MMEvent.Type.HandChange:
                return _handChange;
            case MMEvent.Type.Drop:
                return _drop;
            case MMEvent.Type.Swap:
                return _swap;
            case MMEvent.Type.SpellCast:
                return _spellCast;
            default:
                MMLog.LogError("EVENTCONT: Bad type name!!");
                return null;
        }
    }

    // ex. AddEvent("swap", SwapCallbackMethod, Type.EventEffects)
    static void AddEvent(MMEvent.Type type, System.Delegate e, MMEvent.Behav behav, MMEvent.Moment moment = MMEvent.Moment.End) {
        List<MMEvent> evList = GetEventList(type);

        int i = 0;
        for (; i < evList.Count; i++) {
            if ((int)evList[i].behav < (int)behav)
                break;
        }
        evList.Insert(i, new MMEvent { ev = e, behav = behav, moment = moment });
    }

    static void RemoveEvent(MMEvent.Type type, System.Delegate e) {
        List<MMEvent> evList = GetEventList(type);

        for (int i = 0; i < evList.Count; i++) {
            if (evList[i].ev.Equals(e)) {
                evList.RemoveAt(i);
                return;
            }
        }
        MMLog.LogError("EVENTCONT: RemoveSwapEvent shouldn't get to this point.");
    }

    // -----------------------------------------------------

    public delegate void BoardActionEvent();
    public static event BoardActionEvent boardAction;
    public static void BoardAction() {
        //Debug.MMLog.Log_EventCont("EVENTCONTROLLER: BoardAction event raised, dispatching to " + boardAction.GetInvocationList().Length + " subscribers.");
        if (boardAction != null)
            boardAction.Invoke();
    }

    public delegate IEnumerator TurnEvent(int id);
    private static List<MMEvent> _turnBegin;
    public static IEnumerator TurnBegin() {
        handlingEvents = true; // worth it?
        foreach (MMEvent pack in _turnBegin) {
            yield return ((TurnEvent)pack.ev)(_mm.ActiveP.ID); // OH YEAH
        }
        MMLog.Log_EventCont("Just finished TURN BEGIN events...");
        handlingEvents = false; // worth it?
    }
    public static void AddTurnBeginEvent(TurnEvent ev, MMEvent.Behav type) {
        AddEvent(MMEvent.Type.TurnBegin, ev, type);
    }

    private static List<MMEvent> _turnEnd;
    public static IEnumerator TurnEnd() {
        handlingEvents = true; // worth it?
        foreach (MMEvent pack in _turnEnd) {
            yield return ((TurnEvent)pack.ev)(_mm.ActiveP.ID); // OH YEAH
        }
        MMLog.Log_EventCont("Just finished TURN END events...");
        handlingEvents = false; // worth it?
    }
    public static void AddTurnEndEvent(TurnEvent ev, MMEvent.Behav type) {
        AddEvent(MMEvent.Type.TurnEnd, ev, type);
    }

    public delegate void TimeoutEvent(int id);
    public static event TimeoutEvent timeout;
    public static void Timeout() {
        MMLog.Log_EventCont("Timeout event raised.");
        if (timeout != null) // never will be due to Stats
            timeout.Invoke(_mm.ActiveP.ID);
    }

    //public delegate void CommishDropEvent(string hextag, int col);
    //public static event CommishDropEvent commishDrop;
    //public static void CommishDrop(string hextag, int col) {
    //    if (commishDrop != null) // never will be due to Stats
    //        commishDrop.Invoke(hextag, col);
    //}

    //public delegate void CommishMatchEvent(string[] seqs);
    //public static event CommishMatchEvent commishMatch;
    //public static void CommishMatch(string[] seqs) {
    //    if (commishMatch != null) // never will be due to Stats
    //        commishMatch.Invoke(seqs);
    //}

    //public delegate void CommishTurnDoneEvent();
    //public static event CommishTurnDoneEvent commishTurnDone;
    //public static void CommishTurnDone() {
        //Debug.MMLog.Log_EventCont("EVENTCONT: Commish turn done.");
    //    if (commishTurnDone != null) // never will be due to Stats
    //        commishTurnDone.Invoke();
    //}


    #region GameAction events
    //public delegate void GameActionEvent(int id, int cost);
    //public static event GameActionEvent gameAction;
    //public static void GameAction(int cost) {
    //    //Debug.MMLog.Log_EventCont("EVENTCONTROLLER: GameAction called.");
    //    if (gameAction != null)
    //        gameAction.Invoke(_mm.ActiveP.id, cost);
    //}

    public enum HandChangeState { TurnBeginDeal, PlayerDraw, DrawFromEffect, Discard };
    public delegate IEnumerator HandChangeEvent(HandChangeEventArgs args);
    private static List<MMEvent> _handChange;
    public static IEnumerator HandChange(MMEvent.Moment moment, int id, string hextag, HandChangeState state) {
        handlingEvents = true; // worth it?
        foreach (MMEvent pack in _handChange) {
            //MMLog.Log_EventCont("going thru HANDCHANGE event with type " + pack.behav);
            var args = new HandChangeEventArgs(id, hextag, state);
            if (pack.moment == moment)
                yield return ((HandChangeEvent)pack.ev)(args); // OHYEAH
        }
        MMLog.Log_EventCont("Just finished HANDCHANGE events...");
        handlingEvents = false; // worth it?
    }
    public static void AddHandChangeEvent(HandChangeEvent e, MMEvent.Behav behav, MMEvent.Moment moment) {
        AddEvent(MMEvent.Type.HandChange, e, behav, moment);
    }

    public enum DropState { PlayerDrop, PromptDrop, DropFromEffect, CommishDrop };
    public delegate IEnumerator DropEvent(DropEventArgs args);
    private static List<MMEvent> _drop;
    public static IEnumerator Drop(MMEvent.Moment moment, Hex hex, int col, DropState state) {
        handlingEvents = true; // worth it?
        foreach (MMEvent pack in _drop) {
            //MMLog.Log_EventCont("EVENTCONT: going thru DROP event with type " + pack.behav);
            var args = new DropEventArgs(_mm.ActiveP.ID, hex, col, state); // will inactive player ever drop?
            if (pack.moment == moment)
                yield return ((DropEvent)pack.ev)(args); // OHYEAH
        }
        MMLog.Log_EventCont("Just finished DROP events...");
        handlingEvents = false; // worth it?
    }
    public static void AddDropEvent(DropEvent e, MMEvent.Behav behav, MMEvent.Moment moment) {
        AddEvent(MMEvent.Type.Drop, e, behav, moment);
    }

    public enum SwapState { PlayerSwap, PromptSwap, SwapFromEffect }
    public delegate IEnumerator SwapEvent(SwapEventArgs args);
    private static List<MMEvent> _swap;
    public static IEnumerator Swap(MMEvent.Moment moment, int c1, int r1, int c2, int r2, SwapState state) {
        handlingEvents = true; // worth it?
        foreach (MMEvent pack in _swap) {
            //Debug.MMLog.Log_EventCont("EVENTCONT: going thru swap event with priority " + pack.priority);
            var args = new SwapEventArgs(_mm.ActiveP.ID, c1, r1, c2, r2, state); // will inactive player ever swap?
            if (pack.moment == moment)
                yield return ((SwapEvent)pack.ev)(args);
        }
        MMLog.Log_EventCont("Just finished SWAP events...");
        handlingEvents = false; // worth it?
    }
    public static void AddSwapEvent(SwapEvent e, MMEvent.Behav behav, MMEvent.Moment moment) {
        AddEvent(MMEvent.Type.Swap, e, behav, moment);
    }
    // TODO similar method to convert for removing (if it's ever needed...)

    public delegate IEnumerator SpellCastEvent(int id, Spell spell, TileSeq prereq);
    private static List<MMEvent> _spellCast;
    public static IEnumerator SpellCast(MMEvent.Moment moment, Spell spell, TileSeq prereq) {
        handlingEvents = true; // worth it?
        foreach (MMEvent pack in _spellCast) {
            //Debug.MMLog.Log_EventCont("EVENTCONT: going thru swap event with priority " + pack.priority);
            if (pack.moment == moment)
                yield return ((SpellCastEvent)pack.ev)(_mm.ActiveP.ID, spell, prereq);
        }
        MMLog.Log_EventCont("Just finished SPELLCAST events...");
        handlingEvents = false; // worth it?
    }
    public static void AddSpellCastEvent(SpellCastEvent e, MMEvent.Behav behav, MMEvent.Moment moment) {
        AddEvent(MMEvent.Type.SpellCast, e, behav, moment);
    }
    #endregion


    public delegate void TileEvent(int id, TileBehav tb);
    public static event TileEvent tileRemove;
    public static void TileRemove(TileBehav tb) {
        if (tileRemove != null)
            tileRemove.Invoke(_mm.ActiveP.ID, tb);
    }

    // NOTE: id is ALWAYS the receiver
    public delegate void PlayerHealthChangeEvent(int id, int amount, int newHealth, bool dealt);
    public static event PlayerHealthChangeEvent playerHealthChange;
    public static void PlayerHealthChange(int id, int amount, int newHealth, bool dealt) {
        if (playerHealthChange != null)
            playerHealthChange.Invoke(id, amount, newHealth, dealt);
    }

    public delegate void PlayerMeterChangeEvent(int id, int amount, int newMeter);
    public static event PlayerMeterChangeEvent playerMeterChange;
    public static void PlayerMeterChange(int id, int amount, int newMeter) {
        if (playerMeterChange != null)
            playerMeterChange.Invoke(id, amount, newMeter);
    }

    public delegate void GrabTileEvent(int id, string tag);
    public static event GrabTileEvent grabTile;
    public static void GrabTile(int id, string tag) {
        if (grabTile != null)
            grabTile.Invoke(id, tag);
    }
}


public class MMEvent {
    public enum Type { TurnBegin, TurnEnd, HandChange, Drop, Swap, SpellCast }
    public enum Behav { None = 0, LastStep, GameAction, Stats, EventEffects, Player, Audio, Report, Network, FirstStep }
    public enum Moment { Begin, End }
    public Behav behav;
    public Moment moment;
    public System.Delegate ev;
}


public struct HandChangeEventArgs {
    public int id;
    public string hextag;
    public EventController.HandChangeState state;
    public HandChangeEventArgs(int id, string hextag, EventController.HandChangeState state) {
        this.id = id;
        this.hextag = hextag;
        this.state = state;
    }
}


public struct DropEventArgs {
    public int id;
    public Hex hex;
    public int col;
    public EventController.DropState state;
    public DropEventArgs(int id, Hex hex, int col, EventController.DropState state) {
        this.id = id;
        this.hex = hex;
        this.col = col;
        this.state = state;
    }
}


public struct SwapEventArgs {
    public int id;
    public int c1, r1, c2, r2;
    public EventController.SwapState state;
    public TileBehav TB1 { get { /* TODO */ return null; } }

    public SwapEventArgs(int id, int c1, int r1, int c2, int r2, EventController.SwapState state) {
        this.id = id;
        this.c1 = c1;
        this.r1 = r1;
        this.c2 = c2;
        this.r2 = r2;
        this.state = state;
    }
} 