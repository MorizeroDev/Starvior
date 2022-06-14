using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSeaController2 : MonoBehaviour
{
    public float Speed = 1f;
    private float lastTime;
    public GameObject prefab;
    void Update()
    {
        if (Time.time - lastTime > Speed)
        {
            lastTime = Time.time;
            GameObject go = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, transform.parent);
            go.SetActive(true);
            Speed = Random.Range(0.5f, 1.5f);
        }
    }
}
