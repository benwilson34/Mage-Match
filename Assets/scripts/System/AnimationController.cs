using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MMDebug;

public class AnimationController : MonoBehaviour {

    public AnimationCurve someCustomEase;

    protected static MageMatch _mm;
    private static GameObject _fireballPF;
    private static int _animating;

    public static void Init(MageMatch mm) {
        _mm = mm;
        _fireballPF = (GameObject)Resources.Load("prefabs/anim/fireball");
        //_zombifyPF = (GameObject)Resources.Load("prefabs/anim/zombify");
    }

    public static void PlayAnim(IEnumerator anim) {
        _mm.StartCoroutine(anim);
    }

    public static bool IsAnimating { get { return _animating > 0; } }

    protected static IEnumerator Animate(Tween tween) {
        bool animate = true;
        if(_mm.IsReplayMode)
            animate = _mm.debugSettings.animateReplay;

        if (animate) {
            //_animating++; Do this instead?
            yield return tween.WaitForCompletion();
            //_animating--;
        } else {
            //MMLog.LogWarning("ANIM: Completing tween.");
            tween.Complete();
        }
        yield return null;
    }

    public static IEnumerator WaitForSeconds(float secs) {
        bool animate = true;
        if(_mm.IsReplayMode)
            animate = _mm.debugSettings.animateReplay;

        if (animate)
            yield return new WaitForSeconds(secs);
        else
            yield return null;
    }

    public static IEnumerator _DiscardTile(Transform t) {
        Vector3 spawn = GameObject.Find("tileSpawn").transform.position;
        Tween tween = t.DOMove(spawn, .7f);
        yield return Animate(tween);
    }

    public static IEnumerator _InvokeTile(TileBehav tb) {
        _animating++;
        //Tween swellTween = tb.transform.DOScale(new Vector3(1.25f, 1.25f), .15f);
        Tween colorTween = tb.GetComponent<SpriteRenderer>().DOColor(new Color(0, 0, 0, .4f), .15f);
        //Camera.main.DOShakePosition(.1f, 1.5f, 20, 90, false); // heh

        // should there be a sound here? most spells make sounds right as they're cast
        //AudioController.Trigger(AudioController.HexSoundEffect.Invoke);

        //yield return swellTween.WaitForCompletion();
        yield return Animate(colorTween);
        _animating--;
    }

    public static IEnumerator _InvokeTileRemove(TileBehav tb) {

        // TODO

        _animating++;
        //Tween swellTween = tb.transform.DOScale(new Vector3(1.25f, 1.25f), .15f);
        Tween colorTween = tb.GetComponent<SpriteRenderer>().DOColor(new Color(0, 0, 0, 0), .15f);
        //Camera.main.DOShakePosition(.1f, 1.5f, 20, 90, false); // heh
        AudioController.Trigger(SFX.Hex.Invoke);

        //yield return swellTween.WaitForCompletion();
        yield return Animate(colorTween);
        _animating--;
    }

    public static IEnumerator _DestroyTile(TileBehav tb) {
        _animating++;
        float yPos = tb.transform.position.y;
        Tween tween = tb.transform.DOMoveY(yPos - 1, .25f);
        tb.GetComponent<SpriteRenderer>().DOColor(new Color(0, 1, 0, 0), .25f);
        //Camera.main.DOShakePosition(.1f, 1.5f, 20, 90, false); // heh
        AudioController.Trigger(SFX.Hex.Destroy);

        yield return Animate(tween);
        _animating--;
    }

    public static IEnumerator _MoveTile(TileBehav tb, float duration) {
        _animating++;
        int col = tb.tile.col, row = tb.tile.row;
        Vector3 newPos = HexGrid.GridCoordToPos(col, row);
        MMLog.Log_MageMatch(">>>>>>>>>>>>>>>>>>>>>>>>>about to animate");

        Tween tween = tb.transform.DOMove(newPos, duration).SetEase(Ease.Linear);

        yield return Animate(tween);
        _animating--;
    }

    public static IEnumerator _MoveTileAndDrop(TileBehav tb, int startRow, float duration) {
        _animating++;
        int col = tb.tile.col, row = startRow;
        Vector3 newPos = HexGrid.GridCoordToPos(col, row);
        MMLog.Log_MageMatch(">>>>>>>>>>>>>>>>>>>>>>>>>about to animate");

        Tween tween = tb.transform.DOMove(newPos, duration).SetEase(Ease.Linear);
        yield return Animate(tween);
        MMLog.Log_AnimCont("ANIMCONT: Tile moved, about to do grav. dur=" + duration);

        if(tb.tile.row != row)
            yield return _Grav(tb.transform, col, tb.tile.row);

        _animating--;
    }

    static IEnumerator _Grav(Transform t, int col, int row) {
        Vector2 newPos = HexGrid.GridCoordToPos(col, row);
        float height = t.position.y - newPos.y;

        yield return Animate(t.DOMove(newPos, .04f * height).SetEase(Ease.InQuad));
        AudioController.Trigger(SFX.Hex.GravClick);

        // bounce anim
        // TODO .SetLoops(2, LoopType.Yoyo);
        yield return Animate(t.DOMoveY(newPos.y + .4f, .08f).SetEase(Ease.OutQuad));
        yield return Animate(t.DOMoveY(newPos.y, .08f).SetEase(Ease.InQuad));

        //MMLog.Log_AnimCont(t.name + " is in position: (" + col + ", " + row + ")");
    }

