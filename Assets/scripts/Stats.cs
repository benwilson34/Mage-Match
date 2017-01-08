using UnityEngine;
using System.Collections;

public class Stats {

    public int turns = 0;

    private int commishMatches;

    private MageMatch mm;

    struct PlayerStat {
        public string name;
        public string character;
        public int drops, swaps, matches, spellsCast;
    }

    private PlayerStat ps1, ps2;

    public Stats(Player p1, Player p2) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        ps1 = new PlayerStat() { name = p1.name, character = p1.character.characterName };
        ps2 = new PlayerStat() { name = p2.name, character = p2.character.characterName };
        // TODO init with mm.eventCont
        mm.eventCont.match += OnMatch;
        mm.eventCont.turnChange += OnTurnChange;
        mm.eventCont.commishMatch += OnCommishMatch;
    }

    public void OnMatch(int id, int count) {
        if (id == 1)
            ps1.matches += count;
        else
            ps2.matches += count;
    }

    //public void OnDrop(int id) {
    //    if (id == 1)
    //        ps1.drops++;
    //    else
    //        ps2.drops++;
    //}

    public void OnTurnChange(int id) {
        turns++;
        //Debug.Log("STATS: Turns incremented to " + turns);
    }

    public void OnCommishMatch(int count) {
        commishMatches += count;
    }

}
