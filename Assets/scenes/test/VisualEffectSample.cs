using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VisualEffectSample : MonoBehaviour {

    public Transform tweenGroup;

	void Start () {
        List<Transform> tweens = new List<Transform>();
        foreach (Transform t in tweenGroup)
            tweens.Add(t);

        tweens[0].DOMove(tweens[0].position + new Vector3(2, 1.5f), 1).SetLoops(-1);

        tweens[1].DORotate(new Vector3(0, 0, 150), 1).SetLoops(-1);

        tweens[2].DOScale(new Vector3(1.5f, 1.5f), 1).SetLoops(-1);

        tweens[3].GetComponent<SpriteRenderer>().DOColor(Color.green, 1).SetLoops(-1);

        tweens[4].GetComponent<SpriteRenderer>().DOFade(0, 1).SetLoops(-1);
    }

}
