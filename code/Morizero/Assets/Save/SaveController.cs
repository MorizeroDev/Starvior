using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Save
{
    [System.Serializable]
    public class KeyPair
    {
        public string Key;
        public string Value;
    }
    public static List<KeyPair> KeyPairs = new List<KeyPair>();
    public static string Get(string key, string defaultv)
    {
        int i = KeyPairs.FindIndex(m => m.Key == key);
        if (i != -1)
            return KeyPairs[i].Value;
        else
            return defaultv;
    }
    public static void Put(string key, string value)
    {
        int i = KeyPairs.FindIndex(m => m.Key == key);
        if (i != -1)
            KeyPairs[i].Value = value;
        else
            KeyPairs.Add(new KeyPair { Key = key, Value = value });
    }
}
    public class SaveController : MonoBehaviour
{
    [System.Serializable]
    public struct GameObjectState
    {
        public string Name;
        public Vector3 Position;
        public bool IsActive;
    }
    [System.Serializable]
    public struct CharacterState
    {
        public string Name;
        public Chara.walkDir dir;
    }
    [System.Serializable]
    public struct SaveFile
    {
        public string SaveVersion;
        public string MapName;
        public string BGMClip;
        public string BGSClip;
        public float BGMTime;
        public float BGSTime;
        public string OBGMClip;
        public string OBGSClip;
        public string SaveTime;
        public string ActiveScene;
        public List<Save.KeyPair> KeyPairs;
        public List<GameObjectState> SceneObjects;
        public List<CharacterState> Characters;
        public List<string> History;
        public DramaScript ActiveScript;
        public string DialogPrefab;
        public List<string> PlotName;
        public string ImmersionName;
        public string lCharacter;
        public string ScriptOwnerName;
        public int DramaIndex;
        public List<Dramas.DramaData> DramaData;
        public bool DramaDisableInput;
        public bool DramaNoCallback;
        public bool DramaSuspense;
        public Dramas.DramaLifeTime LifeTime;
        public string DramaDialogTyle;
    }
    public const string LatestVersion = "22.627";
    public static void RestoreGame(string data)
    {
        SaveFile file = JsonUtility.FromJson<SaveFile>(data);
        Save.KeyPairs = file.KeyPairs;
        if (file.DramaData != null || file.ActiveScript != null) 
            if(file.DramaData.Count > 0 || file.ActiveScript.code.Length > 0)
                MapCamera.SuspensionDrama = true;
        Switcher.Carry(file.ActiveScene, loadingCallback: () =>
        {
            Camera.main.Render();
            if(file.BGMClip != "")
            {
                MapCamera.bgm.clip = (AudioClip)Resources.Load("BGM\\" + file.BGMClip);
                MapCamera.bgm.Play();
                MapCamera.bgm.time = file.BGMTime;
            }
            else
            {
                MapCamera.bgm.Stop();
                MapCamera.bgm.clip = null;
            }
            if (file.BGSClip != "")
            {
                MapCamera.bgs.clip = (AudioClip)Resources.Load("BGM\\" + file.BGSClip);
                MapCamera.bgs.Play();
                MapCamera.bgs.time = file.BGSTime;
            }
            else
            {
                MapCamera.bgs.Stop();
                MapCamera.bgs.clip = null;
            }
            if (file.OBGMClip != "") MapCamera.mcamera.BGM = (AudioClip)Resources.Load("BGM\\" + file.OBGMClip);
            if (file.OBGSClip != "") MapCamera.mcamera.BGS = (AudioClip)Resources.Load("BGM\\" + file.OBGSClip);
            foreach (GameObjectState state in file.SceneObjects)
            {
                GameObject go = GetObject(state.Name);
                if(go != null)
                {
                    go.transform.position = state.Position;
                    go.SetActive(state.IsActive);
                }
            }
            foreach (CharacterState state in file.Characters)
            {
                GameObject go = GetObject(state.Name);
                if (go != null)
                {
                    Chara chara = go.GetComponent<Chara>();
                    chara.dir = state.dir;
                    chara.UpdateWalkImage();
                }
            }
            MapCamera.mcamera.FixPos();
            DramaScript s = null;
            Dramas.HistoryDrama = file.History;
            Dramas.HistoryDrama.RemoveAt(Dramas.HistoryDrama.Count - 1);
            if (file.ActiveScript != null) 
            { 
                if(file.ActiveScript.code.Length > 0)
                {
                    if(file.ScriptOwnerName != "")
                    {
                        CheckObj check = GetObject(file.ScriptOwnerName).GetComponent<CheckObj>();
                        s = check.scriptCarrier;
                    }
                    if (s == null)
                    {
                        s = file.ActiveScript;
                        DramaScript.Active = file.ActiveScript;
                    }
                    else
                    {
                        s.currentLine = file.ActiveScript.currentLine;
                        s.code = file.ActiveScript.code;
                        s.DramaAvaliable = file.ActiveScript.DramaAvaliable;
                        DramaScript.Active = s;
                    }
                }
            }
            foreach (string plot in file.PlotName)
                PlotCreator.LoadPlot(plot);
            if (file.DramaData != null)
            {
                if(file.DramaData.Count > 0)
                {
                    Dramas d = null;
                    if (file.DialogPrefab == "Check")
                    {
                        if (s == null)
                        {
                            d = Dramas.LaunchCheck("", () =>
                            {
                                if (!Settings.Active && !Settings.Loading) MapCamera.SuspensionDrama = false;
                            });
                        }
                        else
                        {
                            d = Dramas.LaunchCheck("", s.carryTask);
                        }
                    }
                    else
                    {
                        if (s == null)
                        {
                            d = Dramas.Launch(file.DialogPrefab, () =>
                            {
                                if (!Settings.Active && !Settings.Loading) MapCamera.SuspensionDrama = false;
                            });
                        }
                        else
                        {
                            d = Dramas.LaunchScript(file.DialogPrefab, s.carryTask);
                        }
                    }
                    Dramas.ActiveDrama = d;
                    if (s != null) s.lastDrama = d;
                    d.Drama = file.DramaData;
                    d.NoCallback = file.DramaNoCallback;
                    d.DialogTyle = file.DramaDialogTyle;
                    d.LifeTime = file.LifeTime;
                    d.DisableInput = file.DramaDisableInput;
                    d.Suspense = file.DramaSuspense;
                    d.DramaIndex = file.DramaIndex;
                    Dramas.ImmersionSpeaking = file.ImmersionName;
                    Dramas.lcharacter = file.lCharacter;
                    d.DialogState = 0;
                    if(d.DramaIndex < d.Drama.Count) d.ReadDrama();
                }
            }
            if (file.ActiveScript.code.Length > 0 && file.ActiveScript.currentLine < file.ActiveScript.code.Length)
            {
                if (file.ActiveScript.code[file.ActiveScript.currentLine].TrimStart() == "distribute_choices:")
                {
                    Dramas.ActiveDrama.DramaIndex--;
                    Dramas.ActiveDrama.ReadDrama();
                    Dramas.ActiveDrama.Suspense = false;
                    Dramas.ActiveDrama.DialogState = 0;
                    Dramas.ActiveDrama.Speed = 0;
                    Dramas.ActiveDrama.Update();
                    Dramas.ActiveDrama.Suspense = true;
                    Dramas.ActiveDrama.DramaIndex++;
                    DramaScript.Active.currentLine--;
                    DramaScript.Active.carryTask();
                }
            }
            Loading.Finish();
        });
    }
    public static GameObject GetObject(string path)
    {
        string[] p = path.Split('\\');
        if(p.Length < 2) return null;
        Transform t = null;
        foreach(GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if(go.name == p[1])
            {
                t = go.transform;
                break;
            }
        }
        if (t == null) return null;
        for(int i = 2; i < p.Length; i++)
        {
            t = t.Find(p[i]);
            if(t == null) return null;
        }
        return t.gameObject;
    }
    public static string SaveGame()
    {
        SaveFile file = new SaveFile();
        file.SceneObjects = new List<GameObjectState>();
        file.Characters = new List<CharacterState>();
        file.KeyPairs = Save.KeyPairs;
        file.ActiveScene = SceneManager.GetActiveScene().name;
        if(MapCamera.bgm.clip != null)
        {
            file.BGMClip = MapCamera.bgm.clip.name;
            file.BGMTime = MapCamera.bgm.time;
        }
        else
        {
            file.BGMClip = "";
        }
        if (MapCamera.bgs.clip != null)
        {
            file.BGSClip = MapCamera.bgs.clip.name;
            file.BGSTime = MapCamera.bgs.time;
        }
        else
        {
            file.BGSClip = "";
        }
        if (MapCamera.mcamera.BGM != null)
        {
            file.OBGMClip = MapCamera.mcamera.BGM.name;
        }
        else
        {
            file.OBGMClip = "";
        }
        if (MapCamera.mcamera.BGS != null)
        {
            file.OBGSClip = MapCamera.mcamera.BGS.name;
        }
        else
        {
            file.OBGSClip = "";
        }
        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            AddGameObjectState(go, "", ref file.SceneObjects);
            AddCharacterState(go, "", ref file.Characters);
        }
        if (Dramas.ActiveDrama != null)
        {
            Dramas d = Dramas.ActiveDrama;
            file.DramaData = d.Drama;
            file.DramaDialogTyle = d.DialogTyle;
            file.DramaDisableInput = d.DisableInput;
            file.DramaIndex = d.DramaIndex;
            file.DramaNoCallback = d.NoCallback;
            file.DramaSuspense = d.Suspense;
            file.LifeTime = d.LifeTime;
        }
        if(DramaScript.Active != null)
        {
            if(DramaScript.Active.currentLine < DramaScript.Active.code.Length)
            {
                if (DramaScript.Active.parent != null)
                    file.ScriptOwnerName = GetGameObjectFullPath(DramaScript.Active.parent.gameObject);
                else
                    file.ScriptOwnerName = "";
                file.ActiveScript = DramaScript.Active;
            }
        }
        file.lCharacter = Dramas.lcharacter;
        file.ImmersionName = Dramas.ImmersionSpeaking;
        file.History = Dramas.HistoryDrama;
        file.DialogPrefab = Dramas.PrefabName;
        file.PlotName = PlotCreator.PlotName;
        file.SaveTime = System.DateTime.Now.ToString("yy.MM.dd\nHH:mm");
        file.SaveVersion = LatestVersion;
        file.MapName = MapCamera.mcamera.MapName.text;
        string savedata = JsonUtility.ToJson(file);
        return savedata;
    }
    public static string GetGameObjectFullPath(GameObject go)
    {
        string path = go.name;
        Transform g = go.transform.parent;
        while(g.parent != null)
        {
            path = g.name + "\\" + path;
            g = g.parent;
        }
        path = "\\" + g.name + "\\" + path;
        return path;
    }
    public static void AddGameObjectState(GameObject go,string path,ref List<GameObjectState> gos)
    {
        if (go.layer == 5 || go.layer == 7) return;
        Chara chara = null;
        go.TryGetComponent<Chara>(out chara);
        if (chara != null)
            gos.Add(new GameObjectState { Name = path + "\\" + go.name, Position = go.transform.position, IsActive = go.activeSelf });
        for(int i = 0;i < go.transform.childCount; i++) 
        {
            AddGameObjectState(go.transform.GetChild(i).gameObject, path + "\\" + go.name, ref gos);
        }
    }
    public static void AddCharacterState(GameObject go, string path, ref List<CharacterState> gos)
    {
        if (go.layer == 5 || go.layer == 7) return;
        Chara chara = null;
        go.TryGetComponent<Chara>(out chara);
        if(chara != null)
        {
            gos.Add(new CharacterState { Name = path + "\\" + go.name, dir = chara.dir });
        }
        for (int i = 0; i < go.transform.childCount; i++)
        {
            AddCharacterState(go.transform.GetChild(i).gameObject, path + "\\" + go.name, ref gos);
        }
    }

    public Sprite SaveSprite, NoSaveSprite, NoData;
    public Image Character, Back;
    public RawImage Screenhost;
    public RenderTexture renderTexture;
    private Texture2D mapPreview;
    public Text DateText, MapText;
    public GameObject CoverImage;
    public SaveFile File;
    public Camera CatchCamera;
    public Animator LightAni;
    private Animator UIAni;
    private bool Pressed = false;
    public string FileCode;
    public int Id;
    public static bool SaveMode;
    public static bool SaveShowed = false;
    private void Awake()
    {
        UIAni = GameObject.Find("SaveUI").GetComponent<Animator>();
        if (Id == 5) this.gameObject.SetActive(!SaveMode);
        if (Id == -1 || Id == 4 || Id == 5) return;
        mapPreview = new Texture2D(renderTexture.width, renderTexture.height);
        UpdateAppearance();
    }
    private void Start()
    {
        if (Id != -1) return;
        UIAni.SetFloat("Speed", 0.8f);
        UIAni.Play("SaveUI", 0, 0.0f);
    }
    private void OnDestroy()
    {
        if (Id == -1 || Id == 4) return;
        Destroy(mapPreview);
    }
    public void UpdateAppearance()
    {
        FileCode = PlayerPrefs.GetString("file" + Id, "");
        if (FileCode == "")
        {
            Character.gameObject.SetActive(false);
            DateText.gameObject.SetActive(false);
            MapText.gameObject.SetActive(false);
            Screenhost.gameObject.SetActive(false);
            CoverImage.SetActive(false);
            Back.sprite = SaveMode ? NoSaveSprite : NoData;
        }
        else
        {
            File = JsonUtility.FromJson<SaveFile>(FileCode);
            Character.gameObject.SetActive(true);
            DateText.gameObject.SetActive(true);
            MapText.gameObject.SetActive(true);
            Screenhost.gameObject.SetActive(true);
            mapPreview.LoadImage(System.IO.File.ReadAllBytes(Application.persistentDataPath + "\\file" + Id + ".jpg"));
            Screenhost.texture = mapPreview;
            CoverImage.SetActive(true);
            MapText.text = File.MapName;
            DateText.text = File.SaveTime;
            Back.sprite = SaveSprite;
            Sprite CharaSprite = Resources.Load<Sprite> ("Characters\\" + File.lCharacter);
            if(File.SaveVersion != LatestVersion)
            {
                DateText.text = "旧版本存档，可能存在严重问题。";
                DateText.color = Color.red;
            }
            else
            {
                DateText.color = Color.white;
            }
            if (CharaSprite == null) CharaSprite = Resources.Load<Sprite>("Characters\\世原");
            Character.sprite = CharaSprite;
            Character.SetNativeSize();
            RectTransform rect = Character.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1024f, 1024f / rect.sizeDelta.x * rect.sizeDelta.y);
        }
    }
    public static void ShowSave()
    {
        if (SaveShowed) return;
        SaveShowed = true;
        SceneManager.LoadSceneAsync("Save", LoadSceneMode.Additive);
        //GameObject.Find("SaveUI").GetComponent<Animator>().SetFloat("Speed", 1.0f);
    }
    public void AnimationCallback()
    {
        if (UIAni.GetFloat("Speed") > 0) return;
        SceneManager.UnloadSceneAsync("Save");
        LaunchGame.isLaunched = false;
    }
    public void SaveLightDone()
    {
        Pressed = false;
    }
    public void ApplyOverwriteSave()
    {
        CatchCamera.gameObject.SetActive(true);
        CatchCamera.GetComponent<CameraPlayerFollow>().Update();
        CatchCamera.Render();
        RenderTexture.active = renderTexture;
        mapPreview.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = null;
        mapPreview.Apply();
        CatchCamera.gameObject.SetActive(false);
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "\\file" + Id + ".jpg", mapPreview.EncodeToJPG());
        PlayerPrefs.SetString("file" + Id, SaveGame());
        UpdateAppearance();
    }
    public void OverwriteSave()
    {
        if (Pressed) return;
        Pressed = true;
        LightAni.Play("SaveLight", 0, 0.0f);
    }
    public void OnMouseUp()
    {
        if (!SaveShowed) return;
        if (MakeChoice.choiceFinished > 0 && MakeChoice.UI.FindIndex(m => m.NoRecord) != -1) return;
        //Debug.Log("Here:" + gameObject.name);
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool hasMe = false;
        foreach(RaycastHit2D h in Physics2D.RaycastAll(new Vector2(p.x,p.y), new Vector2(0, 0)))
        {
            SaveController s;
            if(h.collider.gameObject.TryGetComponent<SaveController>(out s))
            {
                if(s.Id == this.Id)
                {
                    hasMe = true; break;
                }
            }
        }
        if (!hasMe) return;
        if (Id == 4)
        {
            UIAni.SetFloat("Speed", -2f);
            UIAni.Play("SaveUI", 0, 1.0f);
            SaveShowed = false;
            return;
        }
        if(Id == 5)
        {
            SndPlayer.Play("loadfile");
            SaveShowed = false;
            Switcher.Carry("EmptyScene");
            return;
        }
        if (SaveMode)
        {
            if(FileCode != "")
            {
                MakeChoice.Create(() =>
                {
                    if (MakeChoice.choiceId == 1) return;
                    SndPlayer.Play("savefile");
                    OverwriteSave();
                }, "您确定要覆盖这个游戏存档吗？", new string[] { "确定", "取消" }, true);
            }
            else
            {
                SndPlayer.Play("savefile");
                OverwriteSave();
            }
        }
        else
        {
            if(FileCode == "")
            {
                return;
            }
            if (File.SaveVersion != LatestVersion)
            {
                MakeChoice.Create(() =>
                {
                    if (MakeChoice.choiceId == 0)
                    {
                        SndPlayer.Play("loadfile");
                        SaveShowed = false;
                        RestoreGame(FileCode);
                    }
                }, "存档属于旧版本并被标记有故障，尝试载入可能出现意外。\n无论如何也要载入此存档吗？", new string[] { "确定", "取消" }, true);
            }
            else
            {
                SndPlayer.Play("loadfile");
                SaveShowed = false;
                RestoreGame(FileCode);
            }
        }
    }
}
