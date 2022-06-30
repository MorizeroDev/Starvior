using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoBtn : MonoBehaviour
{
    public void OnMouseUp()
    {
        if(!Settings.Active && !Settings.Loading)
        {
            Settings.AutoOpenIndex = 6;
            Settings.Show();
        }
    }
}
