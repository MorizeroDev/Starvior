using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
    public Dramas lastDrama;
    public bool DramaAvaliable = false;
    public CheckObj parent;

    public void Done(){
        KillLastDrama();
        MapCamera.SuspensionDrama = false;
        if(callback != null) callback();
    }
    public bool KillLastDrama()
    {
        if (DramaAvaliable)
        {
            lastDrama.NoCallback = true;
            lastDrama.ExitDrama();
            DramaAvaliable = false;
            return true;
        }
        return false;
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
        // 人物朝向改动
        // face:人物,dir
        if(cmd == "face"){
            Chara chara = GameObject.Find(p[0]).GetComponent<Chara>();
            if(p[1] == "left") chara.dir = Chara.walkDir.Left;
            if(p[1] == "right") chara.dir = Chara.walkDir.Right;
            if(p[1] == "up") chara.dir = Chara.walkDir.Up;
            if(p[1] == "down")chara.dir = Chara.walkDir.Down;
            chara.UpdateWalkImage();
            handler = true;
            carryTask();
        }
        // meta
        if(cmd == "exit"){
            Application.Quit();
        }
        // 标签
        // label:标签
        // goto:标签
        if(cmd == "label"){
            handler = true;
            carryTask();
        }
        if(cmd == "goto"){
            for(int i = 0;i < code.Length;i++){
                if(code[i] == "label:" + p[0]){
                    currentLine = i + 1;
                    break;
                }
            }
            handler = true;
            carryTask();
        }
        // 显示和隐藏
        // show:物体
        if(cmd == "show"){
            GameObject.Find(p[0]).SetActive(true);
            handler = true;
            carryTask();
        }
        // hide:物体
        if(cmd == "hide"){
            GameObject.Find(p[0]).SetActive(false);
            handler = true;
            carryTask();
        }
        // 调查任务
        // spy:调查内容
        if(cmd == "spy"){
            KillLastDrama();
            Dramas.LaunchCheck(p[0],carryTask).LifeTime = Dramas.DramaLifeTime.DieWhenReadToEnd;
            handler = true;
        }
        // 退出脚本（如果最后一项任务是需要等待的，则需要加入此行缓冲）
        if(cmd == "break"){
            currentLine = code.Length;
            handler = true;
        }
        // 设置开关
        // set:开关名,global/personal,true/false
        if(cmd == "set"){
            string key = "";
            if(p[1] == "global") key = p[0];
            if(p[1] == "personal") key = parent.gameObject.scene + "." + parent.gameObject.name + "." + p[0];
            PlayerPrefs.SetString(key,p[2]);
            handler = true;
            carryTask();
        }
        // 选择
        // choice:标题,选项0,选项1,...
        if(cmd == "choice"){
            string explain = p[0];
            List<string> choice = new List<string>();
            for(int i = 1;i < p.Length;i++) choice.Add(p[i]);
            handler = true;
            MakeChoice.Create(carryTask,explain,choice.ToArray());
        }
        if(cmd == "distribute_choices"){
            int buff = 0;
            while(true){
                if(code[currentLine].StartsWith("choice:")) buff++;
                if(code[currentLine] == "endchoice:" && buff == 0) break;
                if(code[currentLine] == "case:" + MakeChoice.choiceId.ToString() && buff == 0) break;
                if(code[currentLine] == "endchoice:") buff--;  
                Debug.Log("require:" + "case:" + MakeChoice.choiceId.ToString() + ",buff:" + buff + "\n" + code[currentLine]);  
                currentLine++;
            }
            currentLine++;
            handler = true;
            carryTask();
        }
        // case:选项编号
        if(cmd == "case") {
            int buff = 0;
            while(true){
                if(code[currentLine].StartsWith("choice:")) buff++;
                if(code[currentLine] == "endchoice:" && buff == 0) break;
                if(code[currentLine] == "endchoice:") buff--;    
                currentLine++;
            }
            currentLine++;
            handler = true; 
            carryTask();
        }
        if(cmd == "endchoice") {handler = true; carryTask();}
        // 触发器
        // if:开关名,global/personal,要求的true/false
        if(cmd == "if"){
            string key = "";
            if(p[1] == "global") key = p[0];
            if(p[1] == "personal") key = parent.gameObject.scene + "." + parent.gameObject.name + "." + p[0];
            string data = PlayerPrefs.GetString(key);
            if(p[2] != data){
                int buff = 0;
                while(true){
                    if(code[currentLine].StartsWith("if:")) buff++;
                    if(code[currentLine] == "endif:" && buff == 0) break;
                    if(code[currentLine] == "else:" && buff == 0) break;
                    if(code[currentLine] == "endif:") buff--;    
                    currentLine++;
                }
                currentLine++;
            }
            handler = true;
            carryTask();
        }
        if(cmd == "else") {
            int buff = 0;
            while(true){
                if(code[currentLine].StartsWith("if:")) buff++;
                if(code[currentLine] == "endif:" && buff == 0) break;
                if(code[currentLine] == "endif:") buff--;    
                currentLine++;
            }
            currentLine++;
            handler = true; 
            carryTask();
        }
        if(cmd == "endif") {handler = true; carryTask();}
        // 更换背景音乐
        // bgm:背景音乐名
        if(cmd == "bgm"){
            MapCamera.bgm.clip = (AudioClip)Resources.Load("BGM\\" + p[0]);
            MapCamera.bgm.Play();
            handler = true;
            carryTask();
        }
        // 更换环境音效
        // bgs:环境音效名
        if(cmd == "bgs"){
            MapCamera.bgs.clip = (AudioClip)Resources.Load("BGM\\" + p[0]);
            MapCamera.bgs.Play();
            handler = true;
            carryTask();
        }
        // 停止BGM
        // stopbgm
        if(cmd == "stopbgm"){
            MapCamera.bgm.Stop();
            carryTask();
            handler = true;
        } 
        // stopbgs
        if(cmd == "stopbgs"){
            MapCamera.bgs.Stop();
            carryTask();
            handler = true;
        }  
        // 恢复BGM
        // rebgm
        if(cmd == "rebgm"){
            MapCamera.bgm.Play();
            carryTask();
            handler = true;
        }
        // rebgs
        if(cmd == "rebgs"){
            MapCamera.bgs.Play();
            carryTask();
            handler = true;
        }
        // 等待任务
        // wait:等待时间
        if(cmd == "wait"){
            GameObject fab = (GameObject)Resources.Load("Prefabs\\WaitTicker");    // 载入母体
            GameObject ticker = GameObject.Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
            WaitTicker wait = ticker.GetComponent<WaitTicker>();
            wait.waitTime = float.Parse(p[0]);
            wait.callback = carryTask;
            handler = true;
        }
        // 音效任务
        // snd:音效名称,nowait/wait
        if(cmd == "snd"){
            GameObject fab = (GameObject)Resources.Load("Prefabs\\MusicPlayer");    // 载入母体
            GameObject player = GameObject.Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
            AudioSource source = player.GetComponent<AudioSource>();
            source.clip = (AudioClip)Resources.Load("Snd\\" + p[0]);
            source.volume = PlayerPrefs.GetFloat("Settings.SEVolume", 0.5f);
            player.SetActive(true);
            source.Play();
            GameObject.Destroy(player,source.clip.length);
            handler = true;
            if(p[1] == "nowait") carryTask();
            if(p[1] == "wait") player.GetComponent<DestoryCallback>().callback = carryTask;
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
        // 切换场景任务
        // sw:场景名称
        if(cmd == "sw"){
            MapCamera.HitCheck = null;
            DramaScript nds = DramaCrossScene.Start(this);
            Switcher.Carry(p[0],UnityEngine.SceneManagement.LoadSceneMode.Single,0,nds.carryTask);
            if(MapCamera.bgm != null) {GameObject.Destroy(MapCamera.bgm); MapCamera.bgm = null;}
            if(MapCamera.bgs != null) {GameObject.Destroy(MapCamera.bgs); MapCamera.bgs = null;}
            handler = true;
        }
        // 情节任务
        // plot:情节预制体名称/clear
        if(cmd == "plot"){
            if(p[0] == "clear"){
                PlotCreator.Clear(carryTask);
                if (DramaAvaliable)
                {
                    lastDrama.NoCallback = true;
                    lastDrama.ExitDrama();
                    DramaAvaliable = false;
                }
            }
            else{
                Loading.Start(() => {
                    PlotCreator.LoadPlot(p[0]);
                    Loading.Finish();
                },carryTask);
            }
            handler = true;
        }
        // 对话任务
        // say/immersion:人物,[是否禁止输入]
        // [(shake)/(rainbow)/...]对话内容
        if(cmd == "say" || cmd == "immersion"){
            // 创建剧本框架
            bool dinput = false;
            if(p.Length > 1){
                if(p[1] == "true") dinput = true;
                Debug.Log("DramaScript: say's third param detected:" + dinput);
            }
            Dramas drama = null;
            if (DramaAvaliable)
            {
                string DialogTyle = (cmd == "say" ? "Dialog" : "Immersion");
                if (lastDrama.DialogTyle == DialogTyle)
                {
                    drama = lastDrama;
                    drama.Suspense = false;
                }
                else
                {
                    KillLastDrama();
                }
            }
            if (!DramaAvaliable)
            {
                drama = Dramas.LaunchScript(cmd == "say" ? "DialogFrameSrc" : "Immersion", carryTask);
                drama.DialogTyle = (cmd == "say" ? "Dialog" : "Immersion");
                drama.Drama.Clear();
            }
            string role = p[0];
            // 读取直至对话结束
            while(currentLine + 1 < code.Length){
                sayTag:
                t = code[currentLine].TrimStart().Replace("\\:", "").Split(':');
                Debug.Log("Drama script: (Dialog) " + code[currentLine]);
                // 初始化对话参数
                string motion = "Enter";
                WordEffect.Effect effect = WordEffect.Effect.None;
                float speed = 0.03f;
                // 如果是对话
                if(t.Length == 1){
                    // 格式化对话并提取附加参数
                    p = code[currentLine].TrimStart().Replace("\\:", ":").Replace("(",")").Split(')');
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
            drama.DisableInput = dinput;
            drama.gameObject.SetActive(true);
            lastDrama = drama;
            DramaAvaliable = true;
            handler = true;
            if(dinput) carryTask();
        }
        // 继续对话
        if(cmd == "resume"){
            lastDrama.Resume();
            handler = true;
        }
        // 销毁对话框
        if (cmd == "killdialog")
        {
            if (!KillLastDrama())
                Debug.LogWarning("该脚本在尝试销毁不存在的对话框。");
            carryTask();
            handler = true;
        }
        // 未处理的脚本
        if (!handler){
            Debug.LogWarning("'" + cmd + $"'（行 {currentLine - 1}）未被处理。");
            carryTask(); return;
        }
        if(currentLine >= code.Length) Done();
    }
}
