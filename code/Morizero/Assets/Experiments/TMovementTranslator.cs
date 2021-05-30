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
        public UnityEvent inContinueQueueUnitEvent;
        public UnityEvent inClearQueueEvent;

        public Queue<MovementStatus> statusQueue = new Queue<MovementStatus>();

        public TRayMapBuilder rayMapBuilder;
        public TChara tchara;

        private TChara.WalkTask _preparing_walkTask;
        private Vector2 _tileSize;

        
        private MovementStatus _keepStatus;
        private WalkDirection _Convert(MovementStatus inMovementStatus) // MovementStatus -> WalkDirection
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
            _tileSize = rayMapBuilder.tileSize;
        }

        public void EnqueueStatus(MovementStatus s)
        {
            statusQueue.Enqueue(s);
        }

        public void ClearQueue()
        {
            statusQueue.Clear();
        }

        private bool _IsXMode(MovementStatus inMovementStatus)
        {
            return inMovementStatus == MovementStatus.MovingLeft || inMovementStatus == MovementStatus.MovingRight;
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
            if (statusQueue.Count == 0) return;
            else if (statusQueue.Count > 0 && statusQueue.Peek() != MovementStatus.Start)
            {
                Debug.LogError("Yc Error: Non-Correct type of queue Unit -- Queue Broken!");
                editorControl.EditorControl.EditorPause();
                return;
            }
            else if (statusQueue.Count > 0 && statusQueue.Peek() == MovementStatus.Start)//init if there's a delicious and juicy Queue ready for my consume
            {
                statusQueue.Dequeue();

                if (statusQueue.Peek() == MovementStatus.Completed) 
                {
                    statusQueue.Dequeue();
                    return;// this is that said, I even don't want to push out an empty Queue to the following procedure :P
                }
                else
                {
                    _keepStatus = statusQueue.Dequeue();
                    if (_IsXMode(_keepStatus))
                        _preparing_walkTask = new TChara.WalkTask(_tileSize.x, _Convert(_keepStatus));
                    else
                        _preparing_walkTask = new TChara.WalkTask(_tileSize.y, _Convert(_keepStatus));
                }
            }

            while (statusQueue.Count > 0)
            {
                while(statusQueue.Peek()==_keepStatus)
                {
                    _preparing_walkTask.distance += _IsXMode(_keepStatus) ? _tileSize.x : _tileSize.y;
                    statusQueue.Dequeue();
                }
                _PushOutPrepare();

                if (statusQueue.Peek() == MovementStatus.Completed)
                {
                    statusQueue.Dequeue();
                    break;
                }
                else
                {
                    _keepStatus = statusQueue.Dequeue();
                    _preparing_walkTask = new TChara.WalkTask(_IsXMode(_keepStatus) ? _tileSize.x : _tileSize.y, _Convert(_keepStatus));
                }
            }
        }
    }
}