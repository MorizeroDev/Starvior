using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveBackBtn : MonoBehaviour
{
    public void Click()
    {
        // ·µ»Ø°´Å¥
        Animator UIAni = GameObject.Find("SaveUI").GetComponent<Animator>();
        UIAni.SetFloat("Speed", -2f);
        UIAni.Play("SaveUI", 0, 1.0f);
        SaveController.SaveShowed = false;
        return;
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.Escape)) Click();
    }
}
