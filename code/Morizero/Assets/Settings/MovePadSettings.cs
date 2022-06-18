using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePadSettings : SliderBarEvent
{
    public GameObject example;
    public override void ValueChanged()
    {
        float rate = Value;
        float x = -1084 + rate * (-947 + 1084) * 2;
        float y = -523.9f + rate * (-386.9f + 523.9f) * 2;
        example.transform.localPosition = new Vector3(x, y, 0);
        example.SetActive(true);
        MapCamera.Player.ApplyMovePadSettings();
    }
    private void Start()
    {
        example.SetActive(false);
    }
    public override void MouseDrag()
    {
        //Debug.Log("MouseDrag");
        example.SetActive(true);
    }
    public override void MouseUp()
    {
        //Debug.Log("MouseUp");
        example.SetActive(false);
    }
}
