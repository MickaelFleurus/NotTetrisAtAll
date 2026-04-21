using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SettingsPanel
{
    private VisualElement settingsPanel;
    private AnimatedSlider masterVolume;
    private AnimatedSlider musicVolume;
    private AnimatedSlider soundEffectsVolume;
    private AnimatedButton backButton;

    private VisualElement controlChangeParent;
    public event Action OnClosed;
    NavigationGrid pageNavigation;
    private string currentPreferredInputGroup;

    public SettingsPanel(VisualElement settingsPanel)
    {
        this.settingsPanel = settingsPanel;

        masterVolume = settingsPanel.Q<AnimatedSlider>("MasterAudio");
        musicVolume = settingsPanel.Q<AnimatedSlider>("MusicVolume");
        soundEffectsVolume = settingsPanel.Q<AnimatedSlider>("SFXVolume");
        backButton = settingsPanel.Q<AnimatedButton>("SettingsCancelButton");
        controlChangeParent = settingsPanel.Q<VisualElement>("ControlLabel");

        masterVolume.RegisterValueChangedCallback(OnMasterVolumeChanged);
        musicVolume.RegisterValueChangedCallback(OnMusicVolumeChanged);
        soundEffectsVolume.RegisterValueChangedCallback(OnSoundEffectsVolumeChanged);
        SetupNavigation();

        // Get initial input group and build UI
        currentPreferredInputGroup = GetPreferredInputGroup();
        BuildUI();

        // Listen for device changes
        InputSystem.onDeviceChange += OnInputDeviceChanged;
    }

    ~SettingsPanel()
    {
        InputSystem.onDeviceChange -= OnInputDeviceChanged;
    }

    private void OnInputDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        // Check if the preferred input group has changed
        string newPreferredGroup = GetPreferredInputGroup();
        if (newPreferredGroup != currentPreferredInputGroup)
        {
            currentPreferredInputGroup = newPreferredGroup;
            RebuildUI();
        }
    }

    private void RebuildUI()
    {
        // Clear previous UI and rebuild
        controlChangeParent.Clear();
        BuildUI();
    }

    private void BuildUI()
    {
        string preferredInputGroup = GetPreferredInputGroup();  // Detect latest used device

        foreach (var map in PlayerInputs.Instance.GetCustomControls().asset.actionMaps)
        {
            Label sectionLabel = new Label(map.name);
            sectionLabel.AddToClassList("SettingsCategoryParent");
            sectionLabel.AddToClassList("SettingsLabel");
            sectionLabel.AddToClassList("SettingsControlSectionLabel");
            foreach (var action in map.actions)
            {
                var bindings = action.bindings;

                for (int i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];

                    if (binding.groups == null || !binding.groups.Contains(preferredInputGroup))
                        continue;

                    if (binding.isComposite)
                    {
                        Label actionLabel = new Label(action.name);
                        actionLabel.AddToClassList("SettingsCategoryParent");
                        actionLabel.AddToClassList("SettingsLabel");
                        actionLabel.AddToClassList("SettingsControlSectionLabel");
                        sectionLabel.Add(actionLabel);

                        int j = i + 1;

                        while (j < bindings.Count && bindings[j].isPartOfComposite)
                        {
                            var part = bindings[j];

                            // Skip parts that don't match preferred input group
                            if (!part.groups.Contains(preferredInputGroup))
                            {
                                j++;
                                continue;
                            }

                            VisualElement parentControl = new VisualElement();
                            parentControl.AddToClassList("SettingsCategoryParent");
                            parentControl.AddToClassList("SettingsControlRemapParent");
                            actionLabel.Add(parentControl);

                            // Get friendly name for composite part
                            string friendlyName = GetFriendlyBindingName(part);
                            Label bindingLabel = new Label(friendlyName);
                            parentControl.Add(bindingLabel);

                            Button button = new Button();
                            button.AddToClassList("SettingsControlRemapButton");
                            button.text = GetDisplayNameForBinding(part);
                            parentControl.Add(button);
                            j++;
                        }

                        i = j - 1; // skip processed parts
                    }
                    else if (!binding.isPartOfComposite)
                    {
                        VisualElement parentControl = new VisualElement();
                        parentControl.AddToClassList("SettingsCategoryParent");
                        parentControl.AddToClassList("SettingsControlRemapParent");
                        sectionLabel.Add(parentControl);
                        Label actionLabel = new Label(action.name);
                        parentControl.Add(actionLabel);

                        Button button = new Button();
                        button.AddToClassList("SettingsControlRemapButton");
                        button.text = GetDisplayNameForBinding(binding);
                        parentControl.Add(button);
                    }
                }
            }
            controlChangeParent.Add(sectionLabel);
        }
        Debug.Log("[SettingsPanel] BuildUI() completed");

    }

    private string GetFriendlyBindingName(InputBinding binding)
    {
        // Map common control names to user-friendly names
        string path = binding.effectivePath.ToLower();

        if (path.Contains("/up") || path.Contains("up"))
            return "Up";
        if (path.Contains("/down") || path.Contains("down"))
            return "Down";
        if (path.Contains("/left") || path.Contains("left"))
            return "Left";
        if (path.Contains("/right") || path.Contains("right"))
            return "Right";

        // Fallback to the binding name, but clean it up
        return binding.name == "Negative" || binding.name == "Positive"
            ? binding.path.Split('/')[^1]
            : binding.name;
    }

    private string GetDisplayNameForBinding(InputBinding binding)
    {
        // Extract the control name from the path (e.g., "<Keyboard>/a" -> "A", "<Gamepad>/buttonSouth" -> "B")
        string path = binding.effectivePath;

        // Extract control name (last part after /)
        if (path.Contains("/"))
        {
            string controlName = path.Substring(path.LastIndexOf('/') + 1).ToLower();

            // Gamepad button mappings
            if (path.Contains("Gamepad"))
            {
                return controlName switch
                {
                    "buttonsouth" => "A",
                    "buttoneast" => "B",
                    "buttonwest" => "X",
                    "buttonnorth" => "Y",
                    "leftshoulder" => "LB",
                    "rightshoulder" => "RB",
                    "lefttrigger" => "LT",
                    "righttrigger" => "RT",
                    "leftstick/up" => "L-Stick Up",
                    "leftstick/down" => "L-Stick Down",
                    "leftstick/left" => "L-Stick Left",
                    "leftstick/right" => "L-Stick Right",
                    "rightstick/up" => "R-Stick Up",
                    "rightstick/down" => "R-Stick Down",
                    "rightstick/left" => "R-Stick Left",
                    "rightstick/right" => "R-Stick Right",
                    "start" => "Start",
                    "select" => "Select",
                    _ => controlName
                };
            }

            // Keyboard mappings
            if (path.Contains("Keyboard"))
            {
                return controlName.ToUpper();
            }
        }

        return binding.id.ToString();
    }

    private string GetPreferredInputGroup()
    {
        // Find the device that was used most recently
        InputDevice latestDevice = null;
        double latestTime = 0;

        foreach (var device in InputSystem.devices)
        {
            if (device.lastUpdateTime > latestTime)
            {
                latestTime = device.lastUpdateTime;
                latestDevice = device;
            }
        }

        // Map device type to input group
        if (latestDevice is Gamepad)
            return "Gamepad";
        if (latestDevice is Keyboard)
            return "Keyboard&Mouse";
        if (latestDevice is Mouse)
            return "Keyboard&Mouse";

        // Default to Gamepad if unable to determine
        return "Gamepad";
    }

    private void SetupNavigation()
    {
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(backButton)),
            new NavigationRow(new NavigationCell(masterVolume)),
            new NavigationRow(new NavigationCell(musicVolume)),
            new NavigationRow(new NavigationCell(soundEffectsVolume)),
        };
        Dictionary<VisualElement, Action> actionDictionary = new Dictionary<VisualElement, Action> { { backButton, Hide } };

        pageNavigation = new NavigationGrid(rows, actionDictionary, 0, 1);

        pageNavigation.cancelPressed += Hide;
    }

    public bool IsShown()
    {
        return settingsPanel.style.display == DisplayStyle.Flex;
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
        settingsPanel.style.display = DisplayStyle.Flex;
        masterVolume.value = GameSettings.Instance.MasterVolume * 100f;
        musicVolume.value = GameSettings.Instance.MusicVolume * 100f;
        soundEffectsVolume.value = GameSettings.Instance.SoundEffectsVolume * 100f;

        pageNavigation.Enable();
        pageNavigation.RestoreFocus();
    }

    public void Hide()
    {
        if (settingsPanel.style.display == DisplayStyle.Flex)
        {
            settingsPanel.style.display = DisplayStyle.None;
            pageNavigation.Disable();
            OnClosed.Invoke();
        }
    }
}
