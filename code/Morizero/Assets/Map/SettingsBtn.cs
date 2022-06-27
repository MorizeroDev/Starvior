using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsBtn : MonoBehaviour
{
    public static GameObject ActiveSettingsBtn;
    private void Start()
    {
        ActiveSettingsBtn = this.gameObject;
    }
    void Update()
    {
        if (Loading.isUsing) return;
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
        {
            Settings.Show();
        }
    }
    private void OnMouseUp()
    {
        Settings.Show();
    }
    public void OnClick()
    {
        OnMouseUp();
    }
}
