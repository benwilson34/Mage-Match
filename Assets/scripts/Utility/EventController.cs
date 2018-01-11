﻿//using System; // maybe?
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class EventController {

    public enum Type { None = 0, LastStep, GameAction, Stats, EventEffects, Player, Audio, Network, FirstStep }
    public enum Status { Begin, End }
    public bool handlingEvents = false; // worth it?

    private MageMatch mm;

    public EventController(MageMatch mm) {
        this.mm = mm;
        swap = new List<EventPack>();
        turnBegin = new List<EventPack>();
        turnEnd = new List<EventPack>();
        match = new List<EventPack>();
        drop = new List<EventPack>();
        draw = new List<EventPack>();
        discard = new List<EventPack>();
    }

    struct EventPack {
        public System.Delegate ev;
        public Type type;
        public Status status;
    }

    List<EventPack> GetEventList(string type) {
        switch (type) {
            case "swap":
                return swap;
            case "turnBegin":
                return turnBegin;
            case "turnEnd":
                return turnEnd;
            case "match":
                return match;
            case "draw":
                return draw;
            case "drop":
                return drop;
            case "discard":
                return discard;
            default:
                MMLog.LogError("EVENTCONT: Bad type name!!");
                return null;
        }
    }

    // ex. AddEvent("swap", SwapCallbackMethod, Type.EventEffects)
    void AddEvent(string list, System.Delegate e, Type type, Status status = Status.End) {
        List<EventPack> evList = GetEventList(list);

        int i = 0;
        for (; i < evList.Count; i++) {
            if ((int)evList[i].type < (int)type)
                break;
        }
        evList.Insert(i, new EventPack { ev = e, type = type, status = status });
    }

    void RemoveEvent(string type, System.Delegate e) {
        List<EventPack> evList = GetEventList(type);

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
    public event BoardActionEvent boardAction;
    public void BoardAction() {
        //Debug.MMLog.Log_EventCont("EVENTCONTROLLER: BoardAction event raised, dispatching to " + boardAction.GetInvocationList().Length + " subscribers.");
        if (boardAction != null)
            boardAction.Invoke();
    }

    public delegate IEnumerator TurnBeginEvent(int id);
    private List<EventPack> turnBegin;
    public IEnumerator TurnBegin() {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in turnBegin) {
            yield return ((TurnBeginEvent)pack.ev)(mm.ActiveP().id); // OH YEAH
        }
        MMLog.Log_EventCont("Just finished TURN BEGIN events...");
        handlingEvents = false; // worth it?
    }
    public void AddTurnBeginEvent(TurnBeginEvent ev, Type type) {
        AddEvent("turnBegin", ev, type);
    }

    public delegate IEnumerator TurnEndEvent(int id);
    private List<EventPack> turnEnd;
    public IEnumerator TurnEnd() {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in turnEnd) {
            yield return ((TurnEndEvent)pack.ev)(mm.ActiveP().id); // OH YEAH
        }
        MMLog.Log_EventCont("Just finished TURN END events...");
        handlingEvents = false; // worth it?
    }
    public void AddTurnEndEvent(TurnEndEvent ev, Type type) {
        AddEvent("turnEnd", ev, type);
    }

    public delegate void TimeoutEvent(int id);
    public event TimeoutEvent timeout;
    public void Timeout() {
        MMLog.Log_EventCont("Timeout event raised.");
        if (timeout != null) // never will be due to Stats
            timeout.Invoke(mm.ActiveP().id);
    }

    public delegate void CommishDropEvent(Tile.Element elem, int col);
    public event CommishDropEvent commishDrop;
    public void CommishDrop(Tile.Element elem, int col) {
        if (commishDrop != null) // never will be due to Stats
            commishDrop.Invoke(elem, col);
    }

    public delegate void CommishMatchEvent(string[] seqs);
    public event CommishMatchEvent commishMatch;
    public void CommishMatch(string[] seqs) {
        if (commishMatch != null) // never will be due to Stats
            commishMatch.Invoke(seqs);
    }

    public delegate void CommishTurnDoneEvent();
    public event CommishTurnDoneEvent commishTurnDone;
    public void CommishTurnDone() {
        //Debug.MMLog.Log_EventCont("EVENTCONT: Commish turn done.");
        if (commishTurnDone != null) // never will be due to Stats
            commishTurnDone.Invoke();
    }


    #region GameAction events
    public delegate void GameActionEvent(int id, bool costsAP);
    public event GameActionEvent gameAction;
    public void GameAction(bool costsAP) {
        //Debug.MMLog.Log_EventCont("EVENTCONTROLLER: GameAction called.");
        if (gameAction != null)
            gameAction.Invoke(mm.ActiveP().id, costsAP);
    }

    public delegate IEnumerator DrawEvent(int id, string tag, bool playerAction, bool dealt);
    private List<EventPack> draw;
    public IEnumerator Draw(Status status, int id, string tag, bool playerAction, bool dealt) {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in draw) {
            MMLog.Log_EventCont("going thru draw event with type " + pack.type);
            if (pack.status == status)
                yield return ((DrawEvent)pack.ev)(id, tag, playerAction, dealt); // OHYEAH
        }
        MMLog.Log_EventCont("Just finished DRAW events...");
        handlingEvents = false; // worth it?
    }
    public void AddDrawEvent(DrawEvent e, Type type, Status status) {
        AddEvent("draw", e, type, status);
    }

    public delegate IEnumerator DropEvent(int id, bool playerAction, string tag, int col);
    private List<EventPack> drop;
    public IEnumerator Drop(Status status, bool playerAction, string tag, int col) {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in drop) {
            //Debug.MMLog.Log_EventCont("EVENTCONT: going thru swap event with priority " + pack.priority);
            if (pack.status == status)
                yield return ((DropEvent)pack.ev)(mm.ActiveP().id, playerAction, tag, col); // OHYEAH
        }
        MMLog.Log_EventCont("Just finished DROP events...");
        handlingEvents = false; // worth it?
    }
    public void AddDropEvent(DropEvent e, Type type, Status status) {
        AddEvent("drop", e, type, status);
    }

    public delegate IEnumerator SwapEvent(int id, bool playerAction, int c1, int r1, int c2, int r2);
    private List<EventPack> swap;
    public IEnumerator Swap(Status status, bool playerAction, int c1, int r1, int c2, int r2) {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in swap) {
            //Debug.MMLog.Log_EventCont("EVENTCONT: going thru swap event with priority " + pack.priority);
            if(pack.status == status)
                yield return ((SwapEvent)pack.ev)(mm.ActiveP().id, playerAction, c1, r1, c2, r2);
        }
        MMLog.Log_EventCont("Just finished SWAP events...");
        handlingEvents = false; // worth it?
    }
    public void AddSwapEvent(SwapEvent e, Type type, Status status) {
        AddEvent("swap", e, type, status);
    }
    // TODO similar method to convert for removing (if it's ever needed...)

    public delegate void SpellCastEvent(int id, Spell spell);
    public event SpellCastEvent spellCast;
    public void SpellCast(Spell spell) {
        if (spellCast != null) {
            spellCast.Invoke(mm.ActiveP().id, spell);
        }
    }
    #endregion


    public delegate IEnumerator MatchEvent(int id, string[] seqs);
    private List<EventPack> match;
    public IEnumerator Match(string[] seqs) {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in match) {
            //Debug.MMLog.Log_EventCont("EVENTCONT: Resolving matchEvent with p="+pack.priority);
            yield return ((MatchEvent)pack.ev)(mm.ActiveP().id, seqs); // OH YEAH
        }
        MMLog.Log_EventCont("Just finished MATCH events...");
        handlingEvents = false; // worth it?
    }
    public void AddMatchEvent(MatchEvent ev, Type type) {
        AddEvent("match", ev, type);
    }
    public void RemoveMatchEvent(MatchEvent ev) {
        RemoveEvent("match", ev);
    }

    //public delegate void CascadeEvent(int id, int chain);
    //public event CascadeEvent cascade;
    //public void Cascade(int chain) {
    //    if (cascade != null)
    //        cascade.Invoke(mm.ActiveP().id, chain);
    //}

    public delegate void TileRemoveEvent(int id, TileBehav tb);
    public event TileRemoveEvent tileRemove;
    public void TileRemove(TileBehav tb) {
        if (tileRemove != null)
            tileRemove.Invoke(mm.ActiveP().id, tb);
    }

    // NOTE: id is ALWAYS the receiver
    public delegate void PlayerHealthChangeEvent(int id, int amount, bool dealt);
    public event PlayerHealthChangeEvent playerHealthChange;
    public void PlayerHealthChange(int id, int amount, bool dealt) {
        if (playerHealthChange != null)
            playerHealthChange.Invoke(id, amount, dealt);
    }

    public delegate void PlayerMeterChangeEvent(int id, int amount);
    public event PlayerMeterChangeEvent playerMeterChange;
    public void PlayerMeterChange(int id, int amount) {
        if (playerMeterChange != null)
            playerMeterChange.Invoke(id, amount);
    }

    public delegate void GrabTileEvent(int id, string tag);
    public event GrabTileEvent grabTile;
    public void GrabTile(int id, string tag) {
        if (grabTile != null)
            grabTile.Invoke(id, tag);
    }

    public delegate IEnumerator DiscardEvent(int id, string tag);
    private List<EventPack> discard;
    public IEnumerator Discard(int id, string tag) {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in discard) {
            //Debug.MMLog.Log_EventCont("EVENTCONT: Resolving matchEvent with p="+pack.priority);
            yield return ((DiscardEvent)pack.ev)(id, tag); // OH YEAH
        }
        MMLog.Log_EventCont("Just finished DISCARD events...");
        handlingEvents = false; // worth it?
    }
    public void AddDiscardEvent(DiscardEvent ev, Type type) {
        AddEvent("discard", ev, type);
    }
    public void RemoveDiscardEvent(DiscardEvent ev) {
        RemoveEvent("discard", ev);
    }
}
