using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;


namespace TRayMapBuilder
{
    public class RayMap
    {
        public RayMap(Vector2Int vector2Int)
        {
            buffer = new bool[vector2Int.x, vector2Int.y];
            size = vector2Int;
        }
        public bool[,] buffer;
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

    public class TRayMapBuilder : MonoBehaviour
    {
        public Vector2 anchorPosition;
        public GameObject prefab;

        public RayMap rayMap;
        public UnityEvent unityEvent;
        public bool allowScanStatus = true;

        public Vector2 tileSize;
        public Vector2 centerPos;
        public Vector2 pictureSize;
        #region
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
        #endregion

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

        private void TCreateObject(Vector2 position,bool colorMe)
        {
            GameObject t = Instantiate(prefab);
            t.transform.position = position;
            if (colorMe) t.GetComponent<SpriteRenderer>().color = Color.red;
        }

        private RayMap _Shot(Vector2 tileSize,Vector2 centerPos,Vector2 pictureSize)
        {
            Vector2Int sizeInt = new Vector2Int((int)(pictureSize.x / tileSize.x), (int)(pictureSize.y / tileSize.y));
            RayMap rayMap = new RayMap(sizeInt);
            Vector2Int centerPosInt = new Vector2Int(sizeInt.x / 2, sizeInt.y / 2);
            
            for(int i=0;i<sizeInt.x;i++)
            {
                for(int j=0;j<sizeInt.y;j++)
                {
                    rayMap.buffer[i,j] = ReturnRayResult(new Vector2( ((i - centerPosInt.x) * tileSize.x)+ centerPos.x ,   ((j - centerPosInt.y) * tileSize.y) + centerPos.y ));
                    TCreateObject(new Vector2(((i - centerPosInt.x) * tileSize.x) + centerPos.x, ((j - centerPosInt.y) * tileSize.y) + centerPos.y), rayMap.buffer[i, j]);
                }
            }
            return rayMap;
        }

        // Start is called before the first frame update
        void Start()
        {
            rayMap = _Shot(tileSize,centerPos,pictureSize);
            rayMap.LogDump();
            EditorPause();
        }

        // Update is called once per frame
        void Update()
        { 
            if (allowScanStatus)
            {

            }
            else { }
        }
    }
}
