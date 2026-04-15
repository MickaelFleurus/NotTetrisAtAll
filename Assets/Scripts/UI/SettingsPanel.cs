using System;
using System.Collections.Generic;

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



    public SettingsPanel(VisualElement settingsPanel)
    {
        mSettingsPanel = settingsPanel;

        mMasterVolume = mSettingsPanel.Q<AnimatedSlider>("MasterAudio");
        mMusicVolume = mSettingsPanel.Q<AnimatedSlider>("MusicVolume");
        mSoundEffectsVolume = mSettingsPanel.Q<AnimatedSlider>("SFXVolume");
        mBackButton = mSettingsPanel.Q<AnimatedButton>("SettingsBackButton");
        mBackButton.clicked += () =>
        {
            AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
            Hide();
        };


        mMasterVolume.RegisterValueChangedCallback(OnMasterVolumeChanged);
        mMusicVolume.RegisterValueChangedCallback(OnMusicVolumeChanged);
        mSoundEffectsVolume.RegisterValueChangedCallback(OnSoundEffectsVolumeChanged);
        SetupNavigation();
    }

    private void SetupNavigation()
    {
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(mBackButton)),
            new NavigationRow(new NavigationCell(mMasterVolume)),
            new NavigationRow(new NavigationCell(mMusicVolume)),
            new NavigationRow(new NavigationCell(mSoundEffectsVolume)),
        };
        mPageNavigation = new NavigationGrid(rows, 0, 1);

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

        mPageNavigation.RestoreFocus();
    }

    public void Hide()
    {
        if (mSettingsPanel.style.display == DisplayStyle.Flex)
        {
            mSettingsPanel.style.display = DisplayStyle.None;
            OnClosed.Invoke();
        }
    }

    public bool OnMove(NavigationMoveEvent evt)
    {
        return mPageNavigation.OnNavigationEvent(evt);
    }

}
