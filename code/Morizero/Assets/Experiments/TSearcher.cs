using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using testMovements_myNamespace;

namespace searcher_myNamespace
{
    public class TSearcher : MonoBehaviour
    {
        public Text t; // outPrint, tmep
        public Tmovements outTmovements;
        

        public UnityEvent<TRayMapBuilder_myNamespace.RayMap> inRayMapEvent;
        
        // Start is called before the first frame update
        void Start()
        {
            inRayMapEvent.AddListener(SearchStart);
        }

        private void _PushOut(Status s)
        {
            outTmovements.inMovementsEvent.Invoke(s);
        }

        public void SearchStart(TRayMapBuilder_myNamespace.RayMap rayMap)
        {
            _PushOut(Status.Start);
            //Debug.Log("startSearching!");
            //rayMap.LogDump(t);
            _PushOut(Status.Completed);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
