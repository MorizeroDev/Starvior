using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchGame : MonoBehaviour
{
    public Animator StartAnimation;
    public GameObject WaitFor;
    public static bool isLaunched = false;

    private void OnDestroy()
    {
        //Input.gyro.enabled = false;
    }
    public void AnimationCallback(){
        // Startup Scene
        Switcher.Carry("EmptyScene");
        //Switcher.Carry("Settings");
    }
    public void Launch()
    {
        if (!WaitFor.activeSelf) return;
        if (isLaunched) return;
        bool hasSave = false;
        for (int i = 0; i < 4; i++)
        {
            if (PlayerPrefs.GetString("file" + i, "") != "") hasSave = true;
        }
        string update = "欢迎来到Alpha-630！|这是最后一个Alpha版本啦！|也是6月最后一个版本！";
        if (hasSave)
        {
            isLaunched = true;
            Dramas.PopupDialog("更新须知", update, () =>
            {
                SaveController.SaveMode = false;
                SaveController.ShowSave();
            });
        }
        else
        {
            isLaunched = true;
            Dramas.PopupDialog("更新须知", update, () =>
            {
                StartAnimation.Play("StarFly", 0);
            });
        }
    }
    private void Update() {
        //Camera.main.transform.eulerAngles = new Vector3(0, 0, Input.gyro.gravity.y);
        //Debuger.InstantMessage(Input.gyro.gravity.x + "," + Input.gyro.gravity.y + "," + Input.gyro.gravity.z,new Vector3(0,0,0));
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Z))
            Launch();

    }
}
