using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
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
                if (MakeChoice.choiceId == 1)
                {
                    MapCamera.SuspensionDrama = false;
                    Active = false; Loading = false;
                    Switcher.Carry("Startup");
                }
            }, "您将丢失所有未保存的存档，确定吗？", new string[] { "退出游戏", "返回标题画面", "取消" });
            return;
        }
        GameObject oldTab = Parent.MenuItems[Parent.MenuIndex].GetComponent<Settings>().LinkTab, 
                   newTab = Parent.MenuItems[Index].GetComponent<Settings>().LinkTab;
        oldTabTransform = oldTab.transform;
        oldTab.SetActive(false);
        oldTab.SetActive(true);
        oldTab.GetComponent<Animator>().enabled = true;
        oldTab.GetComponent<Animator>().Play("TabHide", 0, 0.0f);
        Parent.MenuItems[Parent.MenuIndex].GetComponent<Animator>().Play("MenuItemHide", 0, 0.0f);
        newTab.SetActive(true);
        newTab.GetComponent<Animator>().enabled = true;
        newTab.GetComponent<Animator>().Play("TabShow", 0, 0.0f);
        Parent.MenuItems[Index].GetComponent<Animator>().Play("MenuItemShow", 0, 0.0f);
        Parent.MenuIndex = Index;
        Parent.BackIcon.GetComponent<Animator>().Play("BackIconHide", 0, 0.0f);
        Parent.scrollController.ScrollContainer = newTab.transform;
        Parent.scrollController.UpdateContainer();
        if (Parent.MenuIndex == 1 && NeedScrollToBottom)
        {
            Parent.scrollController.ScrollToBottom();
        }
    }
    public void DisableAllTabAnimations()
    {
        foreach(GameObject go in MenuItems)
        {
            GameObject tab = go.GetComponent<Settings>().LinkTab;
            if (tab != null)
            {
                tab.GetComponent<Animator>().enabled = false;
            }
        }
        BackIcon.GetComponent<Animator>().enabled = false;
    }
    public void MenuItemHideCallback()
    {
        GameObject newTab = Parent.MenuItems[Parent.MenuIndex].GetComponent<Settings>().LinkTab;
        Parent.BackIcon.SetActive(false);
        Parent.BackIcon.SetActive(true);
        Parent.BackIcon.GetComponent<Image>().sprite = Parent.MenuItems[Parent.MenuIndex].GetComponent<Settings>().BackIcon.GetComponent<Image>().sprite;
        Parent.BackIcon.GetComponent<Animator>().enabled = true;
        Parent.BackIcon.GetComponent<Animator>().Play("BackIconShow", 0, 0.0f);
        if (oldTabTransform == null) return;
        Parent.scrollController.ResetSavedPosition(oldTabTransform);
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
        MainSettingUI = this;
    }

    public static void Show()
    {
        if (Active || Loading) return;
        Active = true; Loading = true;
        LastSuspensionDrama = MapCamera.SuspensionDrama;
        if (Dramas.DramaUnloading) LastSuspensionDrama = false;
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
        MainSettingUI.DisableAllTabAnimations();
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
