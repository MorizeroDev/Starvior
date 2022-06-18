using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debuger : MonoBehaviour
{
    public Text text;
    private static GameObject InstantMsg = null;
    float dTime = 0;
    int FPSCount = 0, FPS = 0;

    public static void InstantMessage(string s, Vector3 position, Transform parent = null)
    {
        if(InstantMsg == null)
            InstantMsg = (GameObject)Resources.Load("Prefabs\\InstantDebugInfo");    // 载入母体
        if (parent == null)
        {
            GameObject debugger = GameObject.Find("Debuger");
            if (debugger == null) return;
            if (!debugger.activeSelf) return;
            parent = debugger.transform;
        }
        position.z = 0;
        GameObject text = Instantiate(InstantMsg, position, Quaternion.identity, parent);
        text.GetComponent<Text>().text = s;
        text.SetActive(true);
    }

    private void Update()
    {
        FPSCount++;
        dTime += Time.deltaTime;
        if (dTime > 1)
        {
            dTime = 0;
            FPS = FPSCount;
            if (FPS < 10)
            {
                if (text.color != Color.red) text.color = Color.red;
                text.text = $"（！！！极低）{FPS}";
            }
            else if (FPS < 20)
            {
                if (text.color != Color.red) text.color = Color.red;
                text.text = $"（！！很低）{FPS}";
            }
            else if (FPS < 30)
            {
                if (text.color != Color.red) text.color = Color.red;
                text.text = $"（！低）{FPS}";
            }
            else if (FPS < 45)
            {
                if (text.color != Color.yellow) text.color = Color.yellow;
                text.text = "（！较低）" + FPS;
            }
            else if (FPS < 55)
            {
                if(text.color != Color.yellow) text.color = Color.yellow;
                text.text = "！" + FPS;
            }
            else
            {
                if (text.color != Color.white) text.color = Color.white;
                text.text = FPS.ToString();
            }
            FPSCount = 0;
        }

        if (Input.GetKeyUp(KeyCode.F3)) text.gameObject.SetActive(!text.gameObject.activeSelf);

    }
}
