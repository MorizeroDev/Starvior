using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsBackBtn : MonoBehaviour
{
    void Update()
    {
        //Debuger.InstantMessage("I'm alive!!!", this.transform.position);
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            MouseUp();
        }
    }
    public void MouseUp()
    {
        if (Settings.MenuOpen && Settings.ActiveMenu != 6)
        {
            Settings.ActiveSetAnimator.SetFloat("TabSpeed", -2.0f);
            Settings.ActiveSetAnimator.Play("TabEnter", 0, 1.0f);
        }
        else
        {
            Settings.Hide();
        }
        
    }
}
