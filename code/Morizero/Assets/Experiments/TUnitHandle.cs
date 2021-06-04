using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MyNamespace.databridge;

namespace MyNamespace.tunithandle
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
                else if(_bloodCount>0)
                {
                    _isDead = true;
                    return true;
                }
                return false;
            }
        }




        #region
        public class Parament_Attack : IParament
        {
            public Parament_Attack(int n)
            {
                damage = n;
            }
            public int damage;

            public void getValue()
            {
                throw new System.NotImplementedException();
            }
        }
        public class NormalAttackRequestBuilder : IBridgeTaskBuilder
        {
            private BridgeTask bridgeTask = new BridgeTask(1);


            public void BuildOrigin(Object originObject)
            {
                bridgeTask.originObject = originObject;
            }

            public void BuildDestnation(Object destnationObject)
            {
                bridgeTask.destnationObject = destnationObject;
            }

            public void BuildParament(IParament parament)
            {
                bridgeTask.parament = parament;
            }
            
            public BridgeTask GetRequest()
            {
                return bridgeTask;
            }
        }

        public BridgeTask BuildAttackRequest(Object originObject,Parament_Attack parament,Object destnationObject)
        {
            NormalAttackRequestBuilder builder = new NormalAttackRequestBuilder();
            builder.BuildOrigin(originObject);
            builder.BuildParament(parament);
            builder.BuildDestnation(destnationObject);
            return builder.GetRequest();
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

    public class TUnitHandle : MonoBehaviour
    {
        public TDataBridge dataBridge;
        public int bloodCount;
        public Unit unit;
        public bool isAttacker;
        public TUnitHandle TTargetUnitHandle;

        // Start is called before the first frame update
        void Start()
        {
            unit = new Unit(bloodCount);
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
            if(isAttacker)
            {
                if(Input.GetKeyDown(KeyCode.J))
                {
                    dataBridge.EnqueueTask(unit.BuildAttackRequest(this, new Unit.Parament_Attack(3), TTargetUnitHandle));
                }
            }
            else
            {

            }
        }
    }
}
