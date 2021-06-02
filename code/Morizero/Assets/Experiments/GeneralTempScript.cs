using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralTempScript : MonoBehaviour
{
    public GameObject[] wasdObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        wasdObject[0].GetComponent<SpriteRenderer>().color = (Input.GetKey(KeyCode.W) ? Color.green : Color.black);
        wasdObject[1].GetComponent<SpriteRenderer>().color = (Input.GetKey(KeyCode.A) ? Color.green : Color.black);
        wasdObject[2].GetComponent<SpriteRenderer>().color = (Input.GetKey(KeyCode.S) ? Color.green : Color.black);
        wasdObject[3].GetComponent<SpriteRenderer>().color = (Input.GetKey(KeyCode.D) ? Color.green : Color.black);
    }
}
