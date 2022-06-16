using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour
{
    public RectTransform Background, Foreground;
    [Range(0f, 1f)]
    public float Value;
    private void Awake()
    {
        UpdateDisplay();
    }
    private void OnMouseDrag()
    {
        Vector2 mouse = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Background, Input.mousePosition, Camera.main, out mouse);
        float f = (mouse.x - Background.transform.position.x) / Background.sizeDelta.x;
        if (f < 0) f = 0;
        if(f > 1f) f = 1f;
        Value = f;
        UpdateDisplay();
    }
    public void UpdateDisplay()
    {
        Foreground.sizeDelta = new Vector2(Value * Background.sizeDelta.x, Background.sizeDelta.y);
    }
}
