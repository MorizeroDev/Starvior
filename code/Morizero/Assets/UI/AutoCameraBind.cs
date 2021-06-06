using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCameraBind : MonoBehaviour
{
    void Awake() {
        this.GetComponent<Canvas>().worldCamera = Camera.main;
        Debug.Log("AutoCameraBind: attached the camera!");
    }
}
