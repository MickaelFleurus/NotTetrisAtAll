
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

    bool skipNextCancelEvent = false;

    public PauseMenu(VisualElement root)
    {
        this.root = root;

        rootMain = root.Q<VisualElement>("PauseMain");
        continueButton = root.Q<AnimatedButton>("Continue");
        settingsButton = root.Q<AnimatedButton>("Settings");

        restartButton = root.Q<AnimatedButton>("Restart");

        backToMenuButton = root.Q<AnimatedButton>("BackToMenu");

        continueButton.clicked += unpauseGame;
        restartButton.clicked += () => { choiceConfirmationUse = EChoiceConfirmationUse.Restart; ShowChoiceConfirmation(); };
        backToMenuButton.clicked += () => { choiceConfirmationUse = EChoiceConfirmationUse.Exit; ShowChoiceConfirmation(); };


        choiceConfirmationParent = root.Q<VisualElement>("NotSavedWarning");
        confirmQuitButton = choiceConfirmationParent.Q<AnimatedButton>("ExitButton");
        cancelChoiceButton = choiceConfirmationParent.Q<AnimatedButton>("BackButton");


        settingsParent = root.Q<VisualElement>("SettingPanel");

        settingsPanel = new SettingsPanel(settingsParent);


        List<NavigationRow> mainNav = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(continueButton)),
            new NavigationRow(new NavigationCell(restartButton)),
            new NavigationRow(new NavigationCell(settingsButton)),
            new NavigationRow(new NavigationCell(backToMenuButton)),
        };
        pageNavigation = new NavigationGrid(mainNav);
        List<NavigationRow> confirmNav = new List<NavigationRow>() { new NavigationRow(new List<VisualElement>() { cancelChoiceButton, confirmQuitButton }) };
        confirmationNavigation = new NavigationGrid(confirmNav);
    }

    private void RegisterMenuInputs()
    {
        root.RegisterCallback<NavigationMoveEvent>(OnMove, TrickleDown.TrickleDown);
        root.RegisterCallback<NavigationCancelEvent>(OnCancel);
        confirmQuitButton.clicked += OnChoiceConfirmed;
        cancelChoiceButton.clicked += BackToPauseMenu;
        settingsPanel.OnClosed += BackToPauseMenu;
    }

    public void Show()
    {
        BackToPauseMenu();
        root.RemoveFromClassList(hiddenMenuClassName);
        skipNextCancelEvent = true;
    }

    public void Hide()
    {
        root.UnregisterCallback<NavigationMoveEvent>(OnMove, TrickleDown.TrickleDown);
        root.UnregisterCallback<NavigationCancelEvent>(OnCancel);
        root.AddToClassList(hiddenMenuClassName);
    }

    public static void CleanupAllSubscribers()
    {
        unpauseGame = null;
        backToMainMenu = null;
        restartGame = null;
    }

    private void BackToPauseMenu()
    {
        rootMain.style.display = DisplayStyle.Flex;
        choiceConfirmationParent.style.display = DisplayStyle.None;
        choiceConfirmationUse = EChoiceConfirmationUse.None;
        settingsPanel.Hide();
        pageNavigation.RestoreFocus();
    }

    private void OnCancel(NavigationCancelEvent evt)
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuCancelSfx);
        if (skipNextCancelEvent)
        {
            skipNextCancelEvent = false;
            return;
        }
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
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        settingsPanel.Show();
        rootMain.style.display = DisplayStyle.None;
    }

    private void ShowChoiceConfirmation()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        rootMain.style.display = DisplayStyle.None;
        confirmQuitButton.text = choiceConfirmationUse == EChoiceConfirmationUse.Restart ? "Restart" : "Quit";
        choiceConfirmationParent.style.display = DisplayStyle.Flex;
        confirmationNavigation.RestoreFocus();
    }

    private void OnChoiceConfirmed()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.MenuApproveSfx);
        if (choiceConfirmationUse == EChoiceConfirmationUse.Restart) restartGame?.Invoke();
        else backToMainMenu?.Invoke();
    }


    private void OnMove(NavigationMoveEvent evt)
    {
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
