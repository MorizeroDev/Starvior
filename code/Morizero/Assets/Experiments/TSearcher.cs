using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace searcher_myNamespace
{
    public class TSearcher : MonoBehaviour
    {
        public Text t;
        public UnityEvent<TRayMapBuilder_myNamespace.RayMap> inRayMapEvent;
        // Start is called before the first frame update
        void Start()
        {
            inRayMapEvent.AddListener(SearchStart);
        }

        public void SearchStart(TRayMapBuilder_myNamespace.RayMap rayMap)
        {
            Debug.Log("startSearching!");
            rayMap.LogDump(t);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
