using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPlayerFollow : MonoBehaviour
{
    void Update()
    {
        Vector3 pos = MapCamera.Player.gameObject.transform.localPosition;
        pos.z = -10;
        transform.localPosition = pos;
    }
}
