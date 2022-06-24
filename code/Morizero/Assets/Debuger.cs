using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debuger : MonoBehaviour
{
    public Text text;
    public static bool DebugerInitialized = false;
    public GameObject DebugBar;
    public Text Detail;
    public int id;
    public static int line = 0;
    public static bool DebugerOpening = false;
    private static GameObject InstantMsg = null;
    private Camera lastCamera;
    float dTime = 0;
    int FPSCount = 0, FPS = 0;

    public void MouseUp()
    {
        if (id == 0)
        {
            DebugBar.SetActive(!DebugBar.activeSelf);
            DebugerOpening = DebugBar.activeSelf;
        }
        if (id == 1)
        {
            if(DramaScript.Active == null)
            {
                InstantMessage("没有正在运作的剧本脚本。", Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            DramaScript s = DramaScript.Active;
            if (line >= s.code.Length)
            {
                InstantMessage("超出当前剧本脚本行数，请重载行数。", Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            if (line - 1 < 0)
            {
                InstantMessage("已到第一行。", Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            for (int i = line - 1; i >= 0; i--)
            {
                if (s.code[i].Contains(':'))
                {
                    line = i;
                    Detail.text = "行" + line + "：" + s.code[line];
                    break;
                }
            }
        }
        if (id == 2)
        {
            if (DramaScript.Active == null)
            {
                InstantMessage("没有正在运作的剧本脚本。", Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            DramaScript s = DramaScript.Active;
            if (line >= s.code.Length)
            {
                InstantMessage("超出当前剧本脚本行数，请重载行数。", Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            if (line + 1 >= s.code.Length)
            {
                InstantMessage("已到最后一行。", Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            for (int i = line + 1; i < s.code.Length; i++)
            {
                if (s.code[i].Contains(':'))
                {
                    line = i;
                    Detail.text = "行" + line + "：" + s.code[line];
                    break;
                }
            }
        }
        if (id == 4)
        {
            Detail.gameObject.SetActive(!Detail.gameObject.activeSelf);
        }
        if (id == 5)
        {
            if (DramaScript.Active == null)
            {
                InstantMessage("没有正在运作的剧本脚本。", Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            DramaScript s = DramaScript.Active;
            if (line >= s.code.Length)
            {
                InstantMessage("超出当前剧本脚本行数，请重载行数。", Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            s.currentLine = line;
            s.carryTask();
        }
        if (id == 6)
        {
            if (DramaScript.Active == null)
            {
                InstantMessage("没有正在运作的剧本脚本。", Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            DramaScript s = DramaScript.Active;
            line = s.currentLine;
            Detail.text = "行" + line + "：" + s.code[line];
        }
        if (id == 7)
        {
            if (Dramas.ActiveDrama == null)
            {
                InstantMessage("没有正在运作的对话框。", Camera.main.ScreenToWorldPoint(Input.mousePosition));
                return;
            }
            Dramas.ActiveDrama.DramaIndex = Dramas.ActiveDrama.Drama.Count;
            if (Dramas.ActiveDrama.LifeTime == Dramas.DramaLifeTime.NeverDie)
                Dramas.ActiveDrama.DramaDone();
            else
                Dramas.ActiveDrama.ExitDrama();
        }
        if (id == 3)
        {
            MakeChoice.Create(() =>
            {
                if (MakeChoice.choiceId != 5)
                {
                    if (MapCamera.SuspensionDrama && !Settings.Active)
                    {
                        DramaScript.Active.currentLine = DramaScript.Active.code.Length;
                        DramaScript.Active.carryTask();
                    }
                    MapCamera.SuspensionDrama = true;
                }
                if (MakeChoice.choiceId == 0)
                {
                    Switcher.Carry("ROOM_XUELAN", callback: () => MapCamera.SuspensionDrama = false);
                }
                if (MakeChoice.choiceId == 1)
                {
                    Switcher.Carry("Corridor_3F", callback: () => MapCamera.SuspensionDrama = false);
                }
                if (MakeChoice.choiceId == 2)
                {
                    Switcher.Carry("Yard", callback: () => MapCamera.SuspensionDrama = false);
                }
                if (MakeChoice.choiceId == 3)
                {
                    Switcher.Carry("EmptyScene", callback: () => MapCamera.SuspensionDrama = false);
                }
                if (MakeChoice.choiceId == 4)
                {
                    Switcher.Carry("ShitSpace", callback: () => MapCamera.SuspensionDrama = false);
                }
            }, "传送到哪个地图？", new string[] { "雪兰的病房", "三楼走廊", "后院", "虚空", "画中世界", "取消" });
        }
    }
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
    private void Awake()
    {
        if (id != -1) return;
        if (!DebugerInitialized)
        {
            DebugerInitialized = true;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void Update()
    {
        if (id != -1) return;
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

        if (Camera.main != lastCamera)
        {
            lastCamera = Camera.main;
            this.GetComponent<Canvas>().worldCamera = lastCamera;
            Debug.Log("Reset binding camera.");
        }

        if (!Detail.gameObject.activeSelf) return;
        string dstr = "";
        dstr = $"是否禁止移动（DramaSuspension）：{MapCamera.SuspensionDrama.ToString()}" + "\n" +
                $"是否在游戏菜单中：{Settings.Active.ToString()}" + "\n" +
                $"对话记录总数量：{Dramas.HistoryDrama.Count.ToString()}" + "\n" +
                $"Plot预定角色名：" + Dramas.ImmersionSpeaking + "\n";
        if (MapCamera.HitCheck != null)
            dstr += $"当前调查射线击中对象：" + MapCamera.HitCheck.name + "\n";
        if (MapCamera.bgm != null)
            if (MapCamera.bgm.clip != null)
                dstr += $"BGM播放器装载音乐：" + MapCamera.bgm.clip.name + "\n";
        if (MapCamera.bgm != null)
            if (MapCamera.bgs.clip != null)
                dstr += $"BGS播放器装载音乐：" + MapCamera.bgs.clip.name + "\n";
        if (DramaScript.Active != null)
        {
            dstr += $"当前剧本脚本总行数：{DramaScript.Active.code.Length.ToString()}，运行至：{DramaScript.Active.currentLine.ToString()}" + "\n";
            if(DramaScript.Active.currentLine < DramaScript.Active.code.Length)
                dstr += $"当前运行行的剧本脚本：{DramaScript.Active.code[DramaScript.Active.currentLine]}" + "\n";
            dstr += $"是否有可回收的对话框：{DramaScript.Active.DramaAvaliable.ToString()}";
        }
        if (Dramas.ActiveDrama != null)
        {
            dstr += $"当前对话框总对话数量：{Dramas.ActiveDrama.Drama.Count}，已阅读到：{Dramas.ActiveDrama.DramaIndex + 1}" + "\n";
        }
        Detail.text = dstr;
    }
}
