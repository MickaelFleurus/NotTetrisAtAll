using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;


public class GridHandler : MonoBehaviour
{
    private List<List<CellVisualLogic>> cell = new List<List<CellVisualLogic>>();

    [SerializeField] float DropInitialDelayMs = 0.15f;
    [SerializeField] GameObject prefabCell;
    [SerializeField] GameObject GridVisual;
    [SerializeField] InGameUI inGameUI;

    private int score = 0;
    private int lineCompleted = 0;
    private float delayNextDropMs = 0.15f;
    static int pieceCounter = 0;

    PieceObject currentPiece = null;
    PieceObject[] nextPieces = new PieceObject[3];
    PieceObject pieceHeld = null;
    bool canHold = true;

    Timer timer = null;


    private int stuckCount = 0;
    private InputSystem_Actions inputActions;

    public bool pauseGameLoop = true;
    private List<Vector2Int> currentPieceDestinationIndexes = new List<Vector2Int>();


    void Awake()
    {
        var originalPosition = new Vector3(0.0f, 0.0f);
        for (int j = 0; j < Height; j++)
        {
            List<CellVisualLogic> line = new List<CellVisualLogic>();
            originalPosition.x = 0.0f;
            originalPosition.y = j;

            for (int i = 0; i < Width; i++)
            {
                var go = Instantiate(prefabCell, this.transform);
                line.Add(go.GetComponent<CellVisualLogic>());
                originalPosition.x = i;
                go.transform.localPosition = originalPosition;
            }
            cell.Add(line);
        }
        delayNextDropMs = DropInitialDelayMs;
        GridVisual.GetComponent<SpriteRenderer>().size = new Vector2(Width, Height);

        // setup input actions
        inputActions = new InputSystem_Actions();
        inputActions.Player.Move.performed += ctx => OnMove(ctx.ReadValue<float>());
        inputActions.Player.Drop.performed += ctx => OnDrop();
        inputActions.Player.RotateClockwise.performed += ctx => OnRotateClockwise();
        inputActions.Player.RotateCounterClockwise.performed += ctx => OnRotateCounterClockwise();
        inputActions.Player.Hold.performed += ctx => OnHold();
        inputActions.Player.Pause.performed += ctx => OnPause();

        inputActions.Enable();
    }

    void Start()
    {
        var gameData = GameData.Instance;
        inGameUI.UpdateLevel(gameData.levelStart);
        transform.localPosition = new Vector3(-Width / 2.0f, -Height / 2.0f, 0.0f);
        if (gameData.gameMode != EGameMode.TimeLimit)
        {
            timer = new Timer(Timer.ETimerCountDirection.Up);
        }
        else
        {
            timer = new Timer(Timer.ETimerCountDirection.Down, GetTimeLimitMinutes(gameData.timeLimit));
        }
        timer.isDone += OnTimeOver;
    }

    void Update()
    {
        if (pauseGameLoop) return;
        timer.Update(Time.deltaTime);
        inGameUI.UpdateTimer(timer.GetTime());

        if (!currentPiece)
        {
            UseNextPiece();
        }
        else
        {
            delayNextDropMs -= Time.deltaTime;
            if (delayNextDropMs <= 0.0f)
            {
                if (!currentPiece.TryDrop())
                {
                    if (currentPiece.IsInGrid())
                    {
                        stuckCount++;
                    }
                    else
                    {
                        this.enabled = false;
                        inGameUI.ShowGameOver();
                        pauseGameLoop = true;
                        return;
                    }
                }
                if (stuckCount >= 3)
                {
                    PlaceCurrentPiece();
                    stuckCount = 0;
                }
                delayNextDropMs = DropInitialDelayMs;
            }

        }
    }

