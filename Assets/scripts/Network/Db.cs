//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Db : MonoBehaviour {

//    public string profile_url = "https://mage-match.herokuapp.com/api/message";

//    private JSONObject profileJson;

//    // Use this for initialization
//    void Start () {
//        StartCoroutine(GetData());
//	}

//    public IEnumerator GetData() {
//        yield return RetrieveProfileInfo();

//        JSONObject j = new JSONObject(profileJson);
//        //accessData(j);
//        Debug.Log(j.Print());

//        JSONObject prefs = j.list[0].list[0].list[3];
//        Debug.Log(prefs.Print());

//        for (int i = 0; i < prefs.list.Count; i++) {
//            JSONObject obj = prefs.list[i];
//            Debug.Log("\"" + prefs.keys[i] + "\": " + obj.n);
//        }
//    }

//    public IEnumerator RetrieveProfileInfo() {
//        WWW www = new WWW(profile_url);
//        yield return www;
//        profileJson = new JSONObject(www.text);
//    }

//    // TODO hashtable with id as the key?
//    public JSONObject GetProfilePrefs(int i) {
//        if (i < profileJson.list[0].list.Count)
//            return profileJson.list[0].list[i].list[3]; //data.user.prefs object
//        else {
//            Debug.LogError("There's no player with that id!!");
//            return null;
//        }
//    }

//    void accessData(JSONObject obj) {
//        switch (obj.type) {
//            case JSONObject.Type.OBJECT:
//                Debug.Log("{");
//                for (int i = 0; i < obj.list.Count; i++) {
//                    string key = (string)obj.keys[i];
//                    JSONObject j = (JSONObject)obj.list[i];
//                    Debug.Log("\"" + key + "\":");
//                    accessData(j);
//                }
//                Debug.Log("}");

//                break;
//            case JSONObject.Type.ARRAY:
//                Debug.Log("[");
//                foreach (JSONObject j in obj.list) {
//                    accessData(j);
//                    Debug.Log(", ");
//                }
//                Debug.Log("]");
//                break;
//            case JSONObject.Type.STRING:
//                Debug.Log(obj.str);
//                break;
//            case JSONObject.Type.NUMBER:
//                Debug.Log(obj.n);
//                break;
//            case JSONObject.Type.BOOL:
//                Debug.Log(obj.b);
//                break;
//            case JSONObject.Type.NULL:
//                Debug.Log("NULL");
//                break;

//        }
//    }
//}