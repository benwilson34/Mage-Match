using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITooltip : MonoBehaviour, Tooltipable {
    public string tooltipInfo;

    public string GetTooltipInfo() {
        return tooltipInfo;
    }
}
