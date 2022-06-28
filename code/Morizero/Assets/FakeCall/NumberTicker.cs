using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NumberTicker : MonoBehaviour
{
    public static bool Called = false;
    float deltaTime = 0f;
    int sTick;
    public bool CallMode = false;
    public Text text;

    private void Update() {
        deltaTime += Time.deltaTime;
        if (deltaTime > 1f){
            deltaTime = 0;
            sTick++;
            int minute = Mathf.FloorToInt(sTick / 60);
            int second = sTick % 60;
            if(!CallMode) text.text = minute.ToString("00") + ":" + second.ToString("00");
        }
        if(CallMode){
            if(Called) return;
            if (sTick > 5){
                if (Random.Range(0, 5) > 2)
                {
                    sTick = 0;
                    return;
                }
                SceneManager.LoadSceneAsync("FakeCall", LoadSceneMode.Additive);
                Called = true;
            }
        }else{
            if (Input.GetMouseButton(0) && sTick > 10){
                SceneManager.UnloadSceneAsync("FakeCall");
            }
        }
    }
}
