using UnityEngine;

public class CellLogic : MonoBehaviour
{
    public static readonly float BlinkDuration = 0.5f;
    [SerializeField] private Sprite EmptySprite;
    [SerializeField] private Sprite MaybeNextSprite;
    private SpriteRenderer spriteRenderer;
    public enum CellState
    {
        Empty, MaybeNext, Filled, Blinking
    }
    public CellState currentState { get; private set; }


    public Sprite GetSprite() => spriteRenderer.sprite;

    private float blinkingTimeLeft;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = EmptySprite;
        currentState = CellState.Empty;
    }

    public bool IsEmpty() => currentState != CellState.Filled;
    public bool IsFilled() => currentState == CellState.Filled;


    public void Clear()
    {
        currentState = CellState.Empty;
        spriteRenderer.sprite = EmptySprite;
        var color = spriteRenderer.color;
        color.a = 1.0f;
        spriteRenderer.color = color;
    }

    public void MarkAsMaybeNext()
    {
        if (currentState != CellState.Empty) return;
        currentState = CellState.MaybeNext;
        spriteRenderer.sprite = MaybeNextSprite;
        var color = spriteRenderer.color;
        color.a = 0.50f;
        spriteRenderer.color = color;
    }

    // Optionally make SetFilled treat null as Clear:
    public void SetFilled(Sprite sprite)
    {
        if (sprite == null) { Clear(); return; }
        spriteRenderer.sprite = sprite;
        currentState = CellState.Filled;
        var color = spriteRenderer.color;
        color.a = 1.0f;
        spriteRenderer.color = color;
    }

    public void PrepareToBlink()
    {
        if (currentState != CellState.Filled) return;
        currentState = CellState.Blinking;
        blinkingTimeLeft = BlinkDuration;
    }


    // Update is called once per frame
    void Update()
    {
        if (currentState == CellState.Blinking)
        {
            blinkingTimeLeft -= Time.deltaTime;
            float elapsed = BlinkDuration - blinkingTimeLeft;
            var color = spriteRenderer.color;
            color.a = Mathf.PingPong(elapsed * 10.0f, 1);
            spriteRenderer.color = color;
            if (blinkingTimeLeft <= 0)
            {
                Clear();
            }
        }
    }
}
