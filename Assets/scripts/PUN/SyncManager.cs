using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class SyncManager : PunBehaviour {

    private MageMatch mm;

    // Use this for initialization
    void Start() {
        //turnManager = gameObject.AddComponent<PunTurnManager>();
        //turnManager.TurnManagerListener = this;
        //turnManager.TurnDuration = 20f; // TODO use TurnTimer instead?
    }

    // Update is called once per frame
    void Update() {

    }

    public void InitEvents(MageMatch mm, EventController eventCont) {
        this.mm = mm;
        eventCont.draw += OnDrawLocal;
        eventCont.drop += OnDropLocal;
        //eventCont.swap += OnSwapLocal;
        eventCont.AddSwapEvent(OnSwapLocal, 5);
        eventCont.commishDrop += OnCommishDrop;
        eventCont.commishTurnDone += OnCommishTurnDone;
        eventCont.playerHealthChange += OnPlayerHealthChange;
        eventCont.spellCast += OnSpellCast;
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

    public void OnPlayerHealthChange(int id, int amount, bool dealt, bool sent) {
        if (!sent) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandlePlayerHealthChange", PhotonTargets.Others, id, amount, dealt);
        }
    }

    [PunRPC]
    public void HandlePlayerHealthChange(int id, int amount, bool dealt) {
        //Debug.Log("TURNMANAGER: HandlePlayerHealthChange; id=" + id + " amount=" + amount + " dealt=" + dealt);
        mm.GetPlayer(id).ChangeHealth(amount, dealt, true);
    }

    public void OnSpellCast(int id, Spell spell) {
        if (mm.MyTurn()) { // not totally necessary...
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("HandleSpellCast", PhotonTargets.Others, spell.index);
        }
    }

    [PunRPC]
    public void HandleSpellCast(int spellNum) {
        mm.CastSpell(spellNum);
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

    public void StartHotBody(int id) {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("R_StartHotBody", PhotonTargets.All, id);
    }

    [PunRPC]
    public void R_StartHotBody(int id) {
        ((Enfuego)mm.GetPlayer(id).character).hotBody_selects = 1;
    }

    public void SendHotBodySelect(int id, int col, int row) {
        if (mm.MyTurn()) { //?
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("R_HotBodySelect", PhotonTargets.Others, id, col, row);
        }
    }

    [PunRPC]
    public void R_HotBodySelect(int id, int col, int row) {
        ((Enfuego)mm.GetPlayer(id).character).SetHotBodySelect(col, row);
    }

    public void StartZombify(int id) {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("R_StartZombify", PhotonTargets.All, id);
    }

    [PunRPC]
    public void R_StartZombify(int id) {
        ((Gravekeeper)mm.GetPlayer(id).character).zombify_select = true;
    }


    public void SendZombifySelect(int id, int col, int row) {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("R_ZombifySelect", PhotonTargets.Others, id, col, row);
    }


    [PunRPC]
    public void R_ZombifySelect(int id, int col, int row) {
        ((Gravekeeper)mm.GetPlayer(id).character).SetZombifySelect(col, row);
    }
}
