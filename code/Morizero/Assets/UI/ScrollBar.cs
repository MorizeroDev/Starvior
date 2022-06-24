using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarEvent : MonoBehaviour
{
    public float Value;
    public ScrollBar Parent;
    public virtual void ValueChanged()
    {

    }
    public virtual void MouseDrag()
    {

    }
    public virtual void MouseUp()
    {

    }
}
public class ScrollBar : MonoBehaviour
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
            if (!Initialized) return;
            if (LinkDataName != "")
            {
                if (LinkDataName.EndsWith("Volume")) Settings.BroadcastVolumeChange();
                PlayerPrefs.SetFloat(LinkDataName, value);
            }
            if (UIEvent != null)
            {
                UIEvent.Value = v;
                UIEvent.ValueChanged();
            }
        }
    }
    public float v;
    private ScrollBarEvent UIEvent = null;
    public string LinkDataName = "";
    public float DefaultValue;
    private Animator animator;
    private bool Initialized = false;
    private bool AniPlayed = false;
    private void Awake()
    {
        UpdateDisplay();
        animator = GetComponent<Animator>();
        //if (UIEvent != null) Debug.Log("Attached event detected.");
        if (LinkDataName != "") Value = PlayerPrefs.GetFloat(LinkDataName, DefaultValue);
        TryGetComponent<ScrollBarEvent>(out UIEvent);
        if (UIEvent != null) UIEvent.Parent = this;
        Initialized = true;
    }
    public void MouseUp()
    {
        MouseDrag();
        if (AniPlayed)
        {
            AniPlayed = false;
            animator.Play("ScrollBarHide", 0, 0.0f);
        }
        ScrollController.UIUsing = false;
        if (UIEvent != null) UIEvent.MouseUp();
    }
    public void MouseDrag()
    {
        if (!AniPlayed)
        {
            AniPlayed = true;
            animator.Play("ScrollBarShow", 0, 0.0f);
        }
        ScrollController.UIUsing = true;
        Vector2 mouse = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Background, Input.mousePosition, Camera.main, out mouse);
        float f = (Background.transform.position.y - mouse.y) / Background.sizeDelta.y;
        if (f < 0) f = 0;
        if (f > 1f) f = 1f;
        v = f;
        UpdateDisplay();
        if (LinkDataName != "")
        {
            if (LinkDataName.EndsWith("Volume")) Settings.BroadcastVolumeChange();
            PlayerPrefs.SetFloat(LinkDataName, v);
        }
        if (UIEvent != null)
        {
            UIEvent.Value = v;
            UIEvent.ValueChanged();
            UIEvent.MouseDrag();
        }
    }
    public void UpdateDisplay()
    {
        if (v < 0) v = 0;
        if (v > 1) v = 1;
        Foreground.localPosition = new Vector3(Foreground.localPosition.x, Background.localPosition.y - v * Background.sizeDelta.y + Foreground.sizeDelta.y / 2, 0);
    }
}
