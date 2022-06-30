using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartupAniCallback : MonoBehaviour
{
    public Animator StartAnimation;
    public static bool StartupAniPlayed = false;
    private void Awake()
    {
        // ¿ØÖÆÒÆ¶¯¶ËÆÁÄ»³£ÁÁ
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        LaunchGame.isLaunched = false;
        if (!StartupAniPlayed)
        {
            StartupAniPlayed = true;
        }
        else
        {
            StartAnimation.Play("StartupPerform", 0, 1.0f);
            StartAnimation.gameObject.GetComponent<AudioSource>().Play();
        }
        //Input.gyro.enabled = true;
    }
    public void PlayBGM()
    {
        this.GetComponent<AudioSource>().Play();
    }
}
