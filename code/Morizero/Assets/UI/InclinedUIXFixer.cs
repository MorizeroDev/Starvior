using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InclinedUIXFixer : MonoBehaviour
{
    public float K = 1;
    public RectTransform Origin;
    private RectTransform rect;
    private float SX, SY;
    private void Start()
    {
        SX = Origin.localPosition.x;
        SY = Origin.localPosition.y;
        rect = GetComponent<RectTransform>();
    }
    void Update()
    {
        //y = kx => x = y/k
        Vector3 pos = rect.localPosition;
        float tX = (pos.y - SY) / K + SX;
        if (pos.x != tX)
        {
            pos.x = tX;
            rect.localPosition = pos;
        }
    }
}
