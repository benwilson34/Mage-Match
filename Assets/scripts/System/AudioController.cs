using UnityEngine;
using MMDebug;

using Midi;
using System.Collections;

public class AudioController {

    public enum HexSoundEffect { Draw, Pickup, Discard, Grav, Swap, Destroy, Invoke };
    public enum EnfuegoSoundEffect { BurningEnchant, BurningDamage, BurningTimeout, FieryFandango, Baila, Incinerate, WHCK };
    public enum GraveKSoundEffect { ZombieEnchant, ZombieAttack, ZombieGulp, PartyInTheBack, OogieBoogie, PartyCrashers, Motorcycle, SigBell1, SigDrop, SigEffect, SigBell2 };
    public enum ValeriaSoundEffect { SwirlingWater, Healing, Mariposa, RainDance, Bubbles1, Bubbles2, SigCut, SigWaveCrash, ThunderFar, ThunderClose, Rain };
    public enum OtherSoundEffect { GameStart, GameEnd, APGain, LowHealthWarning, FullMeter, TurnTimerWarning, TurnTimeout, UIButton, ChooseTarget, CrowdGasp };

    private MageMatch _mm;
    private bool _isMidiMode = false;
    private OutputDevice _outputDevice;
    private AudioSource _source;

    private AudioClip[] _hexDraw, _hexPickup;
    private AudioClip _hexDiscard;
    private AudioClip[] _tileGrav, _tileSwap, _tileDestroy;
    private AudioClip _tileInvoke;

    private AudioClip[] _burningEnchant, _burningDamage;
    private AudioClip _burningTimeout, _enf_fireyFandango, _enf_baila, _enf_incinerate;
    private AudioClip[] _enf_WHCK;

    private AudioClip[] _zombieEnchant, _zombieAttack;
    private AudioClip _zombieGulp;
    private AudioClip _gra_partyInTheBack, _gra_oogieBoogie, _gra_partyCrashers;
    private AudioClip _gra_motorcycle, _gra_TSbell, _gra_TSdrop, _gra_TSeffect, _gra_churchBell;

    private AudioClip _val_swirlingWater, _val_healing, _val_mariposa, _val_rainDance;
    private AudioClip[] _val_bubbles1, _val_bubbles2, _val_sigCut, _val_sigWaveCrash;
    private AudioClip _val_thunderFar, _val_thunderClose, _val_rain;

    private AudioClip _gameStart, _gameEnd, _apGain, _lowHealthWarning;
    private AudioClip _fullMeter, _turnTimerWarning, _turnTimeout;
    private AudioClip[] _uiButton, _chooseTarget, _deselectTarget;

    private AudioClip _crowdGasp;

