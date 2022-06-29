using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public Text Title;
    public string ENTitle;
    public List<GameObject> MenuItems;
    public GameObject LinkTab;
    private static Transform oldTabTransform;
    public GameObject BackIcon;
    public Text HistoryText;
    public Transform HistoryEndMark;
    private static bool NeedScrollToBottom = false;
    public static Settings MainSettingUI;
    public ScrollController scrollController;
    [HideInInspector]    
    public int ActiveMenu = 0;
    public static List<VolumeSet> VolumeSets = new List<VolumeSet>();
    public static bool Active = false;
    public static bool Loading = false;
    public static bool LastForbiddenMove;
    public static Animator ActiveSetAnimator;
    [HideInInspector]
    public int Index;
    [HideInInspector]
    public Settings Parent;
    [HideInInspector]
    public static bool MenuOpen = false;
    public void OnClick()
    {
        if (MenuOpen) return;
        if (Index == 2)
        {
            // 时空碎片
            SaveController.SaveMode = true;
            SaveController.ShowSave();
            return;
        }
        if (Index == 4)
        {
            // 退出游戏
            MakeChoice.Create(() =>
            {
                if (MakeChoice.choiceId == 0) Application.Quit();
                if (MakeChoice.choiceId == 1)
                {
                    MapCamera.ForbiddenMove = false;
                    Active = false; Loading = false;
                    Destroy(MapCamera.bgm);
                    Destroy(MapCamera.bgs);
                    MapCamera.initTp = -1;
                    MapCamera.bgm = null; MapCamera.bgs = null;
                    Switcher.Carry("Startup");
                }
            }, "您将丢失所有未保存的存档，确定吗？", new string[] { "退出游戏", "返回标题画面", "取消" }, true);
            return;
        }
        Parent.ActiveMenu = Index;
        GameObject Tab = Parent.MenuItems[Index].GetComponent<Settings>().LinkTab;
        oldTabTransform = Tab.transform;

        Tab.SetActive(true);
        Parent.Title.text = Parent.MenuItems[Parent.ActiveMenu].GetComponent<Settings>().ENTitle;
        ActiveSetAnimator.SetFloat("TabSpeed", 1.0f);
        ActiveSetAnimator.Play("TabEnter", 0, 0.0f);
        MenuOpen = true;
        Parent.BackIcon.GetComponent<Image>().sprite = Parent.MenuItems[Parent.ActiveMenu].GetComponent<Settings>().BackIcon.GetComponent<Image>().sprite;
    }
    public void MenuItemHideCallback()
    {
        GameObject Tab = Parent.MenuItems[Parent.ActiveMenu].GetComponent<Settings>().LinkTab;
        if (ActiveSetAnimator.GetFloat("TabSpeed") > 0)
        {
            if (Tab == null) return;
            Parent.scrollController.ScrollContainer = Tab.transform;
            Parent.scrollController.UpdateContainer();
            if (Parent.ActiveMenu == 1 && NeedScrollToBottom)
            {
                Parent.scrollController.ScrollToBottom();
            }
            return;
        }
        MenuOpen = false; Parent.Title.text = "MENU";
        if (oldTabTransform == null) return;
        Parent.scrollController.ResetSavedPosition(oldTabTransform);
        if (Tab != null) Tab.SetActive(false);
    }
    private void Awake()
    {
        if (MenuItems.Count == 0)
        {
            // 事MenuItem~
            return;
        }
        oldTabTransform = scrollController.ScrollContainer;
        for(int i = 0;i < MenuItems.Count;i++)
        {
            MenuItems[i].GetComponent<Settings>().Index = i;
            MenuItems[i].GetComponent<Settings>().Parent = this;
        }
        Parent = this;
        Settings.ActiveSetAnimator = GetComponent<Animator>();
        string his = "";
        foreach (string s in Dramas.HistoryDrama)
        {
            if (s.EndsWith('：'))
            {
                his += "\n<b>" + s + "</b>\n";
            }
            else
            {
                his += s + "\n";
            }
            
        }
        if (his == "") his = "暂无记录。";
        HistoryText.transform.parent.parent.parent.parent.gameObject.SetActive(true);
        HistoryText.transform.parent.parent.gameObject.SetActive(true);
        HistoryText.text = his;
        LayoutRebuilder.ForceRebuildLayoutImmediate(HistoryText.gameObject.GetComponent<RectTransform>());
        Vector3 pos = HistoryEndMark.transform.localPosition;
        float height = HistoryText.gameObject.GetComponent<RectTransform>().sizeDelta.y * 0.858f;
        //Debug.Log(height);
        pos.y -= height;
        NeedScrollToBottom = (height > 1200);
        HistoryEndMark.transform.localPosition = pos;
        HistoryText.transform.parent.parent.gameObject.SetActive(false);
        HistoryText.transform.parent.parent.parent.parent.gameObject.SetActive(false);
        MainSettingUI = this;
    }

    public static void Show()
    {
        if (MapCamera.ForbiddenMove && Dramas.ActiveDrama == null) return;
        if (Active || Loading) return;
        Active = true; Loading = true;
        LastForbiddenMove = MapCamera.ForbiddenMove;
        if (Dramas.DramaUnloading) LastForbiddenMove = false;
        MapCamera.ForbiddenMove = true;
        SettingsBtn.ActiveSettingsBtn.GetComponent<Animator>().Play("SetBtnHide", 0, 0.0f);
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager.LoadSceneAsync("Settings", LoadSceneMode.Additive);
    }
    private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        Loading = false;
    }

    private static void SceneManager_sceneUnloaded(Scene arg0)
    {
        Loading = false; 
        SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
        SettingsBtn.ActiveSettingsBtn.GetComponent<Animator>().Play("SetBtnShow", 0, 0.0f);
    }
    public void AnimationCallback()
    {
        if (this.GetComponent<Animator>().GetFloat("Speed") == 1.0f) return;
        SettingsBtn.ActiveSettingsBtn.SetActive(true);
        SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        SceneManager.UnloadSceneAsync("Settings");
        MapCamera.ForbiddenMove = LastForbiddenMove;
    }
    public static void Hide()
    {
        //Debuger.InstantMessage(Active.ToString() + "/" + Loading.ToString(), Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (!Active || Loading) return;
        Active = false; Loading = true;
        ActiveSetAnimator.SetFloat("Speed", -2.0f);
        ActiveSetAnimator.Play("SettingEnter", 0, 1.0f);
    }

    public static void BroadcastVolumeChange()
    {
        if (MapCamera.mcamera != null) MapCamera.mcamera.ApplyVolumeSettings();
        foreach (VolumeSet vs in VolumeSets)
            vs.ApplyVolumeSettings();
    }
}
