using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocky : Character {

    // TODO passive
    private HexGrid hexGrid;
    private Targeting targeting;

    public Rocky(MageMatch mm, int id, int loadout) {
        playerID = id;
        this.mm = mm;
        hexGrid = mm.hexGrid;
        targeting = mm.targeting;
        spellfx = mm.spellfx;
        characterName = "Rocky";
        spells = new Spell[4];

        if (loadout == 1)
            RockyA();
        else
            RockyB();
    }

    // TODO
    void RockyA() { // Rocky A - Tectonic Titan 
        loadoutName = "Tectonic Titan";
        maxHealth = 1100;

        SetDeckElements(5, 0, 45, 30, 20);

        spells[0] = new Spell(0, "Magnitude 10", "EEMEE", 1, Magnitude10);
        spells[1] = new Spell(1, "Sinkhole", "EAAE", 1, spellfx.Deal496Dmg); //
        spells[2] = new Spell(2, "Boulder Barrage", "MMEE", 1, spellfx.Deal496Dmg); //
        spells[3] = new Spell(3, "Stalagmite", "AEE", 1, Stalagmite);
    }

    // TODO
    void RockyB() { // Rocky B - Continental Champion
        loadoutName = "Continental Champion";
        maxHealth = 1300;

        SetDeckElements(0, 25, 40, 10, 25);

        spells[0] = new Spell(0, "Magnitude 10", "EEMEE", 1, Magnitude10);
        spells[1] = new Spell(1, "Living Flesh Armor", "EWWE", 1, spellfx.Deal496Dmg); //
        spells[2] = new Spell(2, "Figure-Four Leglock", "MEEM", 1, spellfx.Deal496Dmg); //
        spells[3] = new Spell(3, "Stalagmite", "AEE", 1, Stalagmite);
    }


    public IEnumerator Magnitude10() {
        TurnEffect t = new TurnEffect(3, Magnitude10_Turn, Magnitude10_End, null);
        t.priority = 4;
        mm.effectCont.AddEndTurnEffect(t, "mag");
        yield return null;
    }
    IEnumerator Magnitude10_Turn(int id) {
        int dmg = 0;
        for (int col = 0; col < 7; col++) {
            int row = hexGrid.BottomOfColumn(col);
            if (hexGrid.IsSlotFilled(col, row)) {
                Tile t = hexGrid.GetTileAt(col, row);
                if (!t.element.Equals(Tile.Element.Earth)) {
                    mm.RemoveTile(t, true);
                    dmg += 15;
                }
            }
        }
        mm.GetPlayer(id).DealDamage(dmg); 

        yield return null; // for now
    }
    IEnumerator Magnitude10_End(int id) {
        Magnitude10_Turn(id);
        yield return null; // for now
    }

    public void Sinkhole() {

    }

    public void BoulderBarrage() {

    }

    public IEnumerator Stalagmite() {
        yield return targeting.WaitForCellTarget(1);
        if (targeting.WasCanceled())
            yield break;

        CellBehav cb = targeting.GetTargetCBs()[0];
        int col = cb.col;
        int bottomr = hexGrid.BottomOfColumn(col);
        // hardset bottom three cells of column
        GameObject stone;
        for (int i = 0; i < 3; i++) {
            stone = mm.GenerateToken("stone");
            stone.transform.SetParent(GameObject.Find("tilesOnBoard").transform);
            mm.PutTile(stone, col, bottomr + i);
        }
    }

    public void LivingFleshArmor() {

    }

    public void FigureFourLeglock() {

    }
}
