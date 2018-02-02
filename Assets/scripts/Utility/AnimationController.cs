using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MMDebug;

public class AnimationController : MonoBehaviour {

    public AnimationCurve someCustomEase;

    private MageMatch mm;
    private GameObject fireballPF, zombifyPF;
    private int animating;

    // Use this for initialization
    void Start() {
        //mm = GetComponent<MageMatch>();
    }

    public void Init(MageMatch mm) {
        this.mm = mm;
        fireballPF = (GameObject)Resources.Load("prefabs/anim/fireball");
        zombifyPF = (GameObject)Resources.Load("prefabs/anim/zombify");
    }

    // Update is called once per frame
    //void Update () {}

    public void PlayAnim(IEnumerator anim) {
        StartCoroutine(anim);
    }

    public bool IsAnimating() {
        return animating > 0;
    }

    public IEnumerator _DiscardTile(Transform t) {
        Vector3 spawn = GameObject.Find("tileSpawn").transform.position;
        Tween tween = t.DOMove(spawn, .7f);
        yield return tween.WaitForCompletion();
    }

    public IEnumerator _InvokeTile(TileBehav tb) {
        animating++;
        Tween swellTween = tb.transform.DOScale(new Vector3(1.25f, 1.25f), .15f);
        tb.GetComponent<SpriteRenderer>().DOColor(new Color(0, 1, 0, 0), .15f);
        //Camera.main.DOShakePosition(.1f, 1.5f, 20, 90, false); // heh
        mm.audioCont.TileInvoke();

        yield return swellTween.WaitForCompletion();
        animating--;
    }

    public IEnumerator _DestroyTile(TileBehav tb) {
        animating++;
        float yPos = tb.transform.position.y;
        Tween tween = tb.transform.DOMoveY(yPos - 1, .25f);
        tb.GetComponent<SpriteRenderer>().DOColor(new Color(0, 1, 0, 0), .25f);
        //Camera.main.DOShakePosition(.1f, 1.5f, 20, 90, false); // heh
        mm.audioCont.TileDestroy();

        yield return tween.WaitForCompletion();
        animating--;
    }

    public IEnumerator _MoveTile(TileBehav tb, float duration) {
        animating++;
        int col = tb.tile.col, row = tb.tile.row;
        Vector3 newPos = new Vector3(mm.hexGrid.GridColToPos(col), mm.hexGrid.GridRowToPos(col, row)); //?
        MMLog.Log_MageMatch(">>>>>>>>>>>>>>>>>>>>>>>>>about to animate");

        yield return tb.transform.DOMove(newPos, duration).SetEase(Ease.Linear).WaitForCompletion();

        animating--;
    }

    public IEnumerator _MoveTileAndDrop(TileBehav tb, int startRow, float duration) {
        animating++;
        int col = tb.tile.col, row = startRow;
        Vector3 newPos = new Vector3(mm.hexGrid.GridColToPos(col), mm.hexGrid.GridRowToPos(col, row)); //?
        MMLog.Log_MageMatch(">>>>>>>>>>>>>>>>>>>>>>>>>about to animate");

        yield return tb.transform.DOMove(newPos, duration).SetEase(Ease.Linear).WaitForCompletion();
        MMLog.Log_AnimCont("ANIMCONT: Tile moved, about to do grav. dur=" + duration);

        if(tb.tile.row != row)
            yield return _Grav(tb.transform, col, tb.tile.row);

        animating--;
    }

    IEnumerator _Grav(Transform t, int col, int row) {
        Vector2 newPos = mm.hexGrid.GridCoordToPos(col, row);
        float height = t.position.y - newPos.y;

        yield return t.DOMove(newPos, .04f * height).SetEase(Ease.InQuad).WaitForCompletion();
        mm.audioCont.TileGravityClick(t.GetComponent<AudioSource>());

        // bounce anim
        // TODO .SetLoops(2, LoopType.Yoyo);
        yield return t.DOMoveY(newPos.y + .4f, .08f).SetEase(Ease.OutQuad).WaitForCompletion();
        yield return t.DOMoveY(newPos.y, .08f).SetEase(Ease.InQuad).WaitForCompletion();

        //MMLog.Log_AnimCont(t.name + " is in position: (" + col + ", " + row + ")");
    }

    public IEnumerator _Move(Hex hex, Vector3 newPos) {
        yield return hex.transform.DOMove(newPos, .1f).WaitForCompletion();
    }

    public IEnumerator _Burning(TileBehav tb) {
        // TODO spawn from Pinfo
        Transform spawn = GameObject.Find("tileSpawn").transform;
        GameObject fb = Instantiate(fireballPF, spawn);

        Tween t = fb.transform.DOMove(tb.transform.position, .6f);
        t.SetEase(Ease.InQuad);
        yield return t.WaitForCompletion();

        t = fb.GetComponent<SpriteRenderer>().DOColor(new Color(1, 0, 0, 0), .05f);
        yield return t.WaitForCompletion();
        Destroy(fb);
        MMLog.Log_AnimCont("Done animating Burning.");
        //yield return null; //?
    }

    public IEnumerator _Burning_Turn(Player p, TileBehav tb) {
        GameObject fb = Instantiate(fireballPF, tb.transform);

        Vector3 dmgSpot = Camera.main.ScreenToWorldPoint(mm.uiCont.GetPinfo(p.id).position);
        Tween t = fb.transform.DOMove(dmgSpot, .4f);
        t.SetEase(Ease.InQuart);
        yield return t.WaitForCompletion();

        //t = fb.GetComponent<SpriteRenderer>().DOColor(new Color(1, 0, 0, 0), .05f);
        //yield return t.WaitForCompletion();
        Destroy(fb);
        MMLog.Log_AnimCont("Done animating Burning_Turn.");
    }

    Vector3 zomb_origPos;

    public IEnumerator _Zombify(TileBehav tb) {
        // TODO spawn from Pinfo
        Transform spawn = GameObject.Find("tileSpawn").transform;
        GameObject z = Instantiate(zombifyPF, spawn);

        Tween t = z.transform.DOMove(tb.transform.position, .6f);
        t.SetEase(Ease.InQuad);
        yield return t.WaitForCompletion();

        t = z.GetComponent<SpriteRenderer>().DOColor(new Color(0, 1, 0, 0), .05f);
        yield return t.WaitForCompletion();
        Destroy(z);
        MMLog.Log_AnimCont("Done animating Zombify.");
    }

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

    public IEnumerator _UpwardInsert(TileBehav tb) {
        // TODO handle bottom of column
        Transform t = tb.transform;
        t.position = mm.hexGrid.GridCoordToPos(tb.tile.col, tb.tile.row - 1); //safe for bottom?
        t.localScale = new Vector3(.2f, .2f);

        t.DOMoveY(mm.hexGrid.GridRowToPos(tb.tile.col, tb.tile.row), .3f);
        yield return t.DOScale(1f, .3f).WaitForCompletion();
    }
}
