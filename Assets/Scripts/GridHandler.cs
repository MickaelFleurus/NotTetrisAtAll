using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.Playables;
using System;


public class GridHandler : MonoBehaviour
{
    private List<List<CellVisualLogic>> cell = new List<List<CellVisualLogic>>();

    [SerializeField] GameObject prefabCell;
    [SerializeField] GameObject GridVisual;
    [SerializeField] GameObject Mask;
    [SerializeField] InGameUI inGameUI;
    [SerializeField] PlayableDirector playableDirector;
    [SerializeField] ParticuleHandler particuleHandler;

    private bool introFinishedPlaying = false;

    private int level = 0;
    private readonly int MaxLevel = 20;
    private int score = 0;
    private int lineCompleted = 0;
    [SerializeField] float DropInitialDelayMs;
    private float currentDropDelayMs;
    private readonly float MinimumDelayMs = 0.02f;
    private float delayNextDropMs;
    static int pieceCounter = 0;

    PieceObject currentPiece = null;
    PieceObject[] nextPieces = new PieceObject[3];
    PieceObject pieceHeld = null;
    bool canHold = true;

    Timer timer = null;


    private float stuckTimer = 0.0f;
    private readonly float StuckTimerThreshold = 1f;
    private InputSystem_Actions playerInputs;

    public bool pauseGameLoop = true;
    private List<Vector2Int> currentPieceDestinationIndexes = new List<Vector2Int>();

    private InputRepeatHandler moveRepeatHandler;

    private AudioClip currentMusic = null;


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

        playerInputs = new InputSystem_Actions();
        playerInputs.Player.Drop.started += ctx => OnDrop();
        playerInputs.Player.RotateClockwise.started += ctx => OnRotateClockwise();
        playerInputs.Player.RotateCounterClockwise.started += ctx => OnRotateCounterClockwise();
        playerInputs.Player.Hold.started += ctx => OnHold();
        playerInputs.Player.Pause.started += ctx => OnPause();

        playerInputs.Enable();

        PauseMenu.unpauseGame += OnUnpause;

        playableDirector.stopped += SetIntroDone;

