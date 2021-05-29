using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using MyNamespace.tRayMapBuilder;
using MyNamespace.editorControl;
using MyNamespace.tMovement;
using MyNamespace.tCharaExperiment;

namespace MyNamespace.tMovementTranslator
{
    
    //----------------------------------------MONO----------------------------------------//
    public class TMovementTranslator : MonoBehaviour
    {
        public UnityEvent<MovementStatus> inMovementsEvent;

        private TChara.WalkTask _preparing_walkTask;
        private Vector2 tileSize;
        public Queue<MovementStatus> statusQueue = new Queue<MovementStatus>();

        public TRayMapBuilder rayMapBuilder;
        public TChara tchara;
        public UnityEvent inContinueQueueUnitEvent;
        public UnityEvent inClearQueueEvent;

        public MovementStatus nowStatus;
        public MovementStatus nextStatus;

        private WalkDirection _Convert(MovementStatus inMovementStatus)
        {
            switch (inMovementStatus)
            {
                case MovementStatus.MovingUp:
                    return WalkDirection.Up;
                case MovementStatus.MovingRight:
                    return WalkDirection.Right;
                case MovementStatus.MovingDown:
                    return WalkDirection.Down;
                case MovementStatus.MovingLeft:
                    return WalkDirection.Left;
                default:
                    return WalkDirection.Default;
            }
        }

        private void _PushOutPrepare()
        {
            tchara.inEnqueueEvent.Invoke(_preparing_walkTask);
        }

        private void Awake()
        {
            tileSize = rayMapBuilder.tileSize;
        }

        public void EnqueueStatus(MovementStatus s)
        {
            statusQueue.Enqueue(s);
        }

        public void ClearQueue()
        {
            statusQueue.Clear();
        }

        // Start is called before the first frame update
        private void Start()
        {
            inMovementsEvent.AddListener(EnqueueStatus);
            inClearQueueEvent.AddListener(ClearQueue);
            _preparing_walkTask = new TChara.WalkTask();
        }
        
        // Update is called once per frame
        private void Update()
        {
            if (statusQueue.Count > 0 && statusQueue.Peek() == MovementStatus.Start)//init
            {
                nowStatus = statusQueue.Dequeue();
                if(statusQueue.Peek() == MovementStatus.Completed)
                {
                    statusQueue.Dequeue();
                    return;
                }
                else
                {
                    _preparing_walkTask.direction = _Convert(nowStatus);
                }
            }
            else//
            {
                Debug.LogError("Yc Error: Non-Correct type of queue Unit!");
                return;
            }

            while (statusQueue.Count > 0)
            {
                //todo
            }
        }
    }
}