using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEvent : MonoBehaviour
{
    public virtual void OnClick()
    {
    }
}
public class ButtonController : MonoBehaviour
{
    private ButtonEvent UIEvent = null;
    private void Awake()
    {
        TryGetComponent<ButtonEvent>(out UIEvent);
    }
    public void OnClick()
    {
        if (UIEvent != null)
        {
            UIEvent.OnClick();
        }
        GetComponent<Animator>().Play("ButtonPressed", 0, 0.0f);
    }
}
