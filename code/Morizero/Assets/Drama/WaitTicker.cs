using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void WaitTickerCallback();
public class WaitTicker : MonoBehaviour
{
    public WaitTickerCallback callback;
    public float waitTime;
    private float time = 0.0f;

    void Update()
    {
        time += Time.deltaTime;
        if(time > waitTime){
            Debug.Log("Wait done.");
            callback();
            Destroy(this.gameObject);
        }
    }
}
