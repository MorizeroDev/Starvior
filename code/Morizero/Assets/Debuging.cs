using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debuging : MonoBehaviour
{
    public Text text;
    int FPSCount = 0, FPS = 0;
    float dTime = 0;
    private void Update()
    {
        FPSCount++;
        dTime += Time.deltaTime;
        if (dTime > 1)
        {
            dTime = 0;
            FPS = FPSCount;
            FPSCount = 0;
        }

        // Output
        text.text = $"<b>FPS</b>   {FPS}\n" +
                    $"<b>Rendering</b>   " + Time.deltaTime.ToString("f3") + "s\n" +
                    $"<b>Cursor</b>   (" + Input.mousePosition.x.ToString("f2") + "," + Input.mousePosition.y.ToString("f2")+")\n" +
                    $"<b>Running</b>   "+ Time.realtimeSinceStartup.ToString("f3") + "s\n";
    }
}