    public AudioController(MageMatch mm) {
        _source = GameObject.Find("board").GetComponent<AudioSource>();
        this._mm = mm;
        if (mm.IsDebugMode() && mm.debugSettings.midiMode)
            InitMIDI();

        AudioListener.volume = .6f;


        // ----- hexes -----
        _hexDraw = Resources.LoadAll<AudioClip>("sounds/Alpha/Hexes/Draw");
        _hexPickup = Resources.LoadAll<AudioClip>("sounds/Alpha/Hexes/Pickup");
        _hexDiscard = (AudioClip)Resources.LoadAll("sounds/Alpha/Hexes/Discard")[0];

        _tileGrav = Resources.LoadAll<AudioClip>("sounds/Alpha/Hexes/Grav");
        _tileSwap = Resources.LoadAll<AudioClip>("sounds/Alpha/Hexes/Swap");
        _tileDestroy = Resources.LoadAll<AudioClip>("sounds/Alpha/Hexes/Destroy");
        _tileInvoke = (AudioClip)Resources.LoadAll("sounds/Alpha/Hexes/Invoke")[0];


        // ----- enfuego -----
        _burningEnchant = Resources.LoadAll<AudioClip>("sounds/Alpha/Enfuego/BurningEnchant");
        _burningDamage = Resources.LoadAll<AudioClip>("sounds/Alpha/Enfuego/BurningDamage");
        _burningTimeout = (AudioClip)Resources.LoadAll("sounds/Alpha/Enfuego/BurningTimeout")[0];

        _enf_fireyFandango = (AudioClip)Resources.LoadAll("sounds/Alpha/Enfuego/Flamenco")[0];
        _enf_baila = (AudioClip)Resources.LoadAll("sounds/Alpha/Enfuego/Flamenco")[1];
        _enf_incinerate = (AudioClip)Resources.LoadAll("sounds/Alpha/Enfuego/Incinerate")[0];
        _enf_WHCK = Resources.LoadAll<AudioClip>("sounds/Alpha/Enfuego/WHCK");

        
        // ----- gravekeeper -----
        _zombieEnchant = Resources.LoadAll<AudioClip>("sounds/Alpha/Gravekeeper/ZombieEnchant");
        _zombieAttack = Resources.LoadAll<AudioClip>("sounds/Alpha/Gravekeeper/ZombieAttack");
        _zombieGulp = (AudioClip)Resources.LoadAll("sounds/Alpha/Gravekeeper/ZombieGulp")[0];

        _gra_partyInTheBack = (AudioClip)Resources.LoadAll("sounds/Alpha/Gravekeeper/MouthEngine")[1];
        _gra_oogieBoogie = (AudioClip)Resources.LoadAll("sounds/Alpha/Gravekeeper/OogieBoogie")[0];
        _gra_partyCrashers = (AudioClip)Resources.LoadAll("sounds/Alpha/Gravekeeper/PartyCrashers")[1];

        _gra_motorcycle = (AudioClip)Resources.LoadAll("sounds/Alpha/Gravekeeper/MotorcycleSkid")[0];
        _gra_TSbell = (AudioClip)Resources.LoadAll("sounds/Alpha/Gravekeeper/TombstoneBell")[0];
        _gra_TSdrop = (AudioClip)Resources.LoadAll("sounds/Alpha/Gravekeeper/TombstoneDrop")[0];
        _gra_TSeffect = (AudioClip)Resources.LoadAll("sounds/Alpha/Gravekeeper/TombstoneEffect")[0];
        _gra_churchBell = (AudioClip)Resources.LoadAll("sounds/Alpha/Gravekeeper/ChurchBell")[0];


        // ----- valeria -----
        _val_swirlingWater = (AudioClip)Resources.LoadAll("sounds/Alpha/Valeria/SwirlingWater")[0];
        _val_healing = (AudioClip)Resources.LoadAll("sounds/Alpha/Valeria/Healing")[0];
        _val_mariposa = (AudioClip)Resources.LoadAll("sounds/Alpha/Valeria/Mariposa")[0];
        _val_rainDance = (AudioClip)Resources.LoadAll("sounds/Alpha/Valeria/RainDance")[0];
        _val_bubbles1 = Resources.LoadAll<AudioClip>("sounds/Alpha/Valeria/Bubbles1");
        _val_bubbles2 = Resources.LoadAll<AudioClip>("sounds/Alpha/Valeria/Bubbles2");

        _val_sigCut = Resources.LoadAll<AudioClip>("sounds/Alpha/Valeria/SigCut");
        _val_sigWaveCrash = Resources.LoadAll<AudioClip>("sounds/Alpha/Valeria/SigWaveCrash");

        _val_thunderFar = (AudioClip)Resources.LoadAll("sounds/Alpha/Valeria/SigThunderFar")[0];
        _val_thunderClose = (AudioClip)Resources.LoadAll("sounds/Alpha/Valeria/SigThunderClose")[0];
        _val_rain = (AudioClip)Resources.LoadAll("sounds/Alpha/Valeria/SigRain")[0];


        // ----- other -----
        _gameStart = (AudioClip)Resources.Load("sounds/PreAlpha/GameStart/Game Start 5");
        _gameEnd = (AudioClip)Resources.Load("sounds/PreAlpha/GameEnd/Game WIn");

        _apGain = (AudioClip)Resources.Load("sounds/PreAlpha/APGain/AP Gain Sound-4");
        _lowHealthWarning = (AudioClip)Resources.LoadAll("sounds/Alpha/UI/LowHealthWarning")[0];
        _fullMeter = (AudioClip)Resources.Load("sounds/PreAlpha/FullMeter/Sig meter sound");

        _turnTimerWarning = (AudioClip)Resources.Load("sounds/PreAlpha/TurnTimer/TurnTimerWarning");
        _turnTimeout = (AudioClip)Resources.LoadAll("sounds/Alpha/UI/TurnTimeout")[0];

        _uiButton = Resources.LoadAll<AudioClip>("sounds/Alpha/UI/SpellButtonClick2");
        _chooseTarget = Resources.LoadAll<AudioClip>("sounds/Alpha/UI/SpellButtonClick1");
        //_deselectTarget = (AudioClip)Resources.Load("sounds/PreAlpha/UI/Electronic Open Close 4-1");


        Trigger(OtherSoundEffect.GameStart);
    }

