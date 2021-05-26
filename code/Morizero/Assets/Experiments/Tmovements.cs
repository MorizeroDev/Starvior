using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using TRayMapBuilder_myNamespace;
using EditorControl_myNamespace;

namespace testMovements_myNamespace
{
    public enum MovementStatus
    {
        MovingUp,
        MovingRight,
        MovingDown,
        MovingLeft,
        Resting,
        Completed,
        Start,
        Special_SkipFrame
    }

    //----------------------------------------MONO----------------------------------------//
    public class Tmovements : MonoBehaviour
    {
        //public GameObject OrderFrom;
        public GameObject character; // information taker
        private Transform cT;

        private Vector2 tileSize;
        public Queue<MovementStatus> statusQueue = new Queue<MovementStatus>();

        public UnityEvent<MovementStatus> inMovementsEvent;
        public UnityEvent inContinueQueueUnitEvent;
        public UnityEvent inClearQueueEvent;
        
        public float movementExtendCount;
        public Vector2 preRestingPos;

        public float speed;

        public MovementStatus nowStatus;
        public MovementStatus nextStatus;
        public void EnqueueStatus(MovementStatus s)
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

        public void ClearQueue()
        {
            nowStatus = MovementStatus.Special_SkipFrame;
            statusQueue.Clear();
        }

        public void ContinueQueueUnit()
        {
            nowStatus = MovementStatus.Special_SkipFrame; //Stop the movements when a new movement order is clicked

            //[Tip] you can add a bool parament to control this, don't Modifing the UnityEvents XD

            while(statusQueue.Count>0)
                if (statusQueue.Dequeue() == MovementStatus.Completed)
                    break;
        }

        private void ExecMove()
        {
            movementExtendCount += speed * Time.deltaTime;
            if ( nowStatus!=nextStatus && ((nowStatus == MovementStatus.MovingLeft || nowStatus == MovementStatus.MovingRight) ? 
                        (movementExtendCount >= tileSize.x) :
                        (movementExtendCount >= tileSize.y))) //[Tip]wish vomit not vomit your yesterdaymeal when u see this. XD
            {
                switch (nowStatus)
                {
                    case MovementStatus.MovingUp:
                        cT.position = new Vector2(preRestingPos.x,              preRestingPos.y + tileSize.y);
                        break;
                    case MovementStatus.MovingRight:
                        cT.position = new Vector2(preRestingPos.x + tileSize.x, preRestingPos.y);
                        break;
                    case MovementStatus.MovingDown:
                        cT.position = new Vector2(preRestingPos.x             , preRestingPos.y - tileSize.y);
                        break;
                    case MovementStatus.MovingLeft:
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
                nowStatus = MovementStatus.Resting;
                //EditorControl.EditorPause();
            }
            else
            {
                switch(nowStatus)
                {
                    case MovementStatus.MovingUp:
                        cT.position += new Vector3(0, speed * Time.deltaTime, 0);
                        break;
                    case MovementStatus.MovingRight:
                        cT.position += new Vector3(speed * Time.deltaTime, 0, 0);
                        break;
                    case MovementStatus.MovingDown:
                        cT.position += new Vector3(0, -speed * Time.deltaTime, 0);
                        break;
                    case MovementStatus.MovingLeft: 
                        cT.position += new Vector3(-speed * Time.deltaTime, 0, 0);
                        break;
                    default:
                        //Error
                        break;
                }
                if(nowStatus == nextStatus && ((nowStatus == MovementStatus.MovingLeft || nowStatus == MovementStatus.MovingRight) ?
                        (movementExtendCount >= tileSize.x) :
                        (movementExtendCount >= tileSize.y)))
                {
                    preRestingPos = cT.position;
                    movementExtendCount = 0;
                    nowStatus = MovementStatus.Resting;
                }
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            nowStatus = MovementStatus.Completed;
            inMovementsEvent.AddListener(EnqueueStatus);
            inContinueQueueUnitEvent.AddListener(ContinueQueueUnit);
            inClearQueueEvent.AddListener(ClearQueue);
        }

        private bool _IsInEnqueueableMode(MovementStatus s)
        {
            if (
                s == MovementStatus.Completed ||
                s == MovementStatus.Start ||
                s == MovementStatus.Resting
                )
                return true;
            return false;
        }
        
        // Update is called once per frame
        private void Update()
        {
            if(statusQueue.Count > 0 && _IsInEnqueueableMode(nowStatus) )
            {
                nowStatus = statusQueue.Dequeue();
                if (nowStatus != MovementStatus.Completed)
                    nextStatus = statusQueue.Peek();
                else
                    nextStatus = MovementStatus.Start;
            }

            switch (nowStatus)
            {
                case MovementStatus.MovingUp:
                case MovementStatus.MovingRight:
                case MovementStatus.MovingDown:
                case MovementStatus.MovingLeft:
                    ExecMove();//contains "nowStatus = Status.Resting;" when it is compeletely done
                    break;
                case MovementStatus.Resting:
                    //[Tip]This may cause warning, means the queue let skip up one frame
                    break;
                case MovementStatus.Completed:
                    //[Tip]normally, this condition appears when the queue is empty (so this status is the tail element)
                    break;
                case MovementStatus.Start:
                    preRestingPos = cT.position;
                    movementExtendCount = 0;
                    //[Tip]Normally start signal, head.
                    break;
                case MovementStatus.Special_SkipFrame:
                    nowStatus = MovementStatus.Start;
                    break;
                default:
                    //?
                    break;
            }
            
        }
    }
}