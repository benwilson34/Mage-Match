using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class LoadoutData {

	public const int RUNE_COUNT = 7;
        
    public string name;
    public string[] runes;
    public string filepath;
    public bool isDefault = false;

    public static string GetLoadoutDirectory(Character.Ch ch) {
        string path = Application.persistentDataPath + "/Loadouts";
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }

        path += "/" + ch.ToString();
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    public static LoadoutData[] GetLoadoutList(Character.Ch ch) {
        string path = GetLoadoutDirectory(ch);

        List<LoadoutData> loadouts = new List<LoadoutData>();
        loadouts.Add(GetDefaultLoadout(ch));

        foreach (string loadoutPath in Directory.GetFiles(path)) {
            //StreamReader file = File.OpenText(loadoutPath);
            //JObject job = (JObject)JToken.ReadFrom(new JsonTextReader(file));
            LoadoutData loadout = new LoadoutData();
            JsonConvert.PopulateObject(File.ReadAllText(loadoutPath), loadout);
            loadout.filepath = loadoutPath;
            //loadout.isDefault = false;
            loadouts.Add(loadout);
        }

        //if (job[ch.ToString()] != null) {
        //    Debug.Log("Found loadouts for " + ch.ToString());
        //    JArray charLoadouts = (JArray)job[ch.ToString()];

        //    foreach (var prop in charLoadouts.Children()) {
        //        Debug.Log("USERDATA: Read " + prop.ToString());
        //        data = new LoadoutData();
        //        JsonConvert.PopulateObject(prop.ToString(), data);
        //        loadouts.Add(data);
        //    }
        //}
        //file.Close();

        return loadouts.ToArray();
    }

    public static LoadoutData GetDefaultLoadout(Character.Ch ch) {
        string json = (Resources.Load("json/default_loadouts") as TextAsset).text;
        JObject o = JObject.Parse(json);

        LoadoutData loadout = new LoadoutData();
        JsonConvert.PopulateObject(o[ch.ToString()].ToString(), loadout);
        loadout.isDefault = true;
        return loadout;
    }

    public static void SaveLoadout(LoadoutData data) {
        File.WriteAllText(data.filepath, JsonConvert.SerializeObject(data));
        Debug.Log("Saved data to " + data.filepath);
    }
}
