using UnityEngine;
using System.Collections.Generic;
using System.Data.Common;
using System;

public class PieceObject : MonoBehaviour
{
    public List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

    public GridHandler grid { get; private set; }
    public void Initialize(GridHandler g) => grid = g;

    private Vector2Int positionIndex = new Vector2Int(5, 21);
    private List<Vector2Int> pieceIndices = new List<Vector2Int>();


    void Awake()
    {
        spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        foreach (var sr in spriteRenderers)
        {
            Vector3 partPosition = sr.transform.localPosition;
            pieceIndices.Add(new Vector2Int((int)partPosition.x, (int)partPosition.y));
        }

    }

    void Start()
    {
        transform.localPosition = new Vector2(positionIndex.x, positionIndex.y);
    }

    public bool TryDrop()
    {
        foreach (var piece in pieceIndices)
        {
            Vector2Int index = piece + positionIndex;
            index.y--;
            if (!grid.IsFree(index))
            {
                return false;
            }
        }
        positionIndex.y -= 1;
        transform.localPosition = new Vector2(positionIndex.x, positionIndex.y);
        return true;
    }

    public Dictionary<Vector2Int, Sprite> GetIndexes()
    {
        int i = 0;
        Dictionary<Vector2Int, Sprite> indices = new Dictionary<Vector2Int, Sprite>();
        foreach (var piece in pieceIndices)
        {
            Vector2Int index = piece + positionIndex;
            if (index.x >= 0 && index.y >= 0)
            {
                indices.Add(index, spriteRenderers[i].sprite);
            }
            i++;
        }
        return indices;
    }
}
