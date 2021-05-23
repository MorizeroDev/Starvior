using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void WalkTaskCallback();
// 角色控制器
public class Chara : MonoBehaviour
{
    // 朝向枚举
    public enum walkDir{
        Down,Left,Right,Up
    }
    // 行走任务
    public struct walkTask{
        public float xBuff,yBuff;               // 横纵坐标上的任务
        public float x,y;                       // 实际的任务坐标
    }
    // 当列表长度为0时表示行走完毕
    public List<walkTask> walkTasks = new List<walkTask>();

    private Sprite[] Animation;                 // 行走图图像集
    public string Character;                    // 对应的人物
    public bool Controller = false;             // 是否为玩家
    private SpriteRenderer image;               // 图形显示容器
    public walkDir dir;                         // 当前朝向
    private bool walking;                       // 是否正在行走
    private int walkBuff = 1;                   // 行走图系列帧序数
    private float walkspan;                     // 行走图动画间隔控制缓冲
    private float sx,sy,ex,ey;                  // 地图边界x,y - x,y
    private float tx,ty,lx,ly;                  // 目标地点x,y；上一次的坐标x,y
    public GameObject MoveArrow;                // 点击移动反馈
    private bool tMode = false;                 // 点击移动模式（TouchMode）
    public WalkTaskCallback walkTaskCallback;   // 行走人物回调

    private void Awake() {
        // 载入行走图图像集，并初始化相关设置
        Animation = Resources.LoadAll<Sprite>("Players\\" + Character);
        image = this.GetComponent<SpriteRenderer>();
        dir = walkDir.Down;
        UploadWalk();
        // 获取地图边界并处理
        Vector3 size = new Vector3(0.25f,0.25f,0f);
        Vector3 pos = GameObject.Find("startDot").transform.localPosition;
        sx = pos.x + size.x; sy = pos.y - size.y; 
        pos = GameObject.Find("endDot").transform.localPosition;
        ex = pos.x - size.x; ey = pos.y + size.y * 1.7f; 
        // 如果是玩家则绑定至MapCamera
        if(Controller) MapCamera.Player = this;
        // 如果是玩家并且传送数据不为空，则按照传送设置初始化
        if(Controller && MapCamera.initTp != -1){
            dir = MapCamera.initDir;
            UploadWalk();
            // 取得传送位置坐标
            this.transform.localPosition = GameObject.Find("tp" + MapCamera.initTp).transform.localPosition;
        }
    }
    // 更新行走图图形
    void UploadWalk(){
        if(walking){
            // 行走时的图像
            walkspan += Time.deltaTime;
            if(walkspan > 0.1f){
                walkBuff ++;
                if(walkBuff > 2) walkBuff = 0;
                walkspan = 0;
            }
            walking = false;
        }else{
            // 未行走时
            walkspan = 0;
            walkBuff = 1;
        }
        // 设定帧
        image.sprite = Animation[(int)dir * 3 + walkBuff];
    }
    void FixedUpdate()
    {
        // 如果不是玩家
        if(!Controller) return;
        // 如果剧本正在进行则退出
        if(MapCamera.SuspensionDrama && walkTasks.Count == 0) return;
        // 是否正在执行行走任务？
        bool isWalkTask = (walkTasks.Count > 0);
        Vector3 pos = transform.localPosition;

        // 如果有行走任务
        if(isWalkTask){
            // 如果坐标尚未初始化
            if(walkTasks[0].x == 0){
                walkTask task = walkTasks[0];
                task.x = pos.x + 0.5f * task.xBuff;
                task.y = pos.y + 0.5f * task.yBuff;
                walkTasks[0] = task;
            }
            if(Mathf.Abs(walkTasks[0].x - pos.x) <= 0.04f && Mathf.Abs(walkTasks[0].y - pos.y) <= 0.04f){
                walkTasks.RemoveAt(0);
                if(walkTasks.Count == 0) walkTaskCallback();
                UploadWalk();
                return;
            }else if(walkTasks[0].x < pos.x){
                dir = walkDir.Left;
            }else if(walkTasks[0].x > pos.x){
                dir = walkDir.Right;
            }else if(walkTasks[0].y < pos.y){
                dir = walkDir.Up;
            }else if(walkTasks[0].y > pos.y){
                dir = walkDir.Down;
            }
        }

        // 如果屏幕被点击
        if(Input.GetMouseButton(0) && !isWalkTask){
            // 从屏幕坐标换算到世界坐标
            Vector3 mpos = MapCamera.mcamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            mpos.z = 0;
            // 检查是否点击的是UI而不是地板
            if (EventSystem.current.IsPointerOverGameObject()) return;
            // 设置相关参数
            tMode = true;tx = mpos.x;ty = mpos.y;lx = 0;ly = 0;
            // 设置点击反馈
            MoveArrow.transform.localPosition = mpos;
            MoveArrow.SetActive(true);
        }

        if(tMode){
            /**
                TODO：当触摸模式启动时的移动处理
            **/
            tMode = false;
        }else if(!isWalkTask){
            // 非触摸模式时，检测键盘输入
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
