using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionOptions;
    private FullScreenMode windowMode;
    [SerializeField] private AudioSource[] soundEffectSources;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] Slider sfxSlider, musicSlider;

      public int frameTarget = 120;
      
      private void Awake()
      {
        QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = frameTarget;
      }
      
      private void Update()
      {
        SetVolumeSFX(sfxSlider.value);
        SetVolumeMusic(musicSlider.value);
        //SetWindowSize(resolutionOptions.value);
      }

    public void SetWindowMode(string _windowMode)
    {
        if(_windowMode  == "Windowed") {
            windowMode = FullScreenMode.Windowed;
        }
        else if (_windowMode  == "Borderless") {
            windowMode = FullScreenMode.FullScreenWindow;
        }
        else if (_windowMode  == "Fullscreen") {
            windowMode = FullScreenMode.ExclusiveFullScreen;
        }
    }

    public void SetWindowSize(int scale)
    {
        if(scale <= 1) scale = 1;

        int windowWidth = 1280 * scale;
        int windowHeight = 720 * scale;

        Screen.SetResolution(windowWidth, windowHeight, windowMode, 60);
    }

    public void SetGameQuality(int qualityLevel)
    {
        QualitySettings.SetQualityLevel(qualityLevel, false);
        //QualitySettings.renderPipeline = qualityLevels[value];
    }

    public void SetVolumeSFX(float volume)
    {
        foreach(AudioSource audioSource in soundEffectSources)
        {
            audioSource.volume = volume;
        }
    }

    public void SetVolumeMusic(float volume)
    {
        musicSource.volume = volume;
    }
}
