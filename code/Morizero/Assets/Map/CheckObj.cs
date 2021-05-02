using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckObj : MonoBehaviour
{
    public static bool CheckBtnPressed;
    public List<Chara.walkDir> AllowDirection = new List<Chara.walkDir>(3);
    public int CheckType = 0;
    public string Content;
    private void OnTriggerStay2D(Collider2D other) {
        if(other.gameObject == MapCamera.Player.gameObject) {
            if(AllowDirection.Contains(MapCamera.Player.dir)){
                MapCamera.HitCheck = this.gameObject;
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

    private void Update() {
        if(MapCamera.HitCheck != this.gameObject) return;
        if(Input.GetKeyUp(KeyCode.Z) || CheckBtnPressed){
            // 测试对话喽
            CheckBtnPressed = false;
            if(MapCamera.SuspensionDrama) return;
            if(CheckType == 1){
                Dramas.Launch(Content,() => {
                    
                });
            }else{
                Dramas.LaunchCheck(Content,() => {
                    
                });
            }

        }
    }
}
