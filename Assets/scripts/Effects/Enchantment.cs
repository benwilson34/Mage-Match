using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Enchantment : TileEffect {

    public enum Type { None = 0, Burning, Zombie }
    public Type enchType; // private?

    public Enchantment(int id, TileBehav enchantee, Type enchType) 
        : base(id, enchantee, enchType.ToString()) {
        this.enchType = enchType;
        enchantee.SetEnchantment(this);
        EffectManager.AddTileEffect(this);
    }

    public override IEnumerator OnEndEffect() {
        enchantee.ClearEnchantment();
        yield return base.OnEndEffect(); // needed?
    }

}
