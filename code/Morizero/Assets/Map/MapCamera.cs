using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapCamera : MonoBehaviour
{
    public GameObject startDot;
    public GameObject endDot;
    public static Chara Player;
    public static GameObject PlayerCollider;
    public static GameObject HitCheck = null;
    public static Transform HitCheckTransform;
    public static bool SuspensionDrama = false;
    public static MapCamera mcamera = null;
    public static AudioSource bgm,bgs;
    public static Chara.walkDir initDir = Chara.walkDir.Down;
    public static int initTp = -1;

    public GameObject bindObj;
    public GameObject checkHint;
    public Sprite CheckFore,TalkFore;
    public Sprite CheckBack,TalkBack;
    public Image CheckText,CheckImg;
    public Text MapName;
    public Animator animator, PadAni;
    public AudioClip BGM,BGS;
    public float BGMRelativeOverride = 1.0f;
    public float BGSRelativeOverride = 1.0f;
    public bool Disabled = false;
    private float sx = float.MinValue,sy = float.MaxValue,ex = float.MaxValue,ey = float.MinValue;
    
    //游戏的FPS，可在属性窗口中修改
    public int targetFrameRate = 60;
    public void ApplyVolumeSettings()
    {
        if (bgm != null) bgm.volume = PlayerPrefs.GetFloat("Settings.BGMVolume", 1f) * BGMRelativeOverride;
        if (bgs != null) bgs.volume = PlayerPrefs.GetFloat("Settings.BGSVolume", 0.5f) * BGSRelativeOverride;
    }
    private void Awake() {
        //修改当前的FPS
        Application.targetFrameRate = targetFrameRate;

        Dramas.AppendHistory("");
        Dramas.AppendHistory("<" + MapName.text + ">");
        if (SuspensionDrama) PadAni.Play("MoveTip", 0, 1.0f);
        if (bgm == null){
            GameObject fab = (GameObject)Resources.Load("Prefabs\\MusicPlayer");    // 载入母体
            GameObject box = Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
            bgm = box.GetComponent<AudioSource>();
            box.SetActive(true);
            box = Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
            bgs = box.GetComponent<AudioSource>();
            box.SetActive(true);
            DontDestroyOnLoad(bgm);
            DontDestroyOnLoad(bgs);
        }
        ApplyVolumeSettings();
        if (bgm.clip != BGM)
        {
            if (BGM == null)
            {
                bgm.Stop(); bgm.clip = null;
            }
            else
            {
                bgm.clip = BGM; bgm.Play();
            }
        }
        if (bgs.clip != BGS)
        {
            if (BGS == null)
            {
                bgs.Stop(); bgs.clip = null;
            }
            else
            {
                bgs.clip = BGS; bgs.Play();
            }
        }
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
    public void FixPos()
    {
        Vector3 pos = Player.gameObject.transform.localPosition;
        pos.z = -10;
        transform.localPosition = pos;
    }
    private void FixedUpdate() {
        if (Disabled) return;
        Vector3 p = bindObj.transform.localPosition;
        float cs = (HitCheck != null && !MapCamera.SuspensionDrama ? 1.8f : 2f);
        Vector3 pos = transform.localPosition;
        pos.x = pos.x + (p.x - pos.x) / 20;
        pos.y = pos.y + (p.y - pos.y) / 20;
        if(pos.x < sx) pos.x = sx;
        if(pos.x > ex) pos.x = ex;
        if(pos.y > sy) pos.y = sy;
        if(pos.y < ey) pos.y = ey;
        Camera camera = this.GetComponent<Camera>();
        camera.orthographicSize += (cs - camera.orthographicSize) / 20;
        transform.localPosition = pos;
        if(MapCamera.SuspensionDrama && HitCheck != null)
        {
            if(animator.GetFloat("speed") == 1.0f)
                HitCheck.GetComponent<CheckObj>().CheckGoodbye();
        }
        
    }
}
