using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : MonoBehaviour
{
    public static bool isUsing = false;         // 是否在使用
    private Animator animator;
    public delegate void LoadingCallback();
    public LoadingCallback loadingCallback,finishCallback;
    private static Loading loading;

    public static void Start(LoadingCallback callback,LoadingCallback finish = null,bool ShowLoadCircle = false,string LoadingPrefab = "Loading"){
        GameObject fab = (GameObject)Resources.Load("Prefabs\\" + LoadingPrefab);    // 载入母体
        GameObject box = Instantiate(fab, new Vector3(0,0,-1),Quaternion.identity);
        loading = box.GetComponent<Loading>();
        loading.loadingCallback = callback;
        loading.finishCallback = finish;
        if(ShowLoadCircle) box.transform.Find("Ani").gameObject.SetActive(true);
        isUsing = true;
        box.SetActive(true);
    }
    private void Awake() {
        animator = this.GetComponent<Animator>();               // 取得动画控制器
        DontDestroyOnLoad(this.gameObject);                     // 免死金牌
    } 
    public static void Finish(){
        loading.animator.SetFloat("rate",1f);                   // 继续动画
    }
    // 第一过程
    void Stage1(){
        animator.SetFloat("rate",0f);                           // 暂停动画
        loadingCallback();
    }
    // 第二过程（终止过程）
    void Stage2(){
        isUsing = false;                                        // 恢复使用
        if(finishCallback != null) finishCallback();
        Destroy(this.gameObject);                               // 自鲨
    }
}
