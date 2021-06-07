using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintMotion : MonoBehaviour
{
    public float ox,tx;
    public AudioSource bgm;
    public float targetTime;
    private bool ani = false;
    public RectTransform rect;
    void Update()
    {
        if(ani) return;
        Vector3 pos = rect.localPosition;
        float pro = 1 - (targetTime - bgm.time) / 1.2f;
        if(pro > 1) pro = 1;
        pos.x = ox + (tx - ox) * pro;
        rect.localPosition = pos;
        if(pro == 1 && !ani){
            this.GetComponent<Animator>().Play("HintBoom",0);
            ani = true;
        }
    }
}
