using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// BGM闪烁UI
public class BGMLighter : MonoBehaviour
{
    public List<Image> renderers;
    public AudioSource bgm;
    void Update()
    {
        float[] f = new float [8192];
        float total = 0;
        bgm.GetSpectrumData(f, 0, FFTWindow.BlackmanHarris);
        for(int i = 0;i < f.Length;i++) 
            total += f[i];
        total /= 10f;
        if(total > 1) total = 1;
        foreach(Image sr in renderers)
            sr.color = new Color(sr.color.r,sr.color.g,sr.color.b,total);
    }
}
