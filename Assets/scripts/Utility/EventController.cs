using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventController {

    public delegate void DrawEvent(int id); // add the tile they drew
    public event DrawEvent draw;

    public void Draw(int id) {
        if (draw != null)
            draw.Invoke(id);
    }

    public delegate void DropEvent(int col);
    public event DropEvent drop;

    public void Drop(int col) {
        if (drop != null)
            drop.Invoke(col);
    }

    public delegate void SwapEvent(int c1, int r1, int c2, int r2);
    public event SwapEvent swap;

    public void Swap(int c1, int r1, int c2, int r2) {
        if (swap != null)
            swap.Invoke(c1, r1, c2, r2);
    }

    public delegate void MatchEvent(int id, int count);
    public event MatchEvent match;

    public void Match(int id, int count) {
        Debug.Log("EVENTCONTROLLER: Match event raised, dispatching to " + match.GetInvocationList().Length + " subscribers.");
        if (match != null) // never will be due to Stats
            match.Invoke(id, count);
    }

    public delegate void CommishMatchEvent(int count);
    public event CommishMatchEvent commishMatch;

    public void CommishMatch(int count) {
        Debug.Log("EVENTCONTROLLER: CommishMatch event raised, dispatching to " + commishMatch.GetInvocationList().Length + " subscribers.");
        if (commishMatch != null) // never will be due to Stats
            commishMatch.Invoke(count);
    }

    public delegate void BoardActionEvent();
    public BoardActionEvent boardAction;

    public void BoardAction() {
        //Debug.Log("EVENTCONTROLLER: BoardAction event raised, dispatching to " + boardAction.GetInvocationList().Length + " subscribers.");
        if (boardAction != null)
            boardAction.Invoke();
    }

    public delegate void TileRemoveEvent(int id, TileBehav tb);
    public event TileRemoveEvent tileRemove;

    public void TileRemove(int id, TileBehav tb) {
        if (tileRemove != null)
            tileRemove.Invoke(id, tb);
    }

    public delegate void TurnChangeEvent(int id);
    public event TurnChangeEvent turnChange;

    public void TurnChange(int id) {
        Debug.Log("EVENTCONTROLLER: TurnChange event raised, dispatching to " + turnChange.GetInvocationList().Length + " subscribers.");
        if (turnChange != null) // never will be due to Stats
            turnChange.Invoke(id);
    }

    public delegate void SpellCastEvent(int id, Spell spell);
    public event SpellCastEvent spellCast;

    public void SpellCast(int id, Spell spell) {
        if (spellCast != null) {
            spellCast.Invoke(id, spell);
        }
    }

}
