using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordEffect : MonoBehaviour
{
    public enum Effect{
        None,Shake,Rainbow,Rotation,Shine,HeavyShake
    }
    public float basex,basey;
    public Effect effect;
    public RectTransform rect;
    public Text text;

    void Start()
    {
        rect = this.gameObject.GetComponent<RectTransform>();
        text = this.gameObject.GetComponent<Text>();
    }
    void Update()
    {
        if(effect == Effect.HeavyShake)
            rect.localPosition = new Vector3(basex + Random.Range(-20,20),basey + Random.Range(-20,20),0);
        if(effect == Effect.Shake)
            rect.localPosition = new Vector3(basex + Random.Range(-10,10),basey + Random.Range(-10,10),0);
        if(effect == Effect.Rainbow)
            text.color = new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f),text.color.a);
        if(effect == Effect.Rotation)
            rect.localRotation = new Quaternion(0,0,Random.Range(-0.15f,0.15f),rect.localRotation.w);
        if(effect == Effect.Shine)
            text.color = new Color(text.color.r,text.color.g,text.color.b,(Time.realtimeSinceStartup - Mathf.Floor(Time.realtimeSinceStartup)));
    }
}
