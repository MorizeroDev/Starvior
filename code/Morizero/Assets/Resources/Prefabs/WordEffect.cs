using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordEffect : MonoBehaviour
{
    public enum Effect{
        None,Shake,Rainbow,Rotation,Shine,HeavyShake,UltraShake
    }
    public float basex,basey;
    public float boxx, boxy;
    public int Index = 0;
    public Effect effect;
    public RectTransform rect, box;
    public Text text;
    public float time = 0f;

    void Start()
    {
        rect = this.gameObject.GetComponent<RectTransform>();
        box = this.transform.parent.GetComponent<RectTransform>();
        boxx = box.localPosition.x; boxy = box.localPosition.y;
        text = this.gameObject.GetComponent<Text>();
    }
    private void OnDestroy()
    {
        if (Index == 0) box.localPosition = new Vector3(boxx, boxy, 0);
    }
    void Update()
    {
        time += Time.deltaTime;
        bool resetTime = false;
        if (effect == Effect.UltraShake && time > 0.03f)
        {
            rect.localPosition = new Vector3(basex + Random.Range(-15, 15), basey + Random.Range(-15, 15), 0);
            if(Index == 0) box.localPosition = new Vector3(boxx + Random.Range(-10, 10), boxy + Random.Range(-10, 10), 0);
            resetTime = true;
        }
        if (effect == Effect.HeavyShake && time > 0.03f)
        {
            rect.localPosition = new Vector3(basex + Random.Range(-15, 15), basey + Random.Range(-15, 15), 0);
            resetTime = true;
        }  
        if(effect == Effect.Shake && time > 0.06f)
        {
            rect.localPosition = new Vector3(basex + Random.Range(-10, 10), basey + Random.Range(-10, 10), 0);
            resetTime = true;
        }
        if(effect == Effect.Rainbow && time > 0.3f)
        {
            text.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), text.color.a);
            resetTime = true;
        } 
        if(effect == Effect.Rotation && time > 0.06f)
        {
            rect.localRotation = new Quaternion(0, 0, Random.Range(-0.15f, 0.15f), rect.localRotation.w);
            resetTime = true;
        }
        if(effect == Effect.Shine && time > 0.3f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, (Time.realtimeSinceStartup - Mathf.Floor(Time.realtimeSinceStartup)));
            resetTime = true;
        }
        if (resetTime)
        {
            time = 0f;
        }
    }
}
