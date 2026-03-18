using System.Collections.Generic;
using UnityEngine;
using Piece;


public class GridHandler : MonoBehaviour
{
    private List<List<SpriteRenderer>> spriteRenderers = new List<List<SpriteRenderer>>();
    private List<bool> cellState = new List<bool>();

    [SerializeField] float DropInitialDelayMs = 0.15f;
    [SerializeField] PiecePrefabMap prefabMap;
    [SerializeField] GameObject prefabCell;

    private Dictionary<PieceType, GameObject> piecePrefabs = new();
    private int score = 0;
    private int lineCompleted = 0;
    private float delayNextDropMs = 0.15f;
    GameObject currentPieceGo = null;
    PieceObject currentPiece = null;
    private int stuckCount = 0;

    void Awake()
    {
        var originalPosition = new Vector3(0.0f, 0.0f);
        for (int j = 0; j < Height; j++)
        {
            List<SpriteRenderer> line = new List<SpriteRenderer>();
            originalPosition.x = 0.0f;
            originalPosition.y = j;

            for (int i = 0; i < Width; i++)
            {
                var go = Instantiate(prefabCell, this.transform);
                line.Add(go.GetComponent<SpriteRenderer>());
                originalPosition.x = i;
                go.transform.localPosition = originalPosition;
                cellState.Add(false);
            }
            spriteRenderers.Add(line);
        }
        delayNextDropMs = DropInitialDelayMs;

        if (prefabMap != null)
            piecePrefabs = prefabMap.ToDictionary();
        else
            piecePrefabs = new Dictionary<PieceType, GameObject>();
    }

    void Update()
    {
        if (!currentPieceGo)
        {
            var nextType = PieceTypes.GetRandomPieceType();
            currentPieceGo = Instantiate(piecePrefabs[nextType], this.transform);
            currentPiece = currentPieceGo.GetComponent<PieceObject>();
            currentPiece.Initialize(this);
        }
        else
        {
            delayNextDropMs -= Time.deltaTime;
            if (delayNextDropMs <= 0.0f)
            {
                if (!currentPiece.TryDrop())
                {
                    stuckCount++;
                }
                if (stuckCount == 3)
                {
                    PlaceCurrentPiece();
                    stuckCount = 0;
                }
                delayNextDropMs = DropInitialDelayMs;
            }

        }
    }

    private void PlaceCurrentPiece()
    {
        foreach (var pair in currentPiece.GetIndexes())
        {
            spriteRenderers[pair.Key.y][pair.Key.x].sprite = pair.Value;
            cellState[pair.Key.y * Width + pair.Key.x] = true;
        }
        Destroy(currentPieceGo);
        currentPiece = null;
        currentPieceGo = null;
        CheckCompletedLines();
    }

    private void CheckCompletedLines()
    {

    }

    public bool IsFree(Vector2Int indices)
    {
        if (indices.y >= Height) return true;
        if (indices.x < 0 || indices.y < 0 || indices.x >= Width) return false;
        int idx = indices.y * Width + indices.x;
        return !cellState[idx];
    }

    public int Width => 10;
    public int Height => 20;
}
