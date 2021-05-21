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
        movingUp,
        movingRight,
        movingDown,
        movingLeft,
        resting,
        completed
    }

    public class Tmovements : MonoBehaviour
    {
        //public GameObject OrderFrom;
        public GameObject character; // information taker
        private Transform cT;

        private Vector2 tileSize;
        public Queue<Status> statusQueue = new Queue<Status>();
        
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
            //EnqueueStatus(Status.movingRight);/**/
            //EnqueueStatus(Status.movingDown);/**/
            //EnqueueStatus(Status.movingLeft);/**/
            //EnqueueStatus(Status.movingUp);/**/
            //EnqueueStatus(Status.movingRight);/**/
            //EnqueueStatus(Status.movingRight);/**/
            //EnqueueStatus(Status.movingRight);/**/
            //EnqueueStatus(Status.movingDown);/**/
            //EnqueueStatus(Status.movingDown);/**/
            //EnqueueStatus(Status.movingLeft);/**/
            //EnqueueStatus(Status.movingUp);/**/
        }

        public void BlastStatusQueue() //danger motion
        {
            statusQueue.Clear();
        }

        private void ExecMove()
        {
            movementExtendCount += speed * Time.deltaTime;
            if ( (nowStatus == Status.movingLeft || nowStatus == Status.movingRight) ? 
                        (movementExtendCount >= tileSize.x) :
                        (movementExtendCount >= tileSize.y)) //[Tip]wish vomit not vomit your yesterdaymeal when u see this. XD
            {
                switch (nowStatus)
                {
                    case Status.movingUp:
                        cT.position = new Vector2(preRestingPos.x,              preRestingPos.y + tileSize.y);
                        break;
                    case Status.movingRight:
                        cT.position = new Vector2(preRestingPos.x + tileSize.x, preRestingPos.y);
                        break;
                    case Status.movingDown:
                        cT.position = new Vector2(preRestingPos.x             , preRestingPos.y - tileSize.y);
                        break;
                    case Status.movingLeft:
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
                nowStatus = Status.resting;
                //EditorControl.EditorPause();
            }
            else
            {
                switch(nowStatus)
                {
                    case Status.movingUp:
                        cT.position += new Vector3(0, speed * Time.deltaTime, 0);
                        break;
                    case Status.movingRight:
                        cT.position += new Vector3(speed * Time.deltaTime, 0, 0);
                        break;
                    case Status.movingDown:
                        cT.position += new Vector3(0, -speed * Time.deltaTime, 0);
                        break;
                    case Status.movingLeft: 
                        cT.position += new Vector3(-speed * Time.deltaTime, 0, 0);
                        break;
                    default:
                        //Error
                        break;
                }
            }
        }

        private void Start()
        {
            nowStatus = Status.completed;
        }

        private void Update()
        {
            if(statusQueue.Count > 0 && (nowStatus == Status.completed || nowStatus == Status.resting) )
                nowStatus = statusQueue.Dequeue();
            switch (nowStatus)
            {
                case Status.movingUp:
                case Status.movingRight:
                case Status.movingDown:
                case Status.movingLeft:
                    ExecMove();
                    break;
                case Status.resting:
                    //[Tip]This may cause warning, means the queue let skip up one frame
                    break;
                case Status.completed:
                    //[Tip]normally, this condition appears when the queue is empty ( so this status is the tail element)
                    break;
                default:
                    //?
                    break;
            }
        }
    }
}