using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckBtn : MonoBehaviour
{
    private void OnMouseUp() {
        CheckObj.CheckBtnPressed = true;
    }
}
