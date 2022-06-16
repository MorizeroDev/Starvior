using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsFalldown : MonoBehaviour
{
    void Update()
    {
        if(Mathf.Abs(transform.eulerAngles.z) > 18 && Mathf.Abs(transform.eulerAngles.z) < 89.9)
        {
            float t = transform.eulerAngles.z < 0 ? -90f : 90f;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + (t - transform.eulerAngles.z) / 15);
        }
    }
}
