using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchGame : MonoBehaviour
{
    private void Update() {
        if(Input.GetMouseButtonUp(0)){
            Switcher.Carry("EmptyScene");
        }
    }
}
