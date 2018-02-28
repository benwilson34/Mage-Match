using UnityEngine;
using MMDebug;

using Midi;
using System.Collections;

public class AudioController {

    private MageMatch _mm;
    private bool _isMidiMode = false;
    private OutputDevice _outputDevice;

    private AudioSource _source;
    private AudioClip _gameStart, _gameEnd, _hexDraw, _hexDiscard, _hexPickup;
    private AudioClip[] _tileGravityClick, _tileSwap, _tileDestroy, _tileInvoke;
    private AudioClip _enchantZombie, _enchantBurning, _apGain, _lowHealthWarning;
    private AudioClip _fullMeter, _turnTimerWarning, _turnTimeout, _uiButton;
    private AudioClip _chooseTarget, _deselectTarget;

    public AudioController(MageMatch mm) {
        _source = GameObject.Find("board").GetComponent<AudioSource>();
        this._mm = mm;
        if (mm.IsDebugMode() && mm.debugSettings.midiMode)
            InitMIDI();

        AudioListener.volume = .6f;

        _gameStart = (AudioClip)Resources.Load("sounds/GameStart/Game Start 5");
        _gameEnd = (AudioClip)Resources.Load("sounds/GameEnd/Game WIn");

        _hexDraw = (AudioClip)Resources.Load("sounds/HexDraw/Draw High-6");
        _hexDiscard = (AudioClip)Resources.Load("sounds/HexDiscard/Discard High-5");
        _hexPickup = (AudioClip)Resources.Load("sounds/HexPickup/BoardClickEdit2");

        _tileGravityClick = new AudioClip[3];
        _tileGravityClick[0] = (AudioClip)Resources.Load("sounds/TileGravityClick/BoardClickEdit1");
        _tileGravityClick[1] = (AudioClip)Resources.Load("sounds/TileGravityClick/BoardClickEdit2");
        _tileGravityClick[2] = (AudioClip)Resources.Load("sounds/TileGravityClick/BoardClickEdit3");

        _tileSwap = new AudioClip[2];
        _tileSwap[0] = (AudioClip)Resources.Load("sounds/TileSwap/SwapWhoosh1");
        _tileSwap[1] = (AudioClip)Resources.Load("sounds/TileSwap/SwapWhoosh2");

        _tileDestroy = new AudioClip[2];
        _tileDestroy[0] = (AudioClip)Resources.Load("sounds/TileDestroy/TileDestroy1");
        _tileDestroy[1] = (AudioClip)Resources.Load("sounds/TileDestroy/TileDestroy2");

        _tileInvoke = new AudioClip[3];
        _tileInvoke[0] = (AudioClip)Resources.Load("sounds/TileInvoke/match_02");
        _tileInvoke[1] = (AudioClip)Resources.Load("sounds/TileInvoke/match_03");
        _tileInvoke[2] = (AudioClip)Resources.Load("sounds/TileInvoke/match_04");

        _enchantZombie = (AudioClip)Resources.Load("sounds/Enchant/EnchantZombie");
        _enchantBurning = (AudioClip)Resources.Load("sounds/Enchant/EnchantBurning");

        _apGain = (AudioClip)Resources.Load("sounds/APGain/AP Gain Sound-4");

        _lowHealthWarning = (AudioClip)Resources.Load("sounds/LowHealthWarning/LowHealthWarning");

        _fullMeter = (AudioClip)Resources.Load("sounds/FullMeter/Sig meter sound");

        _turnTimerWarning = (AudioClip)Resources.Load("sounds/TurnTimer/TurnTimerWarning");
        _turnTimeout = (AudioClip)Resources.Load("sounds/TurnTimer/TurnTimeout2");

        _uiButton = (AudioClip)Resources.Load("sounds/UI/General Button sound C 2");
        _chooseTarget = (AudioClip)Resources.Load("sounds/UI/Electronic Open Close 4");
        _deselectTarget = (AudioClip)Resources.Load("sounds/UI/Electronic Open Close 4-1");


        GameStart();
    }

