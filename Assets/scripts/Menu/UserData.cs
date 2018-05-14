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

    //private static string _filepath;
    //private static UserData _data;

    public static void Init() {
        //_filepath = Application.persistentDataPath + "/data.json";

        //_data = new UserData();

        if (!PlayerPrefs.HasKey(Pref.Username.ToString()))
            PlayerPrefs.SetString(Pref.Username.ToString(), Environment.UserName);

        if (!PlayerPrefs.HasKey(Pref.MasterVolume.ToString()))
            PlayerPrefs.SetFloat(Pref.MasterVolume.ToString(), .3f);

        if (!PlayerPrefs.HasKey(Pref.SFXVolume.ToString()))
            PlayerPrefs.SetFloat(Pref.SFXVolume.ToString(), .99f);

        if (!PlayerPrefs.HasKey(Pref.MusicVolume.ToString()))
            PlayerPrefs.SetFloat(Pref.MusicVolume.ToString(), .99f);

        //if (File.Exists(_filepath)) {
        //    JsonConvert.PopulateObject(File.ReadAllText(_filepath), _data);
        //} else {
        //    // init & save new data obj
        //    _data.username = Environment.UserName;
        //    _data.masterVolume = 30;
        //    _data.soundFXVolume = 99;
        //    SaveData(_data);
        //}
    }

    //static JObject GetJObject() {
    //    StreamReader file = File.OpenText(_filepath);
    //    JObject job = (JObject)JToken.ReadFrom(new JsonTextReader(file));
    //    file.Close();
    //    return job;
    //}

    //public static string GetUsername() { return _info.username; }

    //public static void SetUsername(string newName) {
    //    //var obj = GetJObject();
    //    //obj["username"] = newName;

    //    // TODO write change to file

    //    _info.username = newName;
    //    SaveData();
    //}

    //public static UserData GetData() { return _data; }

    //public static void SaveData(UserData data) {
    //    File.WriteAllText(_filepath, JsonConvert.SerializeObject(data));
    //}

}
