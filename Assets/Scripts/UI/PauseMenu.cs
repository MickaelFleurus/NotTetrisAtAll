
using System;
using System.Collections.Generic;

using UnityEngine.UIElements;

public class PauseMenu : IScreen
{
    private VisualElement root;
    // Main menu
    private VisualElement rootMain;
    private AnimatedButton continueButton;
    private AnimatedButton settingsButton;
    private AnimatedButton restartButton;
    private AnimatedButton backToMenuButton;

    NavigationGrid nav;

    public static Action unpauseGame;

    ScreenHandler screenHandler;

    public PauseMenu(VisualElement root, ScreenHandler screenHandler)
    {
        this.root = root;
        this.screenHandler = screenHandler;

        rootMain = root.Q<VisualElement>("PauseMain");
        continueButton = root.Q<AnimatedButton>("Continue");
        settingsButton = root.Q<AnimatedButton>("Settings");
        restartButton = root.Q<AnimatedButton>("Restart");
        backToMenuButton = root.Q<AnimatedButton>("BackToMenu");

        Dictionary<VisualElement, Action> submitActions = new Dictionary<VisualElement, Action> {
             { continueButton, OnCancel },
              { restartButton, OnRestartPressed } ,
              { settingsButton, () => screenHandler.RequestShow(ScreenHandler.EScreens.SettingsMenu)},
              { backToMenuButton, OnBackToMenuPressed}
        };

        List<NavigationRow> mainNav = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(continueButton)),
            new NavigationRow(new NavigationCell(restartButton)),
            new NavigationRow(new NavigationCell(settingsButton)),
            new NavigationRow(new NavigationCell(backToMenuButton)),
        };
        nav = new NavigationGrid(mainNav, submitActions);
    }

    public void Show()
    {
        rootMain.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        rootMain.style.display = DisplayStyle.None;
    }

    public static void CleanupAllSubscribers()
    {
        unpauseGame = null;
    }

    private void OnRestartPressed()
    {
        screenHandler.ShowConfirmationMenu("Restart", "Cancel", GameEvents.StartGame, screenHandler.CloseModal);
    }

    private void OnBackToMenuPressed()
    {
        screenHandler.ShowConfirmationMenu("Back To Menu", "Cancel", GameEvents.BackToMainMenu, screenHandler.CloseModal);
    }


    public NavigationGrid GetNavigationGrid()
    {
        return nav;
    }

    public void OnCancel()
    {
        screenHandler.RequestPop();
        unpauseGame?.Invoke();
    }
}
