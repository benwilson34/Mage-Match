using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour, Tooltipable {

    public enum Category { BasicTile, Tile, Charm };

	public enum State { ModalChoice, Hand, Placed, Removed };
    [HideInInspector]
	public State state;
    [HideInInspector]
    public int cost = 1;
    [HideInInspector]
    public bool putBackIntoDeck = false;

    [HideInInspector]
    public string hextag; // auto?
    public int PlayerId { get { return TagPlayer(hextag); } }
    public Category Cat { get { return TagCat(hextag); } }
    public string Title { get { return TagTitle(hextag); } }

    public Player ThisPlayer { get { return _mm.GetPlayer(PlayerId); } }
    public Player Opponent { get { return _mm.GetOpponent(PlayerId); } }

    protected MageMatch _mm;
    private Sprite _flipSprite;
    private bool _flipped = false;
    private bool _quickdraw = false, _duplicate = false;

    public virtual void Init(MageMatch mm) {
        _mm = mm;
        _flipSprite = HexManager.flipSprite;
        SetInitProps();
    }

    public virtual void SetInitProps() { }

    protected void SetQuickdraw() { _quickdraw = true; }
    protected void SetDuplicate() { _duplicate = true; }
    public IEnumerator OnDraw() {
        if (_mm.GetState() == MageMatch.State.BeginningOfGame ||
            _mm.GetState() == MageMatch.State.DebugMenu)
            yield break;

        if (PlayerId == _mm.ActiveP.ID) { // if this hex was generated on that player's turn
            if (_quickdraw)
                yield return HandleQuickdraw();
            if (_duplicate)
                yield return _mm._Duplicate(PlayerId, hextag);
        }
        yield return null;
    }

    IEnumerator HandleQuickdraw() {
        if (!IsCharm(hextag) && HexGrid.IsBoardFull()) {
            // can't be dropped in; quickdraw whiffs
            yield break;
        }
        Player p = _mm.ActiveP;

        //_mm.uiCont.ToggleQuickdrawUI(true, this);

        //_mm.uiCont.ShowLocalAlertText(p.ID, "Choose what to do with the Quickdraw hex!");
        AudioController.Trigger(SFX.Other.Quickdraw_Prompt);
        //_quickdrawWentToHand = false;
        //currentMode = PromptMode.Drop;

        // ignore every hex except this one
        //var ignoredHexes = new List<Hex>();
        //if (_mm.MyTurn()) {
        //    MMDebug.MMLog.Log("PROMPT", "orange", "My quickdraw tile, and my turn. " + hextag);
        //    foreach (Hex h in p.Hand.GetAllHexes()) {
        //        if (!h.EqualsTag(hextag)) {
        //            ignoredHexes.Add(h);
        //            h.Flip();
        //        }
        //    }
        //    MMDebug.MMLog.Log("PROMPT", "orange", ignoredHexes.Count + " restricted tiles in hand.");

        //    //_mm.inputCont.RestrictInteractableHexes(ignoredHexes);
        //}

        if (_mm.IsReplayMode)
            ReplayEngine.GetPrompt();

        // the InputController calls SetDrop for this too
        //_mm.inputCont.SetAllowHandRearrange(false);
        //yield return Prompt.WaitForDrop(ignoredHexes);
        const string title = "Quickdraw";
        const string desc = "You may drop this hex for free, then you will draw another. Otherwise, drag it into your hand to keep it.";

        yield return Prompt.WaitForModalDrop(new List<Hex>() { this }, title, desc);

        //_mm.inputCont.SetAllowHandRearrange(true);

        //if (_mm.MyTurn()) {
        //    foreach (Hex h in ignoredHexes)
        //        h.Flip();
        //    //_mm.inputCont.EndRestrictInteractableHexes();
        //}

        var result = Prompt.DropModalResult;
        if (result == Prompt.ModalResult.ChoseBoard) { // the player dropped it
            AudioController.Trigger(SFX.Other.Quickdraw_Drop);
            p.Hand.Remove(this);
            yield return _mm._Drop(this, Prompt.GetDropCol());
            yield return _mm._Draw(p.ID);
        } 
        //else { // send to hand
        //    //_quickdrawWentToHand = true;
        //    _mm.syncManager.SendChooseHand();
        //    //currentMode = PromptMode.None;
        //    Report.ReportLine("$ PROMPT KEEP QUICKDRAW", false);
        //}

        //_mm.uiCont.ToggleQuickdrawUI(false);

        yield return null;
    }

    public virtual IEnumerator OnDrop(int col) { yield return null; }


    #region ---------- TAG ----------

    public bool EqualsTag(string tag) { return this.hextag.Equals(tag); }

    public bool EqualsTag(Hex hex) { return this.hextag.Equals(hex.hextag); }

    // ex: p2-B-W-005 (player 2 created this Basic tile, type is Water, and it's the fifth one)
    public static int TagPlayer(string tag) { return int.Parse(tag.Substring(1, 1)); }

    public static Category TagCat(string tag) {
        string cat = tag.Split(new char[] { '-' })[1];
        switch (cat) {
            case "B": return Category.BasicTile;
            case "T": return Category.Tile;
            case "C": return Category.Charm;
            default:
                MMDebug.MMLog.LogError("HEX: Bad category name: " + cat);
                return Category.BasicTile;
        }
    }

    public static bool IsCharm(string tag) { return TagCat(tag) == Category.Charm; }

    public static string TagTitle(string tag) { return tag.Split(new char[] { '-' })[2]; }

    public static int TagNum(string tag) { return int.Parse(tag.Split(new char[] { '-' })[3]); }
    #endregion


    public void Flip() {
        Flip(!_flipped);
    }

    public void Flip(bool flipped) {
        if (_flipped == flipped)
            return;

        _flipped = flipped;
        SpriteRenderer rend = GetComponent<SpriteRenderer>();

        Sprite newSprite = _flipSprite;
        //if (!_flipped) {
        //    newSprite = HexManager.flipSprite;
        //} else {
        //    newSprite = _flipSprite;
        //}

        _flipSprite = rend.sprite;
        rend.sprite = newSprite;
    }

    public void Reveal() {
        if (_flipped)
            Flip();
    }

    public bool IsInteractable() {
        return !_flipped;
    }

    public virtual string GetTooltipInfo() {
        return GetTooltipInfo("", "hex", 0, "");
    }

    protected string GetTooltipInfo(string title, string cat, int cost, string desc) {
        string str = "<size=40>" + title + "</size>\n";
        str += "<size=25><i>" + cat + "</i>   <color=red>" + cost + " AP</color></size>\n";
        str += "<size=25><color=grey>tag: " + hextag + "</color></size>\n";
        str += desc;
        return str;
    }
}
