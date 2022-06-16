using UnityEngine;
using System.Collections;

public class LookCamera : MonoBehaviour 
{
    public float speedNormal = 10.0f;
    public float speedFast   = 50.0f;

    public float mouseSensitivityX = 5.0f;
	public float mouseSensitivityY = 5.0f;
    
	float rotY = 0.0f;
    
	void Start()
	{
        transform.localEulerAngles = new Vector3(Random.Range(-89.5f, 89.5f), Random.Range(0f, 360f), 0.0f);
        if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}

	void Update()
	{	
        // rotation        
        float rotX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivityX / 130;
        rotY += Input.GetAxis("Mouse Y") * mouseSensitivityY / 130;
        rotY = Mathf.Clamp(rotY, -89.5f, 89.5f);
        transform.localEulerAngles = new Vector3(-rotY, rotX, 0.0f);

	}
}
