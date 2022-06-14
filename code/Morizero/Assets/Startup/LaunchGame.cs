using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchGame : MonoBehaviour
{
    public Animator StartAnimation;
    public GameObject WaitFor;
    bool isLaunched = false;

    public void PlayBGM()
    {
        this.GetComponent<AudioSource>().Play();
    }
    private void OnDestroy()
    {
        //Input.gyro.enabled = false;
    }
    private void Awake()
    {
        // ¿ØÖÆÒÆ¶¯¶ËÆÁÄ»³£ÁÁ
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //Input.gyro.enabled = true;
    }
    public void AnimationCallback(){
        // Startup Scene
        Switcher.Carry("EmptyScene", "LoadingWhite");
    }
    private void Update() {
        //Camera.main.transform.eulerAngles = new Vector3(0, 0, Input.gyro.gravity.y);
        //Debuger.InstantMessage(Input.gyro.gravity.x + "," + Input.gyro.gravity.y + "," + Input.gyro.gravity.z,new Vector3(0,0,0));
        if (isLaunched) return;
        if(!WaitFor.activeSelf) return;
        if(Input.GetMouseButtonUp(0)){
            isLaunched = true;
            StartAnimation.Play("StarFly", 0);
        }
    }
}
