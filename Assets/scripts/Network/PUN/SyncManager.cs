using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using MMDebug;

public class SyncManager : PunBehaviour {

    private MageMatch _mm;
    private Queue<int> _rands;
    private bool _checkpoint = false;

    public void Init(MageMatch mm) {
        _rands = new Queue<int>();
        _mm = mm;
        _mm.AddEventContLoadEvent(OnEventContLoaded);
    }

    public void OnEventContLoaded() {
        _mm.eventCont.AddDrawEvent(OnDrawLocal, EventController.Type.Network, EventController.Status.Begin);
        _mm.eventCont.AddDropEvent(OnDropLocal, EventController.Type.Network, EventController.Status.Begin);
        _mm.eventCont.AddSwapEvent(OnSwapLocal, EventController.Type.Network, EventController.Status.Begin);
        //eventCont.AddDiscardEvent(OnDiscardLocal, EventController.Type.Network);
        //eventCont.commishDrop += OnCommishDrop;
        //eventCont.commishTurnDone += OnCommishTurnDone;
        //eventCont.playerHealthChange += OnPlayerHealthChange;
        //eventCont.spellCast += OnSpellCast;
    }

    //private bool _syncedSeed = false;

    //public IEnumerator SyncRandomSeed(int seed) {
    //    PhotonView photonView = PhotonView.Get(this);
    //    if (PhotonNetwork.player.ID == 1) { // send/enqueue
    //        photonView.RPC("HandleSyncRandomSeed", PhotonTargets.Others, seed);

    //        MMLog.Log_SyncMan("seed = " + seed);
    //        Random.InitState(seed);
    //    } else {             // wait for rands in queue
    //        MMLog.Log_SyncMan("old seed = " + seed);
    //        yield return new WaitUntil(() => _syncedSeed);
    //    }
    //    yield return null;
    //}
    //[PunRPC]
    //public void HandleSyncRandomSeed(int seed) {
    //    Random.InitState(seed);
    //    MMLog.Log_SyncMan("new seed = " + seed);
    //    _syncedSeed = true;
    //}


    public IEnumerator SyncRand(int id, int value) {
        yield return SyncRands(id, new int[] { value });
    }

    public IEnumerator SyncRands(int id, int[] values) {
        if (_mm.IsReplayMode()) {
            int[] replayRands = _mm.replay.GetSyncedRands();
            foreach (int r in replayRands)
                _rands.Enqueue(r);
        } else if (_mm.IsDebugMode()) {
            HandleSyncRands(values);
        } else {
            PhotonView photonView = PhotonView.Get(this);
            if (id == _mm.myID) { // send/enqueue
                photonView.RPC("HandleSyncRands", PhotonTargets.All, values);
            } else {             // wait for rands in queue
                MMLog.Log_SyncMan("Wating for " + values.Length + " values...");
                yield return new WaitUntil(() => _rands.Count >= values.Length); // will the amount always be the same??
            }
        }
        yield return null;
    }
    [PunRPC]
    public void HandleSyncRands(int[] values) {
        for (int i = 0; i < values.Length; i++) {
            //Debug.MMLog.Log_SyncMan("SYNCMANAGER: Just synced random["+i+"]=" + values[i]);
            _rands.Enqueue(values[i]);
        }
    }

    public int GetRand() {
        return GetRands(1)[0];
    }

    public int[] GetRands(int count) {
        int[] r = new int[count];
        string str = "";
        for (int i = 0; i < count; i++) {
            r[i] = _rands.Dequeue();
            str += r[i].ToString() + " ";
            //Debug.MMLog.Log_SyncMan("SYNCMANAGER: Just read random["+i+"]=" + r[i]);
        }
        _mm.stats.Report("$ SYNC (x" + count + ") " + str, false);
        return r;
    }

    // i don't think this works
    public IEnumerator Checkpoint() {
        if (_mm.IsDebugMode())
            yield break;

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("HandleCheckpoint", PhotonTargets.Others, true);

        yield return new WaitUntil(() => _checkpoint);
        _checkpoint = false;
    }
    [PunRPC]
    public void HandleCheckpoint(bool checkpoint) {
        MMLog.Log_SyncMan("Received checkpoint");
        this._checkpoint = checkpoint;
    }

    public void CheckHandContents(int id) {
        if (_mm.IsDebugMode() || id != _mm.myID)
            return;

        List<string> tags = _mm.GetPlayer(id).hand.Debug_GetAllTags();

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("HandleCheckHandContents", PhotonTargets.Others, id, tags.ToArray());
    }
    [PunRPC]
    public void HandleCheckHandContents(int id, string[] theirTagArray) {
        MMLog.Log_SyncMan("About to check player"+id+"'s hand...");
        List<string> theirTags = new List<string>(theirTagArray);

        bool match = _mm.GetPlayer(id).hand.Debug_CheckTags(theirTags);
        if (!match) {
            List<string> myTags = _mm.GetPlayer(id).hand.Debug_GetAllTags();
            MMLog.LogError("Hand desync!!\nmine =" + myTags.ToString() + 
                "\ntheirs=" + theirTags.ToString());
        }
    }


    public IEnumerator OnDrawLocal(int id, string tag, bool playerAction, bool dealt) {
        //Debug.MMLog.Log_SyncMan("TURNMANAGER: id=" + id + " myID=" + mm.myID);
        //if ((playerAction || dealt) && id == _mm.myID) { // if local, send to remote
        if (_mm.IsDebugMode())
            yield break;

        if (playerAction && id == _mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleDraw", PhotonTargets.Others, id, tag, playerAction, dealt);
        }
        yield return null;
    }
    [PunRPC]
    public void HandleDraw(int id, string tag, bool playerAction, bool dealt) {
        //MMLog.Log_SyncMan("Received draw with tag=" + tag);
        //if (dealt)
        //    StartCoroutine(_mm.GetPlayer(id).DealTile());
        //else
            //StartCoroutine(_mm._Draw(id, "", playerAction));
            _mm.PlayerDrawHex();
    }

