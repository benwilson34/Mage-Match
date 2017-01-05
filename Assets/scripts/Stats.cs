using UnityEngine;
using System.Collections;

public class Stats {

    struct PlayerStat {
        public string name;
        public string character;
        public int drops, swaps, matches, spellsCast;
    }

    private PlayerStat ps1, ps2;

    public Stats(Player p1, Player p2) {
        ps1 = new PlayerStat() { name = p1.name };
        ps2 = new PlayerStat() { name = p2.name };
    }

    public void IncMatch(int id, int count) {
        if (id == 1)
            ps1.matches += count;
        else
            ps2.matches += count;
    }

    public void IncDrops(int id) {
        if (id == 1)
            ps1.drops++;
        else
            ps1.drops++;
    }

    // TODO etc...
}
