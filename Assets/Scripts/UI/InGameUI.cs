using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private Label scoreLabel;
    private Label lineCompletedLabel;
    private Label levelLabel;
    private Image heldPiece;
    private Label timerLabel;
    private List<Image> nextPieces = new List<Image>(3);


    // Game Over Screen
    private Label gameOverLabel;
    private Label timesUpLabel;
    private VisualElement gameOverScreen;
    private VisualElement introScreen;
    private Button restartButton;
    private Button backToMenuButton;
    private Label finalScoreLabel;
    private Label linesCompletedLabel;
    private Label timeSurvivedLabel;
    private Label levelReachedLabel;
    NavigationGrid gameOverNav;

    // Pause menu
    private PauseMenu pauseMenu = null;
    public bool isPauseMenuShown { get; private set; } = false;

    private readonly string hiddenMenuClassName = "GameOverHidden";

    void Start()
    {
        scoreLabel = uiDocument.rootVisualElement.Q<Label>("ScoreValue");
        lineCompletedLabel = uiDocument.rootVisualElement.Q<Label>("LinesValue");
        levelLabel = uiDocument.rootVisualElement.Q<Label>("LevelValue");
        timerLabel = uiDocument.rootVisualElement.Q<Label>("TimeNumber");

        gameOverLabel = uiDocument.rootVisualElement.Q<Label>("GameOverLabel");
        timesUpLabel = uiDocument.rootVisualElement.Q<Label>("TimeIsUpLabel");

        nextPieces.Add(uiDocument.rootVisualElement.Q<Image>("NextPiece1Image"));
        nextPieces.Add(uiDocument.rootVisualElement.Q<Image>("NextPiece2Image"));
        nextPieces.Add(uiDocument.rootVisualElement.Q<Image>("NextPiece3Image"));

        heldPiece = uiDocument.rootVisualElement.Q<Image>("HeldPieceImage");
        gameOverScreen = uiDocument.rootVisualElement.Q<VisualElement>("GameOverScreen");
        finalScoreLabel = uiDocument.rootVisualElement.Q<Label>("FinalScore");
        linesCompletedLabel = uiDocument.rootVisualElement.Q<Label>("LinesCompleted");
        timeSurvivedLabel = uiDocument.rootVisualElement.Q<Label>("TimeSurvived");
        levelReachedLabel = uiDocument.rootVisualElement.Q<Label>("LevelReached");

        introScreen = uiDocument.rootVisualElement.Q<VisualElement>("ReadyGoScreen");

        pauseMenu = new PauseMenu(uiDocument.rootVisualElement.Q<VisualElement>("PauseMenu"));

        restartButton = uiDocument.rootVisualElement.Q<Button>("RestartButton");
        backToMenuButton = uiDocument.rootVisualElement.Q<Button>("BackMainMenuButton");

        restartButton.clicked += RestartGame;
        backToMenuButton.clicked += BackToMainMenu;

        List<NavigationRow> mainNav = new List<NavigationRow>() {
            new NavigationRow(new NavigationCell(restartButton)),
            new NavigationRow(new NavigationCell(backToMenuButton))
        };
        gameOverNav = new NavigationGrid(mainNav);

        PauseMenu.backToMainMenu += BackToMainMenu;
        PauseMenu.restartGame += RestartGame;
    }

    public void UpdateScore(int score)
    {
        scoreLabel.text = score.ToString();
    }

    public void UpdateLines(int lines)
    {
        lineCompletedLabel.text = lines.ToString();
    }

    public void UpdateLevel(int level)
    {
        levelLabel.text = level.ToString();
    }

    public void UpdateTimer(string value)
    {
        timerLabel.text = value;
    }

    public void UpdateHeldPieceTexture(Sprite sprite)
    {
        heldPiece.sprite = sprite;
    }

    public Sprite PushNextPieceTexture(Sprite sprite)
    {
        Sprite popedSprite = nextPieces[0].sprite;
        for (int i = 0; i < nextPieces.Count - 1; i++)
        {
            nextPieces[i].sprite = nextPieces[i + 1].sprite;
        }
        nextPieces[2].sprite = sprite;
        return popedSprite;
    }

    public void ShowGameOver()
    {
        gameOverScreen.RemoveFromClassList(hiddenMenuClassName);
        gameOverScreen.RegisterCallback<NavigationMoveEvent>(OnGameOverMove, TrickleDown.TrickleDown);
        levelLabel.style.display = DisplayStyle.None;
        lineCompletedLabel.style.display = DisplayStyle.None;
        scoreLabel.style.display = DisplayStyle.None;
        timeSurvivedLabel.style.display = DisplayStyle.Flex;

        gameOverLabel.style.display = DisplayStyle.Flex;
        timesUpLabel.style.display = DisplayStyle.None;

        UpdateLabel(finalScoreLabel, scoreLabel.text);
        UpdateLabel(linesCompletedLabel, lineCompletedLabel.text);
        UpdateLabel(timeSurvivedLabel, timerLabel.text);
        UpdateLabel(levelReachedLabel, levelLabel.text);

        gameOverNav.RestoreFocus();
    }

    public void ShowGameTimerDone()
    {
        gameOverScreen.RemoveFromClassList(hiddenMenuClassName);
        gameOverScreen.RegisterCallback<NavigationMoveEvent>(OnGameOverMove, TrickleDown.TrickleDown);
        levelLabel.style.display = DisplayStyle.None;
        lineCompletedLabel.style.display = DisplayStyle.None;
        scoreLabel.style.display = DisplayStyle.None;
        timeSurvivedLabel.style.display = DisplayStyle.None;

        gameOverLabel.style.display = DisplayStyle.None;
        timesUpLabel.style.display = DisplayStyle.Flex;

        UpdateLabel(finalScoreLabel, scoreLabel.text);
        UpdateLabel(linesCompletedLabel, lineCompletedLabel.text);
        UpdateLabel(timeSurvivedLabel, lineCompletedLabel.text);
        UpdateLabel(levelReachedLabel, levelLabel.text);
        gameOverNav.RestoreFocus();
    }

    private void OnGameOverMove(NavigationMoveEvent evt)
    {
        gameOverNav.OnNavigationEvent(evt);
        gameOverScreen.focusController.IgnoreEvent(evt);
    }

    private void UpdateLabel(Label label, string value)
    {
        var text = label.text;
        text = text.Replace("{x}", value.ToString());
        label.text = text;
    }

    public void ShowPauseMenu()
    {
        pauseMenu.Show();
    }

    public void HidePauseMenu()
    {
        pauseMenu.Hide();
    }

    private void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void BackToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void HideIntro()
    {
        introScreen.style.display = DisplayStyle.None;

    }

    public void ShowIntro()
    {
        introScreen.style.display = DisplayStyle.Flex;

    }

    void OnDestroy()
    {
        gameOverScreen.UnregisterCallback<NavigationMoveEvent>(OnGameOverMove, TrickleDown.TrickleDown);
        PauseMenu.backToMainMenu -= BackToMainMenu;
        PauseMenu.restartGame -= RestartGame;
        PauseMenu.CleanupAllSubscribers();
        restartButton.clicked -= RestartGame;
        backToMenuButton.clicked -= BackToMainMenu;
    }
}
