using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SndPlayer : MonoBehaviour
{
    public static void Play(string snd)
    {
        GameObject fab = (GameObject)Resources.Load("Prefabs\\SndPlayer");    // ‘ÿ»Îƒ∏ÃÂ
        GameObject box = Instantiate(fab, new Vector3(0, 0, -1), Quaternion.identity);
        AudioSource a = box.GetComponent<AudioSource>();
        a.clip = Resources.Load<AudioClip>("Snd\\" + snd);
        box.SetActive(true);
        a.Play();
        DontDestroyOnLoad(box);
        Destroy(box, a.clip.length);
    }
}
