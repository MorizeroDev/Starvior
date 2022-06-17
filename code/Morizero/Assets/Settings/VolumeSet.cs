using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeSet : MonoBehaviour
{
    public enum AudioType
    {
        BGM,BGS,SE
    }
    public AudioType audioType;
    public void ApplyVolumeSettings()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        if (audioType == AudioType.BGM)
        {
            foreach(AudioSource source in sources)
                source.volume = PlayerPrefs.GetFloat("Settings.BGMVolume", 1);
        }
        else if (audioType == AudioType.BGS)
        {
            foreach (AudioSource source in sources)
                source.volume = PlayerPrefs.GetFloat("Settings.BGSVolume", 0.1f);
        }
        else if (audioType == AudioType.SE)
        {
            foreach (AudioSource source in sources)
                source.volume = PlayerPrefs.GetFloat("Settings.SEVolume", 0.5f);
        }
    }
    private void Awake()
    {
        ApplyVolumeSettings();
        Settings.VolumeSets.Add(this);
    }
    private void OnDestroy()
    {
        Settings.VolumeSets.Remove(this);
    }
}
