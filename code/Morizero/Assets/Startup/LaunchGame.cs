using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchGame : MonoBehaviour
{
    public Animator StartAnimation;
    public GameObject WaitFor;
    public static bool isLaunched = false;
    public static bool StartupAniPlayed = false;
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
        // 控制移动端屏幕常亮
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        isLaunched = false;
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
    public void AnimationCallback(){
        // Startup Scene
        Switcher.Carry("EmptyScene");
        //Switcher.Carry("Settings");
    }
    private void Update() {
        //Camera.main.transform.eulerAngles = new Vector3(0, 0, Input.gyro.gravity.y);
        //Debuger.InstantMessage(Input.gyro.gravity.x + "," + Input.gyro.gravity.y + "," + Input.gyro.gravity.z,new Vector3(0,0,0));
        if (!WaitFor.activeSelf) return;
        bool hasSave = false;
        for(int i = 0; i < 4; i++)
        {
            if(PlayerPrefs.GetString("file" + i, "") != "") hasSave = true;
        }
        string update = "欢迎来到Alpha-628！|由于存档功能变动，载入旧版本的存档可能引发bug。|载入旧存档引发的bug不需要提交。| |这是倒数第三个Alpha版本！";
        if (hasSave)
        {
            if (isLaunched) return;
            if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Z))
            {
                isLaunched = true;
                Dramas.PopupDialog("更新须知", update, () =>
                {
                    SaveController.SaveMode = false;
                    SaveController.ShowSave();
                });
            }
        }
        else
        {
            if (isLaunched) return;
            if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Z))
            {
                isLaunched = true;
                Dramas.PopupDialog("更新须知", update, () =>
                {
                    StartAnimation.Play("StarFly", 0);
                });
            }
        }

    }
}
