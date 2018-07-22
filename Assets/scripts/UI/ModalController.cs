using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ModalController : MonoBehaviour {

    public enum ModalMode { None, Select, Drag };
    public static ModalMode mode = ModalMode.Drag;

    private static MageMatch _mm;
    private static GameObject _modal;
    private static Transform _slotLayout;
    private static List<Transform> _slots;

    private static GameObject _modalSlotPF;

	// Use this for initialization
	void Start () {
        _modal = transform.Find("Modal").gameObject;

        _slotLayout = _modal.transform.Find("Slots");
        _slots = new List<Transform>();
        foreach (Transform slot in _slotLayout)
            //Destroy(t.gameObject);
            _slots.Add(slot);
    }

    public static void Init(MageMatch mm) {
        _mm = mm;

        _modalSlotPF = Resources.Load("prefabs/ui/ModalSlot") as GameObject;
    }

    public static IEnumerator ShowModal(int id, string title, string desc) {
        //var modal = Resources.Load("prefabs/ui/Modal") as GameObject;
        //modal = Instantiate(modal, modalUI.transform);

        var trans = (RectTransform)_modal.transform;
        var offset = trans.rect.width / 2;
        // change title and description texts
        trans.Find("t_title").GetComponent<Text>().text = title;
        trans.Find("t_desc").GetComponent<Text>().text = desc;

        // place on correct side
        var startPos = new Vector3(0, 0);
        var endPos = new Vector3(0, 0);
        if (_mm.uiCont.IDtoSide(id) == UIController.ScreenSide.Left) {
            endPos.x = 0 + offset;
            startPos.x = endPos.x - (2 * offset);
        } else {
            endPos.x = Screen.width - offset;
            startPos.x = endPos.x + (2 * offset);
        }
        endPos = Camera.main.ScreenToWorldPoint(endPos);
        endPos.z = 0;
        startPos = Camera.main.ScreenToWorldPoint(startPos);
        startPos.z = 0;

        trans.position = startPos;
        _modal.SetActive(true);

        const float modalDur = .08f;
        yield return trans.DOMove(endPos, modalDur).WaitForCompletion();

        // TODO how to generically move hexes onto the modal?
        // ModalController?

        yield return null;
    }

    public static IEnumerator AddHexes(List<Hex> hexes) {
        FillSlotListToCount(hexes.Count);
        yield return new WaitForEndOfFrame(); // needed to redraw layout

        for (int i = 0; i < hexes.Count; i++) {
            var hex = hexes[i];
            hex.state = Hex.State.ModalChoice;
            //var slot = Instantiate(_modalSlotPF, _slotLayout);
            hex.transform.DOMove(_slots[i].transform.position, .04f);
        }
        yield return null;
    }


    public static void FillSlotListToCount(int count) {
    //public static Transform GetSlot(int i) {
        while (_slots.Count < count) {
            var slot = Instantiate(_modalSlotPF, _slotLayout);
            _slots.Add(slot.transform);
        }
        //return _slots[i];
    }

    public static IEnumerator HideModal() {
        // TODO animate
        _modal.SetActive(false);
        //foreach (Transform t in _slotLayout)
        //    Destroy(t.gameObject);
        yield return null;
    }
}
