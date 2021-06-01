using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGeneralTest : MonoBehaviour
{
    public Chara chara;
    // Start is called before the first frame update
    void Start()
    {
        chara = gameObject.GetComponent<Chara>();
        Chara.walkTask walkTask;
        walkTask.x = 1;
        walkTask.y = 0;
        walkTask.xBuff = 0;
        walkTask.yBuff = 0;
        chara.walkTasks.Add(walkTask);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
