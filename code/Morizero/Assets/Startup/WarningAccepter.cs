using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UInput{
    
    public bool IsClick(){
        return Input.GetMouseButtonUp(0);
    }
}
public class WarningAccepter : MonoBehaviour
{
    private void Start() {
        //如果玩家已同意承担风险，则跳过请求界面
        if(PlayerPrefs.GetString("accept_risks?","no") == "yes")
            Switcher.CarryWithLoadCircle("Startup");
    }
    private void OnMouseUp() {
        //玩家愿意承担风险
        PlayerPrefs.SetString("accept_risks?","yes");
        //折回主页面
        Switcher.CarryWithLoadCircle("Startup");
    }
}
