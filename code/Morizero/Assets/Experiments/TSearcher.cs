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

    }

    //----------------------------------------MONO----------------------------------------//
    public class TSearcher : MonoBehaviour
    {
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


        
        public bool Search(ref RayMap inRayMap, ref Queue<MyV2IPair> supplyQueue,ref List<Vector2Int> avoidList,ref Stack<MovementStatus> revMovementStack)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            System.TimeSpan timespan = stopwatch.Elapsed;

            stopwatch.Start();
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
                    if (currentPos.x == myV2IPair.previous.x) 
                    {
                        if (currentPos.y == myV2IPair.previous.y + 1)
                            revMovementStack.Push(MovementStatus.MovingUp);
                        else if (currentPos.y == myV2IPair.previous.y - 1)
                            revMovementStack.Push(MovementStatus.MovingDown);
                        else
                        {
                            //error
                        }
                    }
                    else if (currentPos.y == myV2IPair.previous.y)
                    {
                        if (currentPos.x == myV2IPair.previous.x + 1)
                            revMovementStack.Push(MovementStatus.MovingRight);
                        else if (currentPos.x == myV2IPair.previous.x - 1)
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
                    

                    #region rubbish
                    //seeking Rev Path
                    int safeCount = 0;
                    for (MyV2IPair tSeekKey = myV2IPairQueue_Saved.Dequeue(); safeCount <= 10000000 && myV2IPairQueue_Saved.Count>0;  tSeekKey = myV2IPairQueue_Saved.Dequeue(),safeCount++)
                    {
                        if (tSeekKey.current != myV2IPair.previous)
                            myV2IPairQueue_Saved.Enqueue(tSeekKey);//recycle this element
                        else //tSeekKey.first == myV2IPair.second
                        {
                            if (tSeekKey.current.x == tSeekKey.previous.x)
                            {
                                if (tSeekKey.current.y == tSeekKey.previous.y+1)
                                    revMovementStack.Push(MovementStatus.MovingUp);
                                else if (tSeekKey.current.y == tSeekKey.previous.y - 1)
                                    revMovementStack.Push(MovementStatus.MovingDown);
                                else
                                {
                                    //error
                                }
                            }
                            else if (tSeekKey.current.y == tSeekKey.previous.y)
                            {
                                if (tSeekKey.current.x == tSeekKey.previous.x + 1)
                                    revMovementStack.Push(MovementStatus.MovingRight);
                                else if (tSeekKey.current.x == tSeekKey.previous.x - 1)
                                    revMovementStack.Push(MovementStatus.MovingLeft);
                                else
                                {
                                    //error
                                }
                            }
                            else
                            {
                                if (tSeekKey.previous.x == -233 && tSeekKey.previous.y == -666)
                                {
                                    break;
                                }
                                //error
                            }

                            //update myV2IPair
                            myV2IPair = tSeekKey;
                        }
                    }
                    //debugOption
                    if (safeCount > 500) Debug.Log(safeCount);

                    timespan = stopwatch.Elapsed - timespan;
                    Debug.Log(timespan);
                    #endregion //rubbishRegion

                    stopwatch.Stop();
                    return true;
                }
                else //normal Node
                {
                    avoidList.Add(currentPos);

                    myV2IPairQueue_Saved.Enqueue(myV2IPair);

                    //enqueue
                    if( currentPos.y < inRayMap.size.y-1 && !avoidList.Contains(currentPos + new Vector2Int( 0, 1)) )
                        supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int( 0, 1), currentPos));
                    if (currentPos.x < inRayMap.size.x-1 && !avoidList.Contains(currentPos + new Vector2Int( 1, 0)) )
                        supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int( 1, 0), currentPos));
                    if (currentPos.y > 0 && !avoidList.Contains(currentPos + new Vector2Int( 0,-1)))
                        supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int( 0,-1), currentPos));
                    if (currentPos.x > 0 && !avoidList.Contains(currentPos + new Vector2Int(-1, 0)))
                        supplyQueue.Enqueue(new MyV2IPair(currentPos + new Vector2Int(-1, 0), currentPos));
                }
            }

            stopwatch.Stop();
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

            for (int i = 0; i < rayMap.size.x; i++)
                for (int j = 0; j < rayMap.size.y; j++)
                    if (rayMap.buffer[i, j]) avoidList.Add(new Vector2Int(i, j));

            supplyQueue.Enqueue(new MyV2IPair(rayMap.startPoint,new Vector2Int(-233,-666)) );
            // (-233,-666) is a hooked start point XD

            if (Search(ref rayMap, ref supplyQueue, ref avoidList, ref tMovementStack))
            {
                while (tMovementStack.Count > 0)
                {
                    _PushOut(tMovementStack.Pop());
                }
            }
            else { }

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
