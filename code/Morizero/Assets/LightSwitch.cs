using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public GameObject Light;
    private void Update()
    {
        bool show = TimeZone.Ticks >= (long)TimeZone.DayTick.Dusking || TimeZone.Ticks <= (long)TimeZone.DayTick.SunRising;
        if (Light.activeSelf != show) Light.SetActive(show);
    }
}
