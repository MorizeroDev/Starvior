using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using MyNamespace.databridge;
using MyNamespace.rayMapPathFinding.myQueue;

// 用于Drama Script的回调函数
public delegate void WalkTaskCallback();
// 角色控制器
public class Chara : MonoBehaviour
{
    private class _OutMousePositionBuilder : DefaultBridgeTaskBuilder
    {
        public _OutMousePositionBuilder(Component component)
        {
            _product = new BridgeTask();
            _destnaionComponent = component;
        }
        public BridgeTask BuildProduct(Component originComponent,Vector2 parament)
        {
            DefineBridgeParamentType(BridgeParamentType.Chara_MousePosition_RayMapPathFinding);
            BuildOrigin(originComponent);
            BuildParament(parament);
            BuildDestnation(_destnaionComponent);
            return _product;
        }
        
        private Component _destnaionComponent;
    }
    
    private class _RegisterCharacters : DefaultBridgeTaskBuilder
    {
        
    }

    public TDataBridge dataBridge;

    // 行走参数
    public float speed = 0.06f;
    public const float step = 0.48f;
    private float targetRotation = 0;
    private bool padMode = false;
    private Vector2 srcPadPos,srcClickPos;
    private Transform Pad;

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
    private Vector2 lpos;
    public WalkTaskCallback walkTaskCallback;   // 行走人物回调

    private _OutMousePositionBuilder bridgeTaskbuilderOMP;

    private void Start() {
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
            Pad = GameObject.Find("MapCamera").transform.Find("MovePad").Find("ball");
            srcClickPos = MapCamera.mcamera.GetComponent<Camera>().WorldToScreenPoint(
                            GameObject.Find("MapCamera").transform.Find("MovePad").Find("tipPad").position
                            );
            srcPadPos = Pad.localPosition;
        }
        if(Controller) // only controller can havve a pathfinding movement
            bridgeTaskbuilderOMP = new _OutMousePositionBuilder(dataBridge.defaultRayMapPathFindingScript);
        // 如果是玩家并且传送数据不为空，则按照传送设置初始化
        if(Controller && MapCamera.initTp != -1){
            dir = MapCamera.initDir;
            UploadWalk();
            // 取得传送位置坐标
            this.transform.localPosition = GameObject.Find("tp" + MapCamera.initTp).transform.localPosition;
        }
    }
    // 更新行走图图形
    public void UploadWalk(){
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
    float Move(int x,int y){
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
        return y == 0 ? buff * x : buff * y;
    }

    private void _SpriteRenderer_AutoSortOrder()
    {

    }

    void FixPos(){
        Vector3 mpos = transform.localPosition;
        mpos.x = Mathf.Round((mpos.x - 0.48f) / 0.96f) * 0.96f + 0.48f; 
        mpos.y = Mathf.Round((mpos.y + 0.48f) / 0.96f) * 0.96f - 0.48f; 
        transform.localPosition = mpos;
    }

    void FixedUpdate()
    {
        // 如果剧本正在进行则退出
        if (MapCamera.SuspensionDrama && walkTasks.Count == 0 && Controller)
        {
            return;
        }
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
            if(!Controller) UploadWalk();
            // 修正坐标
            if(isFix){
                Debug.Log("Walktask: " + (walkTasks.Count - 1) + " remaining...");
                FixPos();
                //transform.localPosition = new Vector3(wt.x,wt.y,pos.z);
                walkTasks.Dequeue();
                if(walkTasks.Count == 0){
                    Debug.Log("Walktask: tasks for Drama Script is done.");
                    walkTaskCallback();
                    walking = false;
                    UploadWalk();
                }
            }
        }
        // 如果不是玩家
        if(!Controller) return;

        // 判定调查
        Vector2 spyRay = new Vector2(pos.x,pos.y);
        if(dir == walkDir.Left){
            spyRay.x -= 0.96f;
        }else if(dir == walkDir.Right){
            spyRay.x += 0.96f;
        }else if(dir == walkDir.Up){
            spyRay.y += 0.96f;
        }else{
            spyRay.y -= 0.96f;
        }
        CheckObj checkObj = null;
        foreach(RaycastHit2D crash in Physics2D.RaycastAll(spyRay,new Vector2(0,0))){
            if(crash.collider.gameObject.TryGetComponent<CheckObj>(out checkObj)){
                if(MapCamera.HitCheck != checkObj.gameObject){
                    checkObj.CheckEncounter();
                }
                break;
            }
        }
        if(checkObj == null && MapCamera.HitCheck != null){
            MapCamera.HitCheck.GetComponent<CheckObj>().CheckGoodbye();
        }

        // 如果屏幕被点击
        if (Input.GetMouseButton(0)){
            if(!padMode){
                if(!isWalkTask){
                    Vector3 mpos = MapCamera.mcamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
                    foreach(RaycastHit2D hit in Physics2D.RaycastAll(new Vector2(mpos.x,mpos.y),new Vector2(0,0))){
                        if(hit.collider.gameObject.name == "MovePad"){
                            Animator padAni = Pad.transform.parent.GetComponent<Animator>();
                            padAni.SetFloat("speed",1.0f);
                            padAni.Play("MovePad",0,0f);
                            padMode = true;
                        }
                    }
                }
            } else {
                // 从屏幕坐标换算到世界坐标
                Vector3 mpos = Input.mousePosition;
                // 测算
                float xp = mpos.x - srcClickPos.x,yp = mpos.y - srcClickPos.y;
                float xpro = Mathf.Abs(xp) / 30,ypro = Mathf.Abs(yp) / 30;
                if(xpro > 1) xpro = 1;
                if(ypro > 1) ypro = 1;
                if(xpro == 1 || ypro == 1){
                    if(Mathf.Abs(xp) > Mathf.Abs(yp)){
                        Move(xp > 0 ? 1 : -1, 0);
                        targetRotation = (xp > 0 ? 270f : 90f);
                    }else {
                        Move(0, yp > 0 ? 1 : -1);
                        targetRotation = (yp > 0 ? 0f : 180f);
                    }
                }
                float ro = Pad.eulerAngles.z + (targetRotation - Pad.eulerAngles.z) / 5;
                Pad.eulerAngles = new Vector3(0,0,ro);
            }
        } else if (padMode){
            Animator padAni = Pad.transform.parent.GetComponent<Animator>();
            padAni.SetFloat("speed",-2.0f);
            padAni.Play("MovePad",0,1f);
            padMode = false;
        }

        // 检测键盘输入
        if(!isWalkTask){
            if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
                Move(-1, 0); 
            }else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
                Move(1, 0);
            }else if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
                Move(0, 1);
            }else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
                Move(0, -1);
            }
        }

        lpos = pos;

        // 更新图片
        UploadWalk();

    }
}
