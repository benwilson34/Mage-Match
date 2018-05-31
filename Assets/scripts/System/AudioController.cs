using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class AudioController {

    public enum HexSFX { Draw, Pickup, Discard, Grav, Swap, Destroy, Invoke };
    public enum EnfuegoSFX { BurningEnchant, BurningDamage, BurningTimeout, FieryFandango, Baila, Incinerate, WHCK };
    public enum GravekeeperSFX { ZombieEnchant, ZombieAttack, ZombieGulp, PartyInTheBack, OogieBoogie, PartyCrashers, UndeadUnion, Motorcycle, SigBell1, SigDrop, SigEffect, SigBell2 };
    public enum ValeriaSFX { SwirlingWater, Healing, Mariposa, RainDance, Bubbles1, Bubbles2, Balanco, SigCut, SigWaveCrash, ThunderFar, ThunderClose, Rain };

    public enum Rune_NeutralSFX { SampleCharm, ProteinPills, Leeches, ShuffleGem, Molotov, FiveAlarmBell }
    public enum Rune_EnfuegoSFX { DanceShoes, BurningBracers }
    public enum Rune_GravekeeperSFX { HRForm, PartySnacks }
    public enum Rune_ValeriaSFX { Bandages, WaterLily }

    public enum OtherSoundEffect { BackgroundMusic, GameStart, GameEnd, APGain, LowHealthWarning, FullMeter, TurnTimerWarning, TurnTimeout, UIButton, ChooseTarget, Quickdraw_Prompt, Quickdraw_Drop, CrowdGasp };

    private static MageMatch _mm;
    private static Dictionary<System.Enum, string> _clips;

    public static void Init(MageMatch mm) {
        _mm = mm;

        float masterVolume = UserData.MasterVolume;
        MMLog.Log_AudioCont("Volume = " + masterVolume);
        float sfxVolume = UserData.SFXVolume * masterVolume;
        float musicVolume = UserData.MusicVolume * masterVolume;

        FMODUnity.RuntimeManager.GetVCA("vca:/SoundFX").setVolume(sfxVolume);
        FMODUnity.RuntimeManager.GetVCA("vca:/Music").setVolume(musicVolume);

        _clips = new Dictionary<System.Enum, string>();

        // ----- hexes -----
        _clips.Add(HexSFX.Draw, "event:/Hexes/Hex_Draw");
        _clips.Add(HexSFX.Pickup, "event:/Hexes/Hex_Pickup");
        _clips.Add(HexSFX.Discard, "event:/Hexes/Hex_Discard");

        _clips.Add(HexSFX.Grav, "event:/Hexes/Tile_Grav");
        _clips.Add(HexSFX.Swap, "event:/Hexes/Tile_Swap");
        _clips.Add(HexSFX.Destroy, "event:/Hexes/Tile_Destroy");
        _clips.Add(HexSFX.Invoke, "event:/Hexes/Tile_Invoke");


        // ----- enfuego -----
        _clips.Add(EnfuegoSFX.BurningEnchant, "event:/CharacterSpells/Enfuego/Burning_Enchant");
        _clips.Add(EnfuegoSFX.BurningDamage, "event:/CharacterSpells/Enfuego/Burning_Damage");
        _clips.Add(EnfuegoSFX.BurningTimeout, "event:/CharacterSpells/Enfuego/Burning_Timeout");

        _clips.Add(EnfuegoSFX.FieryFandango, "event:/CharacterSpells/Enfuego/FieryFandango");
        _clips.Add(EnfuegoSFX.Baila, "event:/CharacterSpells/Enfuego/Baila");
        _clips.Add(EnfuegoSFX.Incinerate, "event:/CharacterSpells/Enfuego/Incinerate");
        _clips.Add(EnfuegoSFX.WHCK, "event:/CharacterSpells/Enfuego/Sig_WHCK");

        
        // ----- gravekeeper -----
        _clips.Add(GravekeeperSFX.ZombieEnchant, "event:/CharacterSpells/Gravekeeper/Zombie_Enchant");
        _clips.Add(GravekeeperSFX.ZombieAttack, "event:/CharacterSpells/Gravekeeper/Zombie_Attack");
        _clips.Add(GravekeeperSFX.ZombieGulp, "event:/CharacterSpells/Gravekeeper/Zombie_Gulp");

        _clips.Add(GravekeeperSFX.PartyInTheBack, "event:/CharacterSpells/Gravekeeper/PartyInTheBack");
        _clips.Add(GravekeeperSFX.OogieBoogie, "event:/CharacterSpells/Gravekeeper/OogieBoogie");
        _clips.Add(GravekeeperSFX.PartyCrashers, "event:/CharacterSpells/Gravekeeper/PartyCrashers");
        _clips.Add(GravekeeperSFX.UndeadUnion, "event:/CharacterSpells/Gravekeeper/UndeadUnion");

        _clips.Add(GravekeeperSFX.Motorcycle, "event:/CharacterSpells/Gravekeeper/Sig_Motorcycle");
        _clips.Add(GravekeeperSFX.SigBell1, "event:/CharacterSpells/Gravekeeper/Sig_Bell");
        _clips.Add(GravekeeperSFX.SigDrop, "event:/CharacterSpells/Gravekeeper/Sig_TSDrop");
        _clips.Add(GravekeeperSFX.SigEffect, "event:/CharacterSpells/Gravekeeper/Sig_TSEffect");
        _clips.Add(GravekeeperSFX.SigBell2, "event:/CharacterSpells/Gravekeeper/ChurchBell");


        // ----- valeria -----
        _clips.Add(ValeriaSFX.SwirlingWater, "event:/CharacterSpells/Valeria/SwirlingWater");
        _clips.Add(ValeriaSFX.Healing, "event:/CharacterSpells/Valeria/Healing");
        _clips.Add(ValeriaSFX.Mariposa, "event:/CharacterSpells/Valeria/Mariposa");
        _clips.Add(ValeriaSFX.Rain, "event:/CharacterSpells/Valeria/Rain");
        _clips.Add(ValeriaSFX.RainDance, "event:/CharacterSpells/Valeria/RainDance");
        _clips.Add(ValeriaSFX.Bubbles1, "event:/CharacterSpells/Valeria/Bubbles1");
        _clips.Add(ValeriaSFX.Bubbles2, "event:/CharacterSpells/Valeria/Bubbles2");
        _clips.Add(ValeriaSFX.Balanco, "event:/CharacterSpells/Valeria/Balanco");

        _clips.Add(ValeriaSFX.SigCut, "event:/CharacterSpells/Valeria/Sig_Cut");
        _clips.Add(ValeriaSFX.SigWaveCrash, "event:/CharacterSpells/Valeria/Sig_WaveCrash");

        _clips.Add(ValeriaSFX.ThunderFar, "event:/CharacterSpells/Valeria/ThunderFar");
        _clips.Add(ValeriaSFX.ThunderClose, "event:/CharacterSpells/Valeria/ThunderClose");


        // ----- runes -----
        _clips.Add(Rune_NeutralSFX.SampleCharm, "event:/Runes/Neutral/SampleCharm");
        _clips.Add(Rune_NeutralSFX.ProteinPills, "event:/Runes/Neutral/ProteinPills");
        _clips.Add(Rune_NeutralSFX.Leeches, "event:/Runes/Neutral/Leeches");
        _clips.Add(Rune_NeutralSFX.ShuffleGem, "event:/Runes/Neutral/ShuffleGem");
        _clips.Add(Rune_NeutralSFX.Molotov, "event:/Runes/Neutral/Molotov");
        _clips.Add(Rune_NeutralSFX.FiveAlarmBell, "event:/Runes/Neutral/FiveAlarmBell");

        _clips.Add(Rune_EnfuegoSFX.DanceShoes, "event:/Runes/Enfuego/DanceShoes");
        _clips.Add(Rune_EnfuegoSFX.BurningBracers, "event:/Runes/Enfuego/BurningBracers");

        _clips.Add(Rune_GravekeeperSFX.HRForm, "event:/Runes/Gravekeeper/HRForm");
        _clips.Add(Rune_GravekeeperSFX.PartySnacks, "event:/Runes/Gravekeeper/PartySnacks");

        _clips.Add(Rune_ValeriaSFX.Bandages, "event:/Runes/Valeria/Bandages");
        _clips.Add(Rune_ValeriaSFX.WaterLily, "event:/Runes/Valeria/WaterLily");


        // ----- other -----
        _clips.Add(OtherSoundEffect.BackgroundMusic, "event:/Other/BackgroundMusic");
        _clips.Add(OtherSoundEffect.GameStart, "event:/Other/GameStart");
        _clips.Add(OtherSoundEffect.GameEnd, "event:/Other/GameEnd");

        _clips.Add(OtherSoundEffect.APGain, "event:/Other/APGain");
        _clips.Add(OtherSoundEffect.LowHealthWarning, "event:/Other/LowHealthWarning");
        //_clips.Add(OtherSoundEffect.CrowdGasp, "event:/Other/CrowdGasp");
        _clips.Add(OtherSoundEffect.FullMeter, "event:/Other/FullMeter");

        _clips.Add(OtherSoundEffect.TurnTimerWarning, "event:/Other/TurnTimerWarning");
        _clips.Add(OtherSoundEffect.TurnTimeout, "event:/Other/TurnTimeout");

        _clips.Add(OtherSoundEffect.UIButton, "event:/Other/UIButton");
        _clips.Add(OtherSoundEffect.ChooseTarget, "event:/Other/ChooseTarget");


        _clips.Add(OtherSoundEffect.Quickdraw_Prompt, "event:/Other/Quickdraw_Prompt");
        _clips.Add(OtherSoundEffect.Quickdraw_Drop, "event:/Other/Quickdraw_Drop");


        // TODO trigger bg music before the rest of the match finishes loading
        Trigger(OtherSoundEffect.BackgroundMusic);
        Trigger(OtherSoundEffect.GameStart);
        _mm.AddEventContLoadEvent(OnEventContLoaded);
    }

    public static void OnEventContLoaded() {
        _mm.eventCont.playerHealthChange += LowHealthWarning;
        _mm.eventCont.timeout += TurnTimeout;
    }

    public static void LowHealthWarning(int id, int amount, int newHealth, bool dealt) {
        if (amount < 0 &&
                newHealth + (-amount) >= Character.HEALTH_WARNING_AMT &&
                newHealth < Character.HEALTH_WARNING_AMT) {
            Trigger(OtherSoundEffect.LowHealthWarning);
            //Trigger(OtherSoundEffect.CrowdGasp);
        }
    }

    public static void TurnTimeout(int id) {
        Trigger(OtherSoundEffect.TurnTimeout);
    }

    public static void Trigger(System.Enum sound) {
        if (_mm.IsReplayMode() && !_mm.debugSettings.animateReplay) // should be shell method
            return;

        if (!_clips.ContainsKey(sound)) {
            MMLog.LogError("AUDIOCONT: Couldn't trigger " + sound.ToString() + " because it wasn't found in the dictionary!");
            return;
        }

        string clip = _clips[sound];
        FMODUnity.RuntimeManager.PlayOneShot(clip);
    }
}
