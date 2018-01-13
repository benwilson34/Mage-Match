using UnityEngine;
using System.Collections;

public class AudioController {

    private MageMatch mm;

    private AudioSource source;
    private AudioClip gameStart, gameEnd, hexDraw, hexDiscard, hexPickup;
    private AudioClip[] tileGravityClick, tileSwap, tileDestroy, tileInvoke;
    private AudioClip enchantZombie, enchantBurning, apGain, lowHealthWarning;
    private AudioClip fullMeter, turnTimerWarning, turnTimeout, uiButton;
    private AudioClip chooseTarget, deselectTarget;

    public AudioController(MageMatch mm) {
        source = GameObject.Find("board").GetComponent<AudioSource>();
        this.mm = mm;

        AudioListener.volume = .6f;

        gameStart = (AudioClip)Resources.Load("sounds/GameStart/Game Start 5");
        gameEnd = (AudioClip)Resources.Load("sounds/GameEnd/Game WIn");

        hexDraw = (AudioClip)Resources.Load("sounds/HexDraw/Draw High-6");
        hexDiscard = (AudioClip)Resources.Load("sounds/HexDiscard/Discard High-5");
        hexPickup = (AudioClip)Resources.Load("sounds/HexPickup/BoardClickEdit2");

        tileGravityClick = new AudioClip[3];
        tileGravityClick[0] = (AudioClip)Resources.Load("sounds/TileGravityClick/BoardClickEdit1");
        tileGravityClick[1] = (AudioClip)Resources.Load("sounds/TileGravityClick/BoardClickEdit2");
        tileGravityClick[2] = (AudioClip)Resources.Load("sounds/TileGravityClick/BoardClickEdit3");

        tileSwap = new AudioClip[2];
        tileSwap[0] = (AudioClip)Resources.Load("sounds/TileSwap/SwapWhoosh1");
        tileSwap[1] = (AudioClip)Resources.Load("sounds/TileSwap/SwapWhoosh2");

        tileDestroy = new AudioClip[2];
        tileDestroy[0] = (AudioClip)Resources.Load("sounds/TileDestroy/TileDestroy1");
        tileDestroy[1] = (AudioClip)Resources.Load("sounds/TileDestroy/TileDestroy2");

        tileInvoke = new AudioClip[3];
        tileInvoke[0] = (AudioClip)Resources.Load("sounds/TileInvoke/match_02");
        tileInvoke[1] = (AudioClip)Resources.Load("sounds/TileInvoke/match_03");
        tileInvoke[2] = (AudioClip)Resources.Load("sounds/TileInvoke/match_04");

        enchantZombie = (AudioClip)Resources.Load("sounds/Enchant/EnchantZombie");
        enchantBurning = (AudioClip)Resources.Load("sounds/Enchant/EnchantBurning");

        apGain = (AudioClip)Resources.Load("sounds/APGain/AP Gain Sound-4");

        lowHealthWarning = (AudioClip)Resources.Load("sounds/LowHealthWarning/LowHealthWarning");

        fullMeter = (AudioClip)Resources.Load("sounds/FullMeter/Sig meter sound");

        turnTimerWarning = (AudioClip)Resources.Load("sounds/TurnTimer/TurnTimerWarning");
        turnTimeout = (AudioClip)Resources.Load("sounds/TurnTimer/TurnTimeout2");

        uiButton = (AudioClip)Resources.Load("sounds/UI/General Button sound C 2");
        chooseTarget = (AudioClip)Resources.Load("sounds/UI/Electronic Open Close 4");
        deselectTarget = (AudioClip)Resources.Load("sounds/UI/Electronic Open Close 4-1");


        GameStart();
    }

    public void InitEvents(){
        mm.eventCont.grabTile += HexPickup;
        mm.eventCont.playerHealthChange += LowHealthWarning;
        mm.eventCont.timeout += TurnTimeout;
    }


    public void GameStart() {
        source.clip = gameStart;
        source.Play();
    }

    public void GameEnd() {
        source.clip = gameEnd;
        source.Play();
    }

    public void HexDraw(AudioSource source) {
        source.clip = hexDraw;
        source.Play();
    }

    public void HexDiscard() {
        source.clip = hexDiscard;
        source.Play();
    }

    public void HexPickup(int id, string tag) {
        Hex hex = mm.GetPlayer(id).hand.GetHex(tag);
        AudioSource source = hex.GetComponent<AudioSource>();
        source.clip = hexPickup;
        source.Play();
    }

    public void TileGravityClick(AudioSource source) {
        source.clip = tileGravityClick[Random.Range(0, tileGravityClick.Length)];
        source.Play();
    }

    public void TileSwap(AudioSource source) {
        source.clip = tileSwap[Random.Range(0, tileSwap.Length)]; ;
        source.Play();
    }

    public void TileDestroy() {
        source.clip = tileDestroy[Random.Range(0, tileDestroy.Length)];
        source.Play();
    }

    public void TileInvoke() {
        source.clip = tileInvoke[Random.Range(0, tileInvoke.Length)];
        source.Play();
    }

    public void EnchantZombie(AudioSource source) {
        source.clip = enchantZombie;
        source.Play();
    }

    public void EnchantBurning(AudioSource source) {
        source.clip = enchantBurning;
        source.Play();
    }

    // not needed currently
    public void APGain() {
        source.clip = apGain;
        source.Play();
    }

    public void LowHealthWarning(int id, int amount, int newHealth, bool dealt) {
        if (amount < 0 &&
                newHealth + (-amount) >= Character.HEALTH_WARNING_AMT &&
                newHealth < Character.HEALTH_WARNING_AMT) {
            source.clip = lowHealthWarning;
            source.Play();
        }
    }

    public void FullMeter() {
        source.clip = fullMeter;
        source.Play();
    }

    public void TurnTimerWarning() {
        source.clip = turnTimerWarning;
        source.Play();
    }

    public void TurnTimeout(int id) {
        source.clip = turnTimeout;
        source.Play();
    }

    public void UIClick() {
        source.clip = uiButton;
        source.Play();
    }

    public void ChooseTargets() {
        source.clip = chooseTarget;
        source.Play();
    }

    public void TargetDeselectedSound() {
        source.clip = deselectTarget;
        source.Play();
    }
}