    public void InitEvents(){
        _mm.eventCont.grabTile += HexPickup;
        _mm.eventCont.playerHealthChange += LowHealthWarning;
        _mm.eventCont.timeout += TurnTimeout;
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
                _outputDevice = device;
                return;
            }
        }
        MMLog.LogError("AUDIOCONT: Couldn't find the LoopBe audio device!");
    }

    void SendMIDI(Pitch pitch) {
        _mm.StartCoroutine(_SendMIDI(pitch));
    }

    IEnumerator _SendMIDI(Pitch pitch) {
        _outputDevice.Open();
        _outputDevice.SendNoteOn(Channel.Channel1, pitch, 80);
        yield return new WaitForSeconds(.01f);
        _outputDevice.SendNoteOff(Channel.Channel1, pitch, 80);
        _outputDevice.Close();
    }

    public void GameStart() {
        if (_isMidiMode) {
            SendMIDI(Pitch.C1);
            return;
        }
        _source.clip = _gameStart;
        _source.Play();
    }

    public void GameEnd() {
        if (_isMidiMode) {
            SendMIDI(Pitch.CSharp1);
            return;
        }
        _source.clip = _gameEnd;
        _source.Play();
    }

    public void HexDraw(AudioSource source) {
        if (_isMidiMode) {
            SendMIDI(Pitch.D1);
            return;
        }
        source.clip = _hexDraw;
        source.Play();
    }

    public void HexDiscard() {
        if (_isMidiMode) {
            SendMIDI(Pitch.DSharp1);
            return;
        }
        _source.clip = _hexDiscard;
        _source.Play();
    }

    // doesn't need to be a callback...
    public void HexPickup(int id, string tag) {
        if (_isMidiMode) {
            SendMIDI(Pitch.E1);
            return;
        }
        Hex hex = _mm.GetPlayer(id).hand.GetHex(tag);
        AudioSource source = hex.GetComponent<AudioSource>();
        source.clip = _hexPickup;
        source.Play();
    }

    public void TileGravityClick(AudioSource source) {
        if (_isMidiMode) {
            SendMIDI(Pitch.F1);
            return;
        }
        source.clip = _tileGravityClick[Random.Range(0, _tileGravityClick.Length)];
        source.Play();
    }

    public void TileSwap(AudioSource source) {
        if (_isMidiMode) {
            SendMIDI(Pitch.FSharp1);
            return;
        }
        source.clip = _tileSwap[Random.Range(0, _tileSwap.Length)]; ;
        source.Play();
    }

    public void TileDestroy() {
        if (_isMidiMode) {
            SendMIDI(Pitch.G1);
            return;
        }
        _source.clip = _tileDestroy[Random.Range(0, _tileDestroy.Length)];
        _source.Play();
    }

    public void TileInvoke() {
        if (_isMidiMode) {
            SendMIDI(Pitch.GSharp1);
            return;
        }
        _source.clip = _tileInvoke[Random.Range(0, _tileInvoke.Length)];
        _source.Play();
    }

    public void EnchantZombie(AudioSource source) {
        if (_isMidiMode) {
            SendMIDI(Pitch.A1);
            return;
        }
        source.clip = _enchantZombie;
        source.Play();
    }

    public void EnchantBurning(AudioSource source) {
        if (_isMidiMode) {
            SendMIDI(Pitch.ASharp1);
            return;
        }
        source.clip = _enchantBurning;
        source.Play();
    }

    // not needed currently
    public void APGain() {
        if (_isMidiMode) {
            SendMIDI(Pitch.B1);
            return;
        }
        _source.clip = _apGain;
        _source.Play();
    }

    public void LowHealthWarning(int id, int amount, int newHealth, bool dealt) {
        if (_isMidiMode) {
            SendMIDI(Pitch.C2);
            return;
        }
        if (amount < 0 &&
                newHealth + (-amount) >= Character.HEALTH_WARNING_AMT &&
                newHealth < Character.HEALTH_WARNING_AMT) {
            _source.clip = _lowHealthWarning;
            _source.Play();
        }
    }

    public void FullMeter() {
        if (_isMidiMode) {
            SendMIDI(Pitch.CSharp2);
            return;
        }
        _source.clip = _fullMeter;
        _source.Play();
    }

    public void TurnTimerWarning() {
        if (_isMidiMode) {
            SendMIDI(Pitch.D2);
            return;
        }
        _source.clip = _turnTimerWarning;
        _source.Play();
    }

    public void TurnTimeout(int id) {
        if (_isMidiMode) {
            SendMIDI(Pitch.DSharp2);
            return;
        }
        _source.clip = _turnTimeout;
        _source.Play();
    }

    public void UIClick() {
        if (_isMidiMode) {
            SendMIDI(Pitch.E2);
            return;
        }
        _source.clip = _uiButton;
        _source.Play();
    }

    public void ChooseTargets() {
        if (_isMidiMode) {
            SendMIDI(Pitch.F2);
            return;
        }
        _source.clip = _chooseTarget;
        _source.Play();
    }

    public void TargetDeselectedSound() {
        _source.clip = _deselectTarget;
        _source.Play();
    }
}
