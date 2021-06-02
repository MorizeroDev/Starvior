using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.UI;

using MyNamespace.rayMapPathFinding.rayMap;
using MyNamespace.rayMapPathFinding.myQueueWithIndex;
using MyNamespace.rayMapPathFinding.myV2IPair;
using MyNamespace.rayMapPathFinding.storageTree_Node;
using MyNamespace.rayMapPathFinding.movementStatus;
using MyNamespace.rayMapPathFinding.editorControl;

namespace MyNamespace.rayMapPathFinding
{
    namespace editorControl
    {
#if UNITY_EDITOR
        public static class EditorControl
        {
            public static void EditorPlay()
            {
                EditorApplication.isPlaying = true;
            }

            public static void EditorPause()
            {
                EditorApplication.isPaused = true;
            }

            public static void EditorStop()
            {
                EditorApplication.isPlaying = false;
            }
        }
#else
    public static class EditorControl
    {
        public static void EditorPlay()
        {
            //EditorApplication.isPlaying = true;
        }

        public static void EditorPause()
        {
            //EditorApplication.isPaused = true;
        }

        public static void EditorStop()
        {
            //EditorApplication.isPlaying = false;
        }
    }
#endif
    }
    namespace myQueueWithIndex
    {
        public class MyQueueWithIndex<T> //warning! this is a circle Queue!
        {
            public MyQueueWithIndex(int inSize = 0)
            {
                inerSize = inSize;
                buffer = new T[inerSize];
                pAdd = 0;
                pPeek = 0;
                isEmpty = inSize != 0;
            }