    public IEnumerator OnDropLocal(int id, bool playerAction, string tag, int col) {
        if (_mm.IsDebugMode())
            yield break;

        if (playerAction && id == _mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleDrop", PhotonTargets.Others, id, tag, col);
        }
        yield return null;
    }
    [PunRPC]
    public void HandleDrop(int id, string hextag, int col) {
        Hex hex = _mm.GetPlayer(id).hand.GetHex(hextag);
        _mm.PlayerDropTile(hex, col);
    }

    public IEnumerator OnSwapLocal(int id, bool playerAction, int c1, int r1, int c2, int r2) {
        if (_mm.IsDebugMode())
            yield break;

        if (playerAction && id == _mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleSwap", PhotonTargets.Others, id, c1, r1, c2, r2);
        }
        yield return null;
    }
    [PunRPC]
    public void HandleSwap(int id, int c1, int r1, int c2, int r2) {
        _mm.PlayerSwapTiles(c1, r1, c2, r2);
    }

    public void SendSpellCast(int spellNum) {
        if (_mm.IsDebugMode())
            return;

        if (_mm.MyTurn()) { // not totally necessary...
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleSpellCast", PhotonTargets.Others, spellNum);
        }
    }
    [PunRPC]
    public void HandleSpellCast(int spellNum) {
        StartCoroutine(_mm._CastSpell(spellNum));
    }

    public void TurnTimeout() {
        if (_mm.IsDebugMode())
            return;

        if (_mm.MyTurn()) { // not totally necessary...
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleTimeout", PhotonTargets.Others);
        }
    }
    [PunRPC]
    public void HandleTimeout() {
        _mm.TurnTimeout();
    }

    // --------------- Targeting ----------------

    public void SendTBTarget(TileBehav tb) {
        if (_mm.IsDebugMode())
            return;

        if (_mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleTBTarget", PhotonTargets.Others, tb.tile.col, tb.tile.row);
        }
    }
    [PunRPC]
    public void HandleTBTarget(int col, int row) {
        _mm.targeting.OnTBTarget(_mm.hexGrid.GetTileBehavAt(col, row));
    }

    public void SendCBTarget(CellBehav cb) {
        if (_mm.IsDebugMode())
            return;

        if (_mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleCBTarget", PhotonTargets.Others, cb.col, cb.row);
        }
    }
    [PunRPC]
    public void HandleCBTarget(int col, int row) {
        _mm.targeting.OnCBTarget(_mm.hexGrid.GetCellBehavAt(col, row));
    }

    // TODO merge into SendTargetingMessage
    //public void SendClearTargets() {
    //    if (mm.MyTurn()) {
    //        PhotonView photonView = PhotonView.Get(this);
    //        photonView.RPC("HandleClearTargets", PhotonTargets.Others);
    //    }
    //}
    //[PunRPC]
    //public void HandleClearTargets() {
    //    mm.targeting.ClearTargets();
    //}

    //public void SendCancelTargeting() {
    //    if (mm.MyTurn()) {
    //        PhotonView photonView = PhotonView.Get(this);
    //        photonView.RPC("HandleCancelTargeting", PhotonTargets.Others);
    //    }
    //}
    //[PunRPC]
    //public void HandleCancelTargeting() {
    //    mm.targeting.CancelTargeting();
    //}

    public void SendTBSelection(TileBehav tb) {
        if (_mm.IsDebugMode())
            return;

        if (_mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleTBSelection", PhotonTargets.Others, tb.tile.col, tb.tile.row);
        }
    }
    [PunRPC]
    public void HandleTBSelection(int col, int row) {
        _mm.targeting.OnSelection(_mm.hexGrid.GetTileBehavAt(col, row));
    }

    public void SendCancelSelection() {
        if (_mm.IsDebugMode())
            return;

        if (_mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleCancelSelection", PhotonTargets.Others);
        }
    }
    [PunRPC]
    public void HandleCancelSelection() {
        _mm.targeting.CancelSelection();
    }

    public void SendEndDragTarget() {
        if (_mm.IsDebugMode())
            return;

        if (_mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleEndDragTarget", PhotonTargets.Others);
        }
    }
    [PunRPC]
    public void HandleEndDragTarget() {
        _mm.targeting.EndDragTarget();
    }

    public void SendDropSelection(Hex hex, int col) {
        if (_mm.IsDebugMode())
            return;

        if (_mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleDropSelection", PhotonTargets.Others, hex.hextag, col);
        }
    }
    [PunRPC]
    public void HandleDropSelection(string tag, int col) {
        Prompt.SetDrop(_mm.ActiveP().hand.GetHex(tag), col);
    }

    public void SendSwapSelection(int c1, int r1, int c2, int r2) {
        if (_mm.IsDebugMode())
            return;

        if (_mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleSwapSelection", PhotonTargets.Others, c1, r1, c2, r2);
        }
    }
    [PunRPC]
    public void HandleSwapSelection(int c1, int r1, int c2, int r2) {
        Prompt.SetSwaps(c1, r1, c2, r2);
    }

    public void SendKeepQuickdraw() {
        if (_mm.IsDebugMode())
            return;

        if (_mm.MyTurn()) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleKeepQuickdraw", PhotonTargets.Others);
        }
    }
    [PunRPC]
    public void HandleKeepQuickdraw() {
        Prompt.SetQuickdrawHand();
    }
}
