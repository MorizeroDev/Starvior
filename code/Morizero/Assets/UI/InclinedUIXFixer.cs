using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InclinedUIXFixer : MonoBehaviour
{
    public float K = 1;
    public Transform Origin;
    private float SX, SY;
    private void Start()
    {
        SX = Origin.position.x;
        SY = Origin.position.y;
    }
    void Update()
    {
        //y = kx => x = y/k
        float tX = (this.transform.position.y - SY) / K + SX;
        if (this.transform.position.x != tX)
            this.transform.position = new Vector3(tX, this.transform.position.y, this.transform.position.z);
    }
}
