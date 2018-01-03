using UnityEngine;
using System.Collections;

public class AudioController {

    private MageMatch mm;

    private AudioSource source;
    private AudioClip[] grab, swap, drop, match;
    private AudioClip sigMeterFull, targetSelected, targetDeselected;
    private AudioClip uiButton;

    public AudioController(MageMatch mm) {
        source = GameObject.Find("board").GetComponent<AudioSource>();
        this.mm = mm;

        AudioListener.volume = .6f;

        grab = new AudioClip[5];
        grab[0] = (AudioClip)Resources.Load("sounds/grab_01");
        grab[1] = (AudioClip)Resources.Load("sounds/grab_02");
        grab[2] = (AudioClip)Resources.Load("sounds/grab_03");
        grab[3] = (AudioClip)Resources.Load("sounds/grab_04");
        grab[4] = (AudioClip)Resources.Load("sounds/grab_05");

        swap = new AudioClip[2];
        swap[0] = (AudioClip)Resources.Load("sounds/new/swap/SwapWhoosh1");
        swap[1] = (AudioClip)Resources.Load("sounds/new/swap/SwapWhoosh2");

        drop = new AudioClip[3];
        drop[0] = (AudioClip)Resources.Load("sounds/new/drop/BoardClickEdit1");
        drop[1] = (AudioClip)Resources.Load("sounds/new/drop/BoardClickEdit2");
        drop[2] = (AudioClip)Resources.Load("sounds/new/drop/BoardClickEdit3");

        sigMeterFull = (AudioClip)Resources.Load("sounds/new/sig_meter/Sig meter sound");

        targetSelected = (AudioClip)Resources.Load("sounds/new/ui/Electronic Open Close 4");
        targetDeselected = (AudioClip)Resources.Load("sounds/new/ui/Electronic Open Close 4-1");

        uiButton = (AudioClip)Resources.Load("sounds/new/ui/General Button sound C 2");

        match = new AudioClip[5];
        match[0] = (AudioClip)Resources.Load("sounds/match_01");
        match[1] = (AudioClip)Resources.Load("sounds/match_02");
        match[2] = (AudioClip)Resources.Load("sounds/match_03");
        match[3] = (AudioClip)Resources.Load("sounds/match_04");
        match[4] = (AudioClip)Resources.Load("sounds/match_05");

        //swap_Fire = new AudioClip[3];
        //swap_Fire[0] = (AudioClip)Resources.Load("sounds/swap_fire_01");
        //swap_Fire[1] = (AudioClip)Resources.Load("sounds/swap_fire_02");
        //swap_Fire[2] = (AudioClip)Resources.Load("sounds/swap_fire_03");

        //swap_Water = new AudioClip[3];
        //swap_Water[0] = (AudioClip)Resources.Load("sounds/swap_water_01");
        //swap_Water[1] = (AudioClip)Resources.Load("sounds/swap_water_02");
        //swap_Water[2] = (AudioClip)Resources.Load("sounds/swap_water_03");

        //swap_Earth = new AudioClip[3];
        //swap_Earth[0] = (AudioClip)Resources.Load("sounds/swap_earth_01");
        //swap_Earth[1] = (AudioClip)Resources.Load("sounds/swap_earth_02");
        //swap_Earth[2] = (AudioClip)Resources.Load("sounds/swap_earth_03");

        //swap_Air = new AudioClip[4];
        //swap_Air[0] = (AudioClip)Resources.Load("sounds/air_01");
        //swap_Air[1] = (AudioClip)Resources.Load("sounds/air_02");
        //swap_Air[2] = (AudioClip)Resources.Load("sounds/air_03");
        //swap_Air[3] = (AudioClip)Resources.Load("sounds/air_04");

        //swap_Muscle = new AudioClip[3];
        //swap_Muscle[0] = (AudioClip)Resources.Load("sounds/swap_muscle_01");
        //swap_Muscle[1] = (AudioClip)Resources.Load("sounds/swap_muscle_02");
        //swap_Muscle[2] = (AudioClip)Resources.Load("sounds/swap_muscle_03");

        //drop_Fire = new AudioClip[3];
        //drop_Fire[0] = (AudioClip)Resources.Load("sounds/drop_fire_01");
        //drop_Fire[1] = (AudioClip)Resources.Load("sounds/drop_fire_02");
        //drop_Fire[2] = (AudioClip)Resources.Load("sounds/drop_fire_03");

        //drop_Water = new AudioClip[3];
        //drop_Water[0] = (AudioClip)Resources.Load("sounds/drop_water_01");
        //drop_Water[1] = (AudioClip)Resources.Load("sounds/drop_water_02");
        //drop_Water[2] = (AudioClip)Resources.Load("sounds/drop_water_03");

        //drop_Earth = new AudioClip[3];
        //drop_Earth[0] = (AudioClip)Resources.Load("sounds/drop_earth_01");
        //drop_Earth[1] = (AudioClip)Resources.Load("sounds/drop_earth_02");
        //drop_Earth[2] = (AudioClip)Resources.Load("sounds/drop_earth_03");

        //drop_Air = new AudioClip[4];
        //drop_Air[0] = (AudioClip)Resources.Load("sounds/air_01");
        //drop_Air[1] = (AudioClip)Resources.Load("sounds/air_02");
        //drop_Air[2] = (AudioClip)Resources.Load("sounds/air_03");
        //drop_Air[3] = (AudioClip)Resources.Load("sounds/air_04");

        //drop_Muscle = new AudioClip[3];
        //drop_Muscle[0] = (AudioClip)Resources.Load("sounds/drop_muslce_01");
        //drop_Muscle[1] = (AudioClip)Resources.Load("sounds/drop_muscle_02");
        //drop_Muscle[2] = (AudioClip)Resources.Load("sounds/drop_muscle_03");
    }

