using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDrama : MonoBehaviour
{
    public bool Testing;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnMouseUp()
    {
        // 测试对话喽
        if(Testing) return;
        Testing = true;
        Dramas.Launch("Drama1",() => {
            Testing = false;
        });
    }
}
