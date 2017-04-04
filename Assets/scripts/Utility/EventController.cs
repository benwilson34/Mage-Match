//using System; // maybe?
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventController {

    MageMatch mm;

    public bool handlingEvents = false; // worth it?

    public EventController(MageMatch mm) {
        this.mm = mm;
        swaps = new List<EventPack>();
        turnEnds = new List<EventPack>();
    }

    struct EventPack { public System.Delegate ev; public int priority; }

    List<EventPack> GetEventList(string type) {
        switch (type) {
            case "swap":
                return swaps;
            case "turnEnd":
                return turnEnds;
            default:
                Debug.LogError("EVENTCONT: Bad type name!!");
                return null;
        }
    }

    // ex. AddEvent("swap", SwapCallbackMethod, 1)
    public void AddEvent(string type, System.Delegate e, int priority) {
        List<EventPack> evList = GetEventList(type);

        int i = 0;
        for (; i < evList.Count; i++) {
            if (evList[i].priority < priority)
                break;
        }
        evList.Insert(i, new EventPack { ev = e, priority = priority });
    }


    public void RemoveEvent(string type, System.Delegate e) {
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

    public delegate IEnumerator TurnEndEvent(int id);
    private List<EventPack> turnEnds;
    public IEnumerator TurnEnd() {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in turnEnds) {
            yield return ((TurnEndEvent)pack.ev)(mm.ActiveP().id); // OH YEAH
        }
        Debug.Log("EVENTCONT: Just finished turnEnd events...");
        handlingEvents = false; // worth it?
    }
    public void AddTurnEndEvent(TurnEndEvent ev, int priority) {
        AddEvent("turnEnd", ev, priority);
    }


    public delegate void TurnBeginEvent(int id);
    public event TurnBeginEvent turnBegin;
    public void TurnBegin() {
        if (turnBegin != null) // never will be due to Stats
            turnBegin.Invoke(mm.ActiveP().id);
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

    public delegate void DrawEvent(int id, Tile.Element elem, bool dealt);
    public event DrawEvent draw;
    public void Draw(int id, Tile.Element elem, bool dealt) {
        //Debug.Log("EVENTCONTROLLER: Draw called.");
        if (draw != null)
            draw.Invoke(id, elem, dealt);
    }

    public delegate void DropEvent(int id, Tile.Element elem, int col);
    public event DropEvent drop;
    public void Drop(Tile.Element elem, int col) {
        if (drop != null)
            drop.Invoke(mm.ActiveP().id, elem, col);
    }

    public delegate IEnumerator SwapEvent(int id, int c1, int r1, int c2, int r2);
    private List<EventPack> swaps;
    public IEnumerator Swap(int c1, int r1, int c2, int r2) {
        handlingEvents = true; // worth it?
        foreach (EventPack pack in swaps) {
            Debug.Log("EVENTCONT: going thru swap event with priority " + pack.priority);
            yield return ((SwapEvent)pack.ev)(mm.ActiveP().id, c1, r1, c2, r2); // OH YEAH
        }
        Debug.Log("EVENTCONT: Just finished swap events...");
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


    public delegate void MatchEvent(int id, string[] seqs);
    public event MatchEvent match;
    public void Match(string[] seqs) {
        //Debug.Log("EVENTCONTROLLER: Match event raised, dispatching to " + match.GetInvocationList().Length + " subscribers.");
        if (match != null) // never will be due to Stats
            match.Invoke(mm.ActiveP().id, seqs);
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
    public delegate void PlayerHealthChangeEvent(int id, int amount, bool dealt, bool sent);
    public event PlayerHealthChangeEvent playerHealthChange;
    public void PlayerHealthChange(int id, int amount, bool dealt, bool sent) {
        if (playerHealthChange != null)
            playerHealthChange.Invoke(id, amount, dealt, sent);
    }

    public delegate void GrabTileEvent(int id, Tile.Element elem);
    public event GrabTileEvent grabTile;
    public void GrabTile(Tile.Element elem) {
        if (grabTile != null)
            grabTile.Invoke(mm.ActiveP().id, elem);
    }
}
