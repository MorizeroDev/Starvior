using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DramaCrossScene : MonoBehaviour
{
    public DramaScript script;
    public static DramaScript Start(DramaScript ods){
        GameObject fab = (GameObject)Resources.Load("Prefabs\\Empty");    // 载入母体
        GameObject go = Instantiate(fab,Vector3.zero,Quaternion.identity);
        go.AddComponent(typeof(DramaCrossScene));
        DramaCrossScene dos = go.GetComponent<DramaCrossScene>();
        dos.script = new DramaScript();
        DramaScript ds = dos.script;
        ds.code = ods.code;
        ds.currentLine = ods.currentLine;
        ds.callback = () => {Destroy(go);};
        DontDestroyOnLoad(go);
        Debug.Log("DramaCrossScene: success.");
        return ds;
    }
}

public class PlotCreator : MonoBehaviour
{
    public static List<GameObject> Plots = new List<GameObject>();
    public static void Clear(Loading.LoadingCallback callback){
        Loading.Start(() => {
            foreach(GameObject go in Plots)
                Destroy(go);
            Plots.Clear();
            Loading.Finish();
        },callback);
    }
    public static void LoadPlot(string plots){
        GameObject fab = (GameObject)Resources.Load("Plot\\" + plots);    // 载入母体
        GameObject plot = Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
        plot.SetActive(true);
        Plots.Add(plot);
    }
}

public class DramaScript
{
    public string[] code;
    public int currentLine;
    public DramaCallback callback;
    public void Done(){
        MapCamera.SuspensionDrama = false;
        if(callback != null) callback();
    }
    public void carryTask(){
        if(currentLine >= code.Length) {Done(); return;} 
        string[] t = code[currentLine].Split(':');
        string cmd = t[0];
        if(t.Length <= 1) {currentLine++; carryTask(); return;}
        string[] p = t[1].Split(',');
        bool handler = false;
        currentLine++;

        Debug.Log("Drama script: (Command) '" + cmd + "' (Param) " + t[1]);
        // 人物路径任务
        // path:人物,task1,task2,...
        if(cmd == "path"){
            Chara chara = GameObject.Find(p[0]).GetComponent<Chara>();
            for(int i = 1;i < p.Length;i++){
                float xB = 0,yB = 0;
                if(p[i].StartsWith("left")) xB = -float.Parse(p[i].Substring(4));
                if(p[i].StartsWith("right")) xB = float.Parse(p[i].Substring(5));
                if(p[i].StartsWith("up")) yB = float.Parse(p[i].Substring(2));
                if(p[i].StartsWith("down")) yB = -float.Parse(p[i].Substring(4));
                Debug.Log("Drama script: walk task enqueue:" + xB + "," + yB);
                chara.walkTasks.Enqueue(Chara.walkTask.fromStep(xB, yB));
            }
            chara.walkTaskCallback = carryTask;
            handler = true;
        }
        // 调查任务
        // spy:调查内容
        if(cmd == "spy"){
            Dramas.LaunchCheck(p[0],carryTask);
            handler = true;
        }
        // 预留任务（如果最后一项任务是需要等待的，则需要加入此行缓冲）
        if(cmd == "preserve"){
            handler = true;
        }
        // 传送任务
        // tp:地图名称;传送点编号;朝向
        if(cmd == "tp"){
            MapCamera.initTp = int.Parse(p[1]);
            if(p[2] == "left") MapCamera.initDir = Chara.walkDir.Left;
            if(p[2] == "right") MapCamera.initDir = Chara.walkDir.Right;
            if(p[2] == "up") MapCamera.initDir = Chara.walkDir.Up;
            if(p[2] == "down") MapCamera.initDir = Chara.walkDir.Down;
            MapCamera.HitCheck = null;
            DramaScript nds = DramaCrossScene.Start(this);
            Switcher.Carry(p[0],UnityEngine.SceneManagement.LoadSceneMode.Single,0,nds.carryTask);
            handler = true;
        }
        // 情节任务
        // plot:情节预制体名称/clear
        if(cmd == "plot"){
            if(p[0] == "clear"){
                PlotCreator.Clear(carryTask);
            }else{
                Loading.Start(() => {
                    PlotCreator.LoadPlot(p[0]);
                    Loading.Finish();
                },carryTask);
            }
            handler = true;
        }
        // 对话任务
        // say/immersion:人物
        // [(shake)/(rainbow)/...]对话内容
        if(cmd == "say" || cmd == "immersion"){
            // 创建剧本框架
            Dramas drama = Dramas.LaunchScript(cmd == "say" ? "DialogFrameSrc" : "Immersion", carryTask);
            drama.Drama.Clear();
            string role = p[0];
            // 读取直至对话结束
            while(currentLine + 1 < code.Length){
                sayTag:
                t = code[currentLine].Split(':');
                Debug.Log("Drama script: (Dialog) " + code[currentLine]);
                // 初始化对话参数
                string motion = "Enter";
                WordEffect.Effect effect = WordEffect.Effect.None;
                float speed = 0.03f;
                // 如果是对话
                if(t.Length == 1){
                    // 格式化对话并提取附加参数
                    p = code[currentLine].Replace("(",")").Split(')');
                    // 处理附加参数
                    for(int i = 1;i < p.Length;i+=2){
                        if(i == p.Length - 1) break;
                        if(p[i] == "shake") {
                            effect = WordEffect.Effect.Shake;
                        }else if(p[i] == "rainbow") {
                            effect = WordEffect.Effect.Rainbow;
                        }else if(p[i] == "Leap" || p[i] == "Focus"){
                            motion = p[i];
                        }else{
                            speed = float.Parse(p[i]);
                        }
                    }
                    // 装入结构体
                    Dramas.DramaData data = new Dramas.DramaData{
                        Character = role,
                        motion = motion,
                        Effect = effect,
                        content = p[p.Length - 1],
                        Speed = speed
                    };
                    // 添加对话
                    drama.Drama.Add(data);
                }else{
                    // 对话读取结束，跳出
                    if(t[0] == cmd){
                        role = t[1];
                        currentLine++;
                        goto sayTag;
                    }
                    break;
                }
                currentLine++;
            }
            // 刷新剧本
            drama.ReadDrama();
            drama.gameObject.SetActive(true);
            handler = true;
        }
        // 未处理的脚本
        if(!handler){
            Debug.LogWarning("'" + cmd + $"'（行 {currentLine - 1}）未被处理。");
            carryTask(); return;
        }
        if(currentLine >= code.Length) Done();
    }
}
