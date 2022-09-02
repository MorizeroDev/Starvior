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
        public List<ItemManager.OwnItem> OwnItems;
        public string DramaDialogTyle;
    }
    public const string LatestVersion = "22.627";
    public static bool SaveMode;
    public static bool SaveShowed = false;
    public static SaveFile CurrentFile;
    public static void RestoreGame(string data)
    {
        SaveFile file = JsonUtility.FromJson<SaveFile>(data);
        Save.KeyPairs = file.KeyPairs; MapCamera.initTp = -1;
        if (file.DramaData != null || file.ActiveScript != null) 
            if(file.DramaData.Count > 0 || file.ActiveScript.code.Length > 0)
                MapCamera.ForbiddenMove = true;
        ItemManager.OwnItems = file.OwnItems;
        CurrentFile = file;
        Switcher.Carry(file.ActiveScene, loadingCallback: RestoreAfterMapLoaded);
    }
    public void AnimationCallback()
    {
        if (GameObject.Find("SaveUI").GetComponent<Animator>().GetFloat("Speed") > 0) return;
        SceneManager.UnloadSceneAsync("Save");
        LaunchGame.isLaunched = false;
    }
    public static void RestoreAfterMapLoaded()
    {
        Camera.main.Render();
        if (CurrentFile.BGMClip != "")
        {
            MapCamera.bgm.clip = (AudioClip)Resources.Load("BGM\\" + CurrentFile.BGMClip);
            MapCamera.bgm.Play();
            MapCamera.bgm.time = CurrentFile.BGMTime;
        }
        else
        {
            MapCamera.bgm.Stop();
            MapCamera.bgm.clip = null;
        }
        if (CurrentFile.BGSClip != "")
        {
            MapCamera.bgs.clip = (AudioClip)Resources.Load("BGM\\" + CurrentFile.BGSClip);
            MapCamera.bgs.Play();
            MapCamera.bgs.time = CurrentFile.BGSTime;
        }
        else
        {
            MapCamera.bgs.Stop();
            MapCamera.bgs.clip = null;
        }
        if (CurrentFile.OBGMClip != "") MapCamera.mcamera.BGM = (AudioClip)Resources.Load("BGM\\" + CurrentFile.OBGMClip);
        if (CurrentFile.OBGSClip != "") MapCamera.mcamera.BGS = (AudioClip)Resources.Load("BGM\\" + CurrentFile.OBGSClip);
        foreach (GameObjectState state in CurrentFile.SceneObjects)
        {
            GameObject go = GetObject(state.Name);
            if (go != null)
            {
                go.transform.position = state.Position;
                go.SetActive(state.IsActive);
            }
        }
        foreach (CharacterState state in CurrentFile.Characters)
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
        Dramas.HistoryDrama = CurrentFile.History;
        Dramas.HistoryDrama.RemoveAt(Dramas.HistoryDrama.Count - 1);
        if (CurrentFile.ActiveScript != null)
        {
            if (CurrentFile.ActiveScript.code.Length > 0)
            {
                if (CurrentFile.ScriptOwnerName != "")
                {
                    CheckObj check = GetObject(CurrentFile.ScriptOwnerName).GetComponent<CheckObj>();
                    s = check.scriptCarrier;
                }
                if (s == null)
                {
                    s = CurrentFile.ActiveScript;
                    DramaScript.Active = CurrentFile.ActiveScript;
                }
                else
                {
                    s.currentLine = CurrentFile.ActiveScript.currentLine;
                    s.code = CurrentFile.ActiveScript.code;
                    s.DramaAvaliable = CurrentFile.ActiveScript.DramaAvaliable;
                    DramaScript.Active = s;
                }
            }
        }
        foreach (string plot in CurrentFile.PlotName)
            PlotCreator.LoadPlot(plot);
        if (CurrentFile.DramaData != null)
        {
            if (CurrentFile.DramaData.Count > 0)
            {
                Dramas d = null;
                if (CurrentFile.DialogPrefab == "Check")
                {
                    if (s == null)
                    {
                        d = Dramas.LaunchCheck("", () =>
                        {
                            if (!Settings.Active && !Settings.Loading) MapCamera.ForbiddenMove = false;
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
                        d = Dramas.Launch(CurrentFile.DialogPrefab, () =>
                        {
                            if (!Settings.Active && !Settings.Loading) MapCamera.ForbiddenMove = false;
                        });
                    }
                    else
                    {
                        d = Dramas.LaunchScript(CurrentFile.DialogPrefab, s.carryTask);
                    }
                }
                Dramas.ActiveDrama = d;
                if (s != null) s.lastDrama = d;
                d.Drama = CurrentFile.DramaData;
                d.NoCallback = CurrentFile.DramaNoCallback;
                d.DialogTyle = CurrentFile.DramaDialogTyle;
                d.LifeTime = CurrentFile.LifeTime;
                d.DisableInput = CurrentFile.DramaDisableInput;
                d.Suspense = CurrentFile.DramaSuspense;
                d.DramaIndex = CurrentFile.DramaIndex;
                Dramas.ImmersionSpeaking = CurrentFile.ImmersionName;
                Dramas.lcharacter = CurrentFile.lCharacter;
                d.DialogState = 0;
                if (d.DramaIndex < d.Drama.Count) d.ReadDrama();
            }
        }
        if (CurrentFile.ActiveScript.code.Length > 0 && CurrentFile.ActiveScript.currentLine < CurrentFile.ActiveScript.code.Length)
        {
            if (CurrentFile.ActiveScript.code[CurrentFile.ActiveScript.currentLine].TrimStart() == "distribute_choices:")
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
        file.OwnItems = ItemManager.OwnItems;
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
    public static void ShowSave()
    {
        if (SaveShowed) return;
        SaveShowed = true;
        SceneManager.LoadSceneAsync("Save", LoadSceneMode.Additive);
        //yield return null;
        //GameObject.Find("SaveUI").GetComponent<Animator>().SetFloat("Speed", 1.0f);
    }
}
