using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SettingsPanel
{
    private VisualElement mSettingsPanel;
    private AnimatedSlider mMasterVolume;
    private AnimatedSlider mMusicVolume;
    private AnimatedSlider mSoundEffectsVolume;
    private AnimatedButton mBackButton;
    public event Action OnClosed;


    public SettingsPanel(VisualElement settingsPanel)
    {
        mSettingsPanel = settingsPanel;

        mMasterVolume = mSettingsPanel.Q<AnimatedSlider>("MasterAudio");
        mMusicVolume = mSettingsPanel.Q<AnimatedSlider>("MusicVolume");
        mSoundEffectsVolume = mSettingsPanel.Q<AnimatedSlider>("SFXVolume");
        mBackButton = mSettingsPanel.Q<AnimatedButton>("SettingsBackButton");
        mBackButton.clicked += Hide;

        mMasterVolume.RegisterValueChangedCallback(OnMasterVolumeChanged);
        mMusicVolume.RegisterValueChangedCallback(OnMusicVolumeChanged);
        mSoundEffectsVolume.RegisterValueChangedCallback(OnSoundEffectsVolumeChanged);
    }


    private void OnMasterVolumeChanged(ChangeEvent<float> evt)
    {
        GameSettings.Instance.MasterVolume = evt.newValue * 0.01f;
    }

    private void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        GameSettings.Instance.MusicVolume = evt.newValue * 0.01f;
    }

    private void OnSoundEffectsVolumeChanged(ChangeEvent<float> evt)
    {
        GameSettings.Instance.SoundEffectsVolume = evt.newValue * 0.01f;
    }

    public void Show()
    {
        mSettingsPanel.style.display = DisplayStyle.Flex;
        mMasterVolume.value = GameSettings.Instance.MasterVolume * 100f;
        mMusicVolume.value = GameSettings.Instance.MusicVolume * 100f;
        mSoundEffectsVolume.value = GameSettings.Instance.SoundEffectsVolume * 100f;
        mMasterVolume.Focus();
    }

    public void Hide()
    {
        mSettingsPanel.style.display = DisplayStyle.None;
        OnClosed.Invoke();
    }

    public void OnMove(NavigationMoveEvent evt)
    {
        // No navigation in settings for now
    }

}
