//using System; // maybe?
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventController {

    MageMatch mm;

    public bool handlingEvents = false; // worth it?

    public EventController(MageMatch mm) {
        this.mm = mm;
        swap = new List<EventPack>();
        turnBegin = new List<EventPack>();
        turnEnd = new List<EventPack>();
        match = new List<EventPack>();
        drop = new List<EventPack>();
    }

    struct EventPack { public System.Delegate ev; public int priority; }

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
            case "drop":
                return drop;
            default:
                Debug.LogError("EVENTCONT: Bad type name!!");
                return null;
        }
    }

    // ex. AddEvent("swap", SwapCallbackMethod, 1)
    void AddEvent(string type, System.Delegate e, int priority) {
        List<EventPack> evList = GetEventList(type);

        int i = 0;
        for (; i < evList.Count; i++) {
            if (evList[i].priority < priority)
                break;
        }
        evList.Insert(i, new EventPack { ev = e, priority = priority });
    }


    void RemoveEvent(string type, System.Delegate e) {
        List<EventPack> evList = GetEventList(type);

        for (int i = 0; i < evList.Count; i++) {
            if (evList[i].ev.Equals(e)) {
                evList.RemoveAt(i);
                return;
            }
        }
        Debug.LogError("EVENTCONT: RemoveSwapEvent shouldn't get to this point.");
    }

    // -----------------------------------------------------

    public delegate void BoardActionEvent();
    public event BoardActionEvent boardAction;
    public void BoardAction() {
        //Debug.Log("EVENTCONTROLLER: BoardAction event raised, dispatching to " + boardAction.GetInvocationList().Length + " subscribers.");
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
        Debug.Log("EVENTCONT: Just finished TURN BEGIN events...");
        handlingEvents = false; // worth it?
    }
    public void AddTurnBeginEvent(TurnBeginEvent ev, int priority) {
        AddEvent("turnBegin", ev, priority);
    }

    public delegate IEnumerator TurnEndEvent(int id);
    private List<EventPack> turnEnd;
    public IEnumerator TurnEnd() {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in turnEnd) {
            yield return ((TurnEndEvent)pack.ev)(mm.ActiveP().id); // OH YEAH
        }
        Debug.Log("EVENTCONT: Just finished TURN END events...");
        handlingEvents = false; // worth it?
    }
    public void AddTurnEndEvent(TurnEndEvent ev, int priority) {
        AddEvent("turnEnd", ev, priority);
    }

    public delegate void TimeoutEvent(int id);
    public event TimeoutEvent timeout;
    public void Timeout() {
        Debug.Log("EVENTCONTROLLER: Timeout event raised.");
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
        //Debug.Log("EVENTCONT: Commish turn done.");
        if (commishTurnDone != null) // never will be due to Stats
            commishTurnDone.Invoke();
    }


    #region GameAction events
    public delegate void GameActionEvent(int id, bool costsAP);
    public event GameActionEvent gameAction;
    public void GameAction(bool costsAP) {
        //Debug.Log("EVENTCONTROLLER: GameAction called.");
        if (gameAction != null)
            gameAction.Invoke(mm.ActiveP().id, costsAP);
    }

    // TODO IEnum instead
    public delegate void DrawEvent(int id, bool playerAction, bool dealt, Tile.Element elem);
    public event DrawEvent draw;
    public void Draw(int id, bool playerAction, bool dealt, Tile.Element elem) {
        //Debug.Log("EVENTCONTROLLER: Draw called.");
        if (draw != null)
            draw.Invoke(id, playerAction, dealt, elem);
    }

    public delegate IEnumerator DropEvent(int id, bool playerAction, Tile.Element elem, int col);
    private List<EventPack> drop;
    public IEnumerator Drop(bool playerAction, Tile.Element elem, int col) {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in drop) {
            //Debug.Log("EVENTCONT: going thru swap event with priority " + pack.priority);
            yield return ((DropEvent)pack.ev)(mm.ActiveP().id, playerAction, elem, col); // OHYEAH
        }
        Debug.Log("EVENTCONT: Just finished DROP events...");
        handlingEvents = false; // worth it?
    }
    public void AddDropEvent(DropEvent se, int priority) {
        AddEvent("drop", se, priority);
    }

    public delegate IEnumerator SwapEvent(int id, bool playerAction, int c1, int r1, int c2, int r2);
    private List<EventPack> swap;
    public IEnumerator Swap(bool playerAction, int c1, int r1, int c2, int r2) {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in swap) {
            //Debug.Log("EVENTCONT: going thru swap event with priority " + pack.priority);
            yield return ((SwapEvent)pack.ev)(mm.ActiveP().id, playerAction, c1, r1, c2, r2); // OH YEAH
        }
        Debug.Log("EVENTCONT: Just finished SWAP events...");
        handlingEvents = false; // worth it?
    }
    public void AddSwapEvent(SwapEvent se, int priority) {
        AddEvent("swap", se, priority);
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
            //Debug.Log("EVENTCONT: Resolving matchEvent with p="+pack.priority);
            yield return ((MatchEvent)pack.ev)(mm.ActiveP().id, seqs); // OH YEAH
        }
        Debug.Log("EVENTCONT: Just finished MATCH events...");
        handlingEvents = false; // worth it?
    }
    public void AddMatchEvent(MatchEvent ev, int priority) {
        AddEvent("match", ev, priority);
    }
    public void RemoveMatchEvent(MatchEvent ev) {
        RemoveEvent("match", ev);
    }

    public delegate void CascadeEvent(int id, int chain);
    public event CascadeEvent cascade;
    public void Cascade(int chain) {
        if (cascade != null)
            cascade.Invoke(mm.ActiveP().id, chain);
    }

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

    public delegate void GrabTileEvent(int id, Tile.Element elem);
    public event GrabTileEvent grabTile;
    public void GrabTile(int id, Tile.Element elem) {
        if (grabTile != null)
            grabTile.Invoke(id, elem);
    }
}