        moveRepeatHandler = new InputRepeatHandler(0.20f);
    }

    void Start()
    {
        var gameData = GameData.Instance;
        inGameUI.UpdateLevel(gameData.levelStart);
        level = gameData.levelStart;
        UpdateCurrentDropDelay();
        transform.localPosition = new Vector3(-Width / 2.0f, -Height / 2.0f, 0.0f);
        Mask.transform.localScale = new Vector3(Width, Height, 1f);

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
            return;
        }

        float moveInput = playerInputs.Player.Move.ReadValue<float>();
        if (moveRepeatHandler.ShouldRepeat(moveInput, Time.deltaTime))
        {
            OnMove(moveInput);
        }

        if (currentPiece.currentState == PieceObject.EState.Falling)
        {
            delayNextDropMs -= Time.deltaTime;
            if (delayNextDropMs <= 0.0f)
            {
                currentPiece.Drop();
                delayNextDropMs = currentDropDelayMs;
            }
        }
        else if (currentPiece.currentState == PieceObject.EState.Stuck)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= StuckTimerThreshold)
            {
                if (!currentPiece.IsInGrid())
                {
                    this.enabled = false;
                    inGameUI.ShowGameOver();
                    pauseGameLoop = true;
                    return;
                }

                PlaceCurrentPiece();
                stuckTimer = 0f;
            }
        }


    }

    void OnDestroy()
    {
        if (playerInputs != null)
        {
            playerInputs.Disable();
            playerInputs.Dispose();
        }
        playableDirector.stopped -= SetIntroDone;
    }


    private void SetIntroDone(PlayableDirector ctx)
    {
        introFinishedPlaying = true;
    }

    public void OnGameStart()
    {
        TryMusicChange();
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
        currentPiece.currentState = PieceObject.EState.Falling;
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

    private void UpdatePieceStateAndResetStuckTimer()
    {
        stuckTimer = 0f;
        currentPiece.CheckState();
    }

    private void OnMove(float v)
    {
        if (pauseGameLoop || currentPiece == null) return;
        if (Mathf.Abs(v) > 0.5f)
        {
            int dx = v > 0 ? 1 : -1;
            if (currentPiece.TryMove(dx))
            {
                AudioMixer.Instance.PlaySFX(AudioData.Instance.GameMoveSfx);
                UpdateDestinationIndexes();
                UpdatePieceStateAndResetStuckTimer();
            }
        }
    }

    private void OnDrop()
    {
        if (pauseGameLoop || currentPiece == null) return;
        currentPiece.DropToLowest();
        stuckTimer = 1000f;
        delayNextDropMs = 0.0f;
    }

    private void OnRotateClockwise()
    {
        if (pauseGameLoop || currentPiece == null) return;
        if (currentPiece.TryRotateClockwise())
        {
            AudioMixer.Instance.PlaySFX(AudioData.Instance.GameTurnSfx);
            UpdateDestinationIndexes();
        }
        UpdatePieceStateAndResetStuckTimer();
    }

    private void OnRotateCounterClockwise()
    {
        if (pauseGameLoop || currentPiece == null) return;
        if (currentPiece.TryRotateCounterClockwise())
        {
            AudioMixer.Instance.PlaySFX(AudioData.Instance.GameTurnSfx);
            UpdateDestinationIndexes();
        }
        UpdatePieceStateAndResetStuckTimer();
    }


    private void PlaceCurrentPiece()
    {
        AudioMixer.Instance.PlaySFX(AudioData.Instance.GamePlacedSfx);
        Sprite pieceSprite = Piece.PieceHelper.GetSpriteForColor(currentPiece.color);
        particuleHandler.EmitOnDrop(currentPiece);

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

        // Emit particles for each completed line at its center
        foreach (var lineY in completedLines)
        {
            Vector3 lineCenterPosition = new Vector3(0f, lineY - 10f, 0);
            particuleHandler.EmitOnLineDestroyed(lineCenterPosition);
        }

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

        AudioMixer.Instance.PlaySFX(AudioData.Instance.GameDestroySfx);
        lineCompleted += completedLines.Count;
        score += completedLines.Count * 100;

        int newLevel = GameData.Instance.levelStart + (int)Math.Floor(lineCompleted / 10f);
        if (newLevel > level)
        {
            level = newLevel;
            inGameUI.UpdateLevel(level);
            UpdateCurrentDropDelay();
            TryMusicChange();
        }

        inGameUI.UpdateLines(lineCompleted);
        inGameUI.UpdateScore(score);

        pauseGameLoop = false;
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
            currentPiece.currentState = PieceObject.EState.Falling;
        }
        inGameUI.UpdateHeldPieceTexture(pieceHeld.pieceLook);
        UpdateDestinationIndexes();
        pieceHeld.transform.position = new Vector3(-150, -150, 0);
        pieceHeld.currentState = PieceObject.EState.Held;
        canHold = false;
    }

    private void OnPause()
    {
        if (playableDirector.state == PlayState.Playing)
        {
            playableDirector.Pause();
            inGameUI.HideIntro();
        }
        pauseGameLoop = true;
        playerInputs.Disable();
        inGameUI.ShowPauseMenu();
    }

    private void OnUnpause()
    {
        inGameUI.HidePauseMenu();
        playerInputs.Enable();
        if (!introFinishedPlaying)
        {
            playableDirector.Resume();
            inGameUI.ShowIntro();
        }
        else
        {
            pauseGameLoop = false;
        }
    }

    private void UpdateCurrentDropDelay()
    {
        currentDropDelayMs = DropInitialDelayMs - ((DropInitialDelayMs - MinimumDelayMs) / MaxLevel) * Math.Min(level, MaxLevel);
    }

    private void TryMusicChange()
    {
        if (level < 10 && currentMusic == null)
        {
            int randomChoice = UnityEngine.Random.Range(0, 2);
            currentMusic = randomChoice == 0 ? AudioData.Instance.InGameMusic : AudioData.Instance.InGameMusic2;
            AudioMixer.Instance.PlayMusic(currentMusic);
        }
        else if (level >= 10 && level < 20 && currentMusic != AudioData.Instance.InGameMusicFaster)
        {
            currentMusic = AudioData.Instance.InGameMusicFaster;
            AudioMixer.Instance.PlayMusic(currentMusic);
        }
        else if (level == 20)
        {
            currentMusic = AudioData.Instance.InGameMusicFastest;
            AudioMixer.Instance.PlayMusic(currentMusic);
        }

    }

    public static int Width => 10;
    public static int Height => 20;

}
