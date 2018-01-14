using UnityEngine;
using MMDebug;

using Midi;
using System.Collections;

public class AudioController {

    private MageMatch mm;
    private bool isMidiMode = false;
    private OutputDevice outputDevice;

    private AudioSource source;
    private AudioClip gameStart, gameEnd, hexDraw, hexDiscard, hexPickup;
    private AudioClip[] tileGravityClick, tileSwap, tileDestroy, tileInvoke;
    private AudioClip enchantZombie, enchantBurning, apGain, lowHealthWarning;
    private AudioClip fullMeter, turnTimerWarning, turnTimeout, uiButton;
    private AudioClip chooseTarget, deselectTarget;

    public AudioController(MageMatch mm) {
        source = GameObject.Find("board").GetComponent<AudioSource>();
        this.mm = mm;
        isMidiMode = mm.debugSettings.midiMode;
        if (isMidiMode)
            InitMIDI();

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

    void InitMIDI() {
        if (OutputDevice.InstalledDevices.Count == 0) {
            MMLog.LogError("AUDIOCONT: No MIDI output devices!");
            return;
        }

        for (int i = 0; i < OutputDevice.InstalledDevices.Count; ++i) {
            OutputDevice device = OutputDevice.InstalledDevices[i];
            if (device.Name == "LoopBe Internal MIDI") {
                if(device.IsOpen)
                    MMLog.LogError("AUDIOCONT: Device is open?");
                outputDevice = device;
                return;
            }
        }
        MMLog.LogError("AUDIOCONT: Couldn't find the LoopBe audio device!");
    }

    void SendMIDI(Pitch pitch) {
        mm.StartCoroutine(_SendMIDI(pitch));
    }

    IEnumerator _SendMIDI(Pitch pitch) {
        outputDevice.Open();
        outputDevice.SendNoteOn(Channel.Channel1, pitch, 80);
        yield return new WaitForSeconds(.01f);
        outputDevice.SendNoteOff(Channel.Channel1, pitch, 80);
        outputDevice.Close();
    }

    public void GameStart() {
        if (isMidiMode) {
            SendMIDI(Pitch.C1);
            return;
        }
        source.clip = gameStart;
        source.Play();
    }

    public void GameEnd() {
        if (isMidiMode) {
            SendMIDI(Pitch.CSharp1);
            return;
        }
        source.clip = gameEnd;
        source.Play();
    }

    public void HexDraw(AudioSource source) {
        if (isMidiMode) {
            SendMIDI(Pitch.D1);
            return;
        }
        source.clip = hexDraw;
        source.Play();
    }

    public void HexDiscard() {
        if (isMidiMode) {
            SendMIDI(Pitch.DSharp1);
            return;
        }
        source.clip = hexDiscard;
        source.Play();
    }

    // doesn't need to be a callback...
    public void HexPickup(int id, string tag) {
        if (isMidiMode) {
            SendMIDI(Pitch.E1);
            return;
        }
        Hex hex = mm.GetPlayer(id).hand.GetHex(tag);
        AudioSource source = hex.GetComponent<AudioSource>();
        source.clip = hexPickup;
        source.Play();
    }

    public void TileGravityClick(AudioSource source) {
        if (isMidiMode) {
            SendMIDI(Pitch.F1);
            return;
        }
        source.clip = tileGravityClick[Random.Range(0, tileGravityClick.Length)];
        source.Play();
    }

    public void TileSwap(AudioSource source) {
        if (isMidiMode) {
            SendMIDI(Pitch.FSharp1);
            return;
        }
        source.clip = tileSwap[Random.Range(0, tileSwap.Length)]; ;
        source.Play();
    }

    public void TileDestroy() {
        if (isMidiMode) {
            SendMIDI(Pitch.G1);
            return;
        }
        source.clip = tileDestroy[Random.Range(0, tileDestroy.Length)];
        source.Play();
    }

    public void TileInvoke() {
        if (isMidiMode) {
            SendMIDI(Pitch.GSharp1);
            return;
        }
        source.clip = tileInvoke[Random.Range(0, tileInvoke.Length)];
        source.Play();
    }

    public void EnchantZombie(AudioSource source) {
        if (isMidiMode) {
            SendMIDI(Pitch.A1);
            return;
        }
        source.clip = enchantZombie;
        source.Play();
    }

    public void EnchantBurning(AudioSource source) {
        if (isMidiMode) {
            SendMIDI(Pitch.ASharp1);
            return;
        }
        source.clip = enchantBurning;
        source.Play();
    }

    // not needed currently
    public void APGain() {
        if (isMidiMode) {
            SendMIDI(Pitch.B1);
            return;
        }
        source.clip = apGain;
        source.Play();
    }

    public void LowHealthWarning(int id, int amount, int newHealth, bool dealt) {
        if (isMidiMode) {
            SendMIDI(Pitch.C2);
            return;
        }
        if (amount < 0 &&
                newHealth + (-amount) >= Character.HEALTH_WARNING_AMT &&
                newHealth < Character.HEALTH_WARNING_AMT) {
            source.clip = lowHealthWarning;
            source.Play();
        }
    }

    public void FullMeter() {
        if (isMidiMode) {
            SendMIDI(Pitch.CSharp2);
            return;
        }
        source.clip = fullMeter;
        source.Play();
    }

    public void TurnTimerWarning() {
        if (isMidiMode) {
            SendMIDI(Pitch.D2);
            return;
        }
        source.clip = turnTimerWarning;
        source.Play();
    }

    public void TurnTimeout(int id) {
        if (isMidiMode) {
            SendMIDI(Pitch.DSharp2);
            return;
        }
        source.clip = turnTimeout;
        source.Play();
    }

    public void UIClick() {
        if (isMidiMode) {
            SendMIDI(Pitch.E2);
            return;
        }
        source.clip = uiButton;
        source.Play();
    }

    public void ChooseTargets() {
        if (isMidiMode) {
            SendMIDI(Pitch.F2);
            return;
        }
        source.clip = chooseTarget;
        source.Play();
    }

    public void TargetDeselectedSound() {
        source.clip = deselectTarget;
        source.Play();
    }
}
