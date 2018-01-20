using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour {

    private Transform localBlock, oppBlock, characterBlock;
    private Image localCharPortrait, oppCharPortrait;
    private Text localCharName, oppCharName, charT;
    private Button bReady;
    private GameObject goldSelectionPF, redSelectionPF;

    private GameSettings gameSettings;
    private int myID;
    private Character.Ch localChar = Character.Ch.Test;
    private bool thisPlayerLocked = false;
    private bool otherPlayerLocked = false;
    private bool otherCharacterSet = false;

    // TODO another turn timer

    void Start () {
        CharacterInfo.Init();

        localBlock = GameObject.Find("localBlock").transform;
        oppBlock = GameObject.Find("opponentBlock").transform;
        characterBlock = GameObject.Find("characterBlock").transform;

        localCharPortrait = localBlock.Find("i_charPortrait").GetComponent<Image>();
        localCharName = localBlock.Find("t_charName").GetComponent<Text>();
        localCharName.text = "Choose your fighter!";
        oppCharPortrait = oppBlock.Find("i_charPortrait").GetComponent<Image>();
        oppCharName = oppBlock.Find("t_charName").GetComponent<Text>();
        oppCharName.text = "Choose your fighter!";

        charT = GameObject.Find("t_charInfo").GetComponent<Text>();
        charT.text = "";
        bReady = GameObject.Find("b_ready").GetComponent<Button>();
        bReady.enabled = false;

        gameSettings = GameObject.Find("gameSettings").GetComponent<GameSettings>();

        myID = PhotonNetwork.player.ID;
        //myID = 1; // testing
        string localName, oppName;
        if (myID == 1) {
            localName = gameSettings.p1name;
            oppName = gameSettings.p2name;
        } else {
            localName = gameSettings.p2name;
            oppName = gameSettings.p1name;
        }

        localBlock.transform.Find("t_name").GetComponent<Text>().text = localName;
        oppBlock.transform.Find("t_name").GetComponent<Text>().text = oppName;
	}

    public void OnLocked() { StartCoroutine(ThisPlayerLocked()); }
    private IEnumerator ThisPlayerLocked() {
        // store chosen char and loadout
        gameSettings.SetLocalChar(myID, localChar);

        // disable button & change background to green
        bReady.interactable = false;
        thisPlayerLocked = true;
        Image i = localBlock.transform.Find("i_background").GetComponent<Image>();
        i.color = Color.green;

        // send lock
        PhotonView view = PhotonView.Get(this);
        view.RPC("OpponentLocked", PhotonTargets.Others);

        // then wait for other player to lock in also
        yield return new WaitUntil(() => otherPlayerLocked);
        Debug.Log("Moving forward!!!");

        // then sync chosen character and loadout
        //gameSettings.SyncChar(myID);

        //yield return new WaitUntil(() => otherCharacterSet);
        // then load Game Screen
        yield return new WaitForSeconds(1f);

        Destroy(gameObject.GetComponent<PhotonView>());

        if (PhotonNetwork.isMasterClient) {
            PhotonNetwork.LoadLevel("Game Screen (Landscape)");
        }
    }

    [PunRPC]
    public void OpponentLocked() {
        oppBlock.transform.Find("i_background").GetComponent<Image>().color = Color.green;
        otherPlayerLocked = true;
    }

    public void OnChooseEnfuego() {
        if(!thisPlayerLocked)
            LocalCharacterChosen(Character.Ch.Enfuego);
    }

    public void OnChooseGravekeeper() {
        if(!thisPlayerLocked)
            LocalCharacterChosen(Character.Ch.Gravekeeper);
    }

    void LocalCharacterChosen(Character.Ch ch) {
        Debug.Log("CharacterSelect: local char chosen is " + ch);
        localChar = ch;
        localCharName.text = "" + ch;
        localCharPortrait.sprite = GetCharacterPortrait(ch);
        string info = CharacterInfo.GetCharacterInfo(ch);
        charT.text = info;
        bReady.enabled = true;

        PhotonView view = PhotonView.Get(this);
        view.RPC("OpponentCharacterChosen", PhotonTargets.Others, ch);
    }

    [PunRPC]
    public void OpponentCharacterChosen(Character.Ch ch) {
        Debug.Log("CharacterSelect: opponent char chosen is " + ch);
        oppCharPortrait.sprite = GetCharacterPortrait(ch);
        oppCharName.text = "" + ch;
        gameSettings.SetOpponentChar(myID, ch);
        //otherCharacterSet = true;
    }

    Sprite GetCharacterPortrait(Character.Ch ch) {
        switch (ch) {
            case Character.Ch.Enfuego:
                return Resources.Load<Sprite>("sprites/characters/enfuego");
            case Character.Ch.Gravekeeper:
                return Resources.Load<Sprite>("sprites/characters/gravekeeper");
            default:
                return null;
        }
    }
}
