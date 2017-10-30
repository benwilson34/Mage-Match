using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITooltip : MonoBehaviour, Tooltipable {
    public string tooltipInfo;

    public string GetTooltipInfo() {
        return "This is UI element! " + tooltipInfo;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
