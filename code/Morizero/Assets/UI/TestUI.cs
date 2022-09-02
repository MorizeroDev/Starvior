using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TestUI : UIController
{
    public override void Click()
    {
        if (this.name == "wey")
        {
            Dramas.PopupDialog("Wey", "哈哈，我不信。", () =>
            {
                uibase.focuser.PlayExit();
            });
            return;
        }
        Debug.Log("UI元素" + uibase.id + "被激活。");
        uibase.focuser.PlayExit();
    }
}
