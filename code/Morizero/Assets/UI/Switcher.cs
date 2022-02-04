using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Threading.Tasks;
using System;

public class Switcher : MonoBehaviour
{
    // 动画控制器
    public static string destination;           // 目标场景
    public static LoadSceneMode loadMode;       // 加载方式
    public delegate void SwitcherCallback();
    public static int task;                     // 任务（0=加载，1=卸载）
    /// <summary>
    /// 场景切换器
    /// </summary>
    /// <param name="scene">场景名</param>
    /// <param name="mode">加载方式</param>
    /// <param name="Task">提交任务（0=加载，1=卸载）</param>
    public static void Carry(string scene,LoadSceneMode mode = LoadSceneMode.Single,int task = 0,Loading.LoadingCallback callback = null,bool ShowLoadCircle = false,string LoadingPrefab = "Loading"){
        if(Loading.isUsing) return;
        Switcher.destination = scene; Switcher.loadMode = mode; Switcher.task = task;
        SceneManager.sceneLoaded += SceneLoaded_CallBack;       // 设置回调钩子
        SceneManager.sceneUnloaded += SceneUnLoaded_CallBack;   // 设置回调钩子
        Loading.Start(Load,callback,ShowLoadCircle,LoadingPrefab);
    }
     public static void Carry(string scene, string LoadingPrefab){
        Carry(scene,LoadSceneMode.Single,LoadingPrefab: LoadingPrefab);
    }
    public static void CarryWithLoadCircle(string scene){
        Carry(scene,ShowLoadCircle: true);
    }
    static void Load(){
        if(task == 0){
            SceneManager.LoadSceneAsync(destination,loadMode);  // 异步加载
        }else{
            SceneManager.UnloadSceneAsync(destination);         // 异步卸载
        }
    }
    /**
     * 场景加载完成回调函数
     */
    public static void SceneLoaded_CallBack(Scene scene, LoadSceneMode sceneType)
    {
        SceneManager.sceneLoaded -= SceneLoaded_CallBack;       // 取消回调钩子
        SceneManager.sceneUnloaded -= SceneUnLoaded_CallBack;   // 取消回调钩子
        Loading.Finish();
    }
    /**
     * 场景卸载完成回调函数
     */
    public static void SceneUnLoaded_CallBack(Scene scene)
    {
        SceneManager.sceneLoaded -= SceneLoaded_CallBack;       // 取消回调钩子
        SceneManager.sceneUnloaded -= SceneUnLoaded_CallBack;   // 取消回调钩子
        Loading.Finish();
    }
}
