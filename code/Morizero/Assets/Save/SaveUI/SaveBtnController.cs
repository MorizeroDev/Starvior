using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SaveController;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveBtnController : UIController
{
    public Sprite SaveLitSprite, SaveUnLitSprite, BtnLitSprite, BtnUnLitSprite;
    public Image Character, TmpChara, LitI, UnLitI;
    public RawImage Screenhost, TmpScreenhost;
    public RenderTexture renderTexture;
    private Texture2D mapPreview;
    public Text DateText, MapText;
    public Text CurrentMap, CurrentDialog;
    [HideInInspector]
    public SaveFile File;
    public Camera CatchCamera;
    private Animator UIAni;
    private Sprite CharaSprite;
    private bool Pressed = false;
    [HideInInspector]
    public string FileCode;
    public override void Initialize()
    {
        if (uibase.id == 0)
            System.IO.File.WriteAllBytes(Application.persistentDataPath + "\\empty.jpg", Resources.Load<Texture2D>("noSave").EncodeToJPG());
        UIAni = GameObject.Find("SaveUI").GetComponent<Animator>();
        /**if (Id == 5) this.gameObject.SetActive(!SaveMode);
        if (Id == -1 || Id == 4 || Id == 5) return;**/
        mapPreview = new Texture2D(2560, 1440, TextureFormat.RGB24, false);
        UpdateAppearance();
        if (uibase.id == 0)
        {
            Refresh();
        }
        if (this.name == "NewStart")
        {
            this.gameObject.SetActive(!SaveMode);
            if (SaveMode) uibase.focuser.UI.Remove(uibase);
            DateText.text = ""; MapText.text = "新的故事";
        }
    }
    private void Start()
    {
        if (uibase.id != 0) return;
        UIAni.SetFloat("Speed", 0.8f);
        UIAni.Play("SaveUI", 0, 0.0f);
    }
    private void OnDestroy()
    {
        //if (Id == -1 || Id == 4) return;
        Destroy(mapPreview);
    }
    public void UpdateAppearance()
    {
        FileCode = PlayerPrefs.GetString("file" + uibase.id, "");
        if (FileCode == "")
        {
            DateText.text = ""; MapText.text = "空白存档";
            mapPreview.LoadImage(System.IO.File.ReadAllBytes(Application.persistentDataPath + "\\empty.jpg"));
            LitI.sprite = BtnLitSprite; UnLitI.sprite = BtnUnLitSprite;
        }
        else
        {
            File = JsonUtility.FromJson<SaveFile>(FileCode);
            mapPreview.LoadImage(System.IO.File.ReadAllBytes(Application.persistentDataPath + "\\file" + uibase.id + ".jpg"));
            MapText.text = File.MapName; DateText.text = File.SaveTime;
            LitI.sprite = SaveLitSprite; UnLitI.sprite = SaveUnLitSprite;
            Debug.Log("Character:" + File.lCharacter);
            CharaSprite = Resources.Load<Sprite>("Characters\\" + File.lCharacter);
            if (CharaSprite == null) CharaSprite = Resources.Load<Sprite>("Characters\\世原");
            if (File.SaveVersion != LatestVersion)
            {
                DateText.text = "⚠旧版存档";
            }
        }
    }
    public void Refresh()
    {
        FileCode = PlayerPrefs.GetString("file" + uibase.id, "");
        Screenhost.texture = mapPreview;
        TmpScreenhost.texture = mapPreview;
        if (FileCode == "")
        {
            CurrentMap.text = "空白的故事"; CurrentDialog.text = "一切还定格在发生之前。";
            Character.gameObject.SetActive(false);
            if (this.name == "NewStart")
            {
                CurrentMap.text = "新的故事"; CurrentDialog.text = "让一切开始发生。";
                TmpChara.gameObject.SetActive(false);
            }
        }
        else
        {
            File = JsonUtility.FromJson<SaveFile>(FileCode);
            Debug.Log("Character:" + File.lCharacter);
            if (File.lCharacter == "MakeChoice" || File.lCharacter == "")
                CurrentDialog.text = "......";
            else
                CurrentDialog.text = File.lCharacter + "：" + File.History[File.History.Count - 1] + "";
            CharaSprite = Resources.Load<Sprite>("Characters\\" + File.lCharacter);
            if (CharaSprite == null) CharaSprite = Resources.Load<Sprite>("Characters\\世原");
            Character.gameObject.SetActive(true);
            Character.sprite = CharaSprite;
            Character.SetNativeSize();
            RectTransform rect = Character.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1424f / rect.sizeDelta.y * rect.sizeDelta.x, 1424f);
            TmpChara.sprite = CharaSprite;
            TmpChara.SetNativeSize();
            rect = TmpChara.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1424f / rect.sizeDelta.y * rect.sizeDelta.x, 1424f);
        }
    }
    public override void Lit()
    {
        FileCode = PlayerPrefs.GetString("file" + uibase.id, "");
        Character.gameObject.SetActive(TmpChara.gameObject.activeSelf);
        Character.sprite = TmpChara.sprite;
        Character.SetNativeSize();
        RectTransform rect = Character.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(1424f / rect.sizeDelta.y * rect.sizeDelta.x, 1424f);
        Screenhost.texture = TmpScreenhost.texture;
        TmpScreenhost.texture = mapPreview;
        UIAni.Play((uibase.focuser.lastFocus < uibase.id ? "SaveSwitchToUp" : "SaveSwitchToDown"), 0, 0.0f);
        if (FileCode == "")
        {
            CurrentMap.text = "空白的故事"; CurrentDialog.text = "一切还定格在发生之前。";
            TmpChara.gameObject.SetActive(false);
            if (this.name == "NewStart")
            {
                CurrentMap.text = "新的故事"; CurrentDialog.text = "让一切开始发生。";
                TmpChara.gameObject.SetActive(false);
            }
        }
        else
        {
            CurrentMap.text = File.MapName;
            if (File.lCharacter == "MakeChoice" || File.lCharacter == "")
                CurrentDialog.text = "......";
            else
                CurrentDialog.text = File.lCharacter + "：" + File.History[File.History.Count - 1] + "";
            TmpChara.gameObject.SetActive(true);
            TmpChara.sprite = CharaSprite;
            TmpChara.SetNativeSize();
            rect = TmpChara.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1424f / rect.sizeDelta.y * rect.sizeDelta.x, 1424f);
        }
    }
    public void SaveLightDone()
    {
        Pressed = false;
    }
    public void ApplyOverwriteSave()
    {
        CatchCamera.GetComponent<CameraPlayerFollow>().Update();
        CatchCamera.gameObject.SetActive(true);
        CatchCamera.Render();
        RenderTexture.active = renderTexture;
        mapPreview.ReadPixels(new Rect(0, 0, 2560, 1440), 0, 0, true);
        RenderTexture.active = null;
        //mapPreview.Apply();
        CatchCamera.gameObject.SetActive(false);
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "\\file" + uibase.id + ".jpg", mapPreview.EncodeToJPG());
        PlayerPrefs.SetString("file" + uibase.id, SaveGame());
        UpdateAppearance();
        Refresh();
    }
    public void OverwriteSave()
    {
        if (Pressed) return;
        Pressed = true;
        UIAni.Play("SaveLight", 0, 0.0f);
    }
    public override void Click()
    {
        if (!SaveShowed) return;
        if (MakeChoice.choiceFinished > 0 && MakeChoice.UI.FindIndex(m => m.NoRecord) != -1) return;

        if (this.name == "NewStart")
        {
            // 新的开始
            SndPlayer.Play("loadfile");
            SaveShowed = false;
            Switcher.Carry("EmptyScene");
            return;
        }
        if (SaveMode)
        {
            Character.gameObject.SetActive(TmpChara.gameObject.activeSelf);
            Character.sprite = TmpChara.sprite;
            Character.SetNativeSize();
            RectTransform rect = Character.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1424f / rect.sizeDelta.y * rect.sizeDelta.x, 1424f);
            Screenhost.texture = TmpScreenhost.texture;
            TmpScreenhost.texture = mapPreview;
            if (FileCode != "")
            {
                MakeChoice.Create(() =>
                {
                    if (MakeChoice.choiceId == 1) return;
                    SndPlayer.Play("savefile");
                    SaveLightCallback.current.controller = this;
                    OverwriteSave();
                }, "确定要覆盖这个存档吗，该操作不可逆。", new string[] { "覆盖旧的存档", "让我想想" }, true);
            }
            else
            {
                SndPlayer.Play("savefile");
                SaveLightCallback.current.controller = this;
                OverwriteSave();
            }
        }
        else
        {
            if (FileCode == "")
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
                }, "该存档来源于旧版本，强行读取可能发生意料之外的问题。\n因此发生的问题无需反馈。", new string[] { "继续读取存档", "取消" }, true);
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
