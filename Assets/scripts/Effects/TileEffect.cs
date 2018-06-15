using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// needed so that if a tile is removed, its effects can also be removed
public class TileEffect : LastingEffect {

    public List<EventEffect> effects; // make into a list if there is more than one needed
    public TileBehav enchantee;

    public TileEffect(int id, TileBehav enchantee) : this(id, enchantee, enchantee.Title) { }

    public TileEffect(int id, TileBehav enchantee, string title) : base(title) {
        playerId = id;
        this.enchantee = enchantee;
        effects = new List<EventEffect>();
    }

    public void AddEffect(EventEffect e) {
        effects.Add(e);
        EffectManager.AddEventEffect(e);
    }

    public void ClearEffects() {
        foreach (var effect in effects)
            EffectManager.RemoveEventEffect(effect.tag);
        effects.Clear();
    }

    //public override IEnumerator OnEndEffect() {
    //    // TODO destroy tile?
    //    yield return HexManager._RemoveTile(enchantee, false);
    //    yield return base.OnEndEffect(); // needed?
    //}

}
