using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public static bool Active = false;
    public static bool Loading = false;
    public static bool LastSuspensionDrama;
    public static Animator ActiveSetAnimator;
    static Settings()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
    }

    public static void Show()
    {
        if (Active || Loading) return;
        Active = true; Loading = true;
        LastSuspensionDrama = MapCamera.SuspensionDrama;
        MapCamera.SuspensionDrama = true;
        SettingsBtn.ActiveSettingsBtn.GetComponent<Animator>().Play("SetBtnHide", 0, 0.0f);
        SceneManager.LoadSceneAsync("Settings", LoadSceneMode.Additive);
    }
    private void Start()
    {
        ActiveSetAnimator = GetComponent<Animator>();
    }
    private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Loading = false;
    }

    private static void SceneManager_sceneUnloaded(Scene arg0)
    {
        Loading = false;
        SettingsBtn.ActiveSettingsBtn.GetComponent<Animator>().Play("SetBtnShow", 0, 0.0f);
    }
    public void AnimationCallback()
    {
        if (this.GetComponent<Animator>().GetFloat("Speed") == 1.0f) return;
        SettingsBtn.ActiveSettingsBtn.SetActive(true);
        SceneManager.UnloadSceneAsync("Settings");
        MapCamera.SuspensionDrama = LastSuspensionDrama;
    }
    public static void Hide()
    {
        if (!Active || Loading) return;
        Active = false; Loading = true;
        ActiveSetAnimator.SetFloat("Speed", -2.0f);
        ActiveSetAnimator.Play("SettingEnter", 0, 1.0f);
    }
}
