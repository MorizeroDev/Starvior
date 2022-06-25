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
    public static GameObject MovePad;
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
    [Tooltip("行走速度，不建议更改。")]
    public float speed = 0.06f;
    public const float step = 0.48f;
    private int freemoveFinger = 0;
    private float targetRotation = 0;
    private bool padMode = false;
    private Vector2 srcPadPos,srcClickPos;
    private Transform Pad;

    // NPC设置
    [Tooltip("当玩家靠近时会看向玩家。")]
    public bool AttractPlayer = false;
    [Tooltip("玩家离自己多近时开始看向玩家。")]
    public float AttractDistance = 2f;
    private walkDir orDir;

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
    [Tooltip("对应的人物")]
    public string Character;                    // 对应的人物
    [Tooltip("是否为玩家")]
    public bool Controller = false;             // 是否为玩家
    private SpriteRenderer image;               // 图形显示容器
    [Tooltip("当前朝向")]
    public walkDir dir;                         // 当前朝向
    private bool walking;                       // 是否正在行走
    private int walkBuff = 1;                   // 行走图系列帧序数
    private float walkspan;                     // 行走图动画间隔控制缓冲
    private float sx,sy,ex,ey;                  // 地图边界x,y - x,y
    public WalkTaskCallback walkTaskCallback;   // 行走人物回调

    private _OutMousePositionBuilder bridgeTaskbuilderOMP;

    public void ApplyMovePadSettings()
    {
        srcClickPos = MapCamera.mcamera.GetComponent<Camera>().WorldToScreenPoint(
                            GameObject.Find("MapCamera").transform.Find("MovePad").Find("PadCore").Find("tipPad").position
                            );
        srcPadPos = Pad.localPosition;
        float rate = PlayerPrefs.GetFloat("Settings.MovePad", 0.5f);
        float x = -1084 + rate * (-947 + 1084) * 2;
        float y = -523.9f + rate * (-386.9f + 523.9f) * 2;
        MovePad = GameObject.Find("MapCamera").transform.Find("MovePad").Find("PadCore").gameObject;
        MovePad.transform.localPosition = new Vector3(x, y, 0);
    }
    private void Start() {
        // 载入行走图图像集，并初始化相关设置
        Animation = Resources.LoadAll<Sprite>("Players\\" + Character);
        image = this.GetComponent<SpriteRenderer>();
        // dir = walkDir.Down;
        UpdateWalkImage();
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
            Pad = GameObject.Find("MapCamera").transform.Find("MovePad").Find("PadCore").Find("ball");
            ApplyMovePadSettings();
        }
        if(Controller) // only controller can havve a pathfinding movement
            bridgeTaskbuilderOMP = new _OutMousePositionBuilder(dataBridge.defaultRayMapPathFindingScript);
        // 如果是玩家并且传送数据不为空，则按照传送设置初始化
        if(Controller && MapCamera.initTp != -1){
            dir = MapCamera.initDir;
            UpdateWalkImage();
            // 取得传送位置坐标
            this.transform.localPosition = GameObject.Find("tp" + MapCamera.initTp).transform.localPosition;
        }
        orDir = dir;
    }
    // 更新行走图图形
    public void UpdateWalkImage(){
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
        Vector3 opos = pos;
        float buff = speed * Time.fixedDeltaTime * 60f * (Input.GetKey(KeyCode.X) ? 2 : 1);
        pos.x += buff * x ;
        pos.y += buff * y ;
        if(pos.x < sx) pos.x = sx;
        if(pos.x > ex) pos.x = ex;
        if(pos.y > sy) pos.y = sy;
        if(pos.y < ey) pos.y = ey;
        transform.Translate(new Vector3(pos.x - opos.x, pos.y - opos.y, 0), Space.World);
        //transform.localPosition = pos;
        walking = true;
        if(x != 0) dir = x < 0 ? walkDir.Left : walkDir.Right;
        if(y != 0) dir = y < 0 ? walkDir.Down : walkDir.Up;
        return y == 0 ? buff * x : buff * y;
    }

    public void ClosePadAni()
    {
        if (!padMode) return;

        Animator padAni = Pad.transform.parent.parent.GetComponent<Animator>();
        padAni.SetFloat("speed", -0.5f);
        padAni.Play("MovePad", 0, 1f);
        Pad.eulerAngles = new Vector3(0, 0, targetRotation);
        padMode = false;
        walking = false;
        UpdateWalkImage();
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

    List<Vector3> GetClickPos(int fingerId = -1){
        List<Vector3> pos = new List<Vector3>();
        Vector3 p;
        if (Input.touchSupported){
            foreach(Touch t in Input.touches){
                p = t.position;
                p.z = t.fingerId;
                if (t.fingerId == fingerId || fingerId == -1)
                    pos.Add (p);
            }
        }else{
            p = Input.mousePosition;
            p.z = -1;
            pos.Add (p);
        }
        return pos;
    }
    void NPCUpdate()
    {
        if (AttractPlayer && MapCamera.Player != null)
        {
            float dy = MapCamera.Player.transform.localPosition.y - transform.position.y;
            float dx = MapCamera.Player.transform.localPosition.x - transform.position.x;
            walkDir d = walkDir.Down;
            if(Mathf.Abs(dy) <= AttractDistance && Mathf.Abs(dx) <= AttractDistance)
            {
                if(Mathf.Abs(dx) >= Mathf.Abs(dy))
                {
                    if (dx < 0) d = walkDir.Left; else d = walkDir.Right;
                }
                else
                {
                    if (dy < 0) d = walkDir.Down; else d = walkDir.Up;
                }
                if(d != dir)
                {
                    dir = d;
                    UpdateWalkImage();
                }
            }
            else
            {
                if(orDir != dir)
                {
                    dir = orDir;
                    UpdateWalkImage();
                }
            }
        }
    }

    void FixedUpdate()
    {
        // NPC
        if (walkTasks.Count == 0 && !Controller)
        {
            NPCUpdate();
        }
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
            if(!Controller) UpdateWalkImage();
            // 修正坐标
            if(isFix){
                Debug.Log("Walktask: " + (walkTasks.Count - 1) + " remaining...");
                //FixPos();
                transform.localPosition = new Vector3(wt.x,wt.y,pos.z);
                walkTasks.Dequeue();
                if(walkTasks.Count == 0){
                    Debug.Log("Walktask: tasks for Drama Script is done.");
                    walkTaskCallback();
                    walking = false;
                    UpdateWalkImage();
                }
            }
        }
        // 如果不是玩家
        if(!Controller) return;
        //Debug.Log(GameObject.Find("MapCamera").transform.Find("MovePad").Find("Pad").transform.localPosition.y);
        // 判定调查
        Vector2 spyRay = new Vector2(pos.x,pos.y);
        if(dir == walkDir.Left){
            spyRay.x -= 0.48f;
        }else if(dir == walkDir.Right){
            spyRay.x += 0.48f;
        }else if(dir == walkDir.Up){
            spyRay.y += 0.48f;
        }else{
            spyRay.y -= 0.48f;
        }
        CheckObj checkObj = null;
        if (!CheckObj.TriggerRunning)
        {
            foreach (RaycastHit2D crash in Physics2D.RaycastAll(spyRay, new Vector2(0, 0)))
            {
                if (crash.collider.gameObject.TryGetComponent<CheckObj>(out checkObj))
                {
                    if (MapCamera.HitCheck != checkObj.gameObject && checkObj.triggerType == CheckObj.TriggerType.Passive)
                    {
                        checkObj.CheckEncounter();
                    }
                    break;
                }
            }
            if ((checkObj == null || !checkObj.AllowDirection.Contains(dir)) && MapCamera.HitCheck != null)
            {
                MapCamera.HitCheck.GetComponent<CheckObj>().CheckGoodbye();
            }
        }

        // 如果屏幕被点击
        if (Input.GetMouseButton(0))
        {
            List<Vector3> cpos = GetClickPos(freemoveFinger);
            if (!padMode)
            {
                if (!isWalkTask)
                {
                    foreach (Vector3 cp in cpos)
                    {
                        Vector3 mpos = cp; int fId = (int)cp.z;
                        mpos.z = 0;
                        mpos = MapCamera.mcamera.GetComponent<Camera>().ScreenToWorldPoint(cp);
                        foreach (RaycastHit2D hit in Physics2D.RaycastAll(new Vector2(mpos.x, mpos.y), new Vector2(0, 0)))
                        {
                            if (hit.collider.gameObject.name == "PadCore")
                            {
                                Animator padAni = Pad.transform.parent.parent.GetComponent<Animator>();
                                padAni.SetFloat("speed", 1.0f);
                                padAni.Play("MovePad", 0, 0f);
                                padMode = true;
                                freemoveFinger = fId;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                // 从屏幕坐标换算到世界坐标
                Vector3 mpos = cpos[0];
                mpos.z = 0;
                // 测算
                float xp = mpos.x - srcClickPos.x, yp = mpos.y - srcClickPos.y;
                float xpro = Mathf.Abs(xp) / 30, ypro = Mathf.Abs(yp) / 30;
                if (xpro > 1) xpro = 1;
                if (ypro > 1) ypro = 1;
                if (xpro == 1 || ypro == 1)
                {
                    if (Mathf.Abs(xp) > Mathf.Abs(yp))
                    {
                        Move(xp > 0 ? 1 : -1, 0);
                        targetRotation = (xp > 0 ? 270f : 90f);
                    }
                    else
                    {
                        Move(0, yp > 0 ? 1 : -1);
                        targetRotation = (yp > 0 ? 0f : 180f);
                    }
                }
                float ro = Pad.eulerAngles.z + (targetRotation - Pad.eulerAngles.z) / 5;
                Pad.eulerAngles = new Vector3(0, 0, ro);
            }
        }
        else if (padMode)
        {
            ClosePadAni();
        }

        // 检测键盘输入
        if (!isWalkTask)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                Move(-1, 0);
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                Move(1, 0);
            }
            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                Move(0, 1);
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                Move(0, -1);
            }
        }

        // 更新图片
        UpdateWalkImage();
    }


}
