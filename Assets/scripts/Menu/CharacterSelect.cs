using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour {

    private GameObject localBlock, opponentBlock;
    private Dropdown charDD, loadoutDD;
    private Text charT, loadoutT;
    private Dropdown oppCharDD;
    private Text oppCharT;

    private GameSettings gameSettings;
    private int myID;
    private bool otherPlayerLocked = false;
    private bool otherCharacterSet = false;

    // TODO another turn timer

    void Start () {
        CharacterInfo.Init();

        localBlock = GameObject.Find("localBlock");
        opponentBlock = GameObject.Find("opponentBlock");
        charDD = localBlock.transform.Find("dd_char").GetComponent<Dropdown>();
        loadoutDD = localBlock.transform.Find("dd_loadout").GetComponent<Dropdown>();
        gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        charT = localBlock.transform.Find("t_charInfo").GetComponent<Text>();
        loadoutT = localBlock.transform.Find("t_loadoutInfo").GetComponent<Text>();

        // get opponent UI objects and disable them
        oppCharDD = opponentBlock.transform.Find("dd_char").GetComponent<Dropdown>();
        oppCharT = opponentBlock.transform.Find("t_charInfo").GetComponent<Text>();
        oppCharDD.gameObject.SetActive(false);
        oppCharT.gameObject.SetActive(false);

        myID = PhotonNetwork.player.ID;
        //myID = 1; // testing
        int loadout;

        string localName, oppName;
        if (myID == 1) {
            localName = gameSettings.p1name;
            oppName = gameSettings.p2name;
            charDD.value = (int)gameSettings.p1char;
            Debug.Log("p1 loadout=" + gameSettings.p1loadout);
            loadout = gameSettings.p1loadout;
        } else {
            localName = gameSettings.p2name;
            oppName = gameSettings.p1name;
            charDD.value = (int)gameSettings.p2char;
            loadout = gameSettings.p2loadout;
        }
        UpdateCharInfo(charDD.value);
        loadoutDD.value = loadout; //shitty
        UpdateLoadoutInfo(loadout); //shitty

        localBlock.transform.Find("t_name").GetComponent<Text>().text = localName;
        opponentBlock.transform.Find("t_name").GetComponent<Text>().text = oppName;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnLockedIn() { StartCoroutine(LockedIn()); }
    private IEnumerator LockedIn() {
        // store chosen char and loadout
        gameSettings.SetLocalChar(myID, (Character.Ch)charDD.value);
        gameSettings.SetLocalLoadout(myID, loadoutDD.value);

        // disable dropdowns & change background to green
        charDD.interactable = false;
        loadoutDD.interactable = false;
        Image i = localBlock.transform.Find("i_background").GetComponent<Image>();
        i.color = Color.green;

        // send lock
        PhotonView view = gameSettings.GetComponent<PhotonView>();
        view.RPC("OtherPlayerLocked", PhotonTargets.Others);

        // then wait for other player to lock in also
        yield return new WaitUntil(() => otherPlayerLocked);
        Debug.Log("Moving forward!!!");

        // then sync chosen character and loadout
        gameSettings.SyncCharAndLoadout(myID);

        yield return new WaitUntil(() => otherCharacterSet);
        // then display opponent character (but not loadout)
        // then load Game Screen
        yield return new WaitForSeconds(2f);

        if (PhotonNetwork.isMasterClient) {
            PhotonNetwork.LoadLevel("Game Screen (Landscape)");
        }
    }

    public void OtherPlayerLocked() {
        opponentBlock.transform.Find("i_background").GetComponent<Image>().color = Color.green;
        otherPlayerLocked = true;
    }

    public void OtherCharacterSynced(int index) {
        // turn objects back on
        oppCharDD.gameObject.SetActive(true);
        oppCharT.gameObject.SetActive(true);

        oppCharDD.value = index;
        string info = CharacterInfo.GetCharacterInfo((Character.Ch)index);
        oppCharT.text = info;
        otherCharacterSet = true;
    }

    public void UpdateCharInfo(int index) {
        //Debug.Log("char index is " + index);
        string info = CharacterInfo.GetCharacterInfo((Character.Ch)index);
        charT.text = info;
        loadoutDD.value = 0;
        UpdateLoadoutInfo(0);
        // TODO change names of loadouts in loadoutDD
    }

    public void UpdateLoadoutInfo(int index) {
        //Debug.Log("loadout index is " + index);
        string info = CharacterInfo.GetLoadoutInfo((Character.Ch)charDD.value, index); // ew
        loadoutT.text = info;
    }
}
