using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlayerFollow : MonoBehaviour
{ 
    public void Update()
    {
        transform.localPosition = Camera.main.transform.localPosition;
    }
}
