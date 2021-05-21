using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using EditorControl_myNamespace;

#region
namespace EditorControl_myNamespace
{
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
}
#endregion


namespace TRayMapBuilder_myNamespace
{
    public class RayMap
    {
        public RayMap(Vector2Int vector2Int)
        {
            buffer = new bool[vector2Int.x, vector2Int.y];
            size = vector2Int;
        }
        public bool[,] buffer;
        public Vector2Int startPoint;
        public Vector2Int endPoint;
        public Vector2Int size; //v2 tells how big is this Raymap is

        public void LogDump()
        {
            string s = "";
            for(int i = 0; i < size.x; i++)
            {
                for (int j=0;j<size.y;j++)
                {
                    s += (buffer[i, j] ? 'X' : '0');
                }
                s += '\n';
            }
            Debug.Log(s);
        }
    }


    //----------------------------------------MONO----------------------------------------//
    public class TRayMapBuilder : MonoBehaviour
    {
        public GameObject character; // Main Player's character Object
        private Transform cT;

        public Chara chara;
        public UnityEvent<Vector2> inPosEvent = new UnityEvent<Vector2>();

        public Vector2 anchorPosition;
        public GameObject prefab;

        public RayMap rayMap;
        public UnityEvent unityEvent;
        public bool allowScanStatus = true;

        public Vector2 tileSize;
        private Vector2 centerPos;
        /// <summary>
        /// floatVector2, for how big this area u want to search (RayCast effect area)
        /// </summary>
        public Vector2 pictureSize;
        

        private GameObject TempFather;
        private void TDTempFather()
        {
            Destroy(TempFather);
        }

        private bool ReturnRayResult(Vector2 position)
        {
            RaycastHit2D[] hit2D = Physics2D.RaycastAll(position, Vector2.zero);
            
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

        private void TCreateObject(Vector2 position, bool colorMe,GameObject father)
        {
            GameObject t = Instantiate(prefab);
            t.transform.parent = father.transform;
            t.transform.position = position;
            if (colorMe) t.GetComponent<SpriteRenderer>().color = Color.red;
        }

        private void _Shot(Vector2 outArrowPosition)
        {
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
                for(; outArrowPosition.x > (centerPos.x - tileSize.x/2) ; outArrowPosition -= new Vector2(tileSize.x, 0))
                {
                    t_x++;
                }

                rayMap.endPoint = new Vector2Int(t_x + centerPosInt.x, 0);
            }
            else
            {
                int t_x = 0;
                for (; outArrowPosition.x < (centerPos.x + tileSize.x / 2); outArrowPosition += new Vector2(tileSize.x, 0))
                {
                    t_x--;
                }

                rayMap.endPoint = new Vector2Int(t_x + centerPosInt.x, 0);
            }

            if (outArrowPosition.y > (centerPos.y - tileSize.y / 2))
            {
                int t_y = 0;
                for (; outArrowPosition.y > (centerPos.y - tileSize.y / 2); outArrowPosition -= new Vector2(0, tileSize.y))
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

            Debug.Log("centerPoint: " + rayMap.startPoint.x + " " + rayMap.startPoint.y);
            Debug.Log("endPoint: " + rayMap.endPoint.x + " " + rayMap.endPoint.y);
            #endregion

            TempFather = new GameObject("Father_TempObjects");
            TempFather.transform.position = new Vector3(0, 0, 0);
            
            for(int i=0;i<sizeInt.x;i++)
            {
                for(int j=0;j<sizeInt.y;j++)
                {
                    rayMap.buffer[i,j] = ReturnRayResult(new Vector2( ((i - centerPosInt.x) * tileSize.x)+ centerPos.x ,   ((j - centerPosInt.y) * tileSize.y) + centerPos.y ));
                    TCreateObject(new Vector2(((i - centerPosInt.x) * tileSize.x) + centerPos.x, ((j - centerPosInt.y) * tileSize.y) + centerPos.y), rayMap.buffer[i, j],TempFather);
                }
            }

            //rayMap.LogDump();
            //EditorControl.EditorPause();
        }

        // Start is called before the first frame update
        void Start()
        {
            cT = character.transform;
            centerPos = cT.position;
            chara.inPosEvent.AddListener(_Shot);
        }

        // Update is called once per frame
        void Update()
        { 
            if (allowScanStatus)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    TDTempFather();
                    //centerPos = gameObject.transform.position;
                    //rayMap = _Shot(tileSize, centerPos, pictureSize);
                    //rayMap.LogDump();
                }
            }
            else { }
        }
    }
}
