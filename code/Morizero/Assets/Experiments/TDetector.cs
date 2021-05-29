using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyNamespace.tMovement;

public class TDetector : MonoBehaviour
{
    public Tmovements tmovements;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void OnTriggerEnter2D(Collider2D collision2D)
    {
        if (collision2D.gameObject.name == "movementEndObject")
        {
            Destroy(collision2D.gameObject);
            tmovements.inClearQueueEvent.Invoke();
        }
    }
}
