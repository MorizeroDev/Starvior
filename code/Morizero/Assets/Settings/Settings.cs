using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public List<GameObject> MenuItems;
    public GameObject LinkTab;
    public ScrollController scrollController;
    [HideInInspector]    
    public int MenuIndex = 0;
    public static List<VolumeSet> VolumeSets = new List<VolumeSet>();
    public static bool Active = false;
    public static bool Loading = false;
    public static bool LastSuspensionDrama;
    public static Animator ActiveSetAnimator;
    [HideInInspector]
    public int Index;
    [HideInInspector]
    public Settings Parent;
    public void OnClick()
    {
        if (Parent.MenuIndex == Index) return;
        if (Index == 2)
        {
            // 时空碎片
            return;
        }
        if (Index == 4)
        {
            // 退出游戏
            MakeChoice.Create(() =>
            {
                if (MakeChoice.choiceId == 0) Application.Quit();
            }, "在您已确认存档保存的情况下，确定要退出游戏吗？", new string[] { "退出游戏", "取消" });
            return;
        }
        GameObject oldTab = Parent.MenuItems[Parent.MenuIndex].GetComponent<Settings>().LinkTab, 
                   newTab = Parent.MenuItems[Index].GetComponent<Settings>().LinkTab;
        oldTab.SetActive(false);
        oldTab.SetActive(true);
        oldTab.GetComponent<Animator>().Play("TabHide", 0, 0.0f);
        Parent.MenuItems[Parent.MenuIndex].GetComponent<Animator>().Play("MenuItemHide", 0, 0.0f);
        newTab.SetActive(true);
        newTab.GetComponent<Animator>().Play("TabShow", 0, 0.0f);
        Parent.MenuItems[Index].GetComponent<Animator>().Play("MenuItemShow", 0, 0.0f);
        Parent.MenuIndex = Index;
    }
    public void MenuItemHideCallback()
    {
        GameObject newTab = Parent.MenuItems[Parent.MenuIndex].GetComponent<Settings>().LinkTab;
        if (newTab == null) return;
        Parent.scrollController.ResetPosition();
        Parent.scrollController.ScrollContainer = newTab.transform;
        Parent.scrollController.UpdateContainer();
    }
    private void Awake()
    {
        if (MenuItems.Count == 0)
        {
            // 事MenuItem~
            return;
        }
        for(int i = 0;i < MenuItems.Count;i++)
        {
            MenuItems[i].GetComponent<Settings>().Index = i;
            MenuItems[i].GetComponent<Settings>().Parent = this;
        }
        Settings.ActiveSetAnimator = GetComponent<Animator>();
    }

    public static void Show()
    {
        if (Active || Loading) return;
        Active = true; Loading = true;
        LastSuspensionDrama = MapCamera.SuspensionDrama;
        MapCamera.SuspensionDrama = true;
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
        MapCamera.SuspensionDrama = LastSuspensionDrama;
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
