using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventController {

    MageMatch mm;

    public EventController(MageMatch mm) {
        this.mm = mm;
    }

    public delegate void BoardActionEvent();
    public event BoardActionEvent boardAction;
    public void BoardAction() {
        //Debug.Log("EVENTCONTROLLER: BoardAction event raised, dispatching to " + boardAction.GetInvocationList().Length + " subscribers.");
        if (boardAction != null)
            boardAction.Invoke();
    }

    public delegate void TurnEndEvent(int id);
    public event TurnEndEvent turnEnd;
    public void TurnEnd() {
        if (turnEnd != null) // never will be due to Stats
            turnEnd.Invoke(mm.ActiveP().id);
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

    public delegate void CommishMatchEvent(int count);
    public event CommishMatchEvent commishMatch;
    public void CommishMatch(int count) {
        if (commishMatch != null) // never will be due to Stats
            commishMatch.Invoke(count);
    }

    public delegate void CommishTurnDoneEvent();
    public event CommishTurnDoneEvent commishTurnDone;
    public void CommishTurnDone() {
        if (commishTurnDone != null) // never will be due to Stats
            commishTurnDone.Invoke();
    }


    #region GameAction events

    public delegate void GameActionEvent(int id);
    public event GameActionEvent gameAction;
    public void GameAction() {
        //Debug.Log("EVENTCONTROLLER: GameAction called.");
        if (gameAction != null)
            gameAction.Invoke(mm.ActiveP().id);
    }

    public delegate void DrawEvent(int id, Tile.Element elem, bool dealt);
    public event DrawEvent draw;
    public void Draw(int id, Tile.Element elem, bool dealt) {
        Debug.Log("EVENTCONTROLLER: Draw called.");
        if (draw != null)
            draw.Invoke(id, elem, dealt);
    }

    public delegate void DropEvent(int id, Tile.Element elem, int col);
    public event DropEvent drop;
    public void Drop(Tile.Element elem, int col) {
        if (drop != null)
            drop.Invoke(mm.ActiveP().id, elem, col);
    }

    public delegate void SwapEvent(int id, int c1, int r1, int c2, int r2);
    public event SwapEvent swap;
    public void Swap(int c1, int r1, int c2, int r2) {
        if (swap != null)
            swap.Invoke(mm.ActiveP().id, c1, r1, c2, r2);
    }

    public delegate void SpellCastEvent(int id, Spell spell);
    public event SpellCastEvent spellCast;
    public void SpellCast(Spell spell) {
        if (spellCast != null) {
            spellCast.Invoke(mm.ActiveP().id, spell);
        }
    }

    #endregion


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

    // NOTE: id is ALWAYS the receiver
    public delegate void PlayerHealthChangeEvent(int id, int amount, bool dealt, bool sent);
    public event PlayerHealthChangeEvent playerHealthChange;
    public void PlayerHealthChange(int id, int amount, bool dealt, bool sent) {
        if (playerHealthChange != null)
            playerHealthChange.Invoke(id, amount, dealt, sent);
    }
}