            public MyQueueWithIndex()
            {
                inerSize = 0;
                buffer = new T[0];
                pAdd = pPeek;
                pPeek = 0;
                isEmpty = false;
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
                {
                    pPeek = 0;
                    if (pAdd == 0) isEmpty = true;
                }
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
                    else if (pAdd == pPeek && !isEmpty) // queue full, need expand
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
                catch (System.Exception e)
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
    namespace rayMap
    {
        public class RayMap
        {
            public RayMap(RayMap inRaymap)
            {
                buffer = inRaymap.buffer;
                startPoint = inRaymap.startPoint;
                endPoint = inRaymap.endPoint;
                size = inRaymap.size;
            }

            public RayMap(Vector2Int vector2Int)
            {
                buffer = new bool[vector2Int.x, vector2Int.y];
                size = vector2Int;
            }

            public bool[,] buffer;//blocked == true && walkable == false
            public Vector2Int startPoint;
            public Vector2Int endPoint;
            public Vector2Int size; //v2 tells how big is this Raymap is

            public void LogDump(Text t)
            {
                string s = "";
                for (int j = size.y - 1; j >= 0; j--)
                {
                    for (int i = 0; i < size.x; i++)
                    {
                        s += (i == startPoint.x && j == startPoint.y) ? 'S' : ((i == endPoint.x && j == endPoint.y) ? 'E' : (buffer[i, j] ? 'X' : '0'));
                    }
                    s += '\n';
                }
                Debug.Log(s);
                t.text = s;
            }
        }
    }
    namespace myV2IPair
    {
        public class MyV2IPair
        {
            public Vector2Int current;
            public Vector2Int previous;
            public int index;

            public MyV2IPair()
            {
            }

            public MyV2IPair(Vector2Int x, Vector2Int y)
            {
                this.current = x;
                this.previous = y;
            }
        }
    }
    namespace storageTree_Node
    {
        public class StorageTree
        {
            public StorageTree()
            {
                size = 64;
                data = new StorageTreeNode[size];
                currentPos = 0;
            }

            public int Add(MyV2IPair myV2IPair)
            {
                if (_IsPosEnd())
                    _Expand();
                data[currentPos] = new StorageTreeNode(myV2IPair.current);
                for (int i = 0; i < currentPos; i++)
                {
                    if (data[i].data == myV2IPair.previous)
                    {
                        data[currentPos].father = data[i];

                        StorageTreeNode[] tLeavesArray = new StorageTreeNode[data[i].leavesCount + 1];
                        for (int j = 0; i < data[i].leavesCount; j++)
                            tLeavesArray[j] = data[i].leaves[j];
                        tLeavesArray[data[i].leavesCount] = data[currentPos];
                        data[i].leaves = tLeavesArray;

                        break;
                    }
                }
                currentPos++;
                return currentPos - 1;
            }

            private void _Expand()
            {
                size *= 2;
                StorageTreeNode[] tdata = new StorageTreeNode[size];
                for (int i = 0; i < currentPos; i++)
                    tdata[i] = data[i];
                data = tdata;
            }

            private bool _IsPosEnd()
            {
                return (currentPos == size - 1);
            }

            private int size;
            private int currentPos;
            public StorageTreeNode[] data;
        }
        public class StorageTreeNode
        {
            StorageTreeNode()
            {

            }

            public StorageTreeNode(Vector2Int vector2Int, bool isRoot = false)
            {
                data = vector2Int;
                if (isRoot)
                    father = null;
            }

            public StorageTreeNode father = null;
            public Vector2Int data;
            public StorageTreeNode[] leaves = new StorageTreeNode[0];
            public int leavesCount = 0;
        }
    }
    namespace movementStatus
    {
        public enum MovementStatus
        {
            MovingUp,
            MovingRight,
            MovingDown,
            MovingLeft,
            Completed,
            Start,

            //Resting,
            //Special_SkipFrame,
        }
    }

    //--------------------MONOBEHAVIOUR--------------------//
    public class RayMapPathFinding : MonoBehaviour
    {
        #region EditorFeed
        //public float translator_fixSpeed;
        public Chara chara;

        public GameObject movementEndObject_Prefab;//The object that carries the 

        public SpriteRenderer moveArrowSpriteRenderer;

        public GameObject character; // Main Player's character Object
        private Transform cT;//character Transform
        private Vector2 centerPos;

        //This element is from Component Chara
        //RayMapBuilder's Entrance

        public GameObject prefab;//used for build visual tips
        public RayMap rayMap;
        public bool allowVisualStatus = false;//allow visualize Searching Points
        public Vector2 tileSize;
        public Vector2 pictureSize;// floatVector2, for how big this area u want to search (RayCast effect area)

        private IEnumerator coroutineWorkhandle_RayMap;
        private GameObject TempFather;
        private GameObject _movementEndObject;



        //Searcher's Entrance (activated by RayMapBuilder)
        [HideInInspector]
        public UnityEvent<RayMap> inRayMapEvent;

        //Translator's Updating
        [HideInInspector]
        public UnityEvent<MovementStatus> inMovementsEvent;
        private Chara.walkTask walkTaskUnitOut;

        private Queue<MovementStatus> _queueOfMovementStatus = new Queue<MovementStatus>();

        [HideInInspector]
        public UnityEvent inClearQueueEvent = new UnityEvent();
        #endregion  //EditorFeed

        #region RayMapBuilder
        //----------Entrance----------//
        private void _Shoot(Vector2 outArrowPosition)
        {
            if (!_movementEndObject)
                _movementEndObject = Instantiate(movementEndObject_Prefab);
            _movementEndObject.transform.position = outArrowPosition;
            _movementEndObject.name = "movementEndObject";
            if (coroutineWorkhandle_RayMap != null)
                StopCoroutine(coroutineWorkhandle_RayMap);
            centerPos = cT.position;
            Vector2Int sizeInt = new Vector2Int((int)(pictureSize.x / tileSize.x), (int)(pictureSize.y / tileSize.y));
            rayMap = new RayMap(sizeInt);
            Vector2Int centerPosInt = new Vector2Int(sizeInt.x / 2, sizeInt.y / 2);
            rayMap.startPoint = centerPosInt;
            
            #region Vector2_to_Vector2Int
            if (outArrowPosition.x > (centerPos.x - tileSize.x / 2))
            {
                int t_x = 0;
                for (; outArrowPosition.x > (centerPos.x + tileSize.x / 2); outArrowPosition -= new Vector2(tileSize.x, 0))
                {
                    t_x++;
                }

                rayMap.endPoint = new Vector2Int(t_x + centerPosInt.x, 0);
            }
            else
            {
                int t_x = 0;
                for (; outArrowPosition.x < (centerPos.x - tileSize.x / 2); outArrowPosition += new Vector2(tileSize.x, 0))
                {
                    t_x--;
                }

                rayMap.endPoint = new Vector2Int(t_x + centerPosInt.x, 0);
            }

            if (outArrowPosition.y > (centerPos.y - tileSize.y / 2))
            {
                int t_y = 0;
                for (; outArrowPosition.y > (centerPos.y + tileSize.y / 2); outArrowPosition -= new Vector2(0, tileSize.y))
                {
                    t_y++;
                }

                rayMap.endPoint += new Vector2Int(0, t_y + centerPosInt.y);
            }
            else
            {
                int t_y = 0;
                for (; outArrowPosition.y < (centerPos.y - tileSize.y / 2); outArrowPosition += new Vector2(0, tileSize.y))
                {
                    t_y--;
                }

                rayMap.endPoint += new Vector2Int(0, t_y + centerPosInt.y);
            }

            //Debug.Log("centerPoint: " + rayMap.startPoint.x + " " + rayMap.startPoint.y);
            //Debug.Log("endPoint: " + rayMap.endPoint.x + " " + rayMap.endPoint.y);

            #endregion

            if (TempFather)
            {
                Destroy(TempFather);
            }
            TempFather = new GameObject("Father_TempObjects");
            TempFather.transform.position = new Vector3(0, 0, 0);
            coroutineWorkhandle_RayMap = _CoroutineWork(rayMap, sizeInt, centerPosInt);
            StartCoroutine(coroutineWorkhandle_RayMap);
        }
        private bool _ReturnRayResult(Vector2 inPosition,int inLayerMaskValue = 1<<3 /*Only Furniture for default*/)
        {
            LayerMask layerMask = new LayerMask();
            layerMask.value = inLayerMaskValue;

            RaycastHit2D[] hit2D = Physics2D.RaycastAll(inPosition, Vector2.zero, Mathf.Infinity, layerMask);

            foreach (RaycastHit2D t_hit in hit2D)
            {
                if (!t_hit.collider.isTrigger && t_hit.collider.gameObject.tag != "Player")//[Tip]this tag_name may cause some problem
                {
                    return true;
                }
                else
                {
                    continue;
                }
            }
            return false;
        }
        private void _TvisualizeObject(Vector2 position, bool blocked, GameObject father, bool isEndPoint = false, bool isStartPoint = false)
        {
            GameObject t = Instantiate(prefab);
            t.transform.parent = father.transform;
            t.transform.position = position;
            if (isStartPoint)
            {
                t.GetComponent<SpriteRenderer>().color = Color.yellow;
                return;
            }
            if (!blocked & isEndPoint)
            {
                t.GetComponent<SpriteRenderer>().color = Color.blue;
                return;
            }
            if (blocked)
                t.GetComponent<SpriteRenderer>().color = Color.red;
        }
        private IEnumerator _CoroutineWork(RayMap rayMap, Vector2Int sizeInt, Vector2Int centerPosInt) //generate raymap, no-frameBlock operation
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            long counterTime = stopwatch.ElapsedMilliseconds;
            for (int i = 0; i < sizeInt.x; i++)
            {
                for (int j = 0; j < sizeInt.y; j++)
                {
                    rayMap.buffer[i, j] = _ReturnRayResult(new Vector2(((i - centerPosInt.x) * tileSize.x) + centerPos.x, ((j - centerPosInt.y) * tileSize.y) + centerPos.y));
                    if (allowVisualStatus) _TvisualizeObject(new Vector2(((i - centerPosInt.x) * tileSize.x) + centerPos.x, ((j - centerPosInt.y) * tileSize.y) + centerPos.y), rayMap.buffer[i, j], TempFather, (rayMap.endPoint.x == i && rayMap.endPoint.y == j), (rayMap.startPoint.x == i && rayMap.startPoint.y == j));
                }
                if (stopwatch.ElapsedMilliseconds - counterTime >= 1000 * Time.fixedDeltaTime)
                {
                    counterTime = stopwatch.ElapsedMilliseconds;
                    yield return 0;
                }
            }
            stopwatch.Stop();
            if (rayMap.endPoint.x < rayMap.size.x && rayMap.endPoint.x >= 0 &&
               rayMap.endPoint.y < rayMap.size.y && rayMap.endPoint.x >= 0 &&
               !rayMap.buffer[rayMap.endPoint.x, rayMap.endPoint.y])
            {
                inRayMapEvent.Invoke(rayMap);
            }
            else
            {
                moveArrowSpriteRenderer.color = Color.red;
            }
            yield return 0;
        }
        #endregion

        #region Searcher
        private void _BuildQueueWork(RayMap rayMap)
        {
            if (rayMap.startPoint == rayMap.endPoint) return;
            _PushOut(MovementStatus.Start);
            Queue<MyV2IPair> supplyQueue = new Queue<MyV2IPair>();
            List<Vector2Int> avoidList = new List<Vector2Int>();
            Stack<MovementStatus> tMovementStack = new Stack<MovementStatus>();
            StorageTree storageTree = new StorageTree();

            for (int i = 0; i < rayMap.size.x; i++)
                for (int j = 0; j < rayMap.size.y; j++)
                    if (rayMap.buffer[i, j]) avoidList.Add(new Vector2Int(i, j));
            avoidList.Remove(rayMap.startPoint);//you don't want to bury yourself

            supplyQueue.Enqueue(new MyV2IPair(rayMap.startPoint, new Vector2Int(-233, -666)));
            // (-233,-666) is a hooked start point XD

            if (_Search(ref rayMap, ref supplyQueue, ref avoidList, ref tMovementStack, ref storageTree))
            {
                moveArrowSpriteRenderer.color = Color.green;
                while (tMovementStack.Count > 0)
                {
                    _PushOut(tMovementStack.Pop());
                }
            }
            else
            { }

            _PushOut(MovementStatus.Completed);
        }
        private bool _Search(ref RayMap inRayMap, ref Queue<MyV2IPair> supplyQueue, ref List<Vector2Int> avoidList, ref Stack<MovementStatus> revMovementStack, ref StorageTree storageTree)
        {
            //Queue<MyV2IPair> myV2IPairQueue_Saved = new Queue<MyV2IPair>();

            while (supplyQueue.Count > 0)
            {
                MyV2IPair myV2IPair = supplyQueue.Dequeue();

                Vector2Int currentPos = myV2IPair.current;

                if (avoidList.Contains(currentPos))
                    continue;
                else if (currentPos.x < 0 || currentPos.x > inRayMap.size.x || currentPos.y < 0 || currentPos.y > inRayMap.size.y)
                    continue;

                else if (currentPos == inRayMap.endPoint) // founded
                {
                    int tempIndexn = storageTree.Add(myV2IPair);
                    StorageTreeNode iterator = storageTree.data[tempIndexn];// this point is not registered in the tree yet

                    try
                    {
                        while (iterator.father != null)
                        {
                            if (iterator.data.x == iterator.father.data.x)
                            {
                                if (iterator.data.y == iterator.father.data.y + 1)
                                    revMovementStack.Push(MovementStatus.MovingUp);
                                else if (iterator.data.y == iterator.father.data.y - 1)
                                    revMovementStack.Push(MovementStatus.MovingDown);
                                else
                                {
                                    //error
                                }
                            }
                            else if (iterator.data.y == iterator.father.data.y)
                            {
                                if (iterator.data.x == iterator.father.data.x + 1)
                                    revMovementStack.Push(MovementStatus.MovingRight);
                                else if (iterator.data.x == iterator.father.data.x - 1)
                                    revMovementStack.Push(MovementStatus.MovingLeft);
                                else
                                {
                                    //error
                                }
                            }
                            else
                            {
                                //error
                            }
                            iterator = iterator.father;
                        }
                        return true;
                    }
                    catch (System.Exception e)
                    {
                        EditorControl.EditorPause();
                        Debug.Log(e);
                        return false;
                    }
                }
                else //normal Node
                {
                    avoidList.Add(currentPos);
                    storageTree.Add(myV2IPair);
                    //myV2IPairQueue_Saved.Enqueue(myV2IPair);

                    //enqueue
                    switch(_IdentifyDirection(myV2IPair.current,myV2IPair.previous)) //trend to walk a straight line
                    {
                        case MovementStatus.MovingUp:
                            if (currentPos.y < inRayMap.size.y - 1 && !avoidList.Contains(currentPos + new Vector2Int(0, 1)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(0, 1), currentPos));
                            }
                            if (currentPos.x < inRayMap.size.x - 1 && !avoidList.Contains(currentPos + new Vector2Int(1, 0)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(1, 0), currentPos));
                            }
                            if (currentPos.x > 0 && !avoidList.Contains(currentPos + new Vector2Int(-1, 0)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(-1, 0), currentPos));
                            }
                            if (currentPos.y > 0 && !avoidList.Contains(currentPos + new Vector2Int(0, -1)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(0, -1), currentPos));
                            }
                            break;
                        case MovementStatus.MovingRight:
                            if (currentPos.x < inRayMap.size.x - 1 && !avoidList.Contains(currentPos + new Vector2Int(1, 0)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(1, 0), currentPos));
                            }
                            if (currentPos.y < inRayMap.size.y - 1 && !avoidList.Contains(currentPos + new Vector2Int(0, 1)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(0, 1), currentPos));
                            }
                            if (currentPos.y > 0 && !avoidList.Contains(currentPos + new Vector2Int(0, -1)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(0, -1), currentPos));
                            }
                            if (currentPos.x > 0 && !avoidList.Contains(currentPos + new Vector2Int(-1, 0)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(-1, 0), currentPos));
                            }
                            break;
                        case MovementStatus.MovingDown:
                            if (currentPos.y > 0 && !avoidList.Contains(currentPos + new Vector2Int(0, -1)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(0, -1), currentPos));
                            }
                            if (currentPos.x < inRayMap.size.x - 1 && !avoidList.Contains(currentPos + new Vector2Int(1, 0)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(1, 0), currentPos));
                            }
                            if (currentPos.x > 0 && !avoidList.Contains(currentPos + new Vector2Int(-1, 0)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(-1, 0), currentPos));
                            }
                            if (currentPos.y < inRayMap.size.y - 1 && !avoidList.Contains(currentPos + new Vector2Int(0, 1)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(0, 1), currentPos));
                            }
                            break;
                        case MovementStatus.MovingLeft:
                            if (currentPos.x > 0 && !avoidList.Contains(currentPos + new Vector2Int(-1, 0)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(-1, 0), currentPos));
                            }
                            if (currentPos.y < inRayMap.size.y - 1 && !avoidList.Contains(currentPos + new Vector2Int(0, 1)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(0, 1), currentPos));
                            }
                            if (currentPos.y > 0 && !avoidList.Contains(currentPos + new Vector2Int(0, -1)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(0, -1), currentPos));
                            }
                            if (currentPos.x < inRayMap.size.x - 1 && !avoidList.Contains(currentPos + new Vector2Int(1, 0)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(1, 0), currentPos));
                            }
                            break;
                        default:
                            if (currentPos.y < inRayMap.size.y - 1 && !avoidList.Contains(currentPos + new Vector2Int(0, 1)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(0, 1), currentPos));
                            }
                            if (currentPos.x < inRayMap.size.x - 1 && !avoidList.Contains(currentPos + new Vector2Int(1, 0)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(1, 0), currentPos));
                            }
                            if (currentPos.y > 0 && !avoidList.Contains(currentPos + new Vector2Int(0, -1)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(0, -1), currentPos));
                            }
                            if (currentPos.x > 0 && !avoidList.Contains(currentPos + new Vector2Int(-1, 0)))
                            {
                                supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(-1, 0), currentPos));
                            }
                            break;
                    }
                }
            }
            return false;
        }
        private MovementStatus _IdentifyDirection(Vector2Int current, Vector2Int previous)
        {
            if (current.x == previous.x)
            {
                if (current.y == previous.y + 1)
                    return MovementStatus.MovingUp;
                else if (current.y == previous.y - 1)
                    return MovementStatus.MovingDown;
                else
                {
                    return default;
                }
            }
            else if (current.y == previous.y)
            {
                if (current.x == previous.x + 1)
                    return MovementStatus.MovingRight;
                else if (current.x == previous.x - 1)
                    return MovementStatus.MovingLeft;
                else
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        private void _PushOut(MovementStatus s)
        {
            inMovementsEvent.Invoke(s);
            //outTmovements.inMovementsEvent.Invoke(s);
        }
        #endregion

        #region Translator
        private void _TranslatorEntrance(MovementStatus movementStatus)
        {
            _queueOfMovementStatus.Enqueue(movementStatus);
            if(movementStatus == MovementStatus.Completed)
            {
                _Translate();
            }
        }
        private void _Translate()
        {
            if (_queueOfMovementStatus.Dequeue() != MovementStatus.Start)
            {
                return;
            }
            MovementStatus m_nowStatus;
            //MyQueueWithIndex<Chara.walkTask> m_myQueueWithIndex_rev = new MyQueueWithIndex<Chara.walkTask>();
            Chara.walkTask m_walkTask = new Chara.walkTask(0, 0);
            
            while(_queueOfMovementStatus.Peek()!= MovementStatus.Completed)
            {
                m_nowStatus = _queueOfMovementStatus.Dequeue();
                m_walkTask = new Chara.walkTask(
                    (m_nowStatus == MovementStatus.MovingRight) ? tileSize.x : ((m_nowStatus == MovementStatus.MovingLeft) ? -tileSize.x : 0),
                    (m_nowStatus == MovementStatus.MovingUp) ? tileSize.y : ((m_nowStatus == MovementStatus.MovingDown) ? -tileSize.y : 0),
                    true
                );
                while (_queueOfMovementStatus.Peek() == m_nowStatus)
                {
                    m_walkTask.xBuff += ( (m_nowStatus == MovementStatus.MovingRight) ? tileSize.x : ((m_nowStatus == MovementStatus.MovingLeft) ? - tileSize.x : 0)) / Chara.step;
                    m_walkTask.yBuff += ( (m_nowStatus == MovementStatus.MovingUp) ? tileSize.y : ((m_nowStatus == MovementStatus.MovingDown) ? - tileSize.y : 0)) / Chara.step;
                    _queueOfMovementStatus.Dequeue();
                }
                chara.walkTasks.Enqueue(m_walkTask);
            }
            _queueOfMovementStatus.Dequeue();
            return;
        }
        #endregion

        #region disrupter
        public void disrupter()
        {

        }
        #endregion

        #region UnityCalls
        private void Awake()
        {
            inClearQueueEvent.AddListener(disrupter);
            chara.inPosEvent.AddListener(_Shoot);
            inRayMapEvent.AddListener(_BuildQueueWork);
            inMovementsEvent.AddListener(_TranslatorEntrance);
        }
        // Start is called before the first frame update
        private void Start()
        {
            cT = character.transform;
        }

        // Update is called once per frame
        private void Update()
        {

        }
        #endregion //UnityCalls
    }
}
