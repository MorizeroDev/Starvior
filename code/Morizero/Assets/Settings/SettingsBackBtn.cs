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
            //Debuger.InstantMessage("KEEEEEEEEEEEEEEEEEEEEEY", this.transform.position);            
            Settings.Hide();
        }
    }
    public void MouseUp()
    {
        //Debuger.InstantMessage("MMMMMMMMMMMMMOOOOOOOOOOUSE", this.transform.position);
        Settings.Hide();
    }
}
