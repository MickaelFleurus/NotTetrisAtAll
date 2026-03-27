using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameUI : MonoBehaviour
{

    [SerializeField] private UIDocument uiDocument;
    private Label scoreLabel;
    private Label lineCompletedLabel;
    private Label levelLabel;
    private Image gameRender;
    private Image heldPiece;
    private List<Image> nextPieces = new List<Image>(3);
    // Game Over Screen

    private VisualElement gameOverScreen;
    private Button restartButton;
    private Label finalScoreLabel;
    private Label linesCompletedLabel;
    private Label timeSurvivedLabel;
    private Label levelReachedLabel;

    Gradient myGradient = new Gradient
    {
        colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.02f, 0.05f, 0.20f), 0.0f),
            new GradientColorKey(new Color(0.07f, 0.16f, 0.40f), 0.5f),
            new GradientColorKey(new Color(0.02f, 0.05f, 0.20f), 1.0f)
        },
        alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f)
        }
    };

    void Start()
    {
        var element = uiDocument.rootVisualElement.Q<VisualElement>("Background");
        gameRender = uiDocument.rootVisualElement.Q<Image>("Game");
        scoreLabel = uiDocument.rootVisualElement.Q<Label>("ScoreValue");
        lineCompletedLabel = uiDocument.rootVisualElement.Q<Label>("LinesValue");
        levelLabel = uiDocument.rootVisualElement.Q<Label>("LevelValue");

        nextPieces.Add(uiDocument.rootVisualElement.Q<Image>("NextPiece1Image"));
        nextPieces.Add(uiDocument.rootVisualElement.Q<Image>("NextPiece2Image"));
        nextPieces.Add(uiDocument.rootVisualElement.Q<Image>("NextPiece3Image"));

        heldPiece = uiDocument.rootVisualElement.Q<Image>("HeldPieceImage");
        gameOverScreen = uiDocument.rootVisualElement.Q<VisualElement>("GameOverScreen");
        finalScoreLabel = uiDocument.rootVisualElement.Q<Label>("FinalScore");
        linesCompletedLabel = uiDocument.rootVisualElement.Q<Label>("LinesCompleted");
        timeSurvivedLabel = uiDocument.rootVisualElement.Q<Label>("TimeSurvived");
        levelReachedLabel = uiDocument.rootVisualElement.Q<Label>("LevelReached");

        var texture = GradientToTexture(myGradient);
        element.style.backgroundImage = new StyleBackground(texture);

        gameRender.style.aspectRatio = GridHandler.Width / (float)GridHandler.Height;

        restartButton = uiDocument.rootVisualElement.Q<Button>("RestartButton");

        restartButton.clicked += () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        };
        uiDocument.rootVisualElement.Q<Button>("BackMainMenuButton").clicked += () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        };
    }

    private Texture2D GradientToTexture(Gradient gradient, int width = 16, int height = 256)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(width / 2f, height / 2f);
        float maxDist = Mathf.Max(width, height) / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(pos, center);
                float vignette = 1f - Mathf.Clamp01(dist / maxDist);
                vignette = Mathf.Pow(vignette, 0.5f); // smooth falloff curve

                Color color = gradient.Evaluate(0.5f);
                color = Color.Lerp(Color.black, color, vignette);  // ← blend to black
                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply();
        return tex;
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
        gameOverScreen.RemoveFromClassList("GameOverHidden");
        levelLabel.style.display = DisplayStyle.None;
        lineCompletedLabel.style.display = DisplayStyle.None;
        scoreLabel.style.display = DisplayStyle.None;

        UpdateLabel(finalScoreLabel, scoreLabel.text);
        UpdateLabel(linesCompletedLabel, lineCompletedLabel.text);
        UpdateLabel(timeSurvivedLabel, lineCompletedLabel.text);
        UpdateLabel(levelReachedLabel, levelLabel.text);

        restartButton.Focus();
    }

    private void UpdateLabel(Label label, string value)
    {
        var text = label.text;
        text = text.Replace("{x}", value.ToString());
        label.text = text;
    }

}
