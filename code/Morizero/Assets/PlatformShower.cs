using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformShower : MonoBehaviour
{
    public List<RuntimePlatform> TargetPlatform;
    private void Awake()
    {
        gameObject.SetActive(TargetPlatform.Contains(Application.platform));
    }
}
