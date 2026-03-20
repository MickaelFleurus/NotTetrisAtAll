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

    Gradient myGradient = new Gradient
    {
        colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.007f, 0.047f, 0.40f), 0.0f),
            new GradientColorKey(new Color(0.0f, 0.2f, 1.0f), 0.5f),
            new GradientColorKey(new Color(0.56f, 0.325f, 0.929f), 1.0f)
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

        var texture = GradientToTexture(myGradient);
        element.style.backgroundImage = new StyleBackground(texture);

        gameRender.style.aspectRatio = GridHandler.Width / (float)GridHandler.Height;
    }

    private Texture2D GradientToTexture(Gradient gradient, int width = 16, int height = 256)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            float t = y / (float)(height - 1);
            Color color = gradient.Evaluate(t);

            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, color);
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

}
