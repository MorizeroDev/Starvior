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
            if (LinkDataName != "")
            {
                if (LinkDataName.EndsWith("Volume")) Settings.BroadcastVolumeChange();
                PlayerPrefs.SetFloat(LinkDataName, value);
            }
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
    public void MouseUp()
    {
        MouseDrag();
        ScrollController.UIUsing = false;
    }
    public void MouseDrag()
    {
        ScrollController.UIUsing = true;
        Vector2 mouse = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Background, Input.mousePosition, Camera.main, out mouse);
        float f = (mouse.x - Background.transform.position.x) / Background.sizeDelta.x;
        if (f < 0) f = 0;
        if(f > 1f) f = 1f;
        v = f;
        UpdateDisplay();
        if (LinkDataName != "")
        {
            if (LinkDataName.EndsWith("Volume")) Settings.BroadcastVolumeChange();
            PlayerPrefs.SetFloat(LinkDataName, v);
        }
    }
    public void UpdateDisplay()
    {
        Foreground.sizeDelta = new Vector2(Value * Background.sizeDelta.x, Background.sizeDelta.y);
    }
}
