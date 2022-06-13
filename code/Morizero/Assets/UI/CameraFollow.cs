using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public void Adjust()
    {
        Camera cam = this.gameObject.GetComponent<Camera>();
        cam.rect = Camera.main.rect;
        cam.orthographicSize = Camera.main.orthographicSize;
        cam.nearClipPlane = Camera.main.nearClipPlane;
        cam.farClipPlane = Camera.main.farClipPlane;
        cam.sensorSize = Camera.main.sensorSize;
        this.gameObject.transform.position = Camera.main.transform.position;
        this.gameObject.transform.eulerAngles = Camera.main.transform.eulerAngles;
    }
    void Awake()
    {
        Adjust();
    }
    void Update()
    {
        this.gameObject.transform.position = Camera.main.transform.position;
        this.gameObject.transform.eulerAngles = Camera.main.transform.eulerAngles;
    }
}
