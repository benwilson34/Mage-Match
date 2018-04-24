using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class UserData {

    // this should all be done with a JObject?
    public string username;
    public int masterVolume, soundFXVolume;

    private static string _filepath;
    private static UserData _data;

    public static void Init() {
        _filepath = Application.persistentDataPath + "/data.json";

        _data = new UserData();

        if (File.Exists(_filepath)) {
            JsonConvert.PopulateObject(File.ReadAllText(_filepath), _data);
        } else {
            // init & save new data obj
            _data.username = Environment.UserName;
            _data.masterVolume = 30;
            _data.soundFXVolume = 99;
            SaveData(_data);
        }
    }

    static JObject GetJObject() {
        StreamReader file = File.OpenText(_filepath);
        JObject job = (JObject)JToken.ReadFrom(new JsonTextReader(file));
        return job;
    }

    //public static string GetUsername() { return _info.username; }

    //public static void SetUsername(string newName) {
    //    //var obj = GetJObject();
    //    //obj["username"] = newName;

    //    // TODO write change to file

    //    _info.username = newName;
    //    SaveData();
    //}

    public static UserData GetData() { return _data; }

    public static void SaveData(UserData data) {
        File.WriteAllText(_filepath, JsonConvert.SerializeObject(data));
    }
}
