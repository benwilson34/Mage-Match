using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using System;
using ExitGames.Client.Photon;
using System.IO;

public class MyTurnManager : PunBehaviour, IPunTurnManagerCallbacks {

    private PunTurnManager turnManager; // not needed?
    private MageMatch mm;

    // Use this for initialization
    void Start() {
        turnManager = gameObject.AddComponent<PunTurnManager>();
        turnManager.TurnManagerListener = this;
        turnManager.TurnDuration = 20f; // TODO use TurnTimer instead?
    }

    // Update is called once per frame
    void Update() {

    }

    public void InitEvents(MageMatch mm, EventController eventCont) {
        this.mm = mm;
        eventCont.draw += OnDrawLocal;
        eventCont.drop += OnDropLocal;
        eventCont.swap += OnSwapLocal;
        eventCont.commishDrop += OnCommishDrop;
        eventCont.commishTurnDone += OnCommishTurnDone;
    }

    #region PunTurnManager Callbacks

    public void OnPlayerFinished(PhotonPlayer player, int turn, object move) {
        throw new NotImplementedException();
    }

    public void OnPlayerMove(PhotonPlayer player, int turn, object move) {
        //if (!player.IsLocal) {
        //    // TODO handle action - make class to do it?
        //    playMaker.HandlePlayerAction((PlayerAction)move);
        //}
        throw new NotImplementedException();
    }

    public void OnTurnBegins(int turn) {
        throw new NotImplementedException();
    }

    public void OnTurnCompleted(int turn) {
        throw new NotImplementedException();
    }

    public void OnTurnTimeEnds(int turn) {
        throw new NotImplementedException();
    }

    #endregion

    public void OnDrawLocal(int id, Tile.Element elem, bool dealt) {
        Debug.Log("TURNMANAGER: id=" + id + " myID="+mm.myID);
        if (id == mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleDraw", PhotonTargets.Others, id, elem, dealt);
        }
    }

    [PunRPC]
    public void HandleDraw(int id, Tile.Element elem, bool dealt) {
        mm.GetPlayer(id).DrawTiles(1, elem, dealt, false);
    }

    public void OnDropLocal(int id, Tile.Element elem, int col) {
        if (id == mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleDrop", PhotonTargets.Others, id, elem, col);
        }
    }

   [PunRPC]
    public void HandleDrop(int id, Tile.Element elem, int col) {
        if (mm.currentState == MageMatch.GameState.CommishTurn) { // TODO hacky - change event
            return;
        }
        GameObject go = mm.ActiveP().GetTileFromHand(elem);
        mm.DropTile(col, go);
    }

    public void OnSwapLocal(int id, int c1, int r1, int c2, int r2) {
        if (id == mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleSwap", PhotonTargets.Others, id, c1, r1, c2, r2);
        }
    }

    [PunRPC]
    public void HandleSwap(int id, int c1, int r1, int c2, int r2) {
        mm.SwapTiles(c1, r1, c2, r2);
    }

    public void OnCommishDrop(Tile.Element elem, int col) {
        if (mm.MyTurn()) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleCommishDrop", PhotonTargets.Others, elem, col);
        }
    }

    [PunRPC]
    public void HandleCommishDrop(Tile.Element elem, int col) {
        GameObject go = mm.GenerateTile(elem);
        mm.DropTile(col, go, .08f);
    }

    public void OnCommishTurnDone() {
        if (mm.MyTurn()) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleCommishTurnDone", PhotonTargets.Others);
        }
    }

    [PunRPC]
    public void HandleCommishTurnDone() {
        mm.commishTurn = false;
    }
}