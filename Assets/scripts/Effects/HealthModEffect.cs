using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class HealthModEffect : LastingEffect {

    public enum Type { DealingPercent, DealingBonus, ReceivingPercent, ReceivingBonus };
    public Type type;
    public bool IsDealing { get { return type == Type.DealingPercent || type == Type.DealingBonus; } }
    public bool IsAdditive { get { return type == Type.DealingBonus || type == Type.ReceivingBonus; } }
    public int affectingPlayer;

    public delegate float MyHealthEffect(Player p, int dmg);

    private MyHealthEffect _healthEffect;

    public HealthModEffect(int id, string title, MyHealthEffect healthEffect, Type type, bool affectsOpponent = false) : base(title) {
        this.type = type;
        this.playerId = id;
        this.affectingPlayer = affectsOpponent ? _mm.OpponentId(id) : id;
        this._healthEffect = healthEffect;
    }

    public float GetResult(Player p, int dmg) {
        DecCountLeft();
        return _healthEffect(p, dmg);
    }

}
