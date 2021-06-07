using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using MyNamespace.rayMapPathFinding.myQueueWithIndex;

// 用于Drama Script的回调函数
public delegate void WalkTaskCallback();
// 角色控制器
public class Chara : MonoBehaviour
{
    //public Vector2 outmPos;
    [HideInInspector]
    public UnityEvent<Vector2> inPosEvent = new UnityEvent<Vector2>();
    
    // 行走参数
    public const float speed = 0.05f;
    public const float step = 0.5f;

    // 朝向枚举
    public enum walkDir{
        Down,Left,Right,Up
    }
    // 行走任务
    public class walkTask{
        public float x,y;                        // 实际的任务坐标
        public float xBuff,yBuff;                // 步数坐标（Drama Script）
        private bool useStep = false;
        public bool isCalculated{
            get{
                return (xBuff == 0 && yBuff == 0);
            }
        }
        public void Caculate(Vector3 pos){
            x = pos.x + (useStep ? Chara.step : 1.0f) * xBuff;
            y = pos.y + (useStep ? Chara.step : 1.0f) * yBuff;
            xBuff = 0; yBuff = 0;
            Debug.Log("Walktask: relative position cale: " + x + "," + y);
        }
        public static walkTask fromRaw(float x,float y){
            return new walkTask{
                x = x,y = y,xBuff = 0,yBuff = 0,useStep = false
            };
        } 
        public static walkTask fromStep(float x,float y){
            return new walkTask{
                x = 0,y = 0,xBuff = x,yBuff = y,useStep = true
            };
        } 
        public static walkTask fromRelative(float x,float y){
            return new walkTask{
                x = 0,y = 0,xBuff = x,yBuff = y,useStep = false
            };
        } 
    }
    // 当列表长度为0时表示行走完毕
    public MyQueueWithIndex<walkTask> walkTasks = new MyQueueWithIndex<walkTask>();

    private Sprite[] Animation;                 // 行走图图像集
    public string Character;                    // 对应的人物
    public bool Controller = false;             // 是否为玩家
    private SpriteRenderer image;               // 图形显示容器
    public walkDir dir;                         // 当前朝向
    private bool walking;                       // 是否正在行走
    private int walkBuff = 1;                   // 行走图系列帧序数
    private float walkspan;                     // 行走图动画间隔控制缓冲
    private float sx,sy,ex,ey;                  // 地图边界x,y - x,y
    public GameObject MoveArrow;                // 点击移动反馈
    private bool tMode = false;                 // 点击移动模式（TouchMode）
    private Vector2 lpos;
    private int lposCount;
    public WalkTaskCallback walkTaskCallback;   // 行走人物回调

    private void Awake() {
        // 载入行走图图像集，并初始化相关设置
        Animation = Resources.LoadAll<Sprite>("Players\\" + Character);
        image = this.GetComponent<SpriteRenderer>();
        // dir = walkDir.Down;
        UploadWalk();
        // 获取地图边界并处理
        Vector3 size = new Vector3(0.25f,0.25f,0f);
        Vector3 pos = GameObject.Find("startDot").transform.localPosition;
        sx = pos.x + size.x; sy = pos.y - size.y; 
        pos = GameObject.Find("endDot").transform.localPosition;
        ex = pos.x - size.x; ey = pos.y + size.y * 1.7f; 
        // 如果是玩家则绑定至MapCamera
        if(Controller) {
            MapCamera.Player = this;
            MapCamera.PlayerCollider = this.transform.Find("Pathfinding").gameObject;
        }
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

    // ⚠警告：x和y的取值只能为-1，0，1
    void Move(int x,int y){
        Vector3 pos = transform.localPosition;
        float buff = speed * Time.deltaTime * 60f * (Input.GetKey(KeyCode.X) ? 2 : 1);
        pos.x += buff * x ;
        pos.y += buff * y ;
        if(pos.x < sx) pos.x = sx;
        if(pos.x > ex) pos.x = ex;
        if(pos.y > sy) pos.y = sy;
        if(pos.y < ey) pos.y = ey;
        transform.localPosition = pos;
        walking = true;
        if(x != 0) dir = x < 0 ? walkDir.Left : walkDir.Right;
        if(y != 0) dir = y < 0 ? walkDir.Down : walkDir.Up;
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
            walkTask wt = walkTasks.referencePeek;
            // 如果坐标尚未初始化
            if(!wt.isCalculated) wt.Caculate(pos);
            // 决定是否修正行走坐标（完成行走）
            bool isFix = false;
            if(wt.x < pos.x){
                Move(-1,0);
                if(wt.x >= transform.localPosition.x) isFix = true;
            }else if(wt.x > pos.x){
                Move(1,0);
                if(wt.x <= transform.localPosition.x) isFix = true;
            }else if(wt.y < pos.y){
                Move(0,-1);
                if(wt.y >= transform.localPosition.y) isFix = true;
            }else if(wt.y > pos.y){
                Move(0,1);
                if(wt.y <= transform.localPosition.y) isFix = true;
            }
            // 修正坐标
            if(isFix){
                Debug.Log("Walktask: " + (walkTasks.Count - 1) + " remaining...");
                transform.localPosition = new Vector3(wt.x,wt.y,pos.z);
                walkTasks.Dequeue();
                if(walkTasks.Count == 0){
                    if(tMode){
                        Debug.Log("Walktask: tasks for Pathfinding is done.");
                        tMode = false;
                        MoveArrow.SetActive(false);
                    }else{
                        Debug.Log("Walktask: tasks for Drama Script is done.");
                        walkTaskCallback();
                    } 
                    walking = false;
                    UploadWalk();
                }
            }
        }

        // 如果屏幕被点击
        if(Input.GetMouseButtonUp(0) && !isWalkTask)
        {
            // 必要：开启tMode，将寻路WalkTask与DramaScript的WalkTask区别开来
            tMode = true;
            walkTasks.Clear();
            // 从屏幕坐标换算到世界坐标
            Vector3 mpos = MapCamera.mcamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            mpos.z = 0;
            // 检查是否点击的是UI而不是地板
            if (EventSystem.current.IsPointerOverGameObject()) return;
            // 设置点击反馈
            MoveArrow.transform.localPosition = mpos;
            MoveArrow.SetActive(true);
            lpos = new Vector2(0,0);
            lposCount = 3;
            //prepare for Event to TRayMapBuilder
            inPosEvent.Invoke(mpos);
        }
        
        // 检测键盘输入
        bool isKeyboard = false;
        if((isWalkTask && tMode) || !isWalkTask){
            if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
                Move(-1,0); isKeyboard = true;
            }else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
                Move(1,0); isKeyboard = true;
            }else if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
                Move(0,1); isKeyboard = true;
            }else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
                Move(0,-1); isKeyboard = true;
            }
        }
        // 仅打断寻路task（tMode），不打断DramaScript的task
        if(lpos.x == pos.x && lpos.y == pos.y && isWalkTask && tMode) lposCount--;
        if((isKeyboard || lposCount == 0) && isWalkTask && tMode){
            Debug.Log("Walktask: tasks for Pathfinding is broke.");
            walking = false;
            UploadWalk();
            walkTasks.Clear(); tMode = false; MoveArrow.SetActive(false);
        }
        lpos = pos;

        // 更新图片
        UploadWalk();

    }
}
