using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventController {

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
        Debug.Log("EVENTCONTROLLER: Match event raised, dispatching to " + turnChange.GetInvocationList().Length + " subscribers.");
        if (match != null) // never will due to Stats
            match.Invoke(id, count);
    }

    public delegate void TurnChangeEvent(int id);
    public event TurnChangeEvent turnChange;

    public void TurnChange(int id) {
        Debug.Log("EVENTCONTROLLER: TurnChange event raised, dispatching to " + turnChange.GetInvocationList().Length + " subscribers.");
        if (turnChange != null) // never will due to Stats
            turnChange.Invoke(id);
    }



}
