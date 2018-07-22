using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FutureSight : Charm {

    public override IEnumerator DropEffect() {
        AudioController.Trigger(SFX.Rune_Neutral.FutureSight);

        _mm.uiCont.ShowAlertText("That charm doesn't do anything right now. Sorry!");
        // TODO need an interface and all that
        var hexes = new List<Hex>();
        const int drawCount = 3;
        for (int i = 0; i < drawCount; i++) {
            string nextHextag = ThisPlayer.Deck.GetNextHextag();
            Hex hex = HexManager.GenerateHex(PlayerId, nextHextag);
            //hex.currentState = State.Hand;
            hexes.Add(hex);
        }

        const string title = "Future Sight";
        const string desc = "Drag one hex to your hand; the other two will be discarded";
        yield return Prompt.WaitForModalDrop(hexes, title, desc);
        if (!Prompt.WasSuccessful || 
            Prompt.DropModalResult != Prompt.ModalResult.ChoseHand) {
            Debug.LogError("FutureSight: Prompt wasn't successful. Something is very wrong");
            yield break;
        }

        var hextag = Prompt.GetHandHextag();
        Hex chosenHex = null;
        for (int i = 0; i < hexes.Count; i++) {
            var hex = hexes[i];
            if (hex.EqualsTag(hextag)) {
                chosenHex = hex;
                break;
            } else {
                yield return AnimationController._DiscardTile(hex.transform);
                GameObject.Destroy(hex.gameObject); //should maybe go thru TileMan
            }
        }
        if (chosenHex != null) {
            ThisPlayer.Hand.Add(chosenHex);
        }

        StartCoroutine(ModalController.HideModal());

        yield return null;
    }
}
