using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firework : MonoBehaviour
{
    public string clip;
    public float XPitch,YPitch;
    private bool posdone = false;
    private Vector3 opos,pos;
    private float stime;
    private Animator animator;
    private void Awake() {
        animator = this.GetComponent<Animator>();
    }
    public void PlayTrueClip(){
        stime += Time.deltaTime;
        opos = this.transform.position;
        pos = this.transform.position;
        if(XPitch != 0 || YPitch != 0){
            animator.Play(clip,0);
            opos.x += XPitch; opos.y += YPitch;
        }
        opos.y += 0.45f;
        pos.y += 0.45f;
        this.transform.position = opos;
    }
    private void Update() {
        if(stime == 0) return;
        stime += Time.deltaTime;
        float pro = stime / 0.2f;
        if(pro > 1) pro = 1;
        if((XPitch != 0 || YPitch != 0) && !posdone){
            Vector3 npos = new Vector3(opos.x + (pos.x - opos.x) * pro,opos.y + (pos.y - opos.y) * pro,pos.z);
            this.transform.position  = npos;
            if(pro == 1) posdone = true;
        }
        if(pro == 1){
            if(XPitch == 0 && YPitch == 0){
                animator.Play(clip,0);
            }
        }
    }
}
