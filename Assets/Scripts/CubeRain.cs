using UnityEngine;
using Piece;
using System.Collections;
using System.Collections.Generic;

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
    private const int MaxActiveCubes = 200; // Limit the number of active cubes to prevent performance issues
    private Queue<FallingCube> activeCubes = new Queue<FallingCube>(); // Pool of cubes to reuse

    void Start()
    {
        mainCamera = Camera.main;
        UpdateCameraBounds();
        for (int i = 0; i < MaxActiveCubes; i++)
        {
            activeCubes.Enqueue(SpawnCube()); // Pre-instantiate cubes and add to the pool
        }
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            if (activeCubes.Count > 0)
            {
                LaunchCube();
            }
            spawnTimer = spawnRate;
        }

    }

    private void LaunchCube()
    {
        FallingCube cube = activeCubes.Dequeue();
        // Random horizontal position with edge margins for better spread
        float spawnLeft = screenLeft - edgeMargin;
        float spawnRight = screenRight + edgeMargin;
        float randomX = Random.Range(spawnLeft, spawnRight);
        Vector3 spawnPos = new Vector3(randomX, screenTop, 0f);
        float randomSpeed = Random.Range(minFallSpeed, maxFallSpeed);
        cube.Launch(spawnPos, randomSpeed);
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

    private FallingCube SpawnCube()
    {
        // Create a new GameObject for the cube
        GameObject cubeGO = new GameObject("FallingCube");
        cubeGO.transform.SetParent(this.transform);

        // Randomize cube size
        float randomSize = Random.Range(minCubeSize, maxCubeSize);

        // Add SpriteRenderer
        SpriteRenderer sr = cubeGO.AddComponent<SpriteRenderer>();
        sr.color = Color.white;
        sr.material = new Material(Shader.Find("Sprites/Default"));

        // Set scale based on random size
        cubeGO.transform.localScale = new Vector3(randomSize, randomSize, 1f);

        // Add the falling behavior script
        FallingCube fallingCube = cubeGO.AddComponent<FallingCube>();
        fallingCube.Initialize(screenBottom, sr, this);
        return fallingCube;
    }

    public void QueueCube(FallingCube cube)
    {
        activeCubes.Enqueue(cube);
    }
}

// Helper script to handle individual cube falling
public class FallingCube : MonoBehaviour
{
    private float screenBottom;
    private SpriteRenderer sr;
    private CubeRain parent;

    private bool isActive = false;
    private float fallSpeed;

    public void Initialize(float bottom, SpriteRenderer sr, CubeRain cubeRain)
    {
        screenBottom = bottom;
        this.sr = sr;
        parent = cubeRain;
    }

    public void Launch(Vector3 position, float speed)
    {
        PieceColor randomColor = PieceHelper.GetRandomColor();
        Sprite coloredSprite = PieceHelper.GetSpriteForColor(randomColor);
        sr.sprite = coloredSprite;

        transform.position = position;
        fallSpeed = speed;
        isActive = true;
    }

    void Update()
    {
        if (!isActive) return;

        // Move the cube downward
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Destroy when off screen
        if (transform.position.y < screenBottom - 1f)
        {
            isActive = false;
            parent.QueueCube(this); // Return the cube to the pool instead of destroying
        }
    }
}
