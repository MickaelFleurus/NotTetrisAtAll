using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

class GameOverScreen : IScreen
{
    private Label gameOverLabel;
    private Label timesUpLabel;
    private VisualElement gameOverScreen;
    private Button restartButton;
    private Button backToMenuButton;
    private Label finalScoreLabel;
    private Label linesCompletedLabel;
    private Label timeSurvivedLabel;
    private Label levelReachedLabel;
    NavigationGrid nav;
    VisualElement root;
    ScreenHandler screenHandler;
    private readonly string hiddenMenuClassName = "GameOverHidden";



    public GameOverScreen(VisualElement root, ScreenHandler screenHandler)
    {
        this.root = root;
        this.screenHandler = screenHandler;
        gameOverLabel = root.Q<Label>("GameOverLabel");
        timesUpLabel = root.Q<Label>("TimeIsUpLabel");
        gameOverScreen = root.Q<VisualElement>("GameOverScreen");
        finalScoreLabel = root.Q<Label>("FinalScore");
        linesCompletedLabel = root.Q<Label>("LinesCompleted");
        timeSurvivedLabel = root.Q<Label>("TimeSurvived");
        levelReachedLabel = root.Q<Label>("LevelReached");

        restartButton = root.Q<Button>("RestartButton");
        backToMenuButton = root.Q<Button>("BackMainMenuButton");

        List<NavigationRow> mainNav = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(restartButton)),
            new NavigationRow(new NavigationCell(backToMenuButton))
        };
        Dictionary<VisualElement, Action> submitActions = new Dictionary<VisualElement, Action>
        { { restartButton, GameEvents.StartGame }, { backToMenuButton, GameEvents.BackToMainMenu } };

        nav = new NavigationGrid(mainNav, submitActions);
    }

    public NavigationGrid GetNavigationGrid()
    {
        return nav;
    }

    public void Hide()
    {
        gameOverScreen.AddToClassList(hiddenMenuClassName);
    }

    public void OnCancel()
    {
    }

    public void Show()
    {
        gameOverScreen.RemoveFromClassList(hiddenMenuClassName);
        var data = GameData.Instance.gameOverData;
        if (data.gameOverReason == GameData.GameOverData.EGameOverReason.Lost)
        {
            gameOverLabel.style.display = DisplayStyle.Flex;
            timesUpLabel.style.display = DisplayStyle.None;
            timeSurvivedLabel.style.display = DisplayStyle.Flex;
        }
        else
        {
            gameOverLabel.style.display = DisplayStyle.None;
            timesUpLabel.style.display = DisplayStyle.Flex;
            timeSurvivedLabel.style.display = DisplayStyle.None;
        }

        UpdateLabel(finalScoreLabel, data.finalScore);
        UpdateLabel(linesCompletedLabel, data.lineCompleted);
        UpdateLabel(timeSurvivedLabel, data.finalTimer);
        UpdateLabel(levelReachedLabel, data.finalLevel);
    }

    private void UpdateLabel(Label label, int value)
    {
        var text = label.text;
        text = text.Replace("{x}", value.ToString());
        label.text = text;
    }

    private void UpdateLabel(Label label, Timer value)
    {
        var text = label.text;
        text = text.Replace("{x}", value.GetTime());
        label.text = text;
    }
}
