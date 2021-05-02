using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chara : MonoBehaviour
{
    public enum walkDir{
        Down,Left,Right,Up
    }
    private Sprite[] Animation;
    public string Character;
    public bool Controller = false;
    private SpriteRenderer image;
    public walkDir dir;
    private bool walking;
    private int walkBuff = 1;
    private float walkspan;
    private float sx,sy,ex,ey;

    private void Awake() {
        Animation = Resources.LoadAll<Sprite>("Players\\" + Character);
        image = this.GetComponent<SpriteRenderer>();
        dir = walkDir.Down;
        UploadWalk();
        Vector3 size = new Vector3(0.25f,0.25f,0f);
        Vector3 pos = GameObject.Find("startDot").transform.localPosition;
        sx = pos.x + size.x; sy = pos.y - size.y; 
        pos = GameObject.Find("endDot").transform.localPosition;
        ex = pos.x - size.x; ey = pos.y + size.y * 1.7f; 
        if(Controller) MapCamera.Player = this;
    }

    void UploadWalk(){
        if(walking){
            walkspan += Time.deltaTime;
            if(walkspan > 0.1f){
                walkBuff ++;
                if(walkBuff > 2) walkBuff = 0;
                walkspan = 0;
            }
            walking = false;
        }else{
            walkspan = 0;
            walkBuff = 1;
        }
        image.sprite = Animation[(int)dir * 3 + walkBuff];
    }
    void FixedUpdate()
    {
        if(!Controller) return;

        if(Input.GetKey(KeyCode.A)){
            dir = walkDir.Left;
        }else if(Input.GetKey(KeyCode.D)){
            dir = walkDir.Right;
        }else if(Input.GetKey(KeyCode.W)){
            dir = walkDir.Up;
        }else if(Input.GetKey(KeyCode.S)){
            dir = walkDir.Down;
        }else{
            UploadWalk();
            return;
        }

        Vector3 pos = transform.localPosition;
        pos.x += 0.05f * (dir == walkDir.Left ? -1 : (dir == walkDir.Right ? 1 : 0)) * (Input.GetKey(KeyCode.X) ? 2 : 1);
        pos.y += 0.05f * (dir == walkDir.Up ? 1 : (dir == walkDir.Down ? -1 : 0)) * (Input.GetKey(KeyCode.X) ? 2 : 1);
        if(pos.x < sx) pos.x = sx;
        if(pos.x > ex) pos.x = ex;
        if(pos.y > sy) pos.y = sy;
        if(pos.y < ey) pos.y = ey;
        transform.localPosition = pos;
        walking = true;
        UploadWalk();
    }
}
