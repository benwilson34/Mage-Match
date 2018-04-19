using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

public class PlayerProfile : MonoBehaviour {

    //public Sprite[] frames, backgrounds, foregrounds;

    private Text _tName;

    public void Start() {
        _tName = transform.Find("t_name").GetComponent<Text>();
        _tName.text = "Name: " + _info.localUsername;
    }

    // this should all be done with a JObject?
    private class PlayerProfileInfo {
        public string localUsername;
    }

    private static string _filepath;
    private static PlayerProfileInfo _info;

    public static void Init() {
        _filepath = string.Format("{0}/playerprofile.json", Application.persistentDataPath);

        _info = new PlayerProfileInfo();

        if (File.Exists(_filepath)) {
            JsonConvert.PopulateObject(File.ReadAllText(_filepath), _info);
        } else {
            _info.localUsername = Environment.UserName;
        }
    }

    static JObject GetJObject() {
        StreamReader file = File.OpenText(_filepath);
        JObject job = (JObject)JToken.ReadFrom(new JsonTextReader(file));
        return job;
    }

    public static string GetUsername() { return _info.localUsername; }

    public static void SetUsername(string newName) {
        var obj = GetJObject();
        obj["username"] = newName;

        // TODO write change to file

        _info.localUsername = newName;
    }
}
