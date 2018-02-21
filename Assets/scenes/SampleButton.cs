using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleButton : MonoBehaviour {

    public GameObject samplePrefab;

    private Transform _scrollContent;

	// Use this for initialization
	void Start () {
        _scrollContent = GameObject.Find("ScrollContent").transform;
	}

    public void AddElement() {
        Debug.Log("Adding an element...");
        var elem = Instantiate(samplePrefab, _scrollContent);
    }
}
