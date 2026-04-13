
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenu
{
    private VisualElement root;
    private VisualElement settingsParent;
    private SettingsPanel settingsPanel = null;
    private readonly string hiddenMenuClassName = "GameOverHidden";
    // Main menu
    private VisualElement rootMain;
    private AnimatedButton continueButton;
    private AnimatedButton settingsButton;
    private AnimatedButton restartButton;
    private AnimatedButton backToMenuButton;

    // Choice confirmation menu
    private enum EChoiceConfirmationUse { None, Restart, Exit };
    private EChoiceConfirmationUse choiceConfirmationUse = EChoiceConfirmationUse.None;
    private VisualElement choiceConfirmationParent;
    private AnimatedButton confirmQuitButton;
    private AnimatedButton cancelChoiceButton;

    NavigationGrid pageNavigation;
    NavigationGrid confirmationNavigation;

    public static Action unpauseGame;
    public static Action backToMainMenu;
    public static Action restartGame;

    public PauseMenu(VisualElement root)
    {
        this.root = root;
        if (root == null) { Debug.LogError("PauseMenu root is null!"); return; }

        rootMain = root.Q<VisualElement>("PauseMain");
        if (rootMain == null) Debug.LogError("PauseMain not found!");

        continueButton = root.Q<AnimatedButton>("Continue");
        if (continueButton == null) Debug.LogError("Continue button not found!");

        settingsButton = root.Q<AnimatedButton>("Settings");
        if (settingsButton == null) Debug.LogError("Settings button not found!");

        restartButton = root.Q<AnimatedButton>("Restart");
        if (restartButton == null) Debug.LogError("Restart button not found!");

        backToMenuButton = root.Q<AnimatedButton>("BackToMenu");
        if (backToMenuButton == null) Debug.LogError("BackToMenu button not found!");

        if (continueButton != null) continueButton.clicked += unpauseGame;
        if (restartButton != null) restartButton.clicked += () => { choiceConfirmationUse = EChoiceConfirmationUse.Restart; ShowChoiceConfirmation(); };
        if (backToMenuButton != null) backToMenuButton.clicked += () => { choiceConfirmationUse = EChoiceConfirmationUse.Exit; ShowChoiceConfirmation(); };
        if (settingsButton != null) settingsButton.clicked += ShowSettings;

        choiceConfirmationParent = root.Q<VisualElement>("NotSavedWarning");
        if (choiceConfirmationParent == null) Debug.LogError("NotSavedWarning not found!");

        if (choiceConfirmationParent != null)
        {
            confirmQuitButton = choiceConfirmationParent.Q<AnimatedButton>("ExitButton");
            if (confirmQuitButton == null) Debug.LogError("ExitButton not found!");

            cancelChoiceButton = choiceConfirmationParent.Q<AnimatedButton>("BackButton");
            if (cancelChoiceButton == null) Debug.LogError("BackButton not found!");

            if (confirmQuitButton != null) confirmQuitButton.clicked += OnChoiceConfirmed;
            if (cancelChoiceButton != null) cancelChoiceButton.clicked += BackToPauseMenu;
        }

        settingsParent = root.Q<VisualElement>("SettingPanel");
        if (settingsParent == null) Debug.LogError("SettingPanel not found!");

        if (settingsParent != null)
        {
            settingsPanel = new SettingsPanel(settingsParent);
            if (settingsPanel != null) settingsPanel.OnClosed += BackToPauseMenu;
        }

        List<NavigationRow> mainNav = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(continueButton)),
            new NavigationRow(new NavigationCell(restartButton)),
            new NavigationRow(new NavigationCell(settingsButton)),
            new NavigationRow(new NavigationCell(backToMenuButton)),
        };
        pageNavigation = new NavigationGrid(mainNav);
        List<NavigationRow> confirmNav = new List<NavigationRow>() { new NavigationRow(new List<VisualElement>() { confirmQuitButton, cancelChoiceButton }) };
        confirmationNavigation = new NavigationGrid(confirmNav);
    }

    public void Show()
    {
        if (root == null) { Debug.LogError("PauseMenu root is null in Show()!"); return; }
        root.RemoveFromClassList(hiddenMenuClassName);
        root.RegisterCallback<NavigationMoveEvent>(OnMove, TrickleDown.TrickleDown);
        root.RegisterCallback<NavigationCancelEvent>(OnCancel);
        BackToPauseMenu();
    }

    public void Hide()
    {
        root.UnregisterCallback<NavigationMoveEvent>(OnMove, TrickleDown.TrickleDown);
        root.UnregisterCallback<NavigationCancelEvent>(OnCancel);
        root.AddToClassList(hiddenMenuClassName);
    }

    private void BackToPauseMenu()
    {
        if (rootMain != null) rootMain.style.display = DisplayStyle.Flex;
        if (choiceConfirmationParent != null) choiceConfirmationParent.style.display = DisplayStyle.None;
        choiceConfirmationUse = EChoiceConfirmationUse.None;
        if (settingsPanel != null) settingsPanel.Hide();
        if (continueButton != null) continueButton.Focus();
    }

    private void OnCancel(NavigationCancelEvent evt)
    {
        if (settingsPanel.IsShown() || choiceConfirmationParent.style.display == DisplayStyle.Flex)
        {
            BackToPauseMenu();
        }
        else
        {
            unpauseGame?.Invoke();
        }
    }

    private void ShowSettings()
    {
        settingsPanel.Show();
        rootMain.style.display = DisplayStyle.None;
    }

    private void ShowChoiceConfirmation()
    {
        rootMain.style.display = DisplayStyle.None;
        choiceConfirmationParent.style.display = DisplayStyle.Flex;
        cancelChoiceButton.Focus();
    }

    private void OnChoiceConfirmed()
    {
        if (choiceConfirmationUse == EChoiceConfirmationUse.Restart) restartGame?.Invoke();
        else backToMainMenu?.Invoke();
    }


    private void OnMove(NavigationMoveEvent evt)
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuNavigationSfx);
        bool shouldIgnoreEvent = false;
        if (settingsPanel.IsShown())
        {
            shouldIgnoreEvent = settingsPanel.OnMove(evt);
        }
        else if (choiceConfirmationUse == EChoiceConfirmationUse.None)
        {
            shouldIgnoreEvent = pageNavigation.OnNavigationEvent(evt);
        }
        else
        {
            shouldIgnoreEvent = confirmationNavigation.OnNavigationEvent(evt);
        }
        if (shouldIgnoreEvent)
            root.focusController.IgnoreEvent(evt);
    }

}
