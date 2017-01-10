using UnityEngine;
using System.Collections;

// eventually, this will be where the output file stuff happens?
public class Stats {

    public int turns = 0;

    private int commishMatches;

    private MageMatch mm;

    struct PlayerStat {
        public string name;
        public string character;
        public int draws, drops, swaps, matches, spellsCast;
    }

    private PlayerStat ps1, ps2;

    public Stats(Player p1, Player p2) {
        mm = GameObject.Find("board").GetComponent<MageMatch>();
        ps1 = new PlayerStat() { name = p1.name, character = p1.character.characterName };
        ps2 = new PlayerStat() { name = p2.name, character = p2.character.characterName };

        // TODO reorder methods and subscriptions to make more sense (visually)
        mm.eventCont.match += OnMatch;
        mm.eventCont.turnChange += OnTurnChange;
        mm.eventCont.commishMatch += OnCommishMatch;
        mm.eventCont.spellCast += OnSpellCast;
        mm.eventCont.draw += OnDraw;
    }

    public void OnMatch(int id, int count) {
        if (id == 1)
            ps1.matches += count;
        else
            ps2.matches += count;
    }

    public void OnDraw(int id) {
        if (id == 1)
            ps1.draws++;
        else
            ps2.draws++;
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

    public void OnSpellCast(int id, Spell spell) {
        if (id == 1)
            ps1.spellsCast++;
        else
            ps2.spellsCast++;
    }

}
