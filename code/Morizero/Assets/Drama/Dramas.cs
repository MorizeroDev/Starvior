using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Threading.Tasks;

public class Dramas : MonoBehaviour
{
    #region 动态部分
    public RectTransform sWord;                     // 起始对话框位置
    public RectTransform eWord;                     // 结束对话框位置
    public Text Title;                              // 对话框标题
    public Image Character;                         // 立绘
    public GameObject WordChild;                    // 对话框文本母体
    public Animator Motion;                         // 立绘动画
    #endregion
    private static string DisplayText;              // 要显示的对话
    private static int WaitTicks;                   // 等待计数
    private static int DialogState;                 // 对话框状态（-1=未就绪，0=等待显示，1=等待确认，2=完毕）
    public static Dramas Drama;                     // 剧本控制器
    public static string motion;
    public static WordEffect.Effect Effect;
    // 记录的文本对象
    private static List<GameObject> DisWords = new List<GameObject>();
    public async static Task Prepare(){
        // 载入对话框
        Drama = null;
        await Switcher.Carry("Drama",LoadSceneMode.Additive);
        // 初始化
        DisplayText = ""; DialogState = -1; WaitTicks = 100;
        // 等待状态完毕
        await Task.Run(() => {
            while(Drama == null) Thread.Sleep(100);
        });
        Debug.Log("[Drama] prepred!");
    }
    public async static Task End(){
        // 卸载对话框
        await Switcher.Carry("Drama",LoadSceneMode.Additive,1);
    }
    public async static Task Msg(string character,string content,string motion,WordEffect.Effect effect = WordEffect.Effect.None){
        // 销毁旧的文本
        foreach(GameObject go in DisWords) Destroy(go);
        DisWords.Clear();
        // 初始化
        DisplayText = content; DialogState = 0; WaitTicks = 100; Effect = effect;
        Dramas.motion = motion;
        // 启动
        Drama.Display(character);
        // 等待状态完毕
        await Task.Run(() => {
            while(DialogState != 2) Thread.Sleep(100);
        });
    }
    public async void Display(string character){
        Title.text = character; 
        Character.sprite = Resources.Load<Sprite>("Characters\\" + character);
        Character.SetNativeSize();
        Motion.Play(motion,0);
        float x = sWord.localPosition.x,y = sWord.localPosition.y,step = sWord.sizeDelta.x;
        for(int i = 0;i < DisplayText.Length;i++){
            GameObject word = Instantiate(WordChild,new Vector3(0,0,-1),Quaternion.identity,this.transform);
            word.GetComponent<RectTransform>().localPosition = new Vector3(x,y,0);
            word.GetComponent<Text>().text = DisplayText[i].ToString();
            word.GetComponent<WordEffect>().basex = x;
            word.GetComponent<WordEffect>().basey = y;
            word.GetComponent<WordEffect>().effect = Effect;
            word.SetActive(true);
            DisWords.Add(word);
            x += step;
            if(x >= eWord.localPosition.x + step){
                x = sWord.localPosition.x;
                y -= step*1.2f;
            }
            await Task.Run(() => {
                Thread.Sleep(WaitTicks);
            });
        }
        DialogState = 1;
    }
    void Start()
    {
        
    }
    void Update()
    {
        if(Input.GetMouseButtonUp(0)){
            if(DialogState == 0) WaitTicks = 0;
            if(DialogState == 1) DialogState = 2;
        }
    }
    private void Awake() {
        Dramas.Drama = this.gameObject.GetComponent<Dramas>();
        DialogState = 0;
        Debug.Log("[Drama] Drama filled!");
    }
}
