using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsPanel
{
    private VisualElement mSettingsPanel;
    private AnimatedSlider mMasterVolume;
    private AnimatedSlider mMusicVolume;
    private AnimatedSlider mSoundEffectsVolume;
    private AnimatedButton mBackButton;
    public event Action OnClosed;
    NavigationGrid mPageNavigation;



    public SettingsPanel(VisualElement settingsPanel, MonoBehaviour coroutineRunner)
    {
        mSettingsPanel = settingsPanel;

        mMasterVolume = mSettingsPanel.Q<AnimatedSlider>("MasterAudio");
        mMusicVolume = mSettingsPanel.Q<AnimatedSlider>("MusicVolume");
        mSoundEffectsVolume = mSettingsPanel.Q<AnimatedSlider>("SFXVolume");
        mBackButton = mSettingsPanel.Q<AnimatedButton>("SettingsBackButton");

        mMasterVolume.RegisterValueChangedCallback(OnMasterVolumeChanged);
        mMusicVolume.RegisterValueChangedCallback(OnMusicVolumeChanged);
        mSoundEffectsVolume.RegisterValueChangedCallback(OnSoundEffectsVolumeChanged);
        SetupNavigation(coroutineRunner);
    }

    private void SetupNavigation(MonoBehaviour coroutineRunner)
    {
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(mBackButton)),
            new NavigationRow(new NavigationCell(mMasterVolume)),
            new NavigationRow(new NavigationCell(mMusicVolume)),
            new NavigationRow(new NavigationCell(mSoundEffectsVolume)),
        };
        Dictionary<VisualElement, Action> actionDictionary = new Dictionary<VisualElement, Action> { { mBackButton, Hide } };

        mPageNavigation = new NavigationGrid(rows, coroutineRunner, 0, 1);
        mPageNavigation.SetupSubmitEvent(actionDictionary);
        mPageNavigation.cancelPressed += Hide;
    }

    public bool IsShown()
    {
        return mSettingsPanel.style.display == DisplayStyle.Flex;
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

        mPageNavigation.Enable();
        mPageNavigation.RestoreFocus();
    }

    public void Hide()
    {
        if (mSettingsPanel.style.display == DisplayStyle.Flex)
        {
            mSettingsPanel.style.display = DisplayStyle.None;
            mPageNavigation.Disable();
            OnClosed.Invoke();
        }
    }
}
