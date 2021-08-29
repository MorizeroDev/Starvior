using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningAccepter : MonoBehaviour
{
    private void Start() {
        if(PlayerPrefs.GetString("accept_risks?","no") == "yes")
        Switcher.Carry("Startup");
    }
    private void OnMouseUp() {
        PlayerPrefs.SetString("accept_risks?","yes");
        Switcher.Carry("Startup");
    }
}
