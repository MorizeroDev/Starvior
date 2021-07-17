using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void WaitTickerCallback();
public class WaitTicker : MonoBehaviour
{
    public WaitTickerCallback callback;
    public float waitTime;
    private float time;

    void Update()
    {
        time += Time.deltaTime;
        if(time > waitTime){
            callback();
            Destroy(this.gameObject);
        }
    }
}
