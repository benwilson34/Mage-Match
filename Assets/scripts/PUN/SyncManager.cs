using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class SyncManager : PunBehaviour {

    private MageMatch mm;

    // Use this for initialization
    void Start() {
        rands = new Queue<int>();
    }

    // Update is called once per frame
    void Update() {
    }

    public void InitEvents(MageMatch mm, EventController eventCont) {
        this.mm = mm;
        eventCont.draw += OnDrawLocal;
        eventCont.AddDropEvent(OnDropLocal, 5);
        eventCont.AddSwapEvent(OnSwapLocal, 5);
        //eventCont.commishDrop += OnCommishDrop;
        //eventCont.commishTurnDone += OnCommishTurnDone;
        //eventCont.playerHealthChange += OnPlayerHealthChange;
        //eventCont.spellCast += OnSpellCast;
    }

    public int rand = 0;

    // TODO more responsive!!!
    public IEnumerator SyncRand(int id, int value, string debugName = "") {
        PhotonView photonView = PhotonView.Get(this);
        if (id == mm.myID) { // send
            yield return new WaitUntil(() => rand == -1);
            photonView.RPC("HandleSyncRandom", PhotonTargets.All, value);
        } else {             // receive
            photonView.RPC("HandleSyncRandom", PhotonTargets.All, -1);
            yield return new WaitUntil(() => rand != -1);
        }
        Debug.Log("SYNCMANAGER: Just synced a random, "+debugName+"=" + rand);
    }
    [PunRPC]
    public void HandleSyncRandom(int value) {
        rand = value;
    }


    private Queue<int> rands;
    private bool affectingQueue = false; // will I need this?

    public IEnumerator SyncRands(int id, int[] values) {
        PhotonView photonView = PhotonView.Get(this);
        if (id == mm.myID) { // send/enqueue
            photonView.RPC("HandleSyncRands", PhotonTargets.All, values);
        } else {             // wait for rands in queue
            Debug.Log("SYNCMANAGER: Wating for " + values.Length + " values...");
            yield return new WaitUntil(() => rands.Count >= values.Length); // will the amount always be the same??
        }
        yield return null;
    }
    [PunRPC]
    public void HandleSyncRands(int[] values) {
        for (int i = 0; i < values.Length; i++) {
            Debug.Log("SYNCMANAGER: Just synced random["+i+"]=" + values[i]);
            rands.Enqueue(values[i]);
        }
    }

    public int[] GetRands(int count) {
        int[] r = new int[count];
        for (int i = 0; i < count; i++) {
            r[i] = rands.Dequeue();
            Debug.Log("SYNCMANAGER: Just read random["+i+"]=" + r[i]);
        }
        return r;
    }

    public void OnDrawLocal(int id, Tile.Element elem, bool dealt) {
        //Debug.Log("TURNMANAGER: id=" + id + " myID=" + mm.myID);
        if (id == mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleDraw", PhotonTargets.Others, id, elem, dealt);
        }
    }
    [PunRPC]
    public void HandleDraw(int id, Tile.Element elem, bool dealt) {
        mm.GetPlayer(id).DrawTiles(1, elem, dealt, false);
    }

    public IEnumerator OnDropLocal(int id, Tile.Element elem, int col) {
        if (id == mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleDrop", PhotonTargets.Others, id, elem, col);
        }
        yield return null;
    }
    [PunRPC]
    public void HandleDrop(int id, Tile.Element elem, int col) {
        if (mm.currentState == MageMatch.GameState.CommishTurn) { // TODO hacky - change event
            return;
        }
        GameObject go = mm.ActiveP().GetTileFromHand(elem);
        mm.DropTile(col, go);
    }

    public IEnumerator OnSwapLocal(int id, int c1, int r1, int c2, int r2) {
        if (id == mm.myID) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleSwap", PhotonTargets.Others, id, c1, r1, c2, r2);
        }
        yield return null;
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

    // eventually should be an event?
    public void CommishTurnStart() {
        if (!mm.MyTurn()) { // not totally necessary...
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleCommishTurnStart", PhotonTargets.Others);
        }
    }
    [PunRPC]
    public void HandleCommishTurnStart() {
        mm.commishTurn = true;
    }

    public void OnCommishTurnDone() {
        if (mm.MyTurn()) { // if local, send to remote
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleCommishTurnDone", PhotonTargets.Others);
        }
    }
    [PunRPC]
    public void HandleCommishTurnDone() {
        Debug.Log("TURNMANAGER: CommishTurnDone!");
        mm.commishTurn = false;
    }

    //public void OnPlayerHealthChange(int id, int amount, bool dealt, bool sent) {
    //    if (!sent) {
    //        PhotonView photonView = PhotonView.Get(this);
    //        photonView.RPC("HandlePlayerHealthChange", PhotonTargets.Others, id, amount, dealt);
    //    }
    //}
    //[PunRPC]
    //public void HandlePlayerHealthChange(int id, int amount, bool dealt) {
    //    //Debug.Log("TURNMANAGER: HandlePlayerHealthChange; id=" + id + " amount=" + amount + " dealt=" + dealt);
    //    mm.GetPlayer(id).ChangeHealth(amount, dealt, true);
    //}

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

    //public void SendHotBodyTarget(int col, int row) {
    //    if (mm.MyTurn()) { // not necessary
    //        PhotonView photonView = PhotonView.Get(this);
    //        photonView.RPC("HandleCBTarget", PhotonTargets.Others, cb.col, cb.row);
    //    }
    //}
    //[PunRPC]
    //public void HandleHotBodyTarget(int col, int row) {
    //    mm.targeting.OnCBTarget(mm.hexGrid.GetCellBehavAt(col, row));
    //}

    // --------------- spells ------------------

    //public void StartHotBody(int id) {
    //    PhotonView photonView = PhotonView.Get(this);
    //    photonView.RPC("R_StartHotBody", PhotonTargets.All, id);
    //}
    //[PunRPC]
    //public void R_StartHotBody(int id) {
    //    ((Enfuego)mm.GetPlayer(id).character).hotBody_selects = 1;
    //}

    //public void SendHotBodySelect(int id, int col, int row) {
    //    if (mm.MyTurn()) { //?
    //        PhotonView photonView = PhotonView.Get(this);
    //        photonView.RPC("R_HotBodySelect", PhotonTargets.Others, id, col, row);
    //    }
    //}
    //[PunRPC]
    //public void R_HotBodySelect(int id, int col, int row) {
    //    ((Enfuego)mm.GetPlayer(id).character).SetHotBodySelect(col, row);
    //}

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
}