    public void InitEvents() {
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

    AudioClip GetRandom(AudioClip[] clips) {
        return clips[Random.Range(0, clips.Length)];
    }

    // Do I even need to pass in the AudioSource now?
    public void Trigger(HexSoundEffect sound, AudioSource source = null) {
        if (_mm.IsReplayMode() && !_mm.debugSettings.animateReplay) // should be shell method
            return;

        if (source == null)
            source = _source;

        AudioClip clip = null;
        Pitch pitch = Pitch.A0;
        switch (sound) {
            case HexSoundEffect.Draw:
                clip = GetRandom(_hexDraw);
                //pitch = Pitch.D1;
                break;
            case HexSoundEffect.Pickup:
                clip = GetRandom(_hexPickup);
                //pitch = Pitch.DSharp1;
                break;
            case HexSoundEffect.Discard:
                clip = _hexDiscard;
                //pitch = Pitch.E1;
                break;
            case HexSoundEffect.Grav:
                clip = GetRandom(_tileGrav);
                //pitch = Pitch.F1;
                break;
            case HexSoundEffect.Swap:
                clip = GetRandom(_tileSwap);
                //pitch = Pitch.FSharp1;
                break;
            case HexSoundEffect.Destroy:
                clip = GetRandom(_tileDestroy);
                //pitch = Pitch.G1;
                break;
            case HexSoundEffect.Invoke:
                clip = _tileInvoke;
                //pitch = Pitch.GSharp1;
                break;
        }

        if (_isMidiMode) {
            SendMIDI(pitch);
        } else {
            source.PlayOneShot(clip);
        }
    }

    public void Trigger(EnfuegoSoundEffect sound) {
        if (_mm.IsReplayMode() && !_mm.debugSettings.animateReplay) // should be shell method
            return;

        AudioClip clip = null;
        Pitch pitch = Pitch.A0;
        switch (sound) {
            case EnfuegoSoundEffect.BurningEnchant:
                clip = GetRandom(_burningEnchant);
                //pitch = Pitch.A2;
                break;
            case EnfuegoSoundEffect.BurningDamage:
                clip = GetRandom(_burningDamage);
                //pitch = Pitch.ASharp2;
                break;
            case EnfuegoSoundEffect.BurningTimeout:
                clip = _burningTimeout;
                //pitch = Pitch.B2;
                break;
            case EnfuegoSoundEffect.FieryFandango:
                clip = _enf_fireyFandango;
                //pitch = Pitch.C2;
                break;
            case EnfuegoSoundEffect.Baila:
                clip = _enf_baila;
                //pitch = Pitch.CSharp2;
                break;
            case EnfuegoSoundEffect.Incinerate:
                clip = _enf_incinerate;
                //pitch = Pitch.CSharp2;
                break;
            case EnfuegoSoundEffect.WHCK:
                clip = GetRandom(_enf_WHCK);
                //pitch = Pitch.D2;
                break;
        }

        if (_isMidiMode) {
            SendMIDI(pitch);
        } else {
            _source.PlayOneShot(clip);
        }
    }

    public void Trigger(GraveKSoundEffect sound) {
        if (_mm.IsReplayMode() && !_mm.debugSettings.animateReplay) // should be shell method
            return;

        AudioClip clip = null;
        Pitch pitch = Pitch.A0;
        switch (sound) {
            case GraveKSoundEffect.ZombieEnchant:
                clip = GetRandom(_zombieEnchant);
                //pitch = Pitch.D2;
                break;
            case GraveKSoundEffect.ZombieAttack:
                clip = GetRandom(_zombieAttack);
                //pitch = Pitch.D2;
                break;
            case GraveKSoundEffect.ZombieGulp:
                clip = _zombieGulp;
                //pitch = Pitch.D2;
                break;
            case GraveKSoundEffect.PartyInTheBack:
                clip = _gra_partyInTheBack;
                //pitch = Pitch.D2;
                break;
            case GraveKSoundEffect.OogieBoogie:
                clip = _gra_oogieBoogie;
                //pitch = Pitch.D2;
                break;
            case GraveKSoundEffect.PartyCrashers:
                clip = _gra_partyCrashers;
                //pitch = Pitch.D2;
                break;
            case GraveKSoundEffect.Motorcycle:
                clip = _gra_motorcycle;
                //pitch = Pitch.D2;
                break;
            case GraveKSoundEffect.SigBell1:
                clip = _gra_TSbell;
                //pitch = Pitch.D2;
                break;
            case GraveKSoundEffect.SigDrop:
                clip = _gra_TSdrop;
                //pitch = Pitch.D2;
                break;
            case GraveKSoundEffect.SigEffect:
                clip = _gra_TSeffect;
                //pitch = Pitch.D2;
                break;
            case GraveKSoundEffect.SigBell2:
                clip = _gra_churchBell;
                //pitch = Pitch.D2;
                break;
        }

        if (_isMidiMode) {
            SendMIDI(pitch);
        } else {
            _source.PlayOneShot(clip);
        }
    }

    public void Trigger(ValeriaSoundEffect sound) {
        if (_mm.IsReplayMode() && !_mm.debugSettings.animateReplay) // should be shell method
            return;

        AudioClip clip = null;
        Pitch pitch = Pitch.A0;
        switch (sound) {
            case ValeriaSoundEffect.SwirlingWater:
                clip = _val_swirlingWater;
                break;
            case ValeriaSoundEffect.Healing:
                clip = _val_healing;
                break;
            case ValeriaSoundEffect.Mariposa:
                clip = _val_mariposa;
                break;
            case ValeriaSoundEffect.RainDance:
                clip = _val_rainDance;
                break;
            case ValeriaSoundEffect.Bubbles1:
                clip = GetRandom(_val_bubbles1);
                break;
            case ValeriaSoundEffect.Bubbles2:
                clip = GetRandom(_val_bubbles2);
                break;
            case ValeriaSoundEffect.SigCut:
                clip = GetRandom(_val_sigCut);
                break;
            case ValeriaSoundEffect.SigWaveCrash:
                clip = GetRandom(_val_sigWaveCrash);
                break;
            case ValeriaSoundEffect.ThunderFar:
                clip = _val_thunderFar;
                break;
            case ValeriaSoundEffect.ThunderClose:
                clip = _val_thunderClose;
                break;
            case ValeriaSoundEffect.Rain:
                clip = _val_rain;
                break;
        }

        if (_isMidiMode) {
            SendMIDI(pitch);
        } else {
            _source.PlayOneShot(clip);
        }
    }

    public void Trigger(OtherSoundEffect sound) {
        if (_mm.IsReplayMode() && !_mm.debugSettings.animateReplay) // should be shell method
            return;

        AudioClip clip = null;
        Pitch pitch = Pitch.A0;
        switch (sound) {
            case OtherSoundEffect.GameStart:
                clip = _gameStart;
                //pitch = Pitch.D2;
                break;
            case OtherSoundEffect.GameEnd:
                clip = _gameEnd;
                //pitch = Pitch.D2;
                break;
            case OtherSoundEffect.APGain:
                clip = _apGain;
                //pitch = Pitch.D2;
                break;
            case OtherSoundEffect.LowHealthWarning:
                clip = _lowHealthWarning;
                //pitch = Pitch.D2;
                break;
            case OtherSoundEffect.FullMeter:
                clip = _fullMeter;
                //pitch = Pitch.D2;
                break;
            case OtherSoundEffect.TurnTimerWarning:
                clip = _turnTimerWarning;
                //pitch = Pitch.D2;
                break;
            case OtherSoundEffect.TurnTimeout:
                clip = _turnTimeout;
                //pitch = Pitch.D2;
                break;
            case OtherSoundEffect.UIButton:
                clip = GetRandom(_uiButton);
                //pitch = Pitch.D2;
                break;
            case OtherSoundEffect.ChooseTarget:
                clip = GetRandom(_chooseTarget);
                //pitch = Pitch.D2;
                break;
            case OtherSoundEffect.CrowdGasp:
                clip = _crowdGasp;
                //pitch = Pitch.D2;
                break;
        }

        if (_isMidiMode) {
            SendMIDI(pitch);
        } else {
            _source.PlayOneShot(clip);
        }
    }

    public void LowHealthWarning(int id, int amount, int newHealth, bool dealt) {
        if (amount < 0 &&
                newHealth + (-amount) >= Character.HEALTH_WARNING_AMT &&
                newHealth < Character.HEALTH_WARNING_AMT) {
            Trigger(OtherSoundEffect.LowHealthWarning);
            Trigger(OtherSoundEffect.CrowdGasp);
        }
    }

    public void TurnTimeout(int id) {
        Trigger(OtherSoundEffect.TurnTimeout);
    }
}
