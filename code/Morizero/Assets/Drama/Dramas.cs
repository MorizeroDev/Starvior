using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Text;

public delegate void DramaCallback();
public class Dramas : MonoBehaviour
{
    // 剧本控制器
    public static DramaCallback callback;
    public RectTransform sWord;                     // 起始对话框位置
    public RectTransform eWord;                     // 结束对话框位置
    public Text Title;                              // 对话框标题
    public Image Character;                         // 立绘
    public GameObject WordChild;                    // 对话框文本母体
    public Animator Motion;                         // 立绘动画
    private float Speed;                            // 等待计数
    private int DialogState;                        // 对话框状态（-1=未就绪，0=等待显示，1=等待确认，2=完毕）
    public Sprite Dialog1,Dialog2;
    public Image DialogBox;
    private string motion;
    public GameObject Continue;
    private WordEffect.Effect Effect;
    private string character;
    private string DisplayText;
    // 记录的文本对象
    private static List<GameObject> DisWords = new List<GameObject>();
    // 对话集
    [System.Serializable]
    public struct DramaData{
        public string Character;
        public string content;
        [Range(0.0f,1.0f)]
        public float Speed;
        public WordEffect.Effect Effect;
        public string motion;
    }
    private int DramaIndex = 0,WordIndex = 0;
    private float delTime = 0;
    public List<DramaData> Drama;
    public bool DisableInput = false;

    private float x = 0,y = 0,step = 0;

    public static Dramas LaunchScript(string frame,DramaCallback Callback){
        callback = Callback;
        Debug.Log("Dramas: launched at " + "Dramas\\" + frame);
        GameObject fab = (GameObject)Resources.Load("Dramas\\" + frame);    // 载入母体
        GameObject box = Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
        Dramas drama = box.transform.Find("Dialog").GetComponent<Dramas>();
        drama.Drama = new List<DramaData>();
        box.GetComponent<Canvas>().worldCamera = Camera.main;
        return drama;
    }
    public static Dramas Launch(string DramaName,DramaCallback Callback){
        callback = Callback;
        Debug.Log("Dramas: launched at " + "Dramas\\" + DramaName);
        GameObject fab = (GameObject)Resources.Load("Dramas\\" + DramaName);    // 载入母体
        GameObject box = Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
        Dramas drama = box.transform.Find("Dialog").GetComponent<Dramas>();
        box.GetComponent<Canvas>().worldCamera = Camera.main;
        drama.ReadDrama();
        box.SetActive(true);
        return drama;
    }
    public static Dramas LaunchCheck(string content,DramaCallback Callback){
        callback = Callback;
        GameObject fab = (GameObject)Resources.Load("Dramas\\Check");    // 载入母体
        GameObject box = Instantiate(fab,new Vector3(0,0,-1),Quaternion.identity);
        box.GetComponent<Canvas>().worldCamera = Camera.main;
        Dramas drama = box.transform.Find("Dialog").GetComponent<Dramas>();
        DramaData data = drama.Drama[0];
        data.content = content;
        drama.Drama[0] = data;
        drama.ReadDrama();
        box.SetActive(true);
        return drama;
    }
    public void DisposeWords(){
        foreach(GameObject word in DisWords) Destroy(word);
    }
    public void ReadDrama(){
        DisposeWords();
        if(Continue != null) Continue.SetActive(false);

        DialogState = 0;
        character = Drama[DramaIndex].Character;
        Speed = Drama[DramaIndex].Speed;
        DisplayText = Drama[DramaIndex].content;
        Effect = Drama[DramaIndex].Effect;

        if(character != "旁白" && character != "我"){
            Character.sprite = Resources.Load<Sprite>($"Characters\\{character}");
            Character.SetNativeSize();
            RectTransform rect = Character.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(760f / rect.sizeDelta.y * rect.sizeDelta.x, 760f);
            DialogBox.sprite = Dialog1;
            Title.text = character; 
            Motion.Play("Drama_" + Drama[DramaIndex].motion,0);
            Character.gameObject.SetActive(true);
        }else if(Character.color.a == 1){
            Motion.Play("Drama_Lostfocus",0);
            Title.text = "";
            DialogBox.sprite = Dialog2;
        }
        //Character.gameObject.SetActive(character != "旁白" && character != "我");

        x = sWord.localPosition.x;
        y = sWord.localPosition.y;
        step = sWord.sizeDelta.x;
        WordIndex = 0;
    }
    public void DramaDone(){
        Debug.Log("Done!");
        callback();
    }
    private void Awake() {
        Character.gameObject.SetActive(false);
    }
    public void Resume(){
        DialogState = 2;
    }
    void Update()
    {
        if(DramaIndex >= Drama.Count) return;
        if(!DisableInput){
            if(Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.X)){
                if(DialogState == 0) Speed = 0;
            }
            if(Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Z)){
                if(DialogState == 1) DialogState = 2;
            }
        }
        if(DialogState == 2){
            DialogState = 0;
            DramaIndex++;
            if(DramaIndex >= Drama.Count){
                DisposeWords();
                this.transform.parent.GetComponent<Animator>().Play("ExitDrama",0);
                return;
            }
            ReadDrama();
        }
        delTime += Time.deltaTime;
        if(delTime >= Speed){
            delTime = 0;
            int i = WordIndex;
            if(i >= DisplayText.Length){
                DialogState = 1;
                if(Continue != null) Continue.SetActive(true);
                return;
            }
            GameObject word = Instantiate(WordChild,new Vector3(0,0,-1),new Quaternion(0,0,0,0),this.transform);
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
            WordIndex++;
        }
    }

}
