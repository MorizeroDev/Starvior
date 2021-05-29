using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MyNamespace.tCharaExperiment;

namespace MyNamespace.tMapCamera
{
    public class TMapCamera : MonoBehaviour
    {
        public GameObject startDot;
        public GameObject endDot;
        public static TChara Player;
        public static GameObject HitCheck;
        public static bool SuspensionDrama;
        public static TMapCamera mcamera;
        public static AudioSource bgm,bgs;
        public static WalkDirection initDir = WalkDirection.Down;
        public static int initTp = -1;

        public GameObject bindObj;
        public GameObject checkHint;
        public Sprite CheckFore,TalkFore;
        public Image CheckText;
        public AudioClip BGM,BGS;
        private float sx = float.MinValue,sy = float.MaxValue,ex = float.MaxValue,ey = float.MinValue;
    
        //游戏的FPS，可在属性窗口中修改
        public int targetFrameRate = 60;
        private void Awake() {
            //修改当前的FPS
            Application.targetFrameRate = targetFrameRate;

            if(bgm == null){
                GameObject fab = (GameObject)Resources.Load("Prefabs\\MusicPlayer");    // 载入母体
                GameObject box = Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
                bgm = box.GetComponent<AudioSource>();
                box.SetActive(true);
                box = Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
                bgs = box.GetComponent<AudioSource>();
                box.SetActive(true);
                bgs.volume = 0.5f;
                DontDestroyOnLoad(bgm);
                DontDestroyOnLoad(bgs);
            }
            if(BGM != null && bgm.clip != BGM) {bgm.clip = BGM; bgm.Play();}
            if(BGS != null && bgs.clip != BGS) {bgs.clip = BGS; bgs.Play();}
            mcamera = this;
            Vector3 cornerPos=Camera.main.ViewportToWorldPoint(new Vector3(1f,1f,Mathf.Abs(-Camera.main.transform.position.z)));
            float w = (cornerPos.x - Camera.main.transform.position.x) * 2;
            float h = (cornerPos.y - Camera.main.transform.position.y) * 2;
            Vector3 size = new Vector3(w / 2,h / 2,0f);
            Vector3 pos = startDot.transform.localPosition;
            sx = pos.x + size.x; sy = pos.y - size.y + 1.8f; 
            pos = endDot.transform.localPosition;
            ex = pos.x - size.x; ey = pos.y + size.y * 1f; 
        }
        private void FixedUpdate() {
            Vector3 t = bindObj.transform.localPosition;
            if(HitCheck != null) t = HitCheck.transform.localPosition;
            float cs = (HitCheck != null && !MapCamera.SuspensionDrama ? 1.8f : 2f);
            Vector3 pos = transform.localPosition;
            pos.x = pos.x + (t.x - pos.x) / 20;
            pos.y = pos.y + (t.y - pos.y) / 20;
            if(pos.x < sx) pos.x = sx;
            if(pos.x > ex) pos.x = ex;
            if(pos.y > sy) pos.y = sy;
            if(pos.y < ey) pos.y = ey;
            Camera camera = this.GetComponent<Camera>();
            camera.orthographicSize += (cs - camera.orthographicSize) / 20;
            transform.localPosition = pos;
            checkHint.SetActive(HitCheck != null && !MapCamera.SuspensionDrama);
        }
    }
}