    public static IEnumerator _Draw(Hex hex) {
        var sr = hex.GetComponent<SpriteRenderer>();
        sr.color = new Color(1,1,1,0);
        PlayAnim(Animate(sr.DOFade(1, .15f)));
        hex.transform.localScale = new Vector3(2, 2, 2);
        Tween tween = hex.transform.DOScale(Vector3.one, .15f);

        //yield return tween.WaitForCompletion();
        yield return Animate(tween);
    }

    public static IEnumerator _Move(Hex hex, Vector3 newPos) {
        yield return Animate(hex.transform.DOMove(newPos, .1f));
    }

    public static IEnumerator _Burning(TileBehav tb) {
        // TODO spawn from Pinfo
        Transform spawn = GameObject.Find("tileSpawn").transform;
        GameObject fb = Instantiate(_fireballPF, spawn);

        Tween tween = fb.transform.DOMove(tb.transform.position, .6f).SetEase(Ease.InQuad);
        yield return Animate(tween);

        tween = fb.GetComponent<SpriteRenderer>().DOColor(new Color(1, 0, 0, 0), .05f);
        yield return Animate(tween);
        Destroy(fb);
        MMLog.Log_AnimCont("Done animating Burning.");
        //yield return null; //?
    }

    public static IEnumerator _Burning_Turn(Player p, TileBehav tb) {
        GameObject fb = Instantiate(_fireballPF, tb.transform);

        Vector3 dmgSpot = _mm.uiCont.GetPinfo(p.ID).position;
        Tween tween = fb.transform.DOMove(dmgSpot, .4f);
        tween.SetEase(Ease.InQuart);
        yield return Animate(tween);

        //t = fb.GetComponent<SpriteRenderer>().DOColor(new Color(1, 0, 0, 0), .05f);
        //yield return t.WaitForCompletion();
        Destroy(fb);
        MMLog.Log_AnimCont("Done animating Burning_Turn.");
    }

    private static Vector3 _zomb_origPos;

    public static IEnumerator _Zombify(TileBehav tb) {
        yield return Animate(ShakeTransform(tb.transform));
    }

    public static IEnumerator _Zombify_Attack(Transform zomb, Transform target) {
        _zomb_origPos = zomb.position;
        Vector3 bite = Vector3.Lerp(_zomb_origPos, target.position, 0.75f);
        Tween tween = zomb.DOMove(bite, .03f);
        tween.SetEase(Ease.InQuad);
        yield return Animate(tween);
    }

    public static IEnumerator _Zombify_Back(Transform zomb) {
        Tween tween = zomb.DOMove(_zomb_origPos, .13f);
        tween.SetEase(Ease.OutQuad);
        yield return Animate(tween);
    }

    public static IEnumerator _UpwardInsert(TileBehav tb) {
        // TODO handle bottom of column
        Transform t = tb.transform;
        t.position = HexGrid.GridCoordToPos(tb.tile.col, tb.tile.row - 1); //safe for bottom?
        t.localScale = new Vector3(.2f, .2f);

        t.DOMoveY(HexGrid.GridRowToPos(tb.tile.col, tb.tile.row), .3f);
        yield return Animate(t.DOScale(1f, .3f));
    }


    // ----- GENERAL CONTROLS -----

    protected static void RotateToFace(Transform t, Vector3 point) {
        t.right = point - t.position;
    }

    protected static Tween ShakeTransform(Transform t) {
        Sequence shakeSeq = DOTween.Sequence();
        const float shakeDur = .02f;
        const float shakeDist = .1f;
        const int loops = 5;
        //const float speedRampDur = 1f;
        float origX = t.position.x;
        //Tween timeTween = DOTween.To(() => shakeSeq.timeScale, (x) => shakeSeq.timeScale = x,
        //    2, speedRampDur);
        shakeSeq.Append(t.DOMoveX(origX + shakeDist, shakeDur).SetEase(Ease.Linear));
        //shakeSeq.Join(timeTween);
        shakeSeq.Append(t.DOMoveX(origX - shakeDist, shakeDur * 2).SetEase(Ease.Linear));
        shakeSeq.Append(t.DOMoveX(origX, shakeDur).SetEase(Ease.Linear));
        shakeSeq.SetLoops(loops);
        return shakeSeq;
    }

    protected static Tween ScreenShake(float distance = .5f, int loops = 3) {
        var seq = DOTween.Sequence();
        var origPos = Camera.main.transform.position;
        const float shakeDur = .03f;
        seq.Append(Camera.main.transform.DOMoveX(distance, shakeDur).SetRelative().SetEase(Ease.Linear));
        seq.Append(Camera.main.transform.DOMoveX(distance * -2, shakeDur*2).SetRelative().SetEase(Ease.Linear));
        seq.Append(Camera.main.transform.DOMoveX(distance, shakeDur).SetRelative().SetEase(Ease.Linear));
        seq.SetLoops(loops);

        return seq;
    }
}
