using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarSeaController2 : MonoBehaviour
{
    public float Speed = 1f;
    public TextAsset Beats;
    private float lastTime;
    public GameObject prefab;
    public AudioSource bgm;
    public Text result;
    private int Combo = 0, Perfect = 0, Good = 0, Bad = 0, Miss = 0;
    private string LastPitch = "";
    private bool Played = false;
    public string s = "";
    struct Beat
    {
        public float time;
        public bool beated;
        public bool hited;
        public Color color;
        public GameObject go;
    }
    private List<Beat> beats = new List<Beat>();
    private bool AllBeat = false;
    private int BeatIndex = 0;
    private void Awake()
    {
        string[] s = Beats.text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string b in s)
            beats.Add(new Beat
            {
                time = float.Parse(b),
                beated = false,
                hited = false,
                color = Color.white
            });
    }
    public void UpdateGame()
    {
        result.text = "ÉÏ´Î´ò»÷£º" + LastPitch + "£¬Combo£º" + Combo + "£¬Perfect£º" + Perfect + "£¬Good£º" + Good + "£¬Bad£º" + Bad + "£¬Miss£º" + Miss;
    }
    void Update()
    {
        /**if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S)) s += bgm.time + "\n";
        if (Input.GetKeyUp(KeyCode.W)) Debug.Log(s);**/
        for (int i = 0; i < beats.Count; i++)
        {
            Beat b = beats[i];
            if(i >= BeatIndex)
            {
                if (bgm.time >= b.time && !b.beated)
                {
                    GameObject go = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, transform.parent);
                    go.SetActive(true); go.GetComponent<SpriteRenderer>().color = b.color;
                    b.beated = true; b.go = go;
                    beats[i] = b;
                    BeatIndex++;
                    if (i == beats.Count - 1 && !AllBeat) AllBeat = true;
                    break;
                }
            }
            if(!b.hited && bgm.time - b.time >= 0.3f)
            {
                b.hited = true;
                Miss++; Combo = 0;
                if (Played)
                {
                    UpdateGame();
                }
                beats[i] = b;
            }
            if(Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W))
            {
                float p = Mathf.Abs(bgm.time - b.time);
                if (p < 0.3f)
                {
                    Played = true; Combo++;
                    b.hited = true;
                    if (p > 0.2f)
                    {
                        Bad++;
                        b.color = Color.red;
                    }else if(p > 0.1f)
                    {
                        Good++;
                        b.color = Color.cyan;
                    }
                    else
                    {
                        Perfect++;
                        b.color = Color.yellow;
                    }
                    if(b.go != null) b.go.GetComponent<SpriteRenderer>().color = b.color;
                    LastPitch = (bgm.time - b.time > 0 ? "Late" : "Early");
                    UpdateGame();
                    beats[i] = b;
                }
            }
        }
        if (AllBeat && bgm.time < 7f)
        {
            AllBeat = false;
            Combo = 0; Good = 0; Perfect = 0; Bad = 0; Miss = 0;
            for (int i = 0; i < beats.Count; i++)
            {
                beats[i] = new Beat { time = beats[i].time, beated = false, hited = false, color = Color.white };
            }
        }
        return;
        if (Time.time - lastTime > Speed)
        {
            lastTime = Time.time;
            GameObject go = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, transform.parent);
            go.SetActive(true);
            Speed = Random.Range(0.5f, 1.5f);
        }
    }
}
