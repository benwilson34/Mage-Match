using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class UserData {

    enum Pref { Username, MasterVolume, SFXVolume, MusicVolume, MMCoin };

    public static string Username {
        get { return ZPlayerPrefs.GetString(Pref.Username.ToString()); }
        set {
            ZPlayerPrefs.SetString(Pref.Username.ToString(), value);
            ZPlayerPrefs.Save();
        }
    }
    public static float MasterVolume {
        get { return ZPlayerPrefs.GetFloat(Pref.MasterVolume.ToString()); }
        set {
            ZPlayerPrefs.SetFloat(Pref.MasterVolume.ToString(), value);
            ZPlayerPrefs.Save();
        }
    }
    public static float SFXVolume {
        get { return ZPlayerPrefs.GetFloat(Pref.SFXVolume.ToString()); }
        set {
            ZPlayerPrefs.SetFloat(Pref.SFXVolume.ToString(), value);
            ZPlayerPrefs.Save();
        }
    }
    public static float MusicVolume {
        get { return ZPlayerPrefs.GetFloat(Pref.MusicVolume.ToString()); }
        set {
            ZPlayerPrefs.SetFloat(Pref.MusicVolume.ToString(), value);
            ZPlayerPrefs.Save();
        }
    }
    public static int MMCoin{
        get { return ZPlayerPrefs.GetInt(Pref.MMCoin.ToString()); }
        set {
            ZPlayerPrefs.SetInt(Pref.MMCoin.ToString(), value);
            ZPlayerPrefs.Save();
        }
    }

    public static void Init() {
        ZPlayerPrefs.Initialize("idk some password", "fweamforever!!~");

        if (!ZPlayerPrefs.HasKey(Pref.Username.ToString()))
            ZPlayerPrefs.SetString(Pref.Username.ToString(), Environment.UserName);

        if (!ZPlayerPrefs.HasKey(Pref.MasterVolume.ToString()))
            ZPlayerPrefs.SetFloat(Pref.MasterVolume.ToString(), .3f);

        if (!ZPlayerPrefs.HasKey(Pref.SFXVolume.ToString()))
            ZPlayerPrefs.SetFloat(Pref.SFXVolume.ToString(), .99f);

        if (!ZPlayerPrefs.HasKey(Pref.MusicVolume.ToString()))
            ZPlayerPrefs.SetFloat(Pref.MusicVolume.ToString(), .99f);

        if (!ZPlayerPrefs.HasKey(Pref.MMCoin.ToString()))
            ZPlayerPrefs.SetInt(Pref.MMCoin.ToString(), 0);
    }

}
