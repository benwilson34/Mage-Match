using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class AudioController {

    public enum HexSoundEffect { Draw, Pickup, Discard, Grav, Swap, Destroy, Invoke };
    public enum EnfuegoSoundEffect { BurningEnchant, BurningDamage, BurningTimeout, FieryFandango, Baila, Incinerate, WHCK };
    public enum GraveKSoundEffect { ZombieEnchant, ZombieAttack, ZombieGulp, PartyInTheBack, OogieBoogie, PartyCrashers, Motorcycle, SigBell1, SigDrop, SigEffect, SigBell2 };
    public enum ValeriaSoundEffect { SwirlingWater, Healing, Mariposa, RainDance, Bubbles1, Bubbles2, SigCut, SigWaveCrash, ThunderFar, ThunderClose, Rain };
    public enum OtherSoundEffect { BackgroundMusic, GameStart, GameEnd, APGain, LowHealthWarning, FullMeter, TurnTimerWarning, TurnTimeout, UIButton, ChooseTarget, CrowdGasp };

    private MageMatch _mm;
    private Dictionary<System.Enum, string> _clips;

    public AudioController(MageMatch mm) {
        this._mm = mm;

        float masterVolume = UserData.MasterVolume;
        MMLog.Log_AudioCont("Volume = " + masterVolume);
        float sfxVolume = UserData.SFXVolume * masterVolume;
        float musicVolume = UserData.MusicVolume * masterVolume;

        FMODUnity.RuntimeManager.GetVCA("vca:/SoundFX").setVolume(sfxVolume);
        FMODUnity.RuntimeManager.GetVCA("vca:/Music").setVolume(musicVolume);

        _clips = new Dictionary<System.Enum, string>();

        // ----- hexes -----
        _clips.Add(HexSoundEffect.Draw, "event:/Hexes/Hex_Draw");
        _clips.Add(HexSoundEffect.Pickup, "event:/Hexes/Hex_Pickup");
        _clips.Add(HexSoundEffect.Discard, "event:/Hexes/Hex_Discard");

        _clips.Add(HexSoundEffect.Grav, "event:/Hexes/Tile_Grav");
        _clips.Add(HexSoundEffect.Swap, "event:/Hexes/Tile_Swap");
        _clips.Add(HexSoundEffect.Destroy, "event:/Hexes/Tile_Destroy");
        _clips.Add(HexSoundEffect.Invoke, "event:/Hexes/Tile_Invoke");


        // ----- enfuego -----
        _clips.Add(EnfuegoSoundEffect.BurningEnchant, "event:/Enfuego/Burning_Enchant");
        _clips.Add(EnfuegoSoundEffect.BurningDamage, "event:/Enfuego/Burning_Damage");
        _clips.Add(EnfuegoSoundEffect.BurningTimeout, "event:/Enfuego/Burning_Timeout");

        _clips.Add(EnfuegoSoundEffect.FieryFandango, "event:/Enfuego/Enf_FieryFandango");
        _clips.Add(EnfuegoSoundEffect.Baila, "event:/Enfuego/Enf_Baila");
        _clips.Add(EnfuegoSoundEffect.Incinerate, "event:/Enfuego/Enf_Incinerate");
        _clips.Add(EnfuegoSoundEffect.WHCK, "event:/Enfuego/Enf_WHCK");

        
        // ----- gravekeeper -----
        _clips.Add(GraveKSoundEffect.ZombieEnchant, "event:/Gravekeeper/Zombie_Enchant");
        _clips.Add(GraveKSoundEffect.ZombieAttack, "event:/Gravekeeper/Zombie_Attack");
        _clips.Add(GraveKSoundEffect.ZombieGulp, "event:/Gravekeeper/Zombie_Gulp");

        _clips.Add(GraveKSoundEffect.PartyInTheBack, "event:/Gravekeeper/Gra_PartyInTheBack");
        _clips.Add(GraveKSoundEffect.OogieBoogie, "event:/Gravekeeper/Gra_OogieBoogie");
        _clips.Add(GraveKSoundEffect.PartyCrashers, "event:/Gravekeeper/Gra_PartyCrashers");

        _clips.Add(GraveKSoundEffect.Motorcycle, "event:/Gravekeeper/Gra_Motorcycle");
        _clips.Add(GraveKSoundEffect.SigBell1, "event:/Gravekeeper/Gra_TSBell");
        _clips.Add(GraveKSoundEffect.SigDrop, "event:/Gravekeeper/Gra_TSDrop");
        _clips.Add(GraveKSoundEffect.SigEffect, "event:/Gravekeeper/Gra_TSEffect");
        _clips.Add(GraveKSoundEffect.SigBell2, "event:/Gravekeeper/Gra_ChurchBell");


        // ----- valeria -----
        _clips.Add(ValeriaSoundEffect.SwirlingWater, "event:/Valeria/Val_SwirlingWater");
        _clips.Add(ValeriaSoundEffect.Healing, "event:/Valeria/Val_Healing");
        _clips.Add(ValeriaSoundEffect.Mariposa, "event:/Valeria/Val_Mariposa");
        _clips.Add(ValeriaSoundEffect.RainDance, "event:/Valeria/Val_RainDance");
        _clips.Add(ValeriaSoundEffect.Bubbles1, "event:/Valeria/Val_Bubbles1");
        _clips.Add(ValeriaSoundEffect.Bubbles2, "event:/Valeria/Val_Bubbles2");

        _clips.Add(ValeriaSoundEffect.SigCut, "event:/Valeria/Val_SigCut");
        _clips.Add(ValeriaSoundEffect.SigWaveCrash, "event:/Valeria/Val_SigWaveCrash");

        _clips.Add(ValeriaSoundEffect.ThunderFar, "event:/Valeria/Val_SigThunderFar");
        _clips.Add(ValeriaSoundEffect.ThunderClose, "event:/Valeria/Val_SigThunderClose");
        _clips.Add(ValeriaSoundEffect.Rain, "event:/Valeria/Val_SigRain");


        // ----- other -----
        _clips.Add(OtherSoundEffect.BackgroundMusic, "event:/Other/GameStart");
        _clips.Add(OtherSoundEffect.GameStart, "event:/Other/GameStart");
        _clips.Add(OtherSoundEffect.GameEnd, "event:/Other/GameEnd");

        _clips.Add(OtherSoundEffect.APGain, "event:/Other/APGain");
        _clips.Add(OtherSoundEffect.LowHealthWarning, "event:/Other/LowHealthWarning");
        _clips.Add(OtherSoundEffect.FullMeter, "event:/Other/FullMeter");

        _clips.Add(OtherSoundEffect.TurnTimerWarning, "event:/Other/TurnTimerWarning");
        _clips.Add(OtherSoundEffect.TurnTimeout, "event:/Other/TurnTimeout");

        _clips.Add(OtherSoundEffect.UIButton, "event:/Other/UIButton");
        _clips.Add(OtherSoundEffect.ChooseTarget, "event:/Other/ChooseTarget");


        // TODO trigger bg music before the rest of the match finishes loading
        Trigger(OtherSoundEffect.BackgroundMusic);
        Trigger(OtherSoundEffect.GameStart);
        _mm.AddEventContLoadEvent(OnEventContLoaded);
    }

    public void OnEventContLoaded() {
        _mm.eventCont.playerHealthChange += LowHealthWarning;
        _mm.eventCont.timeout += TurnTimeout;
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

    public void Trigger(System.Enum sound) {
        if (_mm.IsReplayMode() && !_mm.debugSettings.animateReplay) // should be shell method
            return;

        string clip = _clips[sound];

        FMODUnity.RuntimeManager.PlayOneShot(clip);
    }
}
