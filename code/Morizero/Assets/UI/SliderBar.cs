using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderBarEvent : MonoBehaviour
{
    public float Value;
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
public class SliderBar : MonoBehaviour
{

    public RectTransform Background, Foreground, Ball;
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
    private float v;
    private SliderBarEvent UIEvent = null;
    public string LinkDataName = "";
    public float DefaultValue;
    private Animator animator;
    private bool Initialized = false;
    private bool AniPlayed = false;
    private void Awake()
    {
        UpdateDisplay();
        //if (UIEvent != null) Debug.Log("Attached event detected.");
        animator = GetComponent<Animator>();
        if (LinkDataName != "") Value = PlayerPrefs.GetFloat(LinkDataName, DefaultValue);
        TryGetComponent<SliderBarEvent>(out UIEvent);
        Initialized = true;
    }
    public void MouseUp()
    {
        MouseDrag();
        if (AniPlayed)
        {
            AniPlayed = false;
            animator.Play("SliderHide", 0, 0.0f);
        }
        ScrollController.UIUsing = false;
        if (UIEvent != null) UIEvent.MouseUp();
    }
    public void MouseDrag()
    {
        if (!AniPlayed)
        {
            AniPlayed = true;
            animator.Play("SliderShow", 0, 0.0f);
        }
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
        if (UIEvent != null)
        {
            UIEvent.Value = v;
            UIEvent.ValueChanged();
            UIEvent.MouseDrag();
        }
    }
    public void UpdateDisplay()
    {
        Foreground.sizeDelta = new Vector2(Value * Background.sizeDelta.x, Background.sizeDelta.y);
        Ball.localPosition = new Vector3(Background.localPosition.x + Value * Background.sizeDelta.x, Ball.localPosition.y, 0);
    }
}
