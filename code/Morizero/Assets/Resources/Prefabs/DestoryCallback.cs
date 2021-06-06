using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryCallback : MonoBehaviour
{
    public delegate void destoryCallback();
    public destoryCallback callback;
    private void OnDestroy() {
        if(callback != null) callback();
    }
}
