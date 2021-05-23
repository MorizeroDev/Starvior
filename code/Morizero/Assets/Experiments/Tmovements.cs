using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using TRayMapBuilder_myNamespace;
using EditorControl_myNamespace;

namespace testMovements_myNamespace
{
    public enum Status
    {
        MovingUp,
        MovingRight,
        MovingDown,
        MovingLeft,
        Resting,
        Completed,
        Start
    }

    public class Tmovements : MonoBehaviour
    {
        //public GameObject OrderFrom;
        public GameObject character; // information taker
        private Transform cT;

        private Vector2 tileSize;
        public Queue<Status> statusQueue = new Queue<Status>();

        public UnityEvent<Status> inMovementsEvent;
        
        public float movementExtendCount;
        public Vector2 preRestingPos;

        public float speed;

        public Status nowStatus;
        public void EnqueueStatus(Status s)
        {
            statusQueue.Enqueue(s);
        }

        private void Awake()
        {
            movementExtendCount = 0;
            cT = character.transform;
            preRestingPos = cT.position;
            tileSize = character.GetComponent<TRayMapBuilder>().tileSize;
        }

        public void BlastStatusQueue() //danger motion
        {
            statusQueue.Clear();
        }

        private void ExecMove()
        {
            movementExtendCount += speed * Time.deltaTime;
            if ( (nowStatus == Status.MovingLeft || nowStatus == Status.MovingRight) ? 
                        (movementExtendCount >= tileSize.x) :
                        (movementExtendCount >= tileSize.y)) //[Tip]wish vomit not vomit your yesterdaymeal when u see this. XD
            {
                switch (nowStatus)
                {
                    case Status.MovingUp:
                        cT.position = new Vector2(preRestingPos.x,              preRestingPos.y + tileSize.y);
                        break;
                    case Status.MovingRight:
                        cT.position = new Vector2(preRestingPos.x + tileSize.x, preRestingPos.y);
                        break;
                    case Status.MovingDown:
                        cT.position = new Vector2(preRestingPos.x             , preRestingPos.y - tileSize.y);
                        break;
                    case Status.MovingLeft:
                        cT.position = new Vector2(preRestingPos.x - tileSize.x, preRestingPos.y);
                        break;
                    default:
                        //Error
                        Debug.LogError("YcError: Wrong Status");
                        EditorControl.EditorPause();
                        return;
                }

                preRestingPos = cT.position;
                movementExtendCount = 0;
                nowStatus = Status.Resting;
                //EditorControl.EditorPause();
            }
            else
            {
                switch(nowStatus)
                {
                    case Status.MovingUp:
                        cT.position += new Vector3(0, speed * Time.deltaTime, 0);
                        break;
                    case Status.MovingRight:
                        cT.position += new Vector3(speed * Time.deltaTime, 0, 0);
                        break;
                    case Status.MovingDown:
                        cT.position += new Vector3(0, -speed * Time.deltaTime, 0);
                        break;
                    case Status.MovingLeft: 
                        cT.position += new Vector3(-speed * Time.deltaTime, 0, 0);
                        break;
                    default:
                        //Error
                        break;
                }
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            nowStatus = Status.Completed;
            inMovementsEvent.AddListener(EnqueueStatus);
        }

        private bool _IsInEnqueueableMode(Status s)
        {
            if (
                s == Status.Completed ||
                s == Status.Start ||
                s == Status.Resting
                )
                return true;
            return false;
        }
        
        // Update is called once per frame
        private void Update()
        {
            if(statusQueue.Count > 0 && _IsInEnqueueableMode(nowStatus) )
                nowStatus = statusQueue.Dequeue();
            switch (nowStatus)
            {
                case Status.MovingUp:
                case Status.MovingRight:
                case Status.MovingDown:
                case Status.MovingLeft:
                    ExecMove();//contains "nowStatus = Status.Resting;" when it is compeletely done
                    break;
                case Status.Resting:
                    //[Tip]This may cause warning, means the queue let skip up one frame
                    break;
                case Status.Completed:
                    //[Tip]normally, this condition appears when the queue is empty ( so this status is the tail element)
                    break;
                case Status.Start:
                    //[Tip]Normally start signal, head.
                    break;
                default:
                    //?
                    break;
            }
        }
    }
}