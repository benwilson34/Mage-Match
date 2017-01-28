using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventController {

    MageMatch mm;

    public EventController(MageMatch mm) {
        this.mm = mm;
    }

    public delegate void BoardActionEvent();
    public BoardActionEvent boardAction;

    public void BoardAction() {
        //Debug.Log("EVENTCONTROLLER: BoardAction event raised, dispatching to " + boardAction.GetInvocationList().Length + " subscribers.");
        if (boardAction != null)
            boardAction.Invoke();
    }

    public delegate void TurnChangeEvent(int id);
    public event TurnChangeEvent turnChange;

    public void TurnChange() {
        if (turnChange != null) // never will be due to Stats
            turnChange.Invoke(mm.ActiveP().id);
    }

    public delegate void TimeoutEvent(int id);
    public event TimeoutEvent timeout;

    public void Timeout() {
        Debug.Log("EVENTCONTROLLER: Timeout event raised.");
        if (timeout != null) // never will be due to Stats
            timeout.Invoke(mm.ActiveP().id);
    }

    public delegate void CommishMatchEvent(int count);
    public event CommishMatchEvent commishMatch;

    public void CommishMatch(int count) {
        if (commishMatch != null) // never will be due to Stats
            commishMatch.Invoke(count);
    }

    public delegate void CommishTurnDoneEvent(int id);
    public event CommishTurnDoneEvent commishTurnDone;

    public void CommishTurnDone(int id) {
        if (commishTurnDone != null) // never will be due to Stats
            commishTurnDone.Invoke(id);
    }

    public delegate void DrawEvent(int id); // TODO add the tile they drew
    public event DrawEvent draw;

    public void Draw() {
        if (draw != null)
            draw.Invoke(mm.ActiveP().id);
    }

    public delegate void DropEvent(int id, int col);
    public event DropEvent drop;

    public void Drop(int col) {
        if (drop != null)
            drop.Invoke(mm.ActiveP().id, col);
    }

    public delegate void SwapEvent(int id, int c1, int r1, int c2, int r2);
    public event SwapEvent swap;

    public void Swap(int c1, int r1, int c2, int r2) {
        if (swap != null)
            swap.Invoke(mm.ActiveP().id, c1, r1, c2, r2);
    }

    public delegate void MatchEvent(int id, int count);
    public event MatchEvent match;

    public void Match(int count) {
        //Debug.Log("EVENTCONTROLLER: Match event raised, dispatching to " + match.GetInvocationList().Length + " subscribers.");
        if (match != null) // never will be due to Stats
            match.Invoke(mm.ActiveP().id, count);
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

    public delegate void SpellCastEvent(int id, Spell spell);
    public event SpellCastEvent spellCast;

    public void SpellCast(Spell spell) {
        if (spellCast != null) {
            spellCast.Invoke(mm.ActiveP().id, spell);
        }
    }

}