    void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
            inputActions.Dispose();
        }
    }

    public void OnGameStart()
    {
        timer.Start();

        nextPieces[0] = CreateNewPiece();
        nextPieces[1] = CreateNewPiece();
        nextPieces[2] = CreateNewPiece();
        pauseGameLoop = false;
    }

    private void UseNextPiece()
    {
        currentPiece = nextPieces[0];
        nextPieces[0] = nextPieces[1];
        nextPieces[1] = nextPieces[2];
        nextPieces[2] = CreateNewPiece();
        UpdateDestinationIndexes();
    }

    private int GetTimeLimitMinutes(EGameTimeLimit timeLimit)
    {
        return timeLimit switch
        {
            EGameTimeLimit.One => 1,
            EGameTimeLimit.Two => 2,
            EGameTimeLimit.Five => 5,
            EGameTimeLimit.Ten => 10,
            EGameTimeLimit.Twenty => 20,
            _ => 0
        };
    }

    private PieceObject CreateNewPiece()
    {
        var go = new GameObject($"Piece_{pieceCounter}");
        go.transform.SetParent(this.transform, false);

        var piece = go.AddComponent<PieceObject>();
        inGameUI.PushNextPieceTexture(piece.Initialize(this));
        pieceCounter++;
        return piece;
    }

    private void UpdateDestinationIndexes()
    {
        foreach (var index in currentPieceDestinationIndexes)
        {
            if (index.y >= Height || cell[index.y][index.x].IsFilled()) continue;

            cell[index.y][index.x].Clear();
        }
        currentPieceDestinationIndexes.Clear();
        if (currentPiece != null)
            currentPieceDestinationIndexes = currentPiece.GetDestinationIndexes();

        foreach (var index in currentPieceDestinationIndexes)
        {
            if (index.y >= Height) continue;
            cell[index.y][index.x].MarkAsMaybeNext();
        }
    }

    private void OnMove(float v)
    {
        if (pauseGameLoop || currentPiece == null) return;
        if (Mathf.Abs(v) > 0.5f)
        {
            int dx = v > 0 ? 1 : -1;
            if (currentPiece.TryMove(dx))
            {
                UpdateDestinationIndexes();
            }
        }
    }

    private void OnDrop()
    {
        if (pauseGameLoop || currentPiece == null) return;
        currentPiece.DropToLowest();
        stuckCount = 1000;
        delayNextDropMs = 0.0f;
    }

    private void OnRotateClockwise()
    {
        if (pauseGameLoop || currentPiece == null) return;
        if (currentPiece.TryRotateClockwise())
        {
            UpdateDestinationIndexes();
        }
        stuckCount = 0;
    }

    private void OnRotateCounterClockwise()
    {
        if (pauseGameLoop || currentPiece == null) return;
        if (currentPiece.TryRotateCounterClockwise())
        {
            UpdateDestinationIndexes();
        }
        stuckCount = 0;
    }


    private void PlaceCurrentPiece()
    {
        Sprite pieceSprite = Piece.PieceHelper.GetSpriteForColor(currentPiece.color);

        foreach (var index in currentPiece.GetIndexes())
        {
            cell[index.y][index.x].SetFilled(pieceSprite);
        }
        Destroy(currentPiece.gameObject);
        currentPiece = null;
        CheckCompletedLines();
    }

    private void CheckCompletedLines()
    {
        List<int> completedLines = new List<int>();
        for (int y = 0; y < Height; y++)
        {
            if (!cell[y].All(c => c.IsFilled()))
            {
                continue;
            }
            completedLines.Add(y);
            for (int x = 0; x < Width; x++)
            {
                cell[y][x].PrepareToBlink();
            }
        }
        if (completedLines.Count > 0)
        {
            pauseGameLoop = true;
            StartCoroutine(RemoveEmptyLines(completedLines, CellVisualLogic.BlinkDuration + 0.1f));
        }
        else
        {
            canHold = true;
        }

    }

    private IEnumerator RemoveEmptyLines(List<int> completedLines, float delay)
    {
        yield return new WaitForSeconds(delay);

        var remainingRows = new List<Sprite[]>();
        for (int y = 0; y < Height; y++)
        {
            if (!completedLines.Contains(y))
            {
                Sprite[] row = new Sprite[Width];
                for (int x = 0; x < Width; x++)
                {
                    if (cell[y][x].IsFilled())
                    {
                        var sprite = cell[y][x].GetSprite();
                        row[x] = sprite;
                    }
                    else
                    {
                        row[x] = null;
                    }
                }
                remainingRows.Add(row);
            }
        }
        int targetY = 0;
        foreach (var row in remainingRows)
        {
            for (int x = 0; x < Width; x++)
            {
                if (row[x] != null) cell[targetY][x].SetFilled(row[x]);
                else cell[targetY][x].Clear();
            }
            targetY++;
        }

        // clear remaining top rows
        for (int y = targetY; y < Height; y++)
            for (int x = 0; x < Width; x++)
                cell[y][x].Clear();


        if (completedLines.Count > 0)
        {
            lineCompleted += completedLines.Count;
            score += completedLines.Count * 100;
            inGameUI.UpdateLines(lineCompleted);
            inGameUI.UpdateScore(score);
        }
        pauseGameLoop = false;
        canHold = true;
    }

    public bool IsFree(Vector2Int indices)
    {
        if (indices.x < 0 || indices.y < 0 || indices.x >= Width) return false;
        if (indices.y >= Height) return true;
        return cell[indices.y][indices.x].IsEmpty();
    }

    private void OnTimeOver()
    {
        pauseGameLoop = true;
        this.enabled = false;
        inGameUI.ShowGameOver();
    }

    private void OnHold()
    {
        if (!canHold) return;
        if (pieceHeld == null)
        {
            pieceHeld = currentPiece;
            UseNextPiece();
        }
        else
        {
            var temp = pieceHeld;
            pieceHeld = currentPiece;
            currentPiece = temp;
            currentPiece.ResetPosition();
        }
        inGameUI.UpdateHeldPieceTexture(pieceHeld.pieceLook);
        UpdateDestinationIndexes();
        pieceHeld.transform.position = new Vector3(-150, -150, 0);
        canHold = false;
    }

    private void OnPause()
    {

    }

    public static int Width => 10;
    public static int Height => 20;

}
