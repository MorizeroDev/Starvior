using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DramaDispose : MonoBehaviour
{
    public void DramaDone(){
        MapCamera.SuspensionDrama = false;
        Destroy(this.gameObject);
    }
}
