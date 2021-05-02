using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisposeObject : MonoBehaviour
{
    void Dispose()
    {
        Destroy(this.gameObject);
    }
}
