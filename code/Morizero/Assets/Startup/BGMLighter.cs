using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// BGM闪烁UI
public class BGMLighter : MonoBehaviour
{
    public Image left,right;
    public AudioSource bgm;
    float time; bool channel;
    void Start()
    {
        
    }
    void Update()
    {
        float[] f = new float [8192];
        float total = 0;
        bgm.GetSpectrumData(f, 0, FFTWindow.BlackmanHarris);
        for(int i = 0;i < f.Length;i++) 
            total += f[i];
        total /= 3f;
        if(total > 1) total = 1;
        time += Time.deltaTime;
        if(total < 0.2f && time >= 3f) 
        {
            time = 0; 
            channel = !channel;
            left.gameObject.SetActive(channel);
            right.gameObject.SetActive(!channel);
        }
        left.color = new Color(1,1,1,total);
        right.color = new Color(1,1,1,total);
    }
}
