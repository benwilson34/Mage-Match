using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using MMDebug;

public class SyncManager : PunBehaviour {

    private MageMatch mm;
    private Queue<int> rands;
    private bool checkpoint = false;

    public void Init() {
        rands = new Queue<int>();
    }

    public void InitEvents(MageMatch mm, EventController eventCont) {
        this.mm = mm;
        eventCont.AddDrawEvent(OnDrawLocal, EventController.Type.Network);
        eventCont.AddDropEvent(OnDropLocal, EventController.Type.Network);
        eventCont.AddSwapEvent(OnSwapLocal, EventController.Type.Network);
        //eventCont.AddDiscardEvent(OnDiscardLocal, EventController.Type.Network);
        //eventCont.commishDrop += OnCommishDrop;
        //eventCont.commishTurnDone += OnCommishTurnDone;
        //eventCont.playerHealthChange += OnPlayerHealthChange;
        //eventCont.spellCast += OnSpellCast;
    }

    public IEnumerator SyncRand(int id, int value) {
        yield return SyncRands(id, new int[] { value });
    }

    public IEnumerator SyncRands(int id, int[] values) {
        PhotonView photonView = PhotonView.Get(this);
        if (id == mm.myID) { // send/enqueue
            photonView.RPC("HandleSyncRands", PhotonTargets.All, values);
        } else {             // wait for rands in queue
            MMLog.Log_SyncMan("Wating for " + values.Length + " values...");
            yield return new WaitUntil(() => rands.Count >= values.Length); // will the amount always be the same??
        }
        yield return null;
    }
    [PunRPC]
    public void HandleSyncRands(int[] values) {
        for (int i = 0; i < values.Length; i++) {
            //Debug.MMLog.Log_SyncMan("SYNCMANAGER: Just synced random["+i+"]=" + values[i]);
            rands.Enqueue(values[i]);
        }
    }

    public int GetRand() {
        return GetRands(1)[0];
    }

    public int[] GetRands(int count) {
        int[] r = new int[count];
        for (int i = 0; i < count; i++) {
            r[i] = rands.Dequeue();
            //Debug.MMLog.Log_SyncMan("SYNCMANAGER: Just read random["+i+"]=" + r[i]);
        }
        return r;
    }

    public IEnumerator Checkpoint() {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("HandleCheckpoint", PhotonTargets.Others, true);

        yield return new WaitUntil(() => checkpoint);
        checkpoint = false;
    }
    [PunRPC]
    public void HandleCheckpoint(bool checkpoint) {
        MMLog.Log_SyncMan("Received checkpoint");
        this.checkpoint = checkpoint;
    }


    public IEnumerator OnDrawLocal(int id, string tag, bool playerAction, bool dealt) {
        //Debug.MMLog.Log_SyncMan("TURNMANAGER: id=" + id + " myID=" + mm.myID);
        if ((playerAction || dealt) && id == mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleDraw", PhotonTargets.Others, id, tag, playerAction, dealt);
        }
        yield return null;
    }
    [PunRPC]
    public void HandleDraw(int id, string tag, bool playerAction, bool dealt) {
        MMLog.Log_SyncMan("Received draw with tag=" + tag);
        if (dealt)
            StartCoroutine(mm.GetPlayer(id).DealTile(tag));
        else
            StartCoroutine(mm._Draw(id, tag, playerAction));
    }

    public IEnumerator OnDropLocal(int id, bool playerAction, string tag, int col) {
        if (playerAction && id == mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleDrop", PhotonTargets.Others, id, tag, col);
        }
        yield return null;
    }
    [PunRPC]
    public void HandleDrop(int id, string tag, int col) {
        HandObject hex = mm.GetPlayer(id).hand.GetHex(tag);
        mm.PlayerDropTile(col, hex);
    }

    public IEnumerator OnSwapLocal(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        if (playerAction && id == mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleSwap", PhotonTargets.Others, id, c1, r1, c2, r2);
        }
        yield return null;
    }
    [PunRPC]
    public void HandleSwap(int id, int c1, int r1, int c2, int r2) {
        mm.PlayerSwapTiles(c1, r1, c2, r2);
    }

    public void SendSpellCast(int spellNum) {
        if (mm.MyTurn()) { // not totally necessary...
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleSpellCast", PhotonTargets.Others, spellNum);
        }
    }
    [PunRPC]
    public void HandleSpellCast(int spellNum) {
        StartCoroutine(mm.CastSpell(spellNum));
    }

    public void TurnTimeout() {
        if (mm.MyTurn()) { // not totally necessary...
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleTimeout", PhotonTargets.All);
        }
    }
    [PunRPC]
    public void HandleTimeout() {
        mm.TurnTimeout();
    }

    // --------------- Targeting ----------------

    public void SendTBTarget(TileBehav tb) {
        if (mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleTBTarget", PhotonTargets.Others, tb.tile.col, tb.tile.row);
        }
    }
    [PunRPC]
    public void HandleTBTarget(int col, int row) {
        mm.targeting.OnTBTarget(mm.hexGrid.GetTileBehavAt(col, row));
    }

    public void SendCBTarget(CellBehav cb) {
        if (mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleCBTarget", PhotonTargets.Others, cb.col, cb.row);
        }
    }
    [PunRPC]
    public void HandleCBTarget(int col, int row) {
        mm.targeting.OnCBTarget(mm.hexGrid.GetCellBehavAt(col, row));
    }

    // TODO merge into SendTargetingMessage
    public void SendClearTargets() {
        if (mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleClearTargets", PhotonTargets.Others);
        }
    }
    [PunRPC]
    public void HandleClearTargets() {
        mm.targeting.ClearTargets();
    }

    public void SendCancelTargeting() {
        if (mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleCancelTargeting", PhotonTargets.Others);
        }
    }
    [PunRPC]
    public void HandleCancelTargeting() {
        mm.targeting.CancelTargeting();
    }

    public void SendTBSelection(TileBehav tb) {
        if (mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleTBSelection", PhotonTargets.Others, tb.tile.col, tb.tile.row);
        }
    }
    [PunRPC]
    public void HandleTBSelection(int col, int row) {
        mm.targeting.OnSelection(mm.hexGrid.GetTileBehavAt(col, row));
    }

    public void SendCancelSelection() {
        if (mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleCancelSelection", PhotonTargets.Others);
        }
    }
    [PunRPC]
    public void HandleCancelSelection() {
        mm.targeting.CancelSelection();
    }

    public void SendEndDragTarget() {
        if (mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleEndDragTarget", PhotonTargets.Others);
        }
    }
    [PunRPC]
    public void HandleEndDragTarget() {
        mm.targeting.EndDragTarget();
    }
}
