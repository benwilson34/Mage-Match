using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class UserData {

    enum Pref { MasterVolume, SFXVolume, MusicVolume, Username };

    public static string Username {
        get { return PlayerPrefs.GetString(Pref.Username.ToString()); }
        set {
            PlayerPrefs.SetString(Pref.Username.ToString(), value);
            PlayerPrefs.Save();
        }
    }
    public static float MasterVolume {
        get { return PlayerPrefs.GetFloat(Pref.MasterVolume.ToString()); }
        set {
            PlayerPrefs.SetFloat(Pref.MasterVolume.ToString(), value);
            PlayerPrefs.Save();
        }
    }
    public static float SFXVolume {
        get { return PlayerPrefs.GetFloat(Pref.SFXVolume.ToString()); }
        set {
            PlayerPrefs.SetFloat(Pref.SFXVolume.ToString(), value);
            PlayerPrefs.Save();
        }
    }
    public static float MusicVolume {
        get { return PlayerPrefs.GetFloat(Pref.MusicVolume.ToString()); }
        set {
            PlayerPrefs.SetFloat(Pref.MusicVolume.ToString(), value);
            PlayerPrefs.Save();
        }
    }

    public static void Init() {
        if (!PlayerPrefs.HasKey(Pref.Username.ToString()))
            PlayerPrefs.SetString(Pref.Username.ToString(), Environment.UserName);

        if (!PlayerPrefs.HasKey(Pref.MasterVolume.ToString()))
            PlayerPrefs.SetFloat(Pref.MasterVolume.ToString(), .3f);

        if (!PlayerPrefs.HasKey(Pref.SFXVolume.ToString()))
            PlayerPrefs.SetFloat(Pref.SFXVolume.ToString(), .99f);

        if (!PlayerPrefs.HasKey(Pref.MusicVolume.ToString()))
            PlayerPrefs.SetFloat(Pref.MusicVolume.ToString(), .99f);
    }

}
