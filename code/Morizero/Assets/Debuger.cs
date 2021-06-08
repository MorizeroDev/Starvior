using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debuger : MonoBehaviour
{
    public Text text;
    float sTime = 0, dTime = 0;
    int FPSCount = 0, FPS = 0;
    bool show = false;

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

        if(Input.GetKeyUp(KeyCode.F3)) show = !show;

        // Output
        if (sTime < 3){
            sTime += Time.deltaTime;
            text.text = "⚠ Press <b>F3</b> for debug informations...";
            if(show) sTime = 3;
            return;
        }
        
        if(!show){
            text.text = "";
        }else{
            text.text = $"<b>FPS</b>   {FPS}\n" +
                        $"<b>Rendering</b>   {Time.deltaTime.ToString("f3")}s\n" +
                        $"<b>Cursor</b>   ({Input.mousePosition.x.ToString("f2")},{Input.mousePosition.y.ToString("f2")})\n" +
                        $"<b>Running</b>   {Time.realtimeSinceStartup.ToString("f3")}s\n" +
                        $"<b>SuspensionDrama</b>    {MapCamera.SuspensionDrama} \n";
        }

    }
}
