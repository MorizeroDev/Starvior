using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DramaScript
{
    public CheckObj parent;
    public DramaCallback callback;
    public string[] code;
    public int currentLine;
    public void carryTask(){
        if(currentLine >= code.Length) {callback(); return;} 
        string[] t = code[currentLine].Split(':');
        string cmd = t[0];
        string[] p = t[1].Split(',');
        bool handler = false;
        currentLine++;

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
                chara.walkTasks.Add(new Chara.walkTask{
                    xBuff = xB, yBuff = yB, x = 0, y = 0
                });
            }
            chara.walkTaskCallback = carryTask;
            handler = true;
        }
        // 对话任务
        // say:人物
        // [(shake)/(rainbow)/...]对话内容
        if(cmd == "say"){
            // 创建剧本框架
            Dramas drama = Dramas.Launch("DialogFrameSrc",carryTask);
            drama.Drama.Clear();
            string role = p[1];
            // 读取直至对话结束
            while(currentLine + 1 < code.Length){
                currentLine++;
                t = code[currentLine].Split(':');
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
                    cmd = t[0];
                    break;
                }
            }
            // 刷新剧本
            drama.ReadDrama();
            handler = true;
        }
        // 未处理的脚本
        if(!handler){
            Debug.LogWarning("'" + cmd + $"'（行 {currentLine - 1}）未被处理。");
        }
        if(currentLine >= code.Length) callback();
    }
}
