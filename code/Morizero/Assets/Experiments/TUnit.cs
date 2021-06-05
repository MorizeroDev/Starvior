using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Diagnostics;

using MyNamespace.databridge;
using MyNamespace.databridge.AllowedParaments;

namespace MyNamespace.tunit
{
    
    public class Unit
    {
        public Unit(int inBloodCount)
        {
            _bloodCount = inBloodCount;
            _isDead = false;
        }

        public bool IsDead {
            get
            {
                if (_isDead) return true;
                else if(_bloodCount<=0)
                {
                    _isDead = true;
                    return true;
                }
                return false;
            }
        }

        public int BloodCount { get => _bloodCount; set => _bloodCount = value; }

        #region
        
        public class NormalAttackRequestBuilder : BridgeTaskBuilder
        {
            private BridgeTask _bridgeTask = new BridgeTask();

            public BridgeTask BuildProduct(Component attacker, int damage, Component receiver)
            {
                DefineBridgeParamentKind(BridgeParamentKind.TUnitNormalAttackTUnit);

                BuildOrigin(attacker);
                BuildDestnation(receiver);
                BuildParament(damage);

                return _bridgeTask;
            }

            protected override void BuildOrigin(Component originComponent)
            {
                _bridgeTask.originComponent = originComponent;
            }
            protected override void BuildParament(object parament)
            {
                _bridgeTask.parament = parament;
            }
            protected override void DefineBridgeParamentKind(BridgeParamentKind bridgeParamentKind)
            {
                _bridgeTask.bridgeParamentKind = bridgeParamentKind;
            }
            protected override void BuildDestnation(Component destnationComponent)
            {
                _bridgeTask.destinationComponent = destnationComponent;
            }
            public override BridgeTask GetProduct()
            {
                return _bridgeTask;
            }
        }

        public IEnumerator PositionEffect(Queue<BridgeTask> bridgeTasks, Component attacker, PoisonAttackRequestParament parament, Component receiver)
        {
            if (parament.repeatTimesRemaining == 0) yield break;
            yield return new WaitForSeconds(parament.gapTime);

            NormalAttackRequestBuilder tNARequestBuilder = new NormalAttackRequestBuilder();
            PoisonAttackRequestBuilder tPARequestBuilder = new PoisonAttackRequestBuilder();
            bridgeTasks.Enqueue(tNARequestBuilder.BuildProduct(attacker, parament.damage, receiver));
            bridgeTasks.Enqueue(tPARequestBuilder.BuildProduct(attacker, parament.NormalExec(), receiver));
            yield return 0;
        }

        public class PoisonAttackRequestBuilder : BridgeTaskBuilder
        {
            private BridgeTask _bridgeTask = new BridgeTask();

            public BridgeTask BuildProduct(Component attacker, PoisonAttackRequestParament parament, Component receiver)
            {
                DefineBridgeParamentKind(BridgeParamentKind.TUnitPoisonAttackTUnit);

                BuildOrigin(attacker);
                BuildParament(parament);
                BuildDestnation(receiver);

                return _bridgeTask;
            }

            protected override void BuildOrigin(Component originComponent)
            {
                _bridgeTask.originComponent = originComponent;
            }
            protected override void BuildParament(object parament)
            {
                _bridgeTask.parament = parament;
            }
            protected override void DefineBridgeParamentKind(BridgeParamentKind bridgeParamentKind)
            {
                _bridgeTask.bridgeParamentKind = bridgeParamentKind;
            }
            protected override void BuildDestnation(Component destnationComponent)
            {
                _bridgeTask.destinationComponent = destnationComponent;
            }
            public override BridgeTask GetProduct()
            {
                return _bridgeTask;
            }
        }

        #endregion

        public int BeingAttack(int inDamage)
        {
            if (IsDead) return -1;

            _bloodCount -= inDamage;
            return _bloodCount;
        }

        private bool _isDead;
        private int _bloodCount;
    }

    public class TUnit : MonoBehaviour
    {
        public Text bloodShowText;
        public TDataBridge dataBridge;

        public TUnit TReceiverUnitComponent;

        public Unit unit;
        public int bloodCountInit;
        private Unit.NormalAttackRequestBuilder _normalAttackRequestBuilder = new Unit.NormalAttackRequestBuilder();
        private Unit.PoisonAttackRequestBuilder _poisonAttackRequestBuilder = new Unit.PoisonAttackRequestBuilder();
        public bool isAttacker;

        // Start is called before the first frame update
        void Start()
        {
            unit = new Unit(bloodCountInit);
            if(isAttacker)
            {

            }
            else
            {

            }
        }

        // Update is called once per frame
        void Update()
        {
            bloodShowText.text = unit.BloodCount.ToString() ;
            if(isAttacker)
            {
                if(Input.GetKeyDown(KeyCode.J))
                {
                    dataBridge.EnqueueTask(
                        _normalAttackRequestBuilder.BuildProduct(this, 1, TReceiverUnitComponent)
                    );
                }
                if(Input.GetKeyDown(KeyCode.P))
                {
                    dataBridge.EnqueueTask(
                        _poisonAttackRequestBuilder.BuildProduct(this,new PoisonAttackRequestParament(1,0.2f,5), TReceiverUnitComponent)
                    );
                }
                
            }
            else
            {

            }
        }
    }
}
