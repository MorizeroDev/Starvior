using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using MyNamespace.tunit;

namespace MyNamespace.databridge
{
    //--------------------unique parament classes--------------------//

    #region builder
    public abstract class BridgeTaskBuilder
    {
        protected abstract void BuildOrigin(Component originComponent);
        protected abstract void BuildParament(object parament);
        protected abstract void DefineBridgeParamentKind(BridgeParamentKind bridgeParamentKind);
        protected abstract void BuildDestnation(Component destnationComponent);
        public abstract BridgeTask GetProduct();
    }

    public class BridgeTask
    {
        public Component originComponent;
        public object parament;
        public BridgeParamentKind bridgeParamentKind;
        public Component destinationComponent;
    }
    
    public enum BridgeParamentKind
    {
        TUnitNormalAttackTUnit,
        TUnitPoisonAttackTUnit,
    }
    #endregion

    namespace AllowedParaments
    {
        public class PoisonAttackRequestParament
        {
            public PoisonAttackRequestParament(int indamage, float ingapTime, int inrepeatTimes)
            {
                damage = indamage;
                gapTime = ingapTime;
                repeatTimesRemaining = inrepeatTimes;
            }
            public PoisonAttackRequestParament Fade()
            {
                repeatTimesRemaining -= 1;
                return this;
            }
            public int damage;
            public float gapTime;
            public int repeatTimesRemaining;
        }
    }

    public class TDataBridge : MonoBehaviour
    {
        public Queue<BridgeTask> bridgeTasks = new Queue<BridgeTask>(0);
        public Text text;
        private int m_i = 0;
        public void EnqueueTask(BridgeTask inTask)
        {
            bridgeTasks.Enqueue(inTask);
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            //test if clog
            m_i ++ ;
            text.text = m_i.ToString();
            //

            while (bridgeTasks.Count > 0)
            {
                BridgeTask currentTask = bridgeTasks.Dequeue();
                switch(currentTask.bridgeParamentKind)
                {
                    case BridgeParamentKind.TUnitNormalAttackTUnit:
                        {
                            TUnit dC = currentTask.destinationComponent as TUnit;
                            if (dC.unit.IsDead)
                            { }
                            else
                               dC.unit.BeingAttack((int)currentTask.parament);

                        }
                        break;
                    case BridgeParamentKind.TUnitPoisonAttackTUnit:
                        {
                            TUnit dC = currentTask.destinationComponent as TUnit;
                            if (dC.unit.IsDead)
                            { }
                            else
                            {
                                StartCoroutine(
                                    dC.unit.PositionEffect(
                                        bridgeTasks, currentTask.originComponent,
                                        (AllowedParaments.PoisonAttackRequestParament)currentTask.parament,
                                        currentTask.destinationComponent
                                        ));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
