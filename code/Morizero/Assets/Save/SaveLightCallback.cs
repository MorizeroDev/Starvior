using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLightCallback : MonoBehaviour
{
    public SaveController controller;
    private void Awake()
    {
        controller = transform.parent.GetComponent<SaveController>();
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
