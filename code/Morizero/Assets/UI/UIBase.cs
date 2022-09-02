using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class UIController : MonoBehaviour
{
    [HideInInspector]
    public UIBase uibase;
    public virtual void Click()
    {
        Debug.LogWarning("UI元素" + uibase.id + "(" + this.name + ")的激活事件未处理。");
    }
    public virtual void Lit()
    {
        Debug.LogWarning("UI元素" + uibase.id + "(" + this.name + ")的获焦事件未处理。");
    }
    public virtual void Initialize()
    {
        Debug.LogWarning("UI元素" + uibase.id + "(" + this.name + ")的初始化事件未处理。");
    }
}
public class UIBase : MonoBehaviour
{
    [HideInInspector]
    public UIFocus focuser;
    [HideInInspector]
    public int id;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public UIController controller;
    [HideInInspector]
    public RectTransform rect;
    public Animator extraAnimator;
    public bool isActive
    {
        get
        {
            return animator.GetBool("Focus");
        }
        set
        {
            animator.SetBool("Focus", value);
            if (extraAnimator != null) extraAnimator.SetBool("Focus", value);
        }
    }
    public void PlayExit()
    {
        animator.SetBool("Exit", true);
        if (extraAnimator != null) extraAnimator.SetBool("Exit", true);
    }
    public void Click()
    {
        if (!isActive)
        {
            focuser.ChangeFocus(id);
        }
        else
        {
            SndPlayer.Play("choicedone");
            if (controller != null) controller.Click();
        }
    }
    public void Initialize()
    {
        animator = this.transform.Find("Unfocus").GetComponent<Animator>();
        rect = this.transform.Find("Unfocus").GetComponent<RectTransform>();
        controller = this.GetComponent<UIController>();
        if (controller != null)
        {
            controller.uibase = this;
            controller.Initialize();
        }
        else
        {
            Debug.LogWarning("UI元素" + id + "(" + this.name + ")没有响应激活的脚本。");
        }   
    }
    private void Update()
    {
        if (!isActive) return;
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Z) || Input.GetKeyUp(KeyCode.Return))
        {
            Click();
        }
    }
}
