using System.Collections.Generic;
using UnityEngine;
using MMDebug;

public class AudioController {

    public enum HexSoundEffect { Draw, Pickup, Discard, Grav, Swap, Destroy, Invoke };
    public enum EnfuegoSoundEffect { BurningEnchant, BurningDamage, BurningTimeout, FieryFandango, Baila, Incinerate, WHCK };
    public enum GraveKSoundEffect { ZombieEnchant, ZombieAttack, ZombieGulp, PartyInTheBack, OogieBoogie, PartyCrashers, Motorcycle, SigBell1, SigDrop, SigEffect, SigBell2 };
    public enum ValeriaSoundEffect { SwirlingWater, Healing, Mariposa, RainDance, Bubbles1, Bubbles2, SigCut, SigWaveCrash, ThunderFar, ThunderClose, Rain };

    public enum Rune_NeutralSFX { SampleCharm, ProteinPills, Leeches, ShuffleGem, Molotov, FiveAlarmBell }
    public enum Rune_EnfuegoSFX { DanceShoes, BurningBracers }
    public enum Rune_GravekeeperSFX { HRForm, PartySnacks }
    public enum Rune_ValeriaSFX { Bandages, WaterLily }

    public enum OtherSoundEffect { BackgroundMusic, GameStart, GameEnd, APGain, LowHealthWarning, FullMeter, TurnTimerWarning, TurnTimeout, UIButton, ChooseTarget, CrowdGasp };

    private MageMatch _mm;
    private Dictionary<System.Enum, string> clips;

