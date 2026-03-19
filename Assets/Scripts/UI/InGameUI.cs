using UnityEngine;
using UnityEngine.UIElements;

public class InGameUI : MonoBehaviour
{

    [SerializeField] private UIDocument uiDocument;
    private Image gameRender;
    Gradient myGradient = new Gradient
    {
        colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.02f, 0.05f, 0.20f), 0.0f), // bottom (dark)
            new GradientColorKey(new Color(0.07f, 0.16f, 0.40f), 0.5f), // middle (lighter)
            new GradientColorKey(new Color(0.02f, 0.05f, 0.20f), 1.0f)  // top (dark)
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

        var texture = GradientToTexture(myGradient);
        element.style.backgroundImage = new StyleBackground(texture);

        gameRender.style.aspectRatio = GridHandler.Width / (float)GridHandler.Height;

        var gridElement = new GridElement();// fill the root: absolute + zero on all sides ensures it stretches to rootVisualElement size
        uiDocument.rootVisualElement.Add(gridElement);
        var cam = Camera.main;
        uiDocument.rootVisualElement.Add(gridElement);
        AlignGridElementToCamera(cam, gridElement);
        gridElement.MarkDirtyRepaint();

    }


    void Update()
    {

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

    void AlignGridElementToCamera(Camera cam, VisualElement gridElement)
    {
        // world bounds (adjust if your grid origin or cell size differ)
        Vector3 blWorld = new Vector3(0f, 0f, 0f); // bottom-left world
        Vector3 trWorld = new Vector3(GridHandler.Width, GridHandler.Height, 0f); // top-right world (use Width,Height for inclusive margin)

        Vector3 blScreen = cam.WorldToScreenPoint(blWorld);
        Vector3 trScreen = cam.WorldToScreenPoint(trWorld);

        float left = Mathf.Min(blScreen.x, trScreen.x);
        float right = Mathf.Max(blScreen.x, trScreen.x);
        float bottom = Mathf.Min(blScreen.y, trScreen.y);
        float top = Mathf.Max(blScreen.y, trScreen.y);

        float width = right - left;
        float height = top - bottom;

        // UI root origin is top-left; screen origin is bottom-left
        float uiLeft = left;
        float uiTop = Screen.height - top;

        gridElement.style.position = Position.Absolute;
        gridElement.style.left = uiLeft;
        gridElement.style.top = uiTop;
        gridElement.style.width = width;
        gridElement.style.height = height;
        gridElement.MarkDirtyRepaint();
    }
}
