using UnityEngine;
using Piece;

public class CubeRain : MonoBehaviour
{
    [SerializeField] private float spawnRate = 0.2f; // Spawn a cube every 0.2 seconds
    [SerializeField] private float minFallSpeed = 2f;
    [SerializeField] private float maxFallSpeed = 8f;
    [SerializeField] private float minCubeSize = 0.2f;
    [SerializeField] private float maxCubeSize = 1f;
    [SerializeField] private float edgeMargin = 0.5f;

    private float spawnTimer = 0f;
    private Camera mainCamera;
    private float screenLeft;
    private float screenRight;
    private float screenTop;
    private float screenBottom;

    void Start()
    {
        mainCamera = Camera.main;
        UpdateCameraBounds();
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnCube();
            spawnTimer = spawnRate;
        }

    }

    private void UpdateCameraBounds()
    {
        if (mainCamera == null) return;

        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        Vector3 cameraPos = mainCamera.transform.position;
        screenLeft = cameraPos.x - cameraWidth / 2f;
        screenRight = cameraPos.x + cameraWidth / 2f;
        screenTop = cameraPos.y + cameraHeight / 2f;
        screenBottom = cameraPos.y - cameraHeight / 2f;
    }

    private void SpawnCube()
    {
        // Create a new GameObject for the cube
        GameObject cubeGO = new GameObject("FallingCube");
        cubeGO.transform.SetParent(this.transform);

        // Randomize cube size
        float randomSize = Random.Range(minCubeSize, maxCubeSize);

        // Add SpriteRenderer
        SpriteRenderer sr = cubeGO.AddComponent<SpriteRenderer>();

        // Get random color and apply sprite
        PieceColor randomColor = PieceHelper.GetRandomColor();
        Sprite coloredSprite = PieceHelper.GetSpriteForColor(randomColor);
        sr.sprite = coloredSprite;
        sr.color = Color.white;
        sr.material = new Material(Shader.Find("Sprites/Default"));

        // Set scale based on random size
        cubeGO.transform.localScale = new Vector3(randomSize, randomSize, 1f);

        // Add Rigidbody2D for physics
        Rigidbody2D rb = cubeGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // We'll handle gravity manually
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Add BoxCollider2D
        BoxCollider2D collider = cubeGO.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(randomSize, randomSize);

        // Add the falling behavior script
        FallingCube fallingCube = cubeGO.AddComponent<FallingCube>();
        fallingCube.Initialize(Random.Range(minFallSpeed, maxFallSpeed), screenBottom);

        // Random horizontal position with edge margins for better spread
        float spawnLeft = screenLeft - edgeMargin;
        float spawnRight = screenRight + edgeMargin;
        float randomX = Random.Range(spawnLeft, spawnRight);
        cubeGO.transform.position = new Vector3(randomX, screenTop, 0f);
    }
}

// Helper script to handle individual cube falling
public class FallingCube : MonoBehaviour
{
    private float fallSpeed;
    private float screenBottom;

    public void Initialize(float speed, float bottom)
    {
        fallSpeed = speed;
        screenBottom = bottom;
    }

    void Update()
    {
        // Move the cube downward
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Destroy when off screen
        if (transform.position.y < screenBottom - 1f)
        {
            Destroy(gameObject);
        }
    }
}
