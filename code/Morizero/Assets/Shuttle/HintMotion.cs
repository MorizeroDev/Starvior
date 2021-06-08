using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintMotion : MonoBehaviour
{
    public float ox,tx;
    public AudioSource bgm;
    public float targetTime;
    private bool ani = false;
    public int id;
    public RectTransform rect;
    void Update()
    {
        if(ani) return;
        Vector3 pos = rect.localPosition;
        float pro = 1 - (targetTime - bgm.time) / 1.2f;
        if(pro > 1) pro = 1;
        pos.x = ox + (tx - ox) * pro;
        rect.localPosition = pos;
        if(!ani){
            if(bgm.time - targetTime > 0.15f){
                Shuttle.shuttle.Hit(5,3);
                Shuttle.hitLock = false;
                this.GetComponent<Animator>().Play("HintMiss",0);
                ani = true;
            }
            if(id == Shuttle.nowhit){
                if(Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)){
                    if(Mathf.Abs(bgm.time - targetTime) <= 0.15f && !Shuttle.hitLock){
                        float pitch = Mathf.Abs(bgm.time - targetTime);
                        if(pitch <= 0.05f){
                            // Perfect
                            Shuttle.shuttle.Hit(pitch / 0.15f,0);
                        }else if(pitch <= 0.1f){
                            // Good
                            Shuttle.shuttle.Hit(pitch / 0.15f,1);
                        }else{
                            // Okay
                            Shuttle.shuttle.Hit(pitch / 0.15f,2);
                        }
                        this.GetComponent<Animator>().Play("HintBoom",0);
                        ani = true;
                        Shuttle.hitLock = true;
                    }
                }
                if(Input.GetKeyUp(KeyCode.Z) || Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0))
                    Shuttle.hitLock = false;
            }
        }
    }
}
