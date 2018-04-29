using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class UserData {

    public class LoadoutData {
        public string name;
        public string[] runes;
    }

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
        file.Close();
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

    public static LoadoutData[] GetLoadoutList(Character.Ch ch) {
        string path = Application.persistentDataPath + "/user_loadouts.json";
        StreamReader file = File.OpenText(path);
        JObject job = (JObject)JToken.ReadFrom(new JsonTextReader(file));
        LoadoutData data;

        List<LoadoutData> loadouts = new List<LoadoutData>();
        if (job[ch.ToString()] != null) {
            Debug.Log("Found loadouts for " + ch.ToString());
            JArray charLoadouts = (JArray)job[ch.ToString()];

            foreach (var prop in charLoadouts.Children()) {
                Debug.Log("USERDATA: Read " + prop.ToString());
                data = new LoadoutData();
                JsonConvert.PopulateObject(prop.ToString(), data);
                loadouts.Add(data);
            }
        }
        file.Close();

        string json = (Resources.Load("json/default_loadouts") as TextAsset).text;
        JObject o = JObject.Parse(json);

        data = new LoadoutData();
        JsonConvert.PopulateObject(o[ch.ToString()].ToString(), data);
        loadouts.Add(data);

        return loadouts.ToArray();
    }

    public static void SaveLoadout() {
        
    }
}
