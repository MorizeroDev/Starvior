using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarSeaController2 : MonoBehaviour
{
    public float Speed = 1f;
    public TextAsset Beats;
    private float lastTime;
    public GameObject prefab, Panel,comboT;
    public AudioSource bgm;
    public Text result,score;
    private int Combo = 0, Perfect = 0, Good = 0, Bad = 0, Miss = 0;
    private float Score = 0, Accuracy = 0f;
    private string LastPitch = "";
    private bool Played = false;
    public string s = "";
    struct Beat
    {
        public float time;
        public bool beated;
        public bool hited;
        public int track;
        public GameObject go;
    }
    private List<Beat> beats = new List<Beat>();
    private bool AllBeat = false;
    private int BeatIndex = 0;
    private List<RectTransform> Stars = new List<RectTransform>();
    private void Awake()
    {
        string[] s = Beats.text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string b in s)
        {
            string[] t = b.Split(';');
            beats.Add(new Beat
            {
                time = float.Parse(t[0]),
                beated = false,
                hited = false,
                track = int.Parse(t[1])
            });
        }
    }
    public void UpdateGame()
    {
        result.text = "ÉÏ´Î´ò»÷£º" + LastPitch + "£¬Combo£º" + Combo + "£¬Perfect£º" + Perfect + "£¬Good£º" + Good + "£¬Bad£º" + Bad + "£¬Miss£º" + Miss;
        float ac = Mathf.Floor(Accuracy / (BeatIndex + 1) * 10000) / 100;
        string grade = "F";
        if(ac >= 99.9999f){
            grade = "SSS";
        }else if(Combo == BeatIndex){
            grade = "SS";
        }else if(ac > 95f){
            grade = "S";
        }else if(ac > 87f){
            grade = "A";
        }else if(ac > 80f){
            grade = "B";
        }else if(ac > 75f){
            grade = "C";
        }else{
            grade = "F";
        }
        score.text = Mathf.Floor(Score) + "  " + Mathf.Floor(Accuracy / BeatIndex * 10000) / 100 + "%  " + grade;
    }
    public void PrepareStars()
    {
        AllBeat = false; Played = true; Accuracy = 0f;
        Combo = 0; Good = 0; Perfect = 0; Bad = 0; Miss = 0;
        foreach(RectTransform go in Stars)
        {
            if (go != null)
            {
                Destroy(go.gameObject);
            }
        }
        Stars.Clear();
        for (int i = 0; i < beats.Count; i++)
        {
            beats[i] = new Beat { time = beats[i].time, beated = false, hited = false, track = beats[i].track };
        }
        float[] p = {-778f,-259f,259f,778f};
        for (int i = 0; i < beats.Count; i++)
        {
            GameObject go = Instantiate(prefab, new Vector3(0, 0, 0), prefab.transform.localRotation, prefab.transform.parent);
            go.SetActive(true); 
            go.GetComponent<Image>().color = new Color(Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f), Random.Range(0.5f, 1.0f));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(p[beats[i].track], 1199,0);
            Stars.Add(rect);
        }
        Panel.SetActive(true);
        bgm.time = 0f;
    }
    void Update()
    {
        /**if (Input.GetKeyDown(KeyCode.W)) s += bgm.time + ";0\n";
        if (Input.GetKeyDown(KeyCode.E)) s += bgm.time + ";1\n";
        if (Input.GetKeyDown(KeyCode.T)) s += bgm.time + ";2\n";
        if (Input.GetKeyDown(KeyCode.Y)) s += bgm.time + ";3\n";
        if (Input.GetKeyUp(KeyCode.M)) Debug.Log(s);**/
        bool autoMode = Input.GetKey(KeyCode.N);

        if (Input.GetKeyUp(KeyCode.B))
        {
            if(!Played)
            {
                PrepareStars();
            }
            else
            {
                Played = false;
                Panel.SetActive(false);
                foreach(RectTransform go in Stars)
                {
                    if (go != null)
                    {
                        Destroy(go.gameObject);
                    }
                }
                Stars.Clear();
            }
        }
        if (AllBeat || Stars.Count == 0) return;
        for (int i = 0; i < beats.Count; i++)
        {
            Beat b = beats[i];
            if(i >= BeatIndex)
            {
                if (b.beated)
                {
                    if (i == beats.Count - 1 && !AllBeat) AllBeat = true;
                    break;
                }
            }
            if(Stars[i] != null)
            {
                float pitch = b.time - bgm.time;
                if(pitch <= 2f)
                {
                    float pro = 0f;
                    Vector3 p = Stars[i].localPosition;
                    if(pitch < 0)
                    {
                        pro = -pitch / 0.4f;
                        p.y = -438 + (-1199 - -438) * pro;
                        Color c = Stars[i].GetComponent<Image>().color;
                        Stars[i].GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1f - pro);
                    }
                    else
                    {
                        pro = pitch / 2f;
                        pro = 1 - pro;
                        p.y = 1199 + (-438 - 1199) * pro;
                    }
                    Stars[i].localPosition = p;
                }
            }
            if(!b.hited && bgm.time - b.time >= 0.4f)
            {
                b.hited = true; BeatIndex++;
                Miss++; Combo = 0;
                Destroy(Stars[i].gameObject);
                Stars[i] = null;
                if (Played)
                {
                    UpdateGame();
                }
                beats[i] = b;
            }
            KeyCode[] k = {KeyCode.S, KeyCode.D,KeyCode.J,KeyCode.K};
            if(Input.GetKeyDown(k[b.track]) || (autoMode && Mathf.Abs(bgm.time - b.time) <= 0.02f))
            {
                float p = Mathf.Abs(bgm.time - b.time);
                if (p < 0.3f && !b.hited)
                {
                    Played = true; Combo++;
                    b.hited = true; BeatIndex++;
                    GameObject go = Instantiate(comboT, comboT.transform.localPosition, comboT.transform.localRotation, comboT.transform.parent);
                    Text t = go.GetComponent<Text>();
                    RectTransform rect = go.GetComponent<RectTransform>();
                    rect.localPosition = new Vector3(Stars[i].localPosition.x, -555, 0);
                    //Debug.Log(Stars[i].localPosition.x + " " + "-555");
                    if (p > 0.2f)
                    {
                        Bad++;
                        //b.color = Color.red;
                        Accuracy += (p - 0.1f) / 0.2f;
                        Score += 100000 / (beats.Count - 1) * 0.1f;
                        t.text = "Bad\n" + Combo.ToString();
                    }else if(p > 0.1f)
                    {
                        Good++;
                        //b.color = Color.cyan;
                        Accuracy += (p - 0.1f) / 0.2f * 0.2f + 0.8f;
                        Score += 100000 / (beats.Count - 1) * 0.5f;
                        t.text = "Good\n" + Combo.ToString();
                    }
                    else
                    {
                        Perfect++;
                        //b.color = Color.yellow;
                        Accuracy += 1f;
                        Score += 100000 / (beats.Count - 1);
                        t.text = "Perfect\n" + Combo.ToString();
                    }
                    //SndPlayer.Play("hit");
                    //Debug.Log(t.text);
                    go.SetActive(true); 
                    //if(b.go != null) b.go.GetComponent<SpriteRenderer>().color = b.color;
                    LastPitch = (bgm.time - b.time > 0 ? "Late" : "Early");
                    UpdateGame();
                    beats[i] = b;
                }
            }
        }
    }
}
