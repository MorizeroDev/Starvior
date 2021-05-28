using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TRayMapBuilder_myNamespace;
using UnityEngine.Events;
using UnityEngine.UI;
using TMapCamera_myNamespace;

namespace TCharaExperiment_myNamespace
{
    //public delegate void WalkTaskCallback();

    // 角色控制器
    public enum WalkDirection{
        Down,Left,Right,Up
    }

    public class TChara : MonoBehaviour
    {
        //outPuts for the MoveArrow's actual position to Pathfinding System
        public Vector2 outmPos;
        public UnityEvent<Vector2> inPosEvent = new UnityEvent<Vector2>();
    
        // 朝向枚举
        
        // 行走任务
        //public struct WalkTask{
        //    public float xBuff, yBuff;               // 横纵坐标上的任务
        //    public float x, y;                       // 实际的任务坐标
        //}

        // Walk task
        public class WalkTask
        {
            WalkTask(float inDistance, WalkDirection inWalkDirection)
            {
                distance = inDistance;
                direction = inWalkDirection;
            }
            public float distance;      //tell the current destnation (vertical or horizonal line)
            public WalkDirection direction;
        }

        // 当列表长度为0时表示行走完毕
        //public List<WalkTask> walkTasks = new List<WalkTask>();

        // Queue for Walk task
        private class _MyQueueWithIndex<T> //warning! this is a circle Queue!
        {
            public _MyQueueWithIndex(int inSize = 0)
            {
                inerSize = inSize;
                buffer = new T[inerSize];
                pAdd = 0;
                pPeek = (inSize == 0) ? pAdd : pAdd + 1;
                isEmpty = !(inSize == 0);
            }

            public T Peek()
            {
                return buffer[pPeek];
            }

            public ref T referencePeek
            {
                get
                {
                    return ref buffer[pPeek];
                }
            }

            public T Pop()
            {
                if (Count == 0) return default;
                T rev = buffer[pPeek];
                if (pPeek +1 == pAdd)
                    isEmpty = true;
                if (pPeek + 1 == inerSize)
                    pPeek = 0;
                else
                    pPeek++;
                return rev;
            }
            
            public int Count
            {
                get
                {
                    if (pAdd > pPeek)
                        return pAdd - pPeek;
                    else if (pAdd < pPeek)
                    {
                        return inerSize - pPeek + pAdd;
                    }
                    else if (pAdd == pPeek)
                        if (isEmpty) return 0;
                        else return inerSize;
                    else
                    {
                        return -1; //error
                    }
                }
            }

            public void Add(T unit)
            {
                if(pAdd==pPeek && !isEmpty) // queue full, need expand
                {
                    if (inerSize == 0)
                    {
                        buffer = new T[minmalAppendValue];
                        inerSize = minmalAppendValue;
                    }
                    else
                    {
                        T[] container= new T[inerSize * 2];
                        for(int i = pPeek,count = 0; count<inerSize;  count++)
                        {
                            container[count] = buffer[i];
                            i = (i + 1 == inerSize) ? 0 : (i+1);
                        }
                        buffer = container;
                        pPeek = 0;
                        pAdd = inerSize;
                        inerSize *= 2;
                    }
                }


                buffer[pAdd] = unit;

                if (pAdd + 1 == inerSize)  //circle
                    pAdd = 0;
                else
                    pAdd++;
                    

                isEmpty = false;
            }

            private bool isEmpty = true;
            private readonly int minmalAppendValue = 2;
            private int pAdd;               //pointer to the Tail
            private int inerSize;
            private int pPeek;
            public T[] buffer;
        }

        private _MyQueueWithIndex<WalkTask> walkTaskQueue = new _MyQueueWithIndex<WalkTask>();

        /// <summary>
        /// used for shut down the FixedUpdate
        /// </summary>
        public bool debugerLock;
        public Text TipBox;

        public bool isWalkTask;

        private Sprite[] Animation;                 // 行走图图像集
        public string Character;                    // 对应的人物
        public bool Controller = false;             // 是否为玩家
        private SpriteRenderer image;               // 图形显示容器
        public WalkDirection dir;                         // 当前朝向
        private bool walking;                       // 是否正在行走
        private int walkBuff = 1;                   // 行走图系列帧序数
        private float walkspan;                     // 行走图动画间隔控制缓冲
        private float sx,sy,ex,ey;                  // 地图边界x,y - x,y
        private float tx,ty,lx,ly;                  // 目标地点x,y；上一次的坐标x,y
        public GameObject MoveArrow;                // 点击移动反馈
        private bool tMode = false;                 // 点击移动模式（TouchMode）
        //public WalkTaskCallback walkTaskCallback;   // 行走人物回调

        private void Awake() {

            _MyQueueWithIndex<char> test = new _MyQueueWithIndex<char>();
            test.Add('H');
            test.Add('e');
            test.Pop();
            test.Add('l');
            test.Add('l');
            test.Add('o');
            test.Add(' ');
            test.Add('W');
            test.Add('o');
            test.Add('r');
            test.Add('l');
            test.Add('d');
            test.Add('!');
            test.referencePeek = 'c';
            string s = "";
            while (test.Count>0)
            {
                s += test.Pop();
            }
            EditorControl_myNamespace.EditorControl.EditorStop();return;
            //>>>>>
            if (debugerLock && TipBox!=null)
                TipBox.gameObject.SetActive(true);
            else if(TipBox != null)
                TipBox.gameObject.SetActive(false);
            //<<<<<

            // 载入行走图图像集，并初始化相关设置
            Animation = Resources.LoadAll<Sprite>("Players\\" + Character);
            image = this.GetComponent<SpriteRenderer>();
            dir = WalkDirection.Down;
            UploadWalk();
            // 获取地图边界并处理
            Vector3 size = new Vector3(0.25f,0.25f,0f);
            Vector3 pos = GameObject.Find("startDot").transform.localPosition;
            sx = pos.x + size.x; sy = pos.y - size.y; 
            pos = GameObject.Find("endDot").transform.localPosition;
            ex = pos.x - size.x; ey = pos.y + size.y * 1.7f;
            // 如果是玩家则绑定至MapCamera
            if (Controller) TMapCamera.Player = this;
            // 如果是玩家并且传送数据不为空，则按照传送设置初始化
            if(Controller && MapCamera.initTp != -1){
                dir = TMapCamera.initDir;
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
            image.sprite = Animation[((int)dir * 3) + walkBuff];
        }
        private void Update()
        {
            if(Input.GetMouseButtonUp(0))
            {
                //shoot parameter via UnityEvent
                inPosEvent.Invoke(outmPos);
            }
        }

        //<<<<<

        //>>>>>

        void FixedUpdate()
        {
            if (debugerLock) goto endTag;


            // 如果不是玩家
            if(!Controller) return;
            // 如果剧本正在进行则退出
            if (MapCamera.SuspensionDrama && walkTaskQueue.Count == 0) return;
            // 是否正在执行行走任务？
            isWalkTask = (walkTaskQueue.Count > 0);
            Vector3 pos = transform.localPosition;

            // 如果有行走任务
            if(isWalkTask){
                WalkTask iterator = walkTaskQueue.Peek();

                #region old
                //// 如果坐标尚未初始化
                //if(walkTasks[0].x == 0){
                //    WalkTask t_task = walkTasks[0];
                //    t_task.x = pos.x + 0.5f * t_task.xBuff;
                //    t_task.y = pos.y + 0.5f * t_task.yBuff;
                //    walkTasks[0] = t_task;
                //}
                //if(Mathf.Abs(walkTasks[0].x - pos.x) <= 0.04f && Mathf.Abs(walkTasks[0].y - pos.y) <= 0.04f){
                //    walkTasks.RemoveAt(0);
                //    if(walkTasks.Count == 0) walkTaskCallback();
                //    UploadWalk();
                //    return;
                //}else if(walkTasks[0].x < pos.x){
                //    dir = WalkDir.Left;
                //}else if(walkTasks[0].x > pos.x){
                //    dir = WalkDir.Right;
                //}else if(walkTasks[0].y < pos.y){
                //    dir = WalkDir.Up;
                //}else if(walkTasks[0].y > pos.y){
                //    dir = WalkDir.Down;
                //}
                #endregion //old
            }

            // 如果屏幕被点击

            if (Input.GetMouseButton(0) && !isWalkTask){
                MoveArrow.GetComponent<SpriteRenderer>().color = Color.white;

                // 从屏幕坐标换算到世界坐标
                Vector3 mpos = MapCamera.mcamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
                mpos.z = 0;
                // 检查是否点击的是UI而不是地板
                if (EventSystem.current.IsPointerOverGameObject()) return;
                // 设置相关参数
                tMode = true;
                tx = mpos.x;
                ty = mpos.y;
                lx = 0;
                ly = 0;
                // 设置点击反馈
                MoveArrow.transform.localPosition = mpos;
                MoveArrow.SetActive(true);
                //prepare for Event to TRayMapBuilder
                outmPos = mpos;
            }

            if(tMode){
                /**
                    TODO：当触摸模式启动时的移动处理
                **/
                tMode = false;
            }else if(!isWalkTask){
                // 非触摸模式时，检测键盘输入
                if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
                    dir = WalkDirection.Left;
                }else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
                    dir = WalkDirection.Right;
                }else if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
                    dir = WalkDirection.Up;
                }else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
                    dir = WalkDirection.Down;
                }else{
                    UploadWalk();
                    return;
                }
            }

            pos.x += 0.05f * (dir == WalkDirection.Left ? -1 : (dir == WalkDirection.Right ? 1 : 0)) * (Input.GetKey(KeyCode.X) ? 2 : 1);
            pos.y += 0.05f * (dir == WalkDirection.Up ? 1 : (dir == WalkDirection.Down ? -1 : 0)) * (Input.GetKey(KeyCode.X) ? 2 : 1);
            if(pos.x < sx) pos.x = sx;
            if(pos.x > ex) pos.x = ex;
            if(pos.y > sy) pos.y = sy;
            if(pos.y < ey) pos.y = ey;
            transform.localPosition = pos;
            walking = true;
            UploadWalk();


            endTag:
            if (debugerLock)
            {
                if (Input.GetMouseButton(0) )
                {
                    MoveArrow.GetComponent<SpriteRenderer>().color = Color.white;

                    // 从屏幕坐标换算到世界坐标
                    Vector3 mpos = MapCamera.mcamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
                    mpos.z = 0;
                    // 检查是否点击的是UI而不是地板
                    if (EventSystem.current.IsPointerOverGameObject()) return;
                    // 设置相关参数
                    tMode = true; tx = mpos.x; ty = mpos.y; lx = 0; ly = 0;
                    // 设置点击反馈
                    MoveArrow.transform.localPosition = mpos;
                    MoveArrow.SetActive(true);
                    //prepare for Event to TRayMapBuilder
                    outmPos = mpos;
                }
            }
            else { }
        }
    }

}