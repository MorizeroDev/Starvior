using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : CheckObj
{
    public int TPPort = 1;
    public Chara.walkDir TPDirection = Chara.walkDir.Down;
    public string TargetMap = "";
    private bool selfHide = false;

    public override void Update() {
        if(IsActive()){
            MapCamera.initTp = TPPort;MapCamera.initDir = TPDirection;
            //this.transform.parent.GetComponent<SpriteRenderer>().color = new Color(0,0,0,0);
            //this.transform.localPosition = new Vector3(-10000,-10000,-100);
            selfHide = true;
            this.GetComponent<AudioSource>().Play();
            Switcher.Carry(TargetMap);
        }
        if(selfHide)
            MapCamera.HitCheck = null;
    }
}
