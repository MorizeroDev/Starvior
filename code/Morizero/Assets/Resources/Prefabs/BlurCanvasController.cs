using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurCanvasController : MonoBehaviour
{
    private void Awake()
    {
        Camera cam = Camera.main.transform.Find("BlurCamera").GetComponent<Camera>();
        foreach(Canvas canvas in GameObject.FindObjectsOfType<Canvas>())
        {
            if (!canvas.gameObject.Equals(transform.parent.gameObject)) canvas.worldCamera = cam;
        }
    }

    private void OnDestroy()
    {
        foreach (Canvas canvas in GameObject.FindObjectsOfType<Canvas>())
        {
            if (!canvas.gameObject.Equals(transform.parent.gameObject)) canvas.worldCamera = Camera.main;
        }
    }
}
