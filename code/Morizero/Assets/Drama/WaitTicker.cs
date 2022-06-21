using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void WaitTickerCallback();
public class WaitTicker : MonoBehaviour
{
    public WaitTickerCallback callback;
    public float waitTime;
    private float time = 0.0f;

    public static void Create(float time, WaitTickerCallback Callback)
    {
        GameObject fab = (GameObject)Resources.Load("Prefabs\\WaitTicker");    // ‘ÿ»Îƒ∏ÃÂ
        GameObject ticker = GameObject.Instantiate(fab, new Vector3(0, 0, -1), Quaternion.identity);
        WaitTicker wait = ticker.GetComponent<WaitTicker>();
        wait.waitTime = time;
        wait.callback = Callback;
    }
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
