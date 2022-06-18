using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullScreenCheckBox : CheckBoxEvent
{
    public override void ValueChanged()
    {
        Screen.fullScreen = (Value == 0);
    }
}
