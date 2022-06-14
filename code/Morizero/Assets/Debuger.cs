using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debuger : MonoBehaviour
{
    public Text text;
    private static GameObject InstantMsg = null;
    float sTime = 0, dTime = 0;
    int FPSCount = 0, FPS = 0;
    bool show = true;

    public static void InstantMessage(string s, Vector3 position, Transform parent = null)
    {
        if(InstantMsg == null)
            InstantMsg = (GameObject)Resources.Load("Prefabs\\InstantDebugInfo");    // ‘ÿ»Îƒ∏ÃÂ
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
            text.text = $"{FPS}";
            FPSCount = 0;
        }

        if (Input.GetKeyUp(KeyCode.F3)) text.gameObject.SetActive(!text.gameObject.activeSelf);

    }
}
