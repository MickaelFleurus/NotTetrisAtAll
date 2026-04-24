using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUI : MonoBehaviour
{
    private readonly string hiddenMenuClassName = "GameOverHidden";
    [SerializeField] private UIDocument uiDocument;
    private Label scoreLabel;
    private Label lineCompletedLabel;
    private Label levelLabel;
    private Image heldPiece;
    private Label timerLabel;
    private List<Image> nextPieces = new List<Image>(3);
    private VisualElement introScreen;
    ScreenHandler inGameScreenHandler;

    private VisualElement pauseRoot;
    private VisualElement gameOverRoot;


    void Awake()
    {
        inGameScreenHandler = new ScreenHandler(uiDocument);
    }

    void Start()
    {
        scoreLabel = uiDocument.rootVisualElement.Q<Label>("ScoreValue");
        lineCompletedLabel = uiDocument.rootVisualElement.Q<Label>("LinesValue");
        levelLabel = uiDocument.rootVisualElement.Q<Label>("LevelValue");
        timerLabel = uiDocument.rootVisualElement.Q<Label>("TimeNumber");
        introScreen = uiDocument.rootVisualElement.Q<VisualElement>("ReadyGoScreen");
        pauseRoot = uiDocument.rootVisualElement.Q<VisualElement>("PauseMenu");
        gameOverRoot = uiDocument.rootVisualElement.Q<VisualElement>("GameOverScreen");

        nextPieces.Add(uiDocument.rootVisualElement.Q<Image>("NextPiece1Image"));
        nextPieces.Add(uiDocument.rootVisualElement.Q<Image>("NextPiece2Image"));
        nextPieces.Add(uiDocument.rootVisualElement.Q<Image>("NextPiece3Image"));

        heldPiece = uiDocument.rootVisualElement.Q<Image>("HeldPieceImage");
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
        gameOverRoot.RemoveFromClassList(hiddenMenuClassName);
        inGameScreenHandler.RequestShow(ScreenHandler.EScreens.GameOverMenu);
    }

    public void ShowPauseMenu()
    {
        pauseRoot.RemoveFromClassList(hiddenMenuClassName);
        inGameScreenHandler.RequestShow(ScreenHandler.EScreens.PauseMenu);
    }

    public void HidePauseMenu()
    {
        pauseRoot.AddToClassList(hiddenMenuClassName);
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
        inGameScreenHandler.OnDestroy();
        PauseMenu.CleanupAllSubscribers();
    }
}
