using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckObj : MonoBehaviour
{
    public TextAsset Script;
    private DramaScript scriptCarrier = new DramaScript();
    public static bool CheckBtnPressed;
    public static bool CheckAvaliable = false;
    public bool StartOnTrigger;
    private static float checkshowTime;
    public List<Chara.walkDir> AllowDirection = new List<Chara.walkDir>(3);
    public int CheckType = 0;
    public string Content;
    public void CheckEncounter(){
        if(AllowDirection.Contains(MapCamera.Player.dir)){
            MapCamera.HitCheck = this.gameObject;
            MapCamera.HitCheckTransform = this.transform.parent;
            if(CheckType == 0) {
                MapCamera.mcamera.CheckText.sprite = MapCamera.mcamera.CheckFore;
                MapCamera.mcamera.CheckImg.sprite = MapCamera.mcamera.CheckBack;
            }
            if(CheckType == 1) {
                MapCamera.mcamera.CheckText.sprite = MapCamera.mcamera.TalkFore;
                MapCamera.mcamera.CheckImg.sprite = MapCamera.mcamera.TalkBack;
            }
            MapCamera.mcamera.checkHint.SetActive(true);
            checkshowTime = Time.time;
            MapCamera.mcamera.animator.SetFloat("speed",1.0f);
            MapCamera.mcamera.animator.Play("CheckBtn",0,0f);
            CheckAvaliable = true;
        }
    }
    public void CheckGoodbye(){
        MapCamera.HitCheck = null;
        MapCamera.mcamera.animator.SetFloat("speed",-2.0f);
        // 如果调查框显示的时候还太短的话，直接隐藏
        if(Time.time - checkshowTime <= 0.6f)
        {
            MapCamera.mcamera.animator.Play("CheckBtn", 0, 0f);
        }
        else
        {
            MapCamera.mcamera.animator.Play("CheckBtn", 0, 1f);
        }
        CheckAvaliable = false;
    }

    public bool IsActive(){
        if(MapCamera.HitCheck != this.gameObject) return false;
        bool ret = (Input.GetKeyUp(KeyCode.Z) || CheckBtnPressed || StartOnTrigger);
        if(MapCamera.SuspensionDrama) ret = false;
        CheckBtnPressed = false;
        return ret;
    }
    
    private void Awake() {
        scriptCarrier.parent = this;
    }

    public virtual void Update() {
        if(!IsActive()) return;

        MapCamera.SuspensionDrama = true;
        MapCamera.Player.ClosePadAni();
        if (Script != null){
            scriptCarrier.code = Script.text.Split(new string[]{"\r\n"},System.StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0;i < scriptCarrier.code.Length;i++){
                scriptCarrier.code[i] = scriptCarrier.code[i].TrimStart();
            }
            scriptCarrier.currentLine = 0;
            scriptCarrier.carryTask();
            return; 
        }
        if(CheckType == 1){
            Dramas.Launch(Content,() => {
                MapCamera.SuspensionDrama = false;
            });
        }else{
            Dramas.LaunchCheck(Content,() => {
                MapCamera.SuspensionDrama = false;
            });
        }
    }
}
