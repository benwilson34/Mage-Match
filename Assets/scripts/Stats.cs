using UnityEngine;
using System.Collections;

public static class Stats {

    struct PlayerStat {
        public string name;
        public string character;
        public int drops, swaps, matches, spellsCast;
    }

    private static PlayerStat ps1, ps2;

    public static void Init(Player p1, Player p2) {
        ps1 = new PlayerStat() { name = p1.name };
        ps2 = new PlayerStat() { name = p2.name };
    }

    public static void IncMatch(int id, int count) {
        if (id == 1)
            ps1.matches += count;
        else
            ps2.matches += count;
    }

    public static void IncDrops(int id) {
        if (id == 1)
            ps1.drops++;
        else
            ps1.drops++;
    }

    // TODO etc...
}
