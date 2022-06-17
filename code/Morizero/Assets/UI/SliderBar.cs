using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour
{
    public RectTransform Background, Foreground;
    public float Value
    {
        get
        {
            return v;
        }
        set
        {
            v = value;
            UpdateDisplay();
            if(LinkDataName != "") PlayerPrefs.SetFloat(LinkDataName, value);
        }
    }
    private float v;
    public string LinkDataName = "";
    public float DefaultValue;
    private void Awake()
    {
        UpdateDisplay();
        if(LinkDataName != "") Value = PlayerPrefs.GetFloat(LinkDataName, DefaultValue);
    }
    private void OnMouseUp()
    {
        ScrollController.UIUsing = false;
    }
    private void OnMouseDrag()
    {
        ScrollController.UIUsing = true;
        Vector2 mouse = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Background, Input.mousePosition, Camera.main, out mouse);
        float f = (mouse.x - Background.transform.position.x) / Background.sizeDelta.x;
        if (f < 0) f = 0;
        if(f > 1f) f = 1f;
        v = f;
        UpdateDisplay();
    }
    public void UpdateDisplay()
    {
        Foreground.sizeDelta = new Vector2(Value * Background.sizeDelta.x, Background.sizeDelta.y);
    }
}
