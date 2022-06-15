using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSeaController2 : MonoBehaviour
{
    public float Speed = 1f;
    private float lastTime;
    public GameObject prefab;
    public AudioSource bgm;
    private float lastBGM;
    void Update()
    {
        float[] f = new float[8192];
        float total = 0;
        bool BGMLight = false;
        bgm.GetSpectrumData(f, 0, FFTWindow.BlackmanHarris);
        for (int i = 0; i < f.Length; i++)
            total += f[i];
        total /= 9f;
        if (total > 1) total = 1;
        if(lastBGM != total)
        {
            if (total == 1) BGMLight = true;
            lastBGM = total;
        }
        if (Time.time - lastTime > Speed || BGMLight)
        {
            lastTime = Time.time;
            GameObject go = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, transform.parent);
            go.SetActive(true);
            Speed = Random.Range(0.5f, 1.5f);
        }
    }
}
