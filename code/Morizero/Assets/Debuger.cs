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
            FPSCount = 0;
        }

        if(Input.GetKeyUp(KeyCode.F3)) show = !show;

        // Output
        if (sTime < 3){
            sTime += Time.deltaTime;
            text.text = "当前游戏版本：alpha-0221.1";
            if(!show) sTime = 3;
            return;
        }
        
        if(!show){
            text.text = "";
        }else{
            text.text = $"<b>FPS</b>   {FPS}\n" +
                        $"<b>Cursor</b>   ({Input.mousePosition.x.ToString("f2")},{Input.mousePosition.y.ToString("f2")})\n" +
                        $"<b>SuspensionDrama</b>    {MapCamera.SuspensionDrama} \n";
        }

    }
}
