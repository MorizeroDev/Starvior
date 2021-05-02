using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapCamera : MonoBehaviour
{
    public static Chara Player;
    public static GameObject HitCheck;
    public static MapCamera mcamera;

    public GameObject bindObj;
    public GameObject checkHint;
    public Sprite CheckFore,TalkFore;
    public Image CheckText;
    private float sx = float.MinValue,sy = float.MaxValue,ex = float.MaxValue,ey = float.MinValue;
    
    private void Awake() {
        mcamera = this;
        Vector3 cornerPos=Camera.main.ViewportToWorldPoint(new Vector3(1f,1f,Mathf.Abs(-Camera.main.transform.position.z)));
        float w = (cornerPos.x - Camera.main.transform.position.x) * 2;
        float h = (cornerPos.y - Camera.main.transform.position.y) * 2;
        Vector3 size = new Vector3(w / 2,h / 2,0f);
        Vector3 pos = GameObject.Find("startDot").transform.localPosition;
        sx = pos.x + size.x; sy = pos.y - size.y + 0.9f; 
        pos = GameObject.Find("endDot").transform.localPosition;
        ex = pos.x - size.x; ey = pos.y + size.y * 1f; 
       
    }
    private void FixedUpdate() {
        Vector3 t = bindObj.transform.localPosition;
        if(HitCheck != null) t = HitCheck.transform.localPosition;
        float cs = (HitCheck != null ? 1.5f : 2f);
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
        checkHint.SetActive(HitCheck != null);
    }
}