    public AudioController(MageMatch mm) {
        this._mm = mm;

        float volume = UserData.GetData().masterVolume / 100f;
        MMLog.Log_AudioCont("Volume = " + volume);

        FMODUnity.RuntimeManager.GetVCA("vca:/SoundFX").setVolume(volume);
        FMODUnity.RuntimeManager.GetVCA("vca:/Music").setVolume(volume);

        clips = new Dictionary<System.Enum, string>();

        // ----- hexes -----
        clips.Add(HexSoundEffect.Draw, "event:/Hexes/Hex_Draw");
        clips.Add(HexSoundEffect.Pickup, "event:/Hexes/Hex_Pickup");
        clips.Add(HexSoundEffect.Discard, "event:/Hexes/Hex_Discard");

        clips.Add(HexSoundEffect.Grav, "event:/Hexes/Tile_Grav");
        clips.Add(HexSoundEffect.Swap, "event:/Hexes/Tile_Swap");
        clips.Add(HexSoundEffect.Destroy, "event:/Hexes/Tile_Destroy");
        clips.Add(HexSoundEffect.Invoke, "event:/Hexes/Tile_Invoke");


        // ----- enfuego -----
        clips.Add(EnfuegoSoundEffect.BurningEnchant, "event:/CharacterSpells/Enfuego/Burning_Enchant");
        clips.Add(EnfuegoSoundEffect.BurningDamage, "event:/CharacterSpells/Enfuego/Burning_Damage");
        clips.Add(EnfuegoSoundEffect.BurningTimeout, "event:/CharacterSpells/Enfuego/Burning_Timeout");

        clips.Add(EnfuegoSoundEffect.FieryFandango, "event:/CharacterSpells/Enfuego/Enf_FieryFandango");
        clips.Add(EnfuegoSoundEffect.Baila, "event:/CharacterSpells/Enfuego/Enf_Baila");
        clips.Add(EnfuegoSoundEffect.Incinerate, "event:/CharacterSpells/Enfuego/Enf_Incinerate");
        clips.Add(EnfuegoSoundEffect.WHCK, "event:/CharacterSpells/Enfuego/Enf_WHCK");

        
        // ----- gravekeeper -----
        clips.Add(GraveKSoundEffect.ZombieEnchant, "event:/CharacterSpells/Gravekeeper/Zombie_Enchant");
        clips.Add(GraveKSoundEffect.ZombieAttack, "event:/CharacterSpells/Gravekeeper/Zombie_Attack");
        clips.Add(GraveKSoundEffect.ZombieGulp, "event:/CharacterSpells/Gravekeeper/Zombie_Gulp");

        clips.Add(GraveKSoundEffect.PartyInTheBack, "event:/CharacterSpells/Gravekeeper/Gra_PartyInTheBack");
        clips.Add(GraveKSoundEffect.OogieBoogie, "event:/CharacterSpells/Gravekeeper/Gra_OogieBoogie");
        clips.Add(GraveKSoundEffect.PartyCrashers, "event:/CharacterSpells/Gravekeeper/Gra_PartyCrashers");

        clips.Add(GraveKSoundEffect.Motorcycle, "event:/CharacterSpells/Gravekeeper/Gra_Motorcycle");
        clips.Add(GraveKSoundEffect.SigBell1, "event:/CharacterSpells/Gravekeeper/Gra_TSBell");
        clips.Add(GraveKSoundEffect.SigDrop, "event:/CharacterSpells/Gravekeeper/Gra_TSDrop");
        clips.Add(GraveKSoundEffect.SigEffect, "event:/CharacterSpells/Gravekeeper/Gra_TSEffect");
        clips.Add(GraveKSoundEffect.SigBell2, "event:/CharacterSpells/Gravekeeper/Gra_ChurchBell");


        // ----- valeria -----
        clips.Add(ValeriaSoundEffect.SwirlingWater, "event:/CharacterSpells/Valeria/Val_SwirlingWater");
        clips.Add(ValeriaSoundEffect.Healing, "event:/CharacterSpells/Valeria/Val_Healing");
        clips.Add(ValeriaSoundEffect.Mariposa, "event:/CharacterSpells/Valeria/Val_Mariposa");
        clips.Add(ValeriaSoundEffect.RainDance, "event:/CharacterSpells/Valeria/Val_RainDance");
        clips.Add(ValeriaSoundEffect.Bubbles1, "event:/CharacterSpells/Valeria/Val_Bubbles1");
        clips.Add(ValeriaSoundEffect.Bubbles2, "event:/CharacterSpells/Valeria/Val_Bubbles2");

        clips.Add(ValeriaSoundEffect.SigCut, "event:/CharacterSpells/Valeria/Val_SigCut");
        clips.Add(ValeriaSoundEffect.SigWaveCrash, "event:/CharacterSpells/Valeria/Val_SigWaveCrash");

        clips.Add(ValeriaSoundEffect.ThunderFar, "event:/CharacterSpells/Valeria/Val_SigThunderFar");
        clips.Add(ValeriaSoundEffect.ThunderClose, "event:/CharacterSpells/Valeria/Val_SigThunderClose");
        clips.Add(ValeriaSoundEffect.Rain, "event:/CharacterSpells/Valeria/Val_SigRain");


        // ----- runes -----
        clips.Add(Rune_NeutralSFX.SampleCharm, "event:/Runes/Neutral/SampleCharm");
        clips.Add(Rune_NeutralSFX.ProteinPills, "event:/Runes/Neutral/ProteinPills");
        clips.Add(Rune_NeutralSFX.Leeches, "event:/Runes/Neutral/Leeches");
        clips.Add(Rune_NeutralSFX.ShuffleGem, "event:/Runes/Neutral/ShuffleGem");
        clips.Add(Rune_NeutralSFX.Molotov, "event:/Runes/Neutral/Molotov");
        clips.Add(Rune_NeutralSFX.FiveAlarmBell, "event:/Runes/Neutral/FiveAlarmBell");

        clips.Add(Rune_EnfuegoSFX.DanceShoes, "event:/Runes/Enfuego/DanceShoes");
        clips.Add(Rune_EnfuegoSFX.BurningBracers, "event:/Runes/Enfuego/Val_SigRain");

        clips.Add(Rune_GravekeeperSFX.HRForm, "event:/Runes/Valeria/Val_SigRain");
        clips.Add(Rune_GravekeeperSFX.PartySnacks, "event:/Runes/Valeria/BurningBracers");

        clips.Add(Rune_ValeriaSFX.Bandages, "event:/Runes/Valeria/Bandages");
        clips.Add(Rune_ValeriaSFX.WaterLily, "event:/Runes/Valeria/WaterLily");


        // ----- other -----
        clips.Add(OtherSoundEffect.BackgroundMusic, "event:/Other/GameStart");
        clips.Add(OtherSoundEffect.GameStart, "event:/Other/GameStart");
        clips.Add(OtherSoundEffect.GameEnd, "event:/Other/GameEnd");

        clips.Add(OtherSoundEffect.APGain, "event:/Other/APGain");
        clips.Add(OtherSoundEffect.LowHealthWarning, "event:/Other/LowHealthWarning");
        clips.Add(OtherSoundEffect.FullMeter, "event:/Other/FullMeter");

        clips.Add(OtherSoundEffect.TurnTimerWarning, "event:/Other/TurnTimerWarning");
        clips.Add(OtherSoundEffect.TurnTimeout, "event:/Other/TurnTimeout");

        clips.Add(OtherSoundEffect.UIButton, "event:/Other/UIButton");
        clips.Add(OtherSoundEffect.ChooseTarget, "event:/Other/ChooseTarget");


        // TODO trigger bg music before the rest of the match finishes loading
        Trigger(OtherSoundEffect.BackgroundMusic);
        Trigger(OtherSoundEffect.GameStart);
    }

    public void InitEvents() {
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

        string clip = clips[sound];

        FMODUnity.RuntimeManager.PlayOneShot(clip);
    }
}
