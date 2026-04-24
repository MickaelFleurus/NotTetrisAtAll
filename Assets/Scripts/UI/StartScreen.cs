using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class StartScreen : IScreen
{
    private VisualElement root;

    private Button startButton;
    private Button settingsButton;
    private Button creditButton;
    private Button quitButton;
    NavigationGrid nav;
    ScreenHandler screenHandler;

    public StartScreen(VisualElement root, ScreenHandler screenHandler)
    {
        this.root = root;
        this.screenHandler = screenHandler;

        startButton = root.Q<Button>("StartButton");
        quitButton = root.Q<Button>("ExitButton");
        settingsButton = root.Q<Button>("SettingsButton");
        creditButton = root.Q<Button>("CreditsButton");


        Dictionary<VisualElement, Action> submitActions = new Dictionary<VisualElement, Action>
        {
            { startButton, () => screenHandler.RequestShow(ScreenHandler.EScreens.GameModeMenu) },
            { quitButton, GameEvents.CloseGame },
            { settingsButton, () => screenHandler.RequestShow(ScreenHandler.EScreens.SettingsMenu) },
            { creditButton, () => screenHandler.RequestShow(ScreenHandler.EScreens.CreditsMenu) },
        };
        SetupNavigation(submitActions);
    }

    private void SetupNavigation(Dictionary<VisualElement, Action> submitActions)
    {
        List<NavigationRow> rows = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(startButton)),
            new NavigationRow(new NavigationCell(settingsButton)),
            new NavigationRow(new NavigationCell(creditButton)),
            new NavigationRow(new NavigationCell(quitButton)),
        };
        nav = new NavigationGrid(rows, submitActions);
    }

    public void Show()
    {
        root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }

    public NavigationGrid GetNavigationGrid()
    {
        return nav;
    }

    public void OnCancel()
    {
        // Do nothing
    }
}
