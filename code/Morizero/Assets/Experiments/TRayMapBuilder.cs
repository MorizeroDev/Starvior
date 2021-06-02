using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using MyNamespace.editorControl;
using UnityEngine.UI;

using MyNamespace.tRayMapBuilder;
using MyNamespace.tMovement;
using MyNamespace.tSearcher;
using MyNamespace.tCharaExperiment;

#region
namespace MyNamespace.editorControl
{
#if UNITY_EDITOR
    public static class EditorControl
    {
        public static void EditorPlay()
        {
            EditorApplication.isPlaying = true;
        }

        public static void EditorPause()
        {
            EditorApplication.isPaused = true;
        }

        public static void EditorStop()
        {
            EditorApplication.isPlaying = false;
        }
    }
#else
    public static class EditorControl
    {
        public static void EditorPlay()
        {
            //EditorApplication.isPlaying = true;
        }

        public static void EditorPause()
        {
            //EditorApplication.isPaused = true;
        }

        public static void EditorStop()
        {
            //EditorApplication.isPlaying = false;
        }
    }
#endif
}
#endregion


namespace MyNamespace.tRayMapBuilder
{
    public class RayMap
    {
        public RayMap(RayMap inRaymap)
        {
            buffer = inRaymap.buffer;
            startPoint = inRaymap.startPoint;
            endPoint = inRaymap.endPoint;
            size = inRaymap.size;
        }

        public RayMap(Vector2Int vector2Int)
        {
            buffer = new bool[vector2Int.x, vector2Int.y];
            size = vector2Int;
        }

        public bool[,] buffer;//blocked == true && walkable == false
        public Vector2Int startPoint;
        public Vector2Int endPoint;
        public Vector2Int size; //v2 tells how big is this Raymap is

        public void LogDump(Text t)
        {
            string s = "";
            for (int j=size.y-1;j>=0;j--)
            {
                for(int i = 0; i < size.x; i++)
                {
                    s += (i == startPoint.x && j == startPoint.y) ? 'S' : ( (i == endPoint.x && j == endPoint.y) ? 'E' :(buffer[i, j] ? 'X' : '0') );
                }
                s += '\n';
            }
            Debug.Log(s);
            t.text = s;
        }
    }
    //----------------------------------------MONO----------------------------------------//
    public class TRayMapBuilder : MonoBehaviour
    {
        public GameObject movementEndObject_Prefab;
        public GameObject character; // Main Player's character Object

        public TSearcher receiverSearcher;
        public Text t;
        public TChara tchara;
        public UnityEvent<Vector2> inPosEvent = new UnityEvent<Vector2>();
        public GameObject prefab;//used for build visual tips
        public RayMap rayMap;
        public UnityEvent unityEvent;
        public bool allowVisualStatus = false;
        public Vector2 tileSize;
        public Vector2 pictureSize;// floatVector2, for how big this area u want to search (RayCast effect area)

        private Transform cT;//character Transform
        private Vector2 centerPos;
        private IEnumerator coroutineWorkhandle;
        private GameObject TempFather;
        private GameObject _movementEndObject;
        
