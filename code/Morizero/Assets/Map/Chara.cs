using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    private float tx,ty,lx,ly;
    public GameObject MoveArrow;
    private bool tMode = false;

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
        if(Controller && MapCamera.initTp != -1){
            dir = MapCamera.initDir;
            UploadWalk();
            this.transform.localPosition = GameObject.Find("tp" + MapCamera.initTp).transform.localPosition;
        }
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
        if(MapCamera.SuspensionDrama) return;

        if(Input.GetMouseButton(0)){
            Vector3 mpos = MapCamera.mcamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            mpos.z = 0;
            // 检测点击的位置是否是地板
            if (EventSystem.current.IsPointerOverGameObject()) return;
            tMode = true;tx = mpos.x;ty = mpos.y;lx = 0;ly = 0;
            MoveArrow.transform.localPosition = mpos;
            MoveArrow.SetActive(true);
        }
        Vector3 pos = transform.localPosition;

        if(tMode){
            if(Mathf.Abs(lx - pos.x) <= 0.04f && Mathf.Abs(ly - pos.y) <= 0.04f){
                tMode = false;
                UploadWalk();
                MoveArrow.SetActive(false);
                return;
            }
            if(Mathf.Abs(pos.x - tx) <= 0.04f){
                if(Mathf.Abs(pos.y - ty)  <= 0.04f){
                    tMode = false;
                    UploadWalk();
                    MoveArrow.SetActive(false);
                    return;
                }
                else{
                    dir = (ty > pos.y ? walkDir.Up : walkDir.Down);
                    lx = pos.x;ly = pos.y;
                }
            }else{
                dir = (tx > pos.x ? walkDir.Right : walkDir.Left);
                lx = pos.x;ly = pos.y;
            }
        }else{
            if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
                dir = walkDir.Left;
            }else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
                dir = walkDir.Right;
            }else if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
                dir = walkDir.Up;
            }else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
                dir = walkDir.Down;
            }else{
                UploadWalk();
                return;
            }
        }

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
