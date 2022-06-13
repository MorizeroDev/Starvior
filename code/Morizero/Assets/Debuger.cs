using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debuger : MonoBehaviour
{
    public Text text;
    float sTime = 0, dTime = 0;
    int FPSCount = 0, FPS = 0;
    bool show = true;

    private void Update()
    {
        FPSCount++;
        dTime += Time.deltaTime;
        if (dTime > 1)
        {
            dTime = 0;
            FPS = FPSCount;
            text.text = $"{FPS}";
            FPSCount = 0;
        }

        if (Input.GetKeyUp(KeyCode.F3)) text.gameObject.SetActive(!text.gameObject.activeSelf);

    }
}