    public void InitEvents(){
        //mm.eventCont.drop += onDrop;
        mm.eventCont.grabTile += OnGrab;
        mm.eventCont.AddDrawEvent(OnDraw, EventController.Type.Audio);
        mm.eventCont.AddSwapEvent(OnSwap, EventController.Type.Audio);
        mm.eventCont.playerMeterChange += OnPlayerMeterChange;
        //mm.eventCont.match += onMatch;
    }

    public void DropSound(AudioSource source) {
        //source.clip = drop_Earth[Random.Range(0, drop_Earth.Length)]; //This should be its own event?
        source.clip = drop[Random.Range(0, drop.Length)];
        source.Play();
    }

    //public void OnDrop(int id, Tile.Element elem, int col) {
    //    AudioClip clip = null;
    //    switch (elem) {
    //        case Tile.Element.Fire:
    //            clip = drop_Fire[Random.Range(0, drop_Fire.Length)];
    //            break;
    //        case Tile.Element.Water:
    //            clip = drop_Water[Random.Range(0, drop_Water.Length)];
    //            break;
    //        case Tile.Element.Earth:
    //            clip = drop_Earth[Random.Range(0, drop_Earth.Length)];
    //            break;
    //        case Tile.Element.Air:
    //            clip = drop_Air[Random.Range(0, drop_Air.Length)];
    //            break;
    //        case Tile.Element.Muscle:
    //            clip = drop_Muscle[Random.Range(0, drop_Muscle.Length)];
    //            break;
    //    }
    //    GameObject go = mm.ActiveP().GetTileFromHand(elem);
    //    AudioSource source = go.GetComponent<AudioSource>();

    //    source.clip = clip;
    //    source.Play();
    //}

    public void OnGrab(int id, string tag) {
        AudioClip clip = null;
        // TODO get sounds for "colorless" and consumables
        string type = Hex.TagType(tag);
        switch (type) {
            case "F":
                clip = grab[0];
                break;
            case "W":
                clip = grab[1];
                break;
            case "E":
                clip = grab[2];
                break;
            case "A":
                clip = grab[3];
                break;
            case "M":
                clip = grab[4];
                break;
        }
        Hex hex = mm.GetPlayer(id).hand.GetHex(tag);
        AudioSource source = hex.GetComponent<AudioSource>();

        source.clip = clip;
        source.Play();
    }

    public IEnumerator OnDraw(int id, string tag, bool playerAction, bool dealt) {
        OnGrab(id, tag);
        yield return null;
    }

    public IEnumerator OnSwap(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        TileBehav tb = mm.hexGrid.GetTileBehavAt(c1, r1);
        //Tile.Element elem = tb.tile.element;
        //AudioClip clip = null;
        //switch (elem) {
        //    case Tile.Element.Fire:
        //        clip = swap[Random.Range(0, swap.Length)];
        //        break;
        //    case Tile.Element.Water:
        //        clip = swap_Water[Random.Range(0, swap_Water.Length)];
        //        break;
        //    case Tile.Element.Earth:
        //        clip = swap_Earth[Random.Range(0, swap_Earth.Length)];
        //        break;
        //    case Tile.Element.Air:
        //        clip = swap_Air[Random.Range(0, swap_Air.Length)];
        //        break;
        //    case Tile.Element.Muscle:
        //        clip = swap_Muscle[Random.Range(0, swap_Muscle.Length)];
        //        break;
        //}
        AudioSource source = tb.GetComponent<AudioSource>();

        source.clip = swap[Random.Range(0, swap.Length)]; ;
        source.Play();
        yield return null;
    }

	public void BreakSound(){
		source.clip = match [Random.Range (0, 5)];
		source.Play ();
	}

    // still need a way of getting an audiosource...
    //public void OnMatch(int id, string[] seqs) {
    //    AudioClip clip = null;
    //    Tile.Element elem = Tile.CharToElement(seqs[0][0]); // get the first char of the first seq

    //    //switch (elem) {
    //    //    case Tile.Element.Fire:
    //    //        clip = match_Fire [Random.Range (0,match_Fire.Length)]
    //    //        break;
    //    //    case Tile.Element.Water:
    //    //        clip = match_Water[Random.Range(0, match_Water.Length)]
    //    //        break;
    //    //    case Tile.Element.Earth:
    //    //        clip = match_Earth[Random.Range(0, match_Earth.Length)]
    //    //        break;
    //    //    case Tile.Element.Air:
    //    //        clip = match_Air[Random.Range(0, match_Air.Length)]
    //    //        break;
    //    //    case Tile.Element.Muscle:
    //    //        clip = match_Muscle[Random.Range(0, match_Muscle.Length)]
    //    //        break;
    //    //}


    //    // TODO this won't work...how to get the tile? maybe there should just be an audio object with a few AudioSources for everything to use?
    //    GameObject go = mm.ActiveP().hand.GetTile(elem);

    //    AudioSource source = go.GetComponent<AudioSource>();

    //    source.clip = clip;
    //    source.Play();
    //}

    public void OnPlayerMeterChange(int id, int amount) {
        int meter = mm.GetPlayer(id).character.meter;
        if (meter == 100) {
            source.clip = sigMeterFull;
            source.Play();
        }
    }

    public void TargetSound() {
        source.clip = targetSelected;
        source.Play();
    }

    public void TargetDeselectedSound() {
        source.clip = targetDeselected;
        source.Play();
    }

    public void UIsound() {
        source.clip = uiButton;
        source.Play();
    }
}
