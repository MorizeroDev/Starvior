using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using testMovements_myNamespace;
using TRayMapBuilder_myNamespace;

namespace searcher_myNamespace
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
            for(int i=0;i<currentPos;i++)
            {
                if (data[i].data == myV2IPair.previous)
                {
                    data[currentPos].father = data[i];

                    StorageTreeNode[] tLeavesArray = new StorageTreeNode[data[i].leavesCount+1];
                    for (int j = 0; i < data[i].leavesCount; j++)
                        tLeavesArray[j] = data[i].leaves[j];
                    tLeavesArray[data[i].leavesCount] = data[currentPos];
                    data[i].leaves = tLeavesArray;

                    break;
                }
            }
            currentPos++;
            return currentPos-1;
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
            return (currentPos == size-1);
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

        public StorageTreeNode(Vector2Int vector2Int,bool isRoot = false)
        {
            data = vector2Int;
            if(isRoot)
                father = null;
        }

        public StorageTreeNode father = null;
        public Vector2Int data;
        public StorageTreeNode[] leaves = new StorageTreeNode[0];
        public int leavesCount = 0;
    }

    //----------------------------------------MONO----------------------------------------//
    public class TSearcher : MonoBehaviour
    {
        public GameObject MoveArrow;
        public Text t; // outPrint, tmep
        public Tmovements outTmovements;

        public UnityEvent<RayMap> inRayMapEvent;
        

        // Start is called before the first frame update
        void Start()
        {
            inRayMapEvent.AddListener(BuildQueueWork);
            //StartCoroutine(Search_Advanced()); //STILL CONSTRUCTING! an safe Search method that do not delay so much
        }

        private void _PushOut(MovementStatus s)
        {
            outTmovements.inMovementsEvent.Invoke(s);
        }


        
        public bool Search(ref RayMap inRayMap, ref Queue<MyV2IPair> supplyQueue,ref List<Vector2Int> avoidList,ref Stack<MovementStatus> revMovementStack,ref StorageTree storageTree)
        {
            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //System.TimeSpan timespan = stopwatch.Elapsed;
            //stopwatch.Start();

            Queue<MyV2IPair> myV2IPairQueue_Saved = new Queue<MyV2IPair>();

            while(supplyQueue.Count>0)
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
                        while (iterator.father!=null)
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

                        //timespan = stopwatch.Elapsed - timespan;
                        //Debug.Log(timespan);
                        //stopwatch.Stop();
                        return true;
                    }
                    catch(System.Exception e)
                    {
                        EditorControl_myNamespace.EditorControl.EditorPause();
                        Debug.Log(e);
                        return false;
                    }
                }
                else //normal Node
                {
                    avoidList.Add(currentPos);
                    //>>>>>
                    storageTree.Add(myV2IPair);
                    //<<<<<
                    myV2IPairQueue_Saved.Enqueue(myV2IPair);

                    //enqueue
                    if( currentPos.y < inRayMap.size.y-1 && !avoidList.Contains(currentPos + new Vector2Int( 0, 1)) )
                    {
                        supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int( 0, 1), currentPos));
                    }
                    if (currentPos.x < inRayMap.size.x-1 && !avoidList.Contains(currentPos + new Vector2Int( 1, 0)) )
                    {
                        supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int( 1, 0), currentPos));
                    }
                    if (currentPos.y > 0 && !avoidList.Contains(currentPos + new Vector2Int( 0,-1)))
                    {
                        supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int( 0,-1), currentPos));
                    }
                    if (currentPos.x > 0 && !avoidList.Contains(currentPos + new Vector2Int(-1, 0)))
                    {
                        supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(-1, 0), currentPos));
                    }
                }
            }

            //stopwatch.Stop();
            return false;
        }

        

        public void BuildQueueWork(RayMap rayMap)
        {
            //rayMap.LogDump(t);
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

            supplyQueue.Enqueue(new MyV2IPair(rayMap.startPoint,new Vector2Int(-233,-666)) );
            // (-233,-666) is a hooked start point XD

            if (Search(ref rayMap, ref supplyQueue, ref avoidList, ref tMovementStack, ref storageTree))
            {
                MoveArrow.GetComponent<SpriteRenderer>().color = Color.green;
                while (tMovementStack.Count > 0)
                {
                    _PushOut(tMovementStack.Pop());
                }
            }
            else
            { }

            _PushOut(MovementStatus.Completed);
        }

        // Update is called once per frame
        void Update()
        {

        }

        //Experimental zone
        public IEnumerator Search_Advanced()//STILL CONSTRUCTING! an safe Search method that do not delay so much
        {
            int i = 0;
            while(true)
            {
                for(float currentTime = 0;currentTime<Time.fixedTime ;currentTime++)
                {
                    i++;
                }
                Debug.Log(i);
            
                yield return 0; // ³¬Ê±±ê¼Ç
            }
        }
    }
}
