//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Rocky : Character {

//    // TODO passive
//    private HexGrid hexGrid;
//    private Targeting targeting;

//    public Rocky(MageMatch mm, int id) : base(mm, Ch.Rocky, id) {
//        hexGrid = mm.hexGrid;
//        targeting = mm.targeting;
//        objFX = mm.hexFX;
//        characterName = "Rocky";
//        spells = new Spell[4];

//        RockyA(); // TODO replace with new spells

//        InitSpells();
//    }

//    // TODO
//    void RockyA() { // Rocky A - Tectonic Titan 
//        maxHealth = 1100;

//        SetDeckElements(5, 0, 45, 30, 20);

//        spells[0] = new Spell(0, "NOTHING", "EEMEE", Sinkhole); //
//        spells[1] = new Spell(1, "Sinkhole", "EAAE", objFX.Deal496Dmg); //
//        spells[2] = new Spell(2, "Boulder Barrage", "MMEE", objFX.Deal496Dmg); //
//        spells[3] = new Spell(3, "Stalagmite", "AEE", Stalagmite);
//    }

//    // TODO
//    void RockyB() { // Rocky B - Continental Champion
//        maxHealth = 1300;

//        SetDeckElements(0, 25, 40, 10, 25);

//        spells[0] = new Spell(0, "NOTHING", "EEMEE", Sinkhole); //
//        spells[1] = new Spell(1, "Living Flesh Armor", "EWWE", objFX.Deal496Dmg); //
//        spells[2] = new Spell(2, "Figure-Four Leglock", "MEEM", objFX.Deal496Dmg); //
//        spells[3] = new Spell(3, "Stalagmite", "AEE", Stalagmite);
//    }


//    //public IEnumerator Magnitude10() {
//    //    TurnEffect t = new TurnEffect(3, Magnitude10_Turn, Magnitude10_End, null);
//    //    t.priority = 4;
//    //    mm.effectCont.AddEndTurnEffect(t, "mag");
//    //    yield return null;
//    //}
//    //IEnumerator Magnitude10_Turn(int id) {
//    //    int dmg = 0;
//    //    for (int col = 0; col < 7; col++) {
//    //        int row = hexGrid.BottomOfColumn(col);
//    //        if (hexGrid.IsCellFilled(col, row)) {
//    //            Tile t = hexGrid.GetTileAt(col, row);
//    //            if (!t.element.Equals(Tile.Element.Earth)) {
//    //                mm.RemoveTile(t, true);
//    //                dmg += 15;
//    //            }
//    //        }
//    //    }
//    //    mm.GetPlayer(id).DealDamage(dmg); 

//    //    yield return null; // for now
//    //}
//    //IEnumerator Magnitude10_End(int id) {
//    //    Magnitude10_Turn(id);
//    //    yield return null; // for now
//    //}

//    public IEnumerator Sinkhole(TileSeq prereq) {
//        yield return null;
//    }

//    public void BoulderBarrage(TileSeq prereq) {

//    }

//    public IEnumerator Stalagmite(TileSeq prereq) {
//        yield return targeting.WaitForCellTarget(1);

//        CellBehav cb = targeting.GetTargetCBs()[0];
//        int col = cb.col;
//        int bottomr = hexGrid.BottomOfColumn(col);
//        // hardset bottom three cells of column
//        TileBehav stone;
//        for (int i = 0; i < 3; i++) {
//            stone = (TileBehav) tileMan.GenerateToken(playerId, "stone");
//            stone.transform.SetParent(GameObject.Find("tilesOnBoard").transform);
//            mm.PutTile(stone, col, bottomr + i);
//        }
//    }

//    public void LivingFleshArmor(TileSeq prereq) {

//    }

//    public void FigureFourLeglock(TileSeq prereq) {

//    }
//}
