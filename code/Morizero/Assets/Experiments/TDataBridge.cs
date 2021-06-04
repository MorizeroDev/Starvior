using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MyNamespace.databridge
{
    //--------------------unique parament classes--------------------//

    #region
    public interface IBridgeTaskBuilder
    {
        void BuildOrigin(Object originObject);
        void BuildParament(IParament parament);
        void BuildDestnation(Object destnationObject);
        BridgeTask GetRequest();
    }
    public class BridgeTask
    {
        public BridgeTask(int actionCount)
        {
            unityActions = new UnityAction[actionCount];
        }
        public Object originObject;
        public IParament parament;
        public Object destnationObject;
        public UnityAction[] unityActions; 
    }
    public interface IParament
    {
        void getValue();
    }
    #endregion
    public class TDataBridge : MonoBehaviour
    {
        public Queue<BridgeTask> bridgeTasks = new Queue<BridgeTask>(0);

        public void EnqueueTask(BridgeTask inTask)
        {
            bridgeTasks.Enqueue(inTask);
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            while(bridgeTasks.Count>0)
            {
                BridgeTask currentTask = bridgeTasks.Dequeue();
                
                System.Type t = currentTask.originObject.GetType();
            }
        }
    }
}