        private void _TDTempFather()
        {
            Destroy(TempFather);
        }
        private bool _ReturnRayResult(Vector2 position)
        {
            LayerMask layerMask = new LayerMask();
            layerMask.value = ((1 << 3) /*| (1 << 0)*/);//Only Furniture
            RaycastHit2D[] hit2D = Physics2D.RaycastAll(position, Vector2.zero,Mathf.Infinity,layerMask);
            
            foreach (RaycastHit2D t_hit in hit2D)
            {
                if (!t_hit.collider.isTrigger && t_hit.collider.gameObject.tag != "Player")//[Tip]this tag_name may cause some problem
                {
                    return true;
                }
                else
                {
                    continue;
                }
            }
            return false;
        }
        private void _TCreateObject(Vector2 position, bool colorMe,GameObject father,bool iAmEndPoint = false,bool iAmStartPoint = false)
        {
            GameObject t = Instantiate(prefab);
            t.transform.parent = father.transform;
            t.transform.position = position;
            if(iAmStartPoint)
            {
                t.GetComponent<SpriteRenderer>().color = Color.yellow;
                return;
            }
            if (!colorMe & iAmEndPoint)
            {
                t.GetComponent<SpriteRenderer>().color = Color.blue;
                return;
            }
            if (colorMe)
                t.GetComponent<SpriteRenderer>().color = Color.red;
        }
        private IEnumerator _CoroutineWork(RayMap rayMap,Vector2Int sizeInt,Vector2Int centerPosInt) //generate raymap, no-frameBlock operation
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            long counterTime = stopwatch.ElapsedMilliseconds;
            for (int i = 0; i < sizeInt.x; i++)
            {
                for (int j = 0; j < sizeInt.y; j++)
                {
                    rayMap.buffer[i, j] = _ReturnRayResult(new Vector2(((i - centerPosInt.x) * tileSize.x) + centerPos.x, ((j - centerPosInt.y) * tileSize.y) + centerPos.y));
                    if(allowVisualStatus) _TCreateObject(new Vector2(((i - centerPosInt.x) * tileSize.x) + centerPos.x, ((j - centerPosInt.y) * tileSize.y) + centerPos.y), rayMap.buffer[i, j], TempFather, (rayMap.endPoint.x == i && rayMap.endPoint.y == j), (rayMap.startPoint.x == i && rayMap.startPoint.y == j));
                }
                if (stopwatch.ElapsedMilliseconds - counterTime >= 1000*Time.fixedDeltaTime)
                {
                    counterTime = stopwatch.ElapsedMilliseconds;
                    yield return 0;
                }
            }
            stopwatch.Stop();
            if(rayMap.endPoint.x<rayMap.size.x && rayMap.endPoint.x >= 0 &&
               rayMap.endPoint.y < rayMap.size.y && rayMap.endPoint.x >= 0 &&
               !rayMap.buffer[rayMap.endPoint.x, rayMap.endPoint.y])
            {
                receiverSearcher.inRayMapEvent.Invoke(rayMap);
            }
            else
            {
                receiverSearcher.moveArrowSpriteRenderer.GetComponent<SpriteRenderer>().color = Color.red;
            }
            yield return 0;
        }
        //----------InvokeEntrance----------//
        private void _Shoot(Vector2 outArrowPosition)
        {
            if (!_movementEndObject)
                _movementEndObject = Instantiate(movementEndObject_Prefab);
            _movementEndObject.transform.position = outArrowPosition;
            _movementEndObject.name = "movementEndObject";
            if (coroutineWorkhandle!=null)
                StopCoroutine(coroutineWorkhandle);
            centerPos = cT.position;
            Vector2Int sizeInt = new Vector2Int((int)(pictureSize.x / tileSize.x), (int)(pictureSize.y / tileSize.y));
            rayMap = new RayMap(sizeInt);
            Vector2Int centerPosInt = new Vector2Int(sizeInt.x / 2, sizeInt.y / 2);
            rayMap.startPoint = centerPosInt;
            //Vector2ToVector2IntInRayMap
            #region 
            if (outArrowPosition.x> (centerPos.x - tileSize.x / 2))
            {
                int t_x = 0;
                for(; outArrowPosition.x > (centerPos.x + tileSize.x/2) ; outArrowPosition -= new Vector2(tileSize.x, 0))
                {
                    t_x++;
                }

                rayMap.endPoint = new Vector2Int(t_x + centerPosInt.x, 0);
            }
            else
            {
                int t_x = 0;
                for (; outArrowPosition.x < (centerPos.x - tileSize.x / 2); outArrowPosition += new Vector2(tileSize.x, 0))
                {
                    t_x--;
                }

                rayMap.endPoint = new Vector2Int(t_x + centerPosInt.x, 0);
            }

            if (outArrowPosition.y > (centerPos.y - tileSize.y / 2))
            {
                int t_y = 0;
                for (; outArrowPosition.y > (centerPos.y + tileSize.y / 2); outArrowPosition -= new Vector2(0, tileSize.y))
                {
                    t_y++;
                }

                rayMap.endPoint += new Vector2Int(0, t_y + centerPosInt.y);
            }
            else
            {
                int t_y = 0;
                for (; outArrowPosition.y < (centerPos.y - tileSize.y / 2); outArrowPosition += new Vector2(0, tileSize.y))
                {
                    t_y--;
                }

                rayMap.endPoint += new Vector2Int(0, t_y + centerPosInt.y);
            }

            //Debug.Log("centerPoint: " + rayMap.startPoint.x + " " + rayMap.startPoint.y);
            //Debug.Log("endPoint: " + rayMap.endPoint.x + " " + rayMap.endPoint.y);

            #endregion
            if(TempFather)
            {
                _TDTempFather();
            }
            TempFather = new GameObject("Father_TempObjects");
            TempFather.transform.position = new Vector3(0, 0, 0);
            coroutineWorkhandle = _CoroutineWork(rayMap, sizeInt, centerPosInt);
            StartCoroutine(coroutineWorkhandle);
        }
        private void Start()
        {
            cT = character.transform;
            centerPos = cT.position;
            tchara.inPosEvent.AddListener(_Shoot);
        }
        private void Update()
        {

        }
    }
}
