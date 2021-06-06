using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DramaDispose : MonoBehaviour
{
    public void DramaDone(){
        this.transform.Find("Dialog").GetComponent<Dramas>().DramaDone();
        Destroy(this.gameObject);
    }
}
