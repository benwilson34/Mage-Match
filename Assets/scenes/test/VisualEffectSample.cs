using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VisualEffectSample : MonoBehaviour {

    public Transform tweenGroup, comp, compAnim;

	void Start () {
        List<Transform> tweens = new List<Transform>();
        foreach (Transform t in tweenGroup)
            tweens.Add(t);

        tweens[0].DOMove(tweens[0].position + new Vector3(2, 1.5f), 1).SetLoops(-1);

        tweens[1].DORotate(new Vector3(0, 0, 150), 1).SetLoops(-1);

        tweens[2].DOScale(new Vector3(1.5f, 1.5f), 1).SetLoops(-1);

        tweens[3].GetComponent<SpriteRenderer>().DOColor(Color.green, 1).SetLoops(-1);

        tweens[4].GetComponent<SpriteRenderer>().DOFade(0, 1).SetLoops(-1);


        //GameObject.Find("shader").GetComponent<SpriteRenderer>().DOFade(0, 1).SetLoops(-1);


        comp.DOMoveY(comp.position.y + 2, 1).SetEase(Ease.OutQuart).SetLoops(-1);
        compAnim.GetComponent<SpriteRenderer>().DOFade(0, 1).From().SetEase(Ease.OutExpo).SetLoops(-1);
        compAnim.DOScale(1.5f, 1).SetEase(Ease.OutQuart).SetLoops(-1);
    }

}
