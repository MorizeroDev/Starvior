using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Camera cam = this.gameObject.GetComponent<Camera>();
        cam.rect = Camera.main.rect;
        cam.orthographicSize = Camera.main.orthographicSize;
        cam.nearClipPlane = Camera.main.nearClipPlane;
        cam.farClipPlane = Camera.main.farClipPlane;
        cam.sensorSize = Camera.main.sensorSize;
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position = Camera.main.transform.position;
        this.gameObject.transform.eulerAngles = Camera.main.transform.eulerAngles;
    }
}
