using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class MagicAlAnim : AnimationController {

    public static IEnumerator _Jab(int id, TileBehav tb) {
        const float fistStartDistance = 10f;
        var fistStartPos = tb.transform.position;
        var playerOnRight = _mm.uiCont.IDtoSide(id) == UIController.ScreenSide.Right;
        if (playerOnRight)
            fistStartPos.x += fistStartDistance;
        else
            fistStartPos.x -= fistStartDistance;

        // fist object to send across the screen
        var fist = (GameObject)Resources.Load("prefabs/anim/fist");
        fist = Instantiate(fist, fistStartPos, Quaternion.identity);

        if (playerOnRight) // flip sprite if needed
            fist.transform.localScale = new Vector3(-1, 1);

        const float fistDur = .3f;
        yield return fist.transform.DOMoveX(tb.transform.position.x, fistDur).SetEase(Ease.InQuad).WaitForCompletion();


        fist.GetComponent<SpriteRenderer>().DOFade(0, .3f); // fade out fist

        var parts = (GameObject)Resources.Load("prefabs/particles/Pow");
        parts = Instantiate(parts, tb.transform.position, Quaternion.identity);
        parts.transform.localScale = new Vector3(.6f, .6f, .6f); // 60% size

        ScreenShake(.2f, 2).Play();
    }

    public static IEnumerator _Cross(int id, Hex hex) {
        Vector2 fistStartDistance = new Vector2(-10f, 4f); // difference
        var fistStartPos = hex.transform.position;
        fistStartPos.y += fistStartDistance.y;
        var playerOnRight = _mm.uiCont.IDtoSide(id) == UIController.ScreenSide.Right;
        if (playerOnRight)
            fistStartPos.x -= fistStartDistance.x;
        else
            fistStartPos.x += fistStartDistance.x;

        // fist object to send across the screen
        var fist = (GameObject)Resources.Load("prefabs/anim/fist");
        fist = Instantiate(fist, fistStartPos, Quaternion.identity);
        RotateToFace(fist.transform, hex.transform.position);

        if (playerOnRight) // flip sprite if needed
            fist.transform.localScale = new Vector3(-1, 1);

        const float fistDur = .3f;
        yield return fist.transform.DOMove(hex.transform.position, fistDur).SetEase(Ease.InQuad).WaitForCompletion();


        fist.GetComponent<SpriteRenderer>().DOFade(0, .3f); // fade out fist

        var parts = (GameObject)Resources.Load("prefabs/particles/Pow");
        parts = Instantiate(parts, hex.transform.position, Quaternion.identity);
        parts.transform.localScale = new Vector3(.6f, .6f, .6f); // 60% size

        ScreenShake(.2f, 2).Play();
    }

    public static IEnumerator _Hook(int id, TileBehav tb, int c2, int r2) {
        var start = tb.transform.position;
        var end = HexGrid.GridCoordToPos(c2, r2);
        start += (start - end) * 6; // add some distance before the hit

        //var playerOnRight = _mm.uiCont.IDtoSide(id) == UIController.ScreenSide.Right;
        //if (playerOnRight)
        //    fistStartPos.x -= fistStartDistance.x;
        //else
        //    fistStartPos.x += fistStartDistance.x;

        // fist object to send across the screen
        var fist = (GameObject)Resources.Load("prefabs/anim/fist");
        fist = Instantiate(fist, start, Quaternion.identity);
        RotateToFace(fist.transform, end);

        //if (playerOnRight) // flip sprite if needed
        //    fist.transform.localScale = new Vector3(-1, 1);

        const float fistDur = .3f;
        yield return fist.transform.DOMove(tb.transform.position, fistDur).SetEase(Ease.InQuad).WaitForCompletion();


        fist.GetComponent<SpriteRenderer>().DOFade(0, .3f); // fade out fist

        var parts = (GameObject)Resources.Load("prefabs/particles/Pow");
        parts = Instantiate(parts, tb.transform.position, Quaternion.identity);
        parts.transform.localScale = new Vector3(.6f, .6f, .6f); // 60% size

        ScreenShake(.2f, 2).Play();
    }

    public static IEnumerator _StingerStance(int id) {
        // fist object to send across the screen
        var fist = (GameObject)Resources.Load("prefabs/anim/fist");
        fist = Instantiate(fist);

        var pinfoPos = _mm.uiCont.GetPinfo(id).position;
        pinfoPos.z = 0;
        fist.transform.position = pinfoPos;
        var oppPortrait = _mm.uiCont.GetPortrait(_mm.OpponentId(id)).position;
        oppPortrait = Camera.main.ScreenToWorldPoint(oppPortrait);
        oppPortrait.z = 0;

        //var testPosition = HexGrid.GetCellBehavAt(3, 6).transform.position;
        //RotateToFace(fist.transform, testPosition);
        //yield return fist.transform.DOMove(testPosition, .5f).SetEase(Ease.InQuad).WaitForCompletion();
        RotateToFace(fist.transform, oppPortrait);
        yield return fist.transform.DOMove(oppPortrait, .5f).SetEase(Ease.InQuad).WaitForCompletion();

        fist.GetComponent<SpriteRenderer>().DOFade(0, .5f); // fade out fist
        // burst of particles on hit
        var parts = (GameObject)Resources.Load("prefabs/particles/Magic Al Burst");
        parts = Instantiate(parts, oppPortrait, Quaternion.identity);

        parts = (GameObject)Resources.Load("prefabs/particles/Pow");
        parts = Instantiate(parts, oppPortrait, Quaternion.identity);

        yield return ScreenShake().WaitForCompletion();
    }

    public static IEnumerator _SkyUppercut(int col, IEnumerator shootIntoAir) {
        var fistStartPos = HexGrid.GridCoordToPos(col, HexGrid.BottomOfColumn(col)); // pos under col
        fistStartPos.y -= 1f;

        // fist object to send across the screen
        var fist = (GameObject)Resources.Load("prefabs/anim/fist");
        fist = Instantiate(fist, fistStartPos, Quaternion.Euler(0, 0, 90));
        yield return ShakeTransform(fist.transform).WaitForCompletion();

        var endPos = HexGrid.GridCoordToPos(col, HexGrid.TopOfColumn(col));
        const float fistDur = .3f;

        var moveTween = fist.transform.DOMove(endPos, fistDur).SetEase(Ease.InQuad);

        yield return new WaitForSeconds(.05f);
        _mm.StartCoroutine(shootIntoAir); // I don't like that there's logic here...maybe Unity animations can take over for this?

        yield return moveTween.WaitForCompletion();

        // TODO somehow animate for each tile...

        fist.GetComponent<SpriteRenderer>().DOFade(0, .3f); // fade out fist

        //var parts = (GameObject)Resources.Load("prefabs/particles/Pow");
        //parts = Instantiate(parts, hex.transform.position, Quaternion.identity);
        //parts.transform.localScale = new Vector3(.6f, .6f, .6f); // 60% size

        ScreenShake(.2f, 2).Play();
    }

}
