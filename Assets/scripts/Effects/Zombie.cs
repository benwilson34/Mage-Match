using MMDebug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Zombie : Enchantment {

    private const string ZOMBIE_COLOR = "#50C75AFF";
    private static Color _zombieColor;

    public Zombie(int id, TileBehav tb) : base(id, tb, Type.Zombie) {
        _zombieColor = new Color();
        ColorUtility.TryParseHtmlString(ZOMBIE_COLOR, out _zombieColor);
    }

    public static IEnumerator Set(int id, TileBehav tb) {
        yield return Set(id, tb, true);
    }
    static IEnumerator Set(int id, TileBehav tb, bool initAnim) {
        if (initAnim) {
            yield return AnimationController._Zombify(tb);
            AudioController.Trigger(SFX.Gravekeeper.Zombie_Enchant);
        }

        new Zombie(id, tb); // looks weird but this is how it is right now

        //MMLog.Log("ObjEffects", "orange", "Zombify's enchtype is "+ench.enchType);
        yield return tb.GetComponent<TileGFX>()._AnimateTint(_zombieColor);
    }

    public IEnumerator Attack() {
        TileBehav tb = enchantee; // refactor?
        if (tb == null)
            MMLog.LogError("SPELLFX: >>>>>Zombify called with a null tile!! Maybe it was removed?");

        List<TileBehav> tbs = HexGrid.GetSmallAreaTiles(tb.tile.col, tb.tile.row);
        tbs = TileFilter.FilterByAbleEnch(tbs, Type.Zombie);

        if (tbs.Count == 0) { // no targets
            MMLog.Log_EnchantFx("Zombify at " + tb.PrintCoord() + " has no targets!");
            yield break;
        }

        int rand = Random.Range(0, tbs.Count);
        yield return _mm.syncManager.SyncRand(playerId, rand);
        TileBehav selectTB = tbs[_mm.syncManager.GetRand()];
        MMLog.Log_EnchantFx("Zombify attacking TB at " + selectTB.PrintCoord());

        yield return AnimationController._Zombify_Attack(tb.transform, selectTB.transform); // anim 1

        if (selectTB.tile.IsElement(Tile.Element.Muscle)) {
            HexManager.RemoveTile(selectTB.tile, false); // maybe?
            AudioController.Trigger(SFX.Gravekeeper.Zombie_Gulp);

            _mm.GetPC(playerId).DealDamage(10);
            _mm.GetPC(playerId).Heal(10);
        } else {
            _mm.StartCoroutine(Set(playerId, selectTB));
            AudioController.Trigger(SFX.Gravekeeper.Zombie_Attack);
        }

        yield return AnimationController._Zombify_Back(tb.transform); // anim 2

        //MMLog.Log_EnchantFx("----- Zombify at " + tb.PrintCoord() + " done -----");
        yield return null; // needed?
    }

    public override IEnumerator OnEndEffect() {
        // TODO SFX
        yield return null;
    }
}
