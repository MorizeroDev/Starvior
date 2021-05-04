using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CheckBtn : MonoBehaviour
{
    public void OnClick(BaseEventData data) {
        CheckObj.CheckBtnPressed = true;
    }
}
