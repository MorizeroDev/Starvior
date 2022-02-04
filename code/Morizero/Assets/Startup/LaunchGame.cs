using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchGame : MonoBehaviour
{
    public Animator StartAnimation;
    public GameObject WaitFor;
    bool isLaunched = false;
    public void AnimationCallback(){
        // Startup Scene
        Switcher.Carry("EmptyScene", "LoadingWhite");
    }
    private void Update() {
        if(isLaunched) return;
        if(!WaitFor.activeSelf) return;
        if(Input.GetMouseButtonUp(0)){
            isLaunched = true;
            StartAnimation.Play("StarFly", 0);
        }
    }
}
