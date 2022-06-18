using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsBackBtn : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            Settings.Hide();
        }
    }
    public void MouseUp()
    {
        Settings.Hide();
    }
}
