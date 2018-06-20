using System.Collections.Generic;
using UnityEngine;
using MMDebug;
using System;

public class SFX {
    public enum Hex { Draw, Pickup, Discard, GravClick, Swap, Destroy, Invoke };
    public enum Other { BackgroundMusic, GameStart, GameEnd, APGain, LowHealthWarning, FullMeter, TurnTimerWarning, TurnTimeout, UIButton, ChooseTarget, Quickdraw_Prompt, Quickdraw_Drop, CrowdGasp };

    public enum Enfuego { Burning_Enchant, Burning_Damage, Burning_Timeout, FieryFandango, Baila, Incinerate, Sig_WHCK };
    public enum Gravekeeper { Zombie_Enchant, Zombie_Attack, Zombie_Gulp, PartyInTheBack, OogieBoogie, PartyCrashers, UndeadUnion, Sig_Motorcycle, Sig_Bell1, Sig_TSDrop, Sig_TSEffect, Sig_Bell2 };
    public enum Valeria { SwirlingWater, Healing, Mariposa, RainDance, Bubbles1, Bubbles2, Balanco, Sig_Cut, Sig_WaveCrash, ThunderFar, ThunderClose, Rain };
    public enum MagicAl { Jab, Hook, Cross, StingerStance, Flutterfly, SkyUppercut, StormForceFootwork };

    public enum Rune_Neutral { SampleCharm, Redesign, Molotov, Leeches, Bolster, LegWeights, RollingBone, Stardust, Sanctuary, EvilDoll, Lifestealer, LivingMana, FutureSight, Soulbind, FiveAlarmBell };
    public enum Rune_Enfuego { RoaringFlame, GleamingGolpe, ScorchingSpin, CausticCastanet };
    public enum Rune_Gravekeeper { Recruit, Engorge };
    public enum Rune_Valeria { HealingHands, WaterLily };
    public enum Rune_MagicAl { IllusoryFist, RopeADope };
}

public class AudioController {

    private static MageMatch _mm;
    private static Dictionary<Enum, string> _clips;

    public static void Init(MageMatch mm) {
        _mm = mm;

        float masterVolume = UserData.MasterVolume;
        MMLog.Log_AudioCont("Volume = " + masterVolume);
        float sfxVolume = UserData.SFXVolume * masterVolume;
        float musicVolume = UserData.MusicVolume * masterVolume;

        FMODUnity.RuntimeManager.GetVCA("vca:/SoundFX").setVolume(sfxVolume);
        FMODUnity.RuntimeManager.GetVCA("vca:/Music").setVolume(musicVolume);

        _clips = new Dictionary<Enum, string>();

        // ----- character sfx -----
        LoadCharacterClips(_mm.gameSettings.p1char);
        LoadCharacterClips(_mm.gameSettings.p2char);


        // ----- hexes -----
        foreach (SFX.Hex sfx in Enum.GetValues(typeof(SFX.Hex)))
            LoadClip(sfx, "Hex");


        // ----- runes -----
        LoadClip(SFX.Rune_Neutral.SampleCharm, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.Redesign, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.Molotov, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.Leeches, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.Bolster, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.LegWeights, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.RollingBone, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.Stardust, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.Sanctuary, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.EvilDoll, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.Lifestealer, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.LivingMana, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.FutureSight, "Runes/Neutral");
        LoadClip(SFX.Rune_Neutral.Soulbind, "Runes/Neutral");

        LoadClip(SFX.Rune_Enfuego.RoaringFlame, "Runes/Enfuego");
        LoadClip(SFX.Rune_Enfuego.GleamingGolpe, "Runes/Enfuego");
        LoadClip(SFX.Rune_Enfuego.ScorchingSpin, "Runes/Enfuego");
        LoadClip(SFX.Rune_Enfuego.CausticCastanet, "Runes/Enfuego");

        LoadClip(SFX.Rune_Gravekeeper.Recruit, "Runes/Gravekeeper");
        LoadClip(SFX.Rune_Gravekeeper.Engorge, "Runes/Gravekeeper");

        LoadClip(SFX.Rune_Valeria.HealingHands, "Runes/Gravekeeper");
        LoadClip(SFX.Rune_Valeria.WaterLily, "Runes/Gravekeeper");

        LoadClip(SFX.Rune_MagicAl.IllusoryFist, "Runes/MagicAl");
        LoadClip(SFX.Rune_MagicAl.RopeADope, "Runes/MagicAl");


        // ----- other -----
        foreach (SFX.Other sfx in Enum.GetValues(typeof(SFX.Other)))
            LoadClip(sfx, "Other");


        Trigger(SFX.Other.BackgroundMusic);
        _mm.AddEventContLoadEvent(OnEventContLoaded);
    }

    static void LoadClip(Enum key, params string[] folders) {
        List<string> tokens = new List<string>() { "event:" };
        tokens.AddRange(folders);
        tokens.Add(key.ToString());
        _clips.Add(key, string.Join("/", tokens.ToArray()));
    }

    static void LoadCharacterClips(Character.Ch ch) {
        Array enumArray = null;
        switch (ch) {
            case Character.Ch.Enfuego:
                enumArray = Enum.GetValues(typeof(SFX.Enfuego));
                break;
            case Character.Ch.Gravekeeper:
                enumArray = Enum.GetValues(typeof(SFX.Gravekeeper));
                break;
            case Character.Ch.Valeria:
                enumArray = Enum.GetValues(typeof(SFX.Valeria));
                break;
            case Character.Ch.MagicAl:
                enumArray = Enum.GetValues(typeof(SFX.MagicAl));
                break;

            default:
                MMLog.LogWarning("Can't load SFX for character: " + ch.ToString());
                return;
        }

        string chStr = ch.ToString();
        foreach (Enum sfx in enumArray)
            LoadClip(sfx, "CharacterSpells", chStr);
    }

    static void LoadRuneClip(Character.Ch ch, string rune) {
        // TODO try to parse Rune_Neutral or Rune_{ch}
        // maybe look up the character from the rune info?
    }

    public static void OnEventContLoaded() {
        EventController.playerHealthChange += LowHealthWarning;
        EventController.timeout += TurnTimeout;
    }

    public static void LowHealthWarning(int id, int amount, int newHealth, bool dealt) {
        if (amount < 0 &&
                newHealth + (-amount) >= Character.HEALTH_WARNING_AMT &&
                newHealth < Character.HEALTH_WARNING_AMT) {
            Trigger(SFX.Other.LowHealthWarning);
            //Trigger(OtherSoundEffect.CrowdGasp);
        }
    }

    public static void TurnTimeout(int id) {
        Trigger(SFX.Other.TurnTimeout);
    }

    public static void Trigger(Enum sound) {
        if (_mm.IsReplayMode && !_mm.debugSettings.animateReplay) // should be shell method
            return;

        if (!_clips.ContainsKey(sound)) {
            MMLog.LogError("AUDIOCONT: Couldn't trigger " + sound.ToString() + " because it wasn't found in the dictionary!");
            return;
        }

        string clip = _clips[sound];
        FMODUnity.RuntimeManager.PlayOneShot(clip);
    }
}
