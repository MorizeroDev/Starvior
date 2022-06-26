using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public string BGMClip;
        public string BGSClip;
        public float BGMTime;
        public float BGSTime;
        public string OBGMClip;
        public string OBGSClip;
        public string SaveTime;
        public string ActiveScene;
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
    public static void RestoreGame(string data)
    {
        SaveFile file = JsonUtility.FromJson<SaveFile>(data);
        if (file.DramaData != null || file.DramaData != null) MapCamera.SuspensionDrama = true;
        Switcher.Carry(file.ActiveScene, callback: () =>
        {
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
            DramaScript s = null;
            Dramas.HistoryDrama = file.History;
            Dramas.HistoryDrama.RemoveAt(Dramas.HistoryDrama.Count - 1);
            if (file.ActiveScript != null)
            {
                CheckObj check = GetObject(file.ScriptOwnerName).GetComponent<CheckObj>();
                s = check.scriptCarrier;
                s.currentLine = file.ActiveScript.currentLine;
                s.code = file.ActiveScript.code;
                s.DramaAvaliable = file.ActiveScript.DramaAvaliable;
                DramaScript.Active = s;
            }
            foreach (string plot in file.PlotName)
                PlotCreator.LoadPlot(plot);
            if (file.DramaData != null)
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
                d.ReadDrama();
            }
        });
    }
    public static GameObject GetObject(string path)
    {
        string[] p = path.Split('\\');
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
    public static void SaveGame()
    {
        SaveFile file = new SaveFile();
        file.SceneObjects = new List<GameObjectState>();
        file.Characters = new List<CharacterState>();
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
        }
        if(DramaScript.Active != null)
        {
            if(DramaScript.Active.currentLine < DramaScript.Active.code.Length)
            {
                file.ScriptOwnerName = GetGameObjectFullPath(DramaScript.Active.parent.gameObject);
                file.ActiveScript = DramaScript.Active;
            }
        }
        file.lCharacter = Dramas.lcharacter;
        file.ImmersionName = Dramas.ImmersionSpeaking;
        file.History = Dramas.HistoryDrama;
        file.DialogPrefab = Dramas.PrefabName;
        file.PlotName = PlotCreator.PlotName;
        file.SaveTime = System.DateTime.Now.ToString("yy.MM.dd\nHH:mm");
        string savedata = JsonUtility.ToJson(file);
        System.IO.File.WriteAllText("D:\\save.txt", savedata);
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
}
