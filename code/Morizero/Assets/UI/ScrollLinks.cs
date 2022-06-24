using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollLinks : ScrollBarEvent
{
    public ScrollController linkScrollController;
    public override void ValueChanged()
    {
        linkScrollController.ScrollToTarget(Value);
    }
    private void Awake()
    {
        linkScrollController.Callback = ValueFix;
    }
    public void ValueFix(float value)
    {
        Parent.v = value;
        Parent.UpdateDisplay();
    }
}
