using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckObj : MonoBehaviour
{
    public Chara.walkDir RequireDir;
    public int CheckType = 0;
    private void OnCollisionStay2D(Collision2D other) {
        /**Vector3 Pp = MapCamera.Player.transform.position;
        Vector3 Px = MapCamera.Player.transform.localScale;
        float XD = 0,YD = 0;
        if((int)MapCamera.Player.dir == 0) {YD = -0.5f;Pp.y -= (Px.y/2)*0;}
        if((int)MapCamera.Player.dir == 1) {XD = -0.5f;Pp.x -= (Px.x/2)*0;}
        if((int)MapCamera.Player.dir == 2) {XD = 0.5f;Pp.x += (Px.x/2)*0;}
        if((int)MapCamera.Player.dir == 3) {YD = 0.5f;Pp.y += (Px.y/2)*0;}

        int FACE = 0;
        RaycastHit[] hit = Physics.RaycastAll(new Vector3(Pp.x,Pp.y,0.1f),new Vector3(XD,YD,0));
        foreach(RaycastHit c in hit){
            if(c.collider.gameObject.name == this.name) {
                FACE = 1;
            }
        }
        **/
        if(other.gameObject == MapCamera.Player.gameObject && RequireDir == MapCamera.Player.dir) {
            MapCamera.HitCheck = this.gameObject;
            if(CheckType == 0) MapCamera.mcamera.CheckText.sprite = MapCamera.mcamera.CheckFore;
            if(CheckType == 1) MapCamera.mcamera.CheckText.sprite = MapCamera.mcamera.TalkFore;
        }
    }

    private void OnCollisionExit2D(Collision2D other) {
        if(MapCamera.HitCheck == this.gameObject) MapCamera.HitCheck = null;
    }
}
