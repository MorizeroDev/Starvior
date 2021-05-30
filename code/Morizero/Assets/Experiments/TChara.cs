using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

using MyNamespace.tMapCamera;
using MyNamespace.tRayMapBuilder;
using MyNamespace.myQueueWithIndex;

namespace MyNamespace.myQueueWithIndex
{
    public class MyQueueWithIndex<T> //warning! this is a circle Queue!
    {
        public MyQueueWithIndex(int inSize = 0)
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

        public ref T realPeek // equals to referencePeek
        {
            get
            {
                return ref buffer[pPeek];
            }
        }

        public ref T referencePeek // equals to realPeek
        {
            get
            {
                return ref buffer[pPeek];
            }
        }

        public T Dequeue()
        {
            if (Count == 0) return default;
            T rev = buffer[pPeek];
            if (pPeek + 1 == pAdd)
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

        public void Enqueue(T unit)
        {
            try
            {
                if (inerSize == 0)
                {
                    buffer = new T[minmalAppendValue];
                    inerSize = minmalAppendValue;
                }
                if (pAdd == pPeek && !isEmpty) // queue full, need expand
                {
                    T[] container = new T[inerSize * 2];
                    for (int i = pPeek, count = 0; count < inerSize; count++)
                    {
                        container[count] = buffer[i];
                        i = (i + 1 == inerSize) ? 0 : (i + 1);
                    }
                    buffer = container;
                    pPeek = 0;
                    pAdd = inerSize;
                    inerSize *= 2;
                }


                buffer[pAdd] = unit;

                if (pAdd + 1 == inerSize)  //circle
                    pAdd = 0;
                else
                    pAdd++;


                isEmpty = false;
            }
            catch(System.Exception e)
            {
                Debug.LogError("Yc Error! at MyQueueWithIndex<T>.Enqueue");
                editorControl.EditorControl.EditorPause();
            }
        }

        /// <summary>
        /// Danger Motion!
        /// </summary>
        public void Clear()
        {
            buffer = new T[inerSize];
            isEmpty = true;
            pAdd = 0;
            pPeek = 0;
        }

        public void reInit()
        {
            buffer = new T[minmalAppendValue];
            isEmpty = true;
            pAdd = 0;
            pPeek = 0;
        }

        private bool isEmpty = true;
        private readonly int minmalAppendValue = 2;
        private int pAdd;               //pointer to the Tail
        private int inerSize;
        private int pPeek;
        public T[] buffer;
    }
}

namespace MyNamespace.tCharaExperiment
{
    //public delegate void WalkTaskCallback();

    // 角色控制器
    public enum WalkDirection{
        Down,Left,Right,Up,Default
    }


    //--------------------MONO--------------------//
    public class TChara : MonoBehaviour
    {
        public UnityEvent inClearQueueEvent = new UnityEvent();
        public UnityEvent<WalkTask> inEnqueueEvent = new UnityEvent<WalkTask>();
        //outPuts for the MoveArrow's actual position to Pathfinding System
        public Vector2 outmPos;
        public UnityEvent<Vector2> inPosEvent = new UnityEvent<Vector2>();
    
        // 朝向枚举
        
        // 行走任务
        //public struct WalkTask{
        //    public float xBuff, yBuff;               // 横纵坐标上的任务
        //    public float x, y;                       // 实际的任务坐标
        //}

        // Walk task form
        public class WalkTask
        {
            public WalkTask()
            {
                distance = 0f;
                direction = WalkDirection.Default;
            }

            public bool IsUseless
            {
                get
                {
                    return direction == WalkDirection.Default;
                }
            }

            public WalkTask(float inDistance, WalkDirection inWalkDirection)
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
        private MyQueueWithIndex<WalkTask> walkTaskQueue = new MyQueueWithIndex<WalkTask>();

        private bool _isAdjustWalkTask = false;
        private float _adjustedSneekPerFrame = 0f;

        public Text TipBox;

        public bool isWalkTask;

        public float sneekPerFrame;
        private Sprite[] Animation;                 // 行走图图像集
        public string Character;                    // 对应的人物
        public bool Controller = false;             // 是否为玩家
        private SpriteRenderer image;               // 图形显示容器
        public WalkDirection dir;                         // 当前朝向
        private bool walking_anime;                       // 是否正在行走
        private int walkBuff = 1;                   // 行走图系列帧序数
        private float walkspan;                     // 行走图动画间隔控制缓冲
        private float sx,sy,ex,ey;                  // 地图边界x,y - x,y
        //private float tx,ty,lx,ly;                  // 目标地点x,y；上一次的坐标x,y
        public GameObject MoveArrow;                // 点击移动反馈
        private bool tMode = false;                 // 点击移动模式（TouchMode）
        //public WalkTaskCallback walkTaskCallback;   // 行走人物回调

