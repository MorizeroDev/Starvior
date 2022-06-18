using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabHideAniCallback : MonoBehaviour
{
    public void Callback()
    {
        gameObject.SetActive(false);
    }
}
