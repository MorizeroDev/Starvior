using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.UI;

public class UIFocus : MonoBehaviour
{
    public static UIFocus active;
    [HideInInspector]
    public List<UIBase> UI;
    [HideInInspector]
    public int lastFocus;
    [HideInInspector]
    public RectTransform rect;
    public int row = 1, column = 1;
    private void Awake()
    {
        UI = new List<UIBase>();
        rect = this.transform.Find("Sprite").GetComponent<RectTransform>();
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            UIBase ui;
            if (transform.parent.GetChild(i).TryGetComponent<UIBase>(out ui))
            {
                UI.Add(ui);
            }
        }
        for(int i = 0; i < UI.Count; i++)
        {
            UI[i].focuser = this;
            UI[i].id = i;
            UI[i].Initialize();
        }
        UI[0].isActive = true;
        //if (UI[0].controller != null) UI[0].controller.Lit();
        if (row == -1) row = UI.Count;
        if (column == -1) column = UI.Count;
        AdjustPosition();
        active = this;
    }
    public void AdjustPosition()
    {
        this.transform.localPosition = new Vector3(
                                            UI[lastFocus].transform.localPosition.x - UI[lastFocus].rect.sizeDelta.x * UI[lastFocus].transform.localScale.x / 2 - rect.sizeDelta.x, 
                                            UI[lastFocus].transform.localPosition.y, 0);
    }
    public void ChangeFocus(int focus)
    {
        SndPlayer.Play("choiceswitch");
        UI[lastFocus].isActive = false;
        UI[focus].isActive = true;
        if (UI[focus].controller != null) UI[focus].controller.Lit();
        lastFocus = focus;
        AdjustPosition();   
    }
    public void PlayExit()
    {
        foreach (UIBase ui in UI)
            ui.PlayExit();
        this.transform.Find("Sprite").GetComponent<Animator>().SetBool("Exit", true);
    }
    public void PlayEnter()
    {
        foreach (UIBase ui in UI)
        {
            ui.PlayEnter();
        }
        this.transform.Find("Sprite").GetComponent<Animator>().SetBool("Exit", false);
        this.transform.Find("Sprite").GetComponent<Animator>().Play("FocuserLoop", 0, 0.0f);
    }
    public void Update()
    {
        int f = lastFocus;
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            f = lastFocus - 1;
            if (f < 0) f = UI.Count - 1;
        }
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            f = lastFocus + 1;
            if (f >= UI.Count) f = 0;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            f = lastFocus - column;
            if (f < 0) f += row * column;
            if (f < 0 || f >= UI.Count) f = lastFocus;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            f = lastFocus + column;
            if (f >= UI.Count) f -= row * column;
            if (f < 0 || f >= UI.Count) f = lastFocus;
        }
        if (f != lastFocus)
        {
            ChangeFocus(f);
        }
    }
}