        private void Awake() {
            
            // 载入行走图图像集，并初始化相关设置
            Animation = Resources.LoadAll<Sprite>("Players\\" + Character);
            image = GetComponent<SpriteRenderer>();
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

            inEnqueueEvent.AddListener(Enqueueme);
        }
        
        // 更新行走图图形
        void UploadWalk(){
            if(walking_anime){
                // 行走时的图像
                walkspan += Time.deltaTime;
                if(walkspan > 0.1f){
                    walkBuff ++;
                    if(walkBuff > 2) walkBuff = 0;
                    walkspan = 0;
                }
                walking_anime = false;
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
            // 如果屏幕被点击
            if (Input.GetMouseButton(0))
            {
                MoveArrow.GetComponent<SpriteRenderer>().color = Color.white;

                // 从屏幕坐标换算到世界坐标
                Vector3 mpos = MapCamera.mcamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
                mpos.z = 0;
                // 检查是否点击的是UI而不是地板
                if (EventSystem.current.IsPointerOverGameObject()) return;
                // 设置相关参数
                
                //tx = mpos.x;
                //ty = mpos.y;
                //lx = 0;
                //ly = 0;

                //设置点击反馈
                MoveArrow.transform.localPosition = mpos;
                MoveArrow.SetActive(true);

                //prepare Event for TRayMapBuilder
                outmPos = mpos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                //shoot parameter via UnityEvent
                //tchara.inPosEvent.AddListener(_Shot);
                isWalkTask = true;
                walkTaskQueue.Clear();
                inPosEvent.Invoke(outmPos);
            }
        }
        
        public void Enqueueme(WalkTask task)
        {
            walkTaskQueue.Enqueue(task);
        }

        private void Start()
        {
            inClearQueueEvent.AddListener(() => {
                _isAdjustWalkTask = false;
                isWalkTask = false;
                walkTaskQueue.Clear();
            });
            editorControl.EditorControl.EditorPause();
        }

        void FixedUpdate()
        {
            // 如果不是玩家
            if (!Controller) return;
            // 如果剧本正在进行则退出
            if (MapCamera.SuspensionDrama && walkTaskQueue.Count == 0) return;
            
            Vector3 pos = transform.localPosition;

            if (walkTaskQueue.Count > 0)
            {
                WalkTask _walkTask_iterator = walkTaskQueue.referencePeek;
                if (_walkTask_iterator.IsUseless)
                {
                    UploadWalk();
                    return;
                }

                if (_walkTask_iterator.distance - sneekPerFrame < 0)
                {
                    _isAdjustWalkTask = true;
                    _adjustedSneekPerFrame = sneekPerFrame - _walkTask_iterator.distance;
                    walkTaskQueue.Dequeue();
                }
                else
                {
                    dir = _walkTask_iterator.direction;
                    _walkTask_iterator.distance -= sneekPerFrame;
                }
            }
            else
            {
                walking_anime = false;
                UploadWalk();
                return;
            }


            if (_isAdjustWalkTask) // adjust position to fit correct movement, especially turnning
            {
                pos.x += _adjustedSneekPerFrame * (dir == WalkDirection.Left ? -1 : (dir == WalkDirection.Right ? 1 : 0)) * (Input.GetKey(KeyCode.X) ? 2 : 1);
                pos.y += _adjustedSneekPerFrame * (dir == WalkDirection.Up ? 1 : (dir == WalkDirection.Down ? -1 : 0)) * (Input.GetKey(KeyCode.X) ? 2 : 1);
                _isAdjustWalkTask = false;
                UploadWalk();
                return;
            }
            pos.x += sneekPerFrame * (dir == WalkDirection.Left ? -1 : (dir == WalkDirection.Right ? 1 : 0)) * (Input.GetKey(KeyCode.X) ? 2 : 1);
            pos.y += sneekPerFrame * (dir == WalkDirection.Up ? 1 : (dir == WalkDirection.Down ? -1 : 0)) * (Input.GetKey(KeyCode.X) ? 2 : 1);
            
            //touch wall!
            if (pos.x < sx) pos.x = sx;
            if (pos.x > ex) pos.x = ex;
            if (pos.y > sy) pos.y = sy;
            if (pos.y < ey) pos.y = ey;

            transform.localPosition = pos;
            walking_anime = true;
            UploadWalk(); 
        }
    }

}