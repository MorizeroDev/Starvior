using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TimeZone : MonoBehaviour
{
    public enum DayTick
    {
        MidNight = 0,
        SunRise = 360,
        SunRising = 390,
        Morning = 420,
        Dusk = 1020,
        Dusking = 1050,
        Night = 1080,
        MidNight2 = 1440,
    }
    public static long Ticks = 1000;
    public static string DisplayTime
    {
        get
        {
            string spanname = "";
            if (Ticks < (long)DayTick.SunRise)
                spanname = "Áè³¿";
            else if (Ticks >= (long)DayTick.Night)
                spanname = "Ò¹Íí";
            else if (Ticks >= (long)DayTick.SunRise && Ticks < (long)DayTick.Morning)
                spanname = "·÷Ïþ";
            else if (Ticks >= (long)DayTick.Dusk && Ticks < (long)DayTick.Night)
                spanname = "»Æ»è";
            else
                spanname = "Ò¹Íí";

            return $"{Math.Floor(Ticks * 1.0 / 60)}:{(Ticks % 60).ToString("00")}({spanname})";
        }
    }
    public static Dictionary<long, Color> DayColor;
    private float deltaTime = 0;
    public Light2D sun;
    static TimeZone()
    {
        DayColor = new Dictionary<long, Color>();
        DayColor.Add((long)DayTick.SunRise, new Color(8f / 255f, 0f / 255f, 72f / 255f));
        DayColor.Add((long)DayTick.SunRising, new Color(135f / 255f, 96f / 255f, 0f / 255f));
        DayColor.Add((long)DayTick.Morning, new Color(1,1,1));
        DayColor.Add((long)DayTick.Dusk, new Color(1, 1, 1));
        DayColor.Add((long)DayTick.Dusking, new Color(135f / 255f, 96f / 255f, 0f / 255f));
        DayColor.Add((long)DayTick.Night, new Color(8f / 255f, 0f / 255f, 72f / 255f));
        DayColor.Add((long)DayTick.MidNight, new Color(8f / 255f, 0f / 255f, 72f / 255f));
        DayColor.Add((long)DayTick.MidNight2, new Color(8f / 255f, 0f / 255f, 72f / 255f));
    }
    void Update()
    {
        deltaTime += Time.deltaTime;
        if (deltaTime > 0.1f)
        {
            deltaTime -= 0.1f;
            Ticks++;
            if (Ticks == 60 * 24)
            {
                Ticks = 0;
            }
        }
        long[] ticks = { 0, 360, 390, 420, 1020, 1050, 1080, 1440 };
        for(int i = 0;i < ticks.Length - 1; i++)
        {
            if (Ticks >= ticks[i] && Ticks <= ticks[i + 1])
            {
                float length = ticks[i + 1] - ticks[i];
                Color a = DayColor[ticks[i]], b = DayColor[ticks[i + 1]];
                float p1 = 1 - (Ticks - ticks[i]) * 1f / length, p2 = 1 - (ticks[i + 1] - Ticks) * 1f / length;
                //Debug.Log(Ticks + ":" + ticks[i] + "->" + ticks[i + 1] + "(" + length + $") ({a.r},{a.g},{a.b})->({b.r},{b.g},{b.b})");
                sun.color = new Color(p1 * a.r + p2 * b.r, p1 * a.g + p2 * b.g, p1 * a.b + p2 * b.b);
                break;
            }
        }
    }
}
