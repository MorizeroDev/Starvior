using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLightCallback : MonoBehaviour
{
    public SaveBtnController controller;
    public static SaveLightCallback current;
    private void Awake()
    {
        current = this;
    }
    public void Stage1()
    {
        if(controller == null) return;
        controller.ApplyOverwriteSave();
    }
    public void Stage2()
    {
        if (controller == null) return;
        controller.SaveLightDone();
    }
}
