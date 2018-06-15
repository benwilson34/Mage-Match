using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MMDebug;

public abstract class Effect {

    public enum Behav { None = 0, TickDown, APChange, Damage, Healing, Destruct, Remove, Add, Enchant, Movement }

    public string tag;
    public int playerId;
    public string title;

    protected static MageMatch _mm;

    public Effect(string title) {
        this.title = title;
    }

    public static void Init(MageMatch mm) {
        _mm = mm;
    }

    public static int TagPlayer(string tag) {
        return int.Parse(tag.Split('-')[0]);
    }

    public static MMEvent.Type TagType(string tag) {
        return (MMEvent.Type)Enum.Parse(typeof(MMEvent.Type), tag.Split('-')[1]);
    }

    public static string TagTitle(string tag) {
        return tag.Split('-')[2];
    }

    public static int TagNum(string tag) {
        return int.Parse(tag.Split('-')[3]);
    }
}



public abstract class LastingEffect : Effect {

    public int turnsLeft = -1;
    public int countLeft = -1;
    public int stacks = 1; // TODO
    public bool NeedRemove { get { return turnsLeft == 0 || countLeft == 0; } }
    public Func<IEnumerator> onEndEffect;

    public LastingEffect(string title) : base(title) { }

    public void DecTurnsLeft() {
        turnsLeft--;
    }

    public void DecCountLeft() {
        countLeft--;
    }

    public virtual IEnumerator OnEndEffect() {
        if(onEndEffect != null)
            yield return onEndEffect();
        yield return null;
    }
}



public abstract class EventEffect : LastingEffect {

    public MMEvent.Type eventType;
    public Behav behav;

    public EventEffect(int id, MMEvent.Type type, Behav behav, string title) : base(title) {
        this.playerId = id;
        this.eventType = type;
        this.behav = behav;
    }
}



public abstract class TurnEffect : EventEffect {

    private EventController.TurnEvent _turnEffect;

    public TurnEffect(MMEvent.Type eventType, int id, string title, Behav behav, EventController.TurnEvent turnEffect) : base(id, eventType, behav, title) {
        this._turnEffect = turnEffect;
    }

    public IEnumerator OnTurnEffect(int id) {
        if (_turnEffect != null) {
            yield return _turnEffect(id);
            DecCountLeft();
        }
    }
}

public class TurnBeginEffect : TurnEffect {
    public TurnBeginEffect(int id, string title, Behav behav, EventController.TurnEvent turnEffect)
        : base(MMEvent.Type.TurnBegin, id, title, behav, turnEffect) { }
}

public class TurnEndEffect : TurnEffect {
    public TurnEndEffect(int id, string title, Behav behav, EventController.TurnEvent turnEffect)
        : base(MMEvent.Type.TurnEnd, id, title, behav, turnEffect) { }
}



public class HandChangeEffect : EventEffect {

    private EventController.HandChangeEvent _handEffect;

    public HandChangeEffect(int id, string title, Behav behav, EventController.HandChangeEvent handEffect) : base(id, MMEvent.Type.HandChange, behav, title) {
        _handEffect = handEffect;    
    }

    public IEnumerator OnHandChange(HandChangeEventArgs args) {
        if (_handEffect != null) {
            yield return _handEffect(args);
            DecCountLeft();
        }
    }
}



public class DropEffect : EventEffect {

    private EventController.DropEvent _dropEffect;

    public DropEffect(int id, string title, Behav behav, EventController.DropEvent dropEffect) 
        : base(id, MMEvent.Type.Drop, behav, title) {
        this._dropEffect = dropEffect;
    }

    public IEnumerator OnDrop(DropEventArgs args) {
        if (_dropEffect != null) { // why would it be?
            MMLog.Log("DROPEFFECT", "black", "About to trigger dropEffect!");
            yield return _dropEffect(args);
            DecCountLeft();
        }
        yield return null;
    }
}



public class SwapEffect : EventEffect {

    private EventController.SwapEvent _swapEffect;

    public SwapEffect(int id, string title, Behav behav, EventController.SwapEvent swapEffect) 
        : base(id, MMEvent.Type.Swap, behav, title) {
        this._swapEffect = swapEffect;
    }

    public IEnumerator OnSwap(SwapEventArgs args) {
        if (_swapEffect != null) {
            MMLog.Log("SWAPEFFECT", "black", "About to trigger swapEffect!");
            yield return _swapEffect(args);
            DecCountLeft();
        }
        yield return null;
    }
}
