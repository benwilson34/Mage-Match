using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lobby : Photon.PunBehaviour {

    public GameObject entryPF;

    private RoomSettings rs;
    private GameObject b_join, content;
    private List<GameObject> items;
    private LobbyEntry currentEntry;

    private GameObject testSettingsMenu;

	void Start () {
        rs = GameObject.Find("roomSettings").GetComponent<RoomSettings>();
        b_join = GameObject.Find("b_Join");
        content = GameObject.Find("Scroll View").transform.Find("Viewport").Find("Content").gameObject;

        PhotonNetwork.ConnectUsingSettings("1");

        items = new List<GameObject>();
        testSettingsMenu = GameObject.Find("testSettingsMenu");
        ToggleTestSettings();
	}
	
	void Update () {
		
	}

    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinLobby();
        //InvokeRepeating("UpdateRoomList", 0f, 1f);
    }

    public void UpdateRoomList() {
        //Debug.Log("Looking for rooms...");
        foreach (GameObject go in items) { Destroy(go); }
        items.Clear();

        //if (PhotonNetwork.insideLobby) {
        //    Debug.Log("lobby name: " + PhotonNetwork.lobby.Name);
        //}

        foreach (RoomInfo game in PhotonNetwork.GetRoomList()) {
            Debug.Log(game.Name);
            Debug.Log(game.PlayerCount);
            Debug.Log(game.MaxPlayers);
            string hostName = (string)game.CustomProperties["hostName"];
            Debug.Log("contains key=" + game.CustomProperties.ContainsKey("hostName"));
            Debug.Log("count=" + game.CustomProperties.Keys.Count);
            Debug.Log("hostName=" + hostName);

            Transform entry = Instantiate(entryPF).transform;
            entry.GetComponent<LobbyEntry>().roomName = game.Name;
            entry.Find("t_Name").GetComponent<Text>().text = hostName;

            entry.SetParent(content.transform, false);
            items.Add(entry.gameObject);
        }
    }

    public void StartNewMatch() {
        rs.isNewRoom = true;
        SceneManager.LoadScene("Launcher");
    }

    public void JoinMatch() {
        rs.isNewRoom = false;
        rs.roomName = currentEntry.roomName;
        SceneManager.LoadScene("Launcher");
    }

    public void Quickstart() {
        SceneManager.LoadScene("Quickstart Launcher");
    }

    public void ToggleTestSettings() {
        testSettingsMenu.SetActive(!testSettingsMenu.GetActive());
    }

    public void StartTestScene() {
        GameObject debugSettingsGO = new GameObject("debugSettings");
        DebugSettings dbs = debugSettingsGO.AddComponent<DebugSettings>();
        dbs.applyAPcost = testSettingsMenu.transform.Find("tog_applyAPcosts").GetComponent<Toggle>().isOn;
        dbs.onePlayerMode = testSettingsMenu.transform.Find("tog_onlyP1").GetComponent<Toggle>().isOn;

        GameSettings settings = new GameObject("gameSettings").AddComponent<GameSettings>();
        settings.p1name = "GOD";
        settings.p2name = "Training dummy";
        settings.turnTimerOn = false;

        PhotonNetwork.Disconnect();

        SceneManager.LoadScene("Game Screen (Landscape)");
    }

    public void SetCurrentEntry(LobbyEntry entry) {
        if (currentEntry != null) {
            currentEntry.transform.Find("bg").GetComponent<Image>().color = Color.yellow;
        }
        currentEntry = entry;
        entry.transform.Find("bg").GetComponent<Image>().color = Color.red;

        b_join.GetComponent<Button>().interactable = true;
    }
}
