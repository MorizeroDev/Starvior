using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckObj : MonoBehaviour
{
    public TextAsset Script;
    private DramaScript scriptCarrier = new DramaScript();
    public static bool CheckBtnPressed;
    public List<Chara.walkDir> AllowDirection = new List<Chara.walkDir>(3);
    public int CheckType = 0;
    public string Content;
    private void OnTriggerStay2D(Collider2D other) {
        if(other.gameObject == MapCamera.PlayerCollider) {
            if(AllowDirection.Contains(MapCamera.Player.dir)){
                MapCamera.HitCheck = this.gameObject;
                MapCamera.HitCheckTransform = this.transform.parent;
                if(CheckType == 0) MapCamera.mcamera.CheckText.sprite = MapCamera.mcamera.CheckFore;
                if(CheckType == 1) MapCamera.mcamera.CheckText.sprite = MapCamera.mcamera.TalkFore;
            }else if(MapCamera.HitCheck == this.gameObject){
                MapCamera.HitCheck = null;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(MapCamera.HitCheck == this.gameObject) MapCamera.HitCheck = null;
    }

    public bool IsActive(){
        if(MapCamera.HitCheck != this.gameObject) return false;
        bool ret = (Input.GetKeyUp(KeyCode.Z) || CheckBtnPressed);
        if(MapCamera.SuspensionDrama) ret = false;
        CheckBtnPressed = false;
        return ret;
    }
    
    public virtual void Update() {
        if(!IsActive()) return;

        MapCamera.SuspensionDrama = true;
        if(Script != null){
            scriptCarrier.code = Script.text.Split(new string[]{"\r\n"},System.StringSplitOptions.RemoveEmptyEntries);
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
