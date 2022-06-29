using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBtn : ButtonEvent
{
    public SliderBar MovePad, BGM, BGS, SE;
    public CheckBox FullScreen, DialogSpeed, AutoContinue;
    public override void OnClick()
    {
        MakeChoice.Create(() => 
        {
            if (MakeChoice.choiceId == 0)
            {
                MovePad.Value = 0.5f; BGM.Value = 1f; BGS.Value = 0.5f; SE.Value = 0.5f;
                MovePad.gameObject.GetComponent<MovePadSettings>().MouseUp();
                FullScreen.Value = 1; DialogSpeed.Value = 1; AutoContinue.Value = 1;
            }
        }, "确定要重置所有设定吗？", new string[] { "确定", "取消" }, true);
    }
}
