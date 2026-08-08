using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] private GameSettingSO gameSetting;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider uiVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    // AudioMixer에 Exposed 처리한 파라미터 이름과 일치해야 함
    private const string MASTER_PARAM = "MasterVolume";
    private const string BGM_PARAM = "BgmVolume";
    private const string SFX_PARAM = "SfxVolume";
    private const string UI_PARAM = "UIVolume";

    void OnEnable()
    {
        LoadSliderValues();
    }

    private void LoadSliderValues()
    {
        masterVolumeSlider.value = gameSetting.SettingData.masterVolume;
        bgmVolumeSlider.value = gameSetting.SettingData.bgmVolume;
        uiVolumeSlider.value = gameSetting.SettingData.uiVolume;
        sfxVolumeSlider.value = gameSetting.SettingData.sfxVolume;
    }
    
    
    // 선형 값(0.0001~1)을 데시벨(-80~0)로 변환하여 AudioMixer에 전달
    

    public void OnChangeMasterVolume()
    {
        gameSetting.SettingData.masterVolume = masterVolumeSlider.value;
        gameSetting.SetAudioMixerVolume(MASTER_PARAM, masterVolumeSlider.value);
    }

    public void OnChangeBgmVolume()
    {
        gameSetting.SettingData.bgmVolume = bgmVolumeSlider.value;
        gameSetting.SetAudioMixerVolume(BGM_PARAM, bgmVolumeSlider.value);
    }
    public void OnChangeUIVolume()
    {
        gameSetting.SettingData.uiVolume = uiVolumeSlider.value;
        gameSetting.SetAudioMixerVolume(UI_PARAM, uiVolumeSlider.value);
    }


    public void OnChangeSfxVolume()
    {
        gameSetting.SettingData.sfxVolume = sfxVolumeSlider.value;
        gameSetting.SetAudioMixerVolume(SFX_PARAM, sfxVolumeSlider.value);
    }

    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        gameSetting.SaveSettings();
    }
}
