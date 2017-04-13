using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimationController : MonoBehaviour {

    public AnimationCurve gravEase;

    private MageMatch mm;
    private int animating;

	// Use this for initialization
	void Start () {
        mm = GetComponent<MageMatch>();
	}
	
	// Update is called once per frame
	//void Update () {}

    public void PlayAnim(IEnumerator anim) {
        StartCoroutine(anim);
    }

    public bool IsAnimating() {
        return animating > 0;
    }

    public IEnumerator _RemoveTile(TileBehav tb) {
        animating++;
        Tween swellTween = tb.transform.DOScale(new Vector3(1.25f, 1.25f), .15f);
        tb.GetComponent<SpriteRenderer>().DOColor(new Color(0, 1, 0, 0), .15f);
        //Camera.main.DOShakePosition(.1f, 1.5f, 20, 90, false); // heh
        mm.audioCont.BreakSound();

        yield return swellTween.WaitForCompletion();
        animating--;
    }

    public IEnumerator _MoveTile(TileBehav tb, int startrow, float duration) {
        animating++;
        int col = tb.tile.col, row = startrow;
        Vector3 newPos = new Vector3(mm.hexGrid.GridColToPos(col), mm.hexGrid.GridRowToPos(col, row)); //?
        Tween moveTween = tb.transform.DOMove(newPos, duration, false);

        yield return moveTween.WaitForCompletion();
        //Debug.Log("ANIMCONT: Tile moved, about to do grav. dur=" + duration);
        yield return _Grav(tb.transform, col, tb.tile.row);

        animating--;
    }

    IEnumerator _Grav(Transform trans, int col, int row) {
        Vector2 newPos = mm.hexGrid.GridCoordToPos(col, row);
        float height = trans.position.y - newPos.y;
        Tween tween = trans.DOMove(newPos, .08f * height, false);
        //tween.SetEase (Ease.InQuad);
        tween.SetEase(gravEase);

        yield return tween.WaitForCompletion();
        //		Debug.Log (transform.name + " is in position: (" + tile.col + ", " + tile.row + ")");
    }

    public IEnumerator _AlignHand(Player p, float dur, bool linear) {
        TileBehav tb;
        Tween tween;
        Vector3 handPos = p.handSlot.position, tilePos;
        for (int i = 0; i < p.hand.Count; i++) {
            tb = p.hand[i];
            //			Debug.Log ("AlignHand hand[" + i + "] = " + tb.transform.name + ", position is (" + handSlot.position.x + ", " + handSlot.position.y + ")");
            if (p.id == 1) {
                if (i < 2)
                    tilePos = new Vector3(handPos.x - i - .5f, handPos.y + HexGrid.horiz);
                else if (i < 5)
                    tilePos = new Vector3(handPos.x - (i-2), handPos.y);
                else
                    tilePos = new Vector3(handPos.x - (i-5) - .5f, handPos.y - HexGrid.horiz);
            } else {
                if (i < 2)
                    tilePos = new Vector3(handPos.x + i + .5f, handPos.y + HexGrid.horiz);
                else if (i < 5)
                    tilePos = new Vector3(handPos.x + (i-2), handPos.y);
                else
                    tilePos = new Vector3(handPos.x + (i-5) + .5f, handPos.y - HexGrid.horiz);
            }

            tween = tb.transform.DOMove(tilePos, dur, false);
            if (linear || i == p.hand.Count - 1)
                yield return tween.WaitForCompletion();
        }
    }


    public IEnumerator _Burning(TileBehav tb) {
        GameObject fireballPF = (GameObject)Resources.Load("prefabs/anim/fireball"); // field
        Transform spawn = GameObject.Find("tileSpawn").transform;
        GameObject fb = Instantiate(fireballPF, spawn);

        Tween t = fb.transform.DOMove(tb.transform.position, 1.2f);
        t.SetEase(Ease.InQuad);
        yield return t.WaitForCompletion();

        t = fb.GetComponent<SpriteRenderer>().DOColor(new Color(1, 0, 0, 0), .05f);
        yield return t.WaitForCompletion();
        Destroy(fb);
        Debug.Log("ANIMCONT: Done animating Burning.");
        //yield return null; //?
    }

    public IEnumerator _Burning_Turn(Player p, TileBehav tb) {
        GameObject fireballPF = (GameObject)Resources.Load("prefabs/anim/fireball"); // field
        GameObject fb = Instantiate(fireballPF, tb.transform);

        Vector3 dmgSpot = Camera.main.ScreenToWorldPoint(mm.uiCont.GetPinfo(p.id).position);
        Tween t = fb.transform.DOMove(dmgSpot, .4f);
        t.SetEase(Ease.InQuart);
        yield return t.WaitForCompletion();

        //t = fb.GetComponent<SpriteRenderer>().DOColor(new Color(1, 0, 0, 0), .05f);
        //yield return t.WaitForCompletion();
        Destroy(fb);
        Debug.Log("ANIMCONT: Done animating Burning_Turn.");
    }

    Vector3 zomb_origPos;

    public IEnumerator _Zombify_Attack(Transform zomb, Transform target) {
        zomb_origPos = zomb.position;
        Vector3 bite = Vector3.Lerp(zomb_origPos, target.position, 0.75f);
        Tween t = zomb.DOMove(bite, .03f);
        t.SetEase(Ease.InQuad);
        yield return t.WaitForCompletion();
    }

    public IEnumerator _Zombify_Back(Transform zomb) {
        Tween t = zomb.DOMove(zomb_origPos, .13f);
        t.SetEase(Ease.OutQuad);
        yield return t.WaitForCompletion();
    }
}
