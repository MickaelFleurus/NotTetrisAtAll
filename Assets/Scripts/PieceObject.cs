using UnityEngine;
using System.Collections.Generic;
using System.Data.Common;
using System;
using System.Linq;


public class PieceObject : MonoBehaviour
{
    public List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

    public GridHandler grid { get; private set; }
    public void Initialize(GridHandler g) => grid = g;

    private Vector2Int positionIndex = new Vector2Int(5, 21);
    private List<Vector2Int> pieceIndices = new List<Vector2Int>();


    private readonly Vector2Int[] kicks = {
        new Vector2Int(0,0),
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
    };


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

    public List<Vector2Int> GetDestinationIndexes()
    {
        Vector2Int lowestPosition = positionIndex;
        lowestPosition.y = FigureLowestPossibleHeight();

        List<Vector2Int> indices = new List<Vector2Int>();
        foreach (var piece in pieceIndices)
        {
            Vector2Int index = piece + lowestPosition;
            indices.Add(index);
        }
        return indices;
    }

    public bool DropToLowest()
    {
        positionIndex.y = FigureLowestPossibleHeight();
        transform.localPosition = new Vector2(positionIndex.x, positionIndex.y);

        return true;
    }

    public bool TryMove(int dx)
    {
        // check if each block can move horizontally by dx
        foreach (var piece in pieceIndices)
        {
            Vector2Int index = piece + positionIndex;
            index.x += dx;
            if (!grid.IsFree(index))
            {
                return false;
            }
        }
        positionIndex.x += dx;
        transform.localPosition = new Vector2(positionIndex.x, positionIndex.y);
        return true;
    }

    public bool TryRotateClockwise()
    {
        if (pieceIndices.Count == 0) return false;
        return TryRotate((orig) =>
        {
            var newPosition = orig;
            newPosition.x = newPosition.y;
            newPosition.y = -orig.x;
            return newPosition;
        });
    }

    public bool TryRotateCounterClockwise()
    {
        if (pieceIndices.Count == 0) return false;
        return TryRotate((orig) =>
        {
            var newPosition = orig;
            newPosition.x = -newPosition.y;
            newPosition.y = orig.x;
            return newPosition;
        });
    }

    private bool TryRotate(Func<Vector2Int, Vector2Int> rotationFunc)
    {
        if (pieceIndices.Count == 0) return false;
        List<Vector2Int> newPieceIndices = new List<Vector2Int>();

        for (int i = 0; i < pieceIndices.Count; ++i)
        {
            newPieceIndices.Add(rotationFunc(pieceIndices[i]));
        }

        foreach (var kick in kicks)
        {
            bool ok = true;
            for (int i = 0; i < newPieceIndices.Count; i++)
            {
                var gridIdx = newPieceIndices[i] + positionIndex + kick;
                if (!grid.IsFree(gridIdx))
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
            {
                pieceIndices = newPieceIndices;
                positionIndex += kick;
                transform.localPosition = new Vector2(positionIndex.x, positionIndex.y);
                for (int i = 0; i < spriteRenderers.Count && i < pieceIndices.Count; i++)
                {
                    spriteRenderers[i].transform.localPosition = new Vector3(pieceIndices[i].x, pieceIndices[i].y, 0f);
                }
                return true;
            }
        }

        return false;
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

    public bool IsInGrid()
    {
        foreach (var piece in pieceIndices)
        {
            Vector2Int index = piece + positionIndex;
            if (index.x >= 0 && index.y >= 0 && index.x < GridHandler.Width && index.y < GridHandler.Height)
            {
                return true;
            }
        }
        return false;
    }

    private int FigureLowestPossibleHeight()
    {
        int lowestY = Math.Min(positionIndex.y, GridHandler.Height - 1);
        List<Vector2Int> validPositions = new List<Vector2Int>();
        var lowestPosition = positionIndex;
        lowestPosition.y = lowestY;

        foreach (var indice in pieceIndices)
        {
            validPositions.Add(indice + lowestPosition);
        }
        Func<List<Vector2Int>, bool> checkValid = (positions) =>
        {
            return positions.All(position => grid.IsFree(position));
        };
        while (checkValid(validPositions))
        {
            for (int i = 0; i < validPositions.Count; i++)
            {
                validPositions[i] -= Vector2Int.up;
            }
            lowestY--;
        }
        return lowestY + 1;

    }
}
