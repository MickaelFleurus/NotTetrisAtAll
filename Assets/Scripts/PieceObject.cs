using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


public class PieceObject : MonoBehaviour
{
    public List<GameObject> parts = new List<GameObject>();
    public Piece.PieceColor color { get; private set; }

    public GridHandler grid { get; private set; }

    private Vector2Int positionIndex = new Vector2Int(GridHandler.Width / 2, GridHandler.Height + 1);
    private List<Vector2Int> pieceIndices = new List<Vector2Int>();
    static int pieceCounter = 0;


    private readonly Vector2Int[] kicks = {
        new Vector2Int(0,0),
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
    };


    public Sprite Initialize(GridHandler g)
    {
        grid = g;
        color = Piece.PieceHelper.GetRandomColor();
        this.pieceIndices = Piece.PieceHelper.GetRandomPieceShape(GridHandler.PieceSize);
        var sprite = Piece.PieceHelper.GetSpriteForColor(color);
        for (int i = 0; i < pieceIndices.Count; i++)
        {
            GameObject go = new GameObject($"Piece_{pieceCounter}_{i}");
            go.transform.parent = this.transform;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = 1;
            sr.material = new Material(Shader.Find("Sprites/Default"));
            sr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            parts.Add(go);
            go.transform.localPosition = new Vector3(pieceIndices[i].x, pieceIndices[i].y, 0f);

            Debug.Log($"Part {i}: sprite={sprite.name}, sr.color={sr.color}");
        }
        pieceCounter++;
        return Piece.PieceHelper.CreatePieceSpriteFile(pieceIndices, color);
    }

    void Awake()
    {
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
                for (int i = 0; i < parts.Count && i < pieceIndices.Count; i++)
                {
                    parts[i].transform.localPosition = new Vector3(pieceIndices[i].x, pieceIndices[i].y, 0f);
                }
                return true;
            }
        }

        return false;
    }

    public List<Vector2Int> GetIndexes()
    {
        return pieceIndices.Select(p => p + positionIndex).ToList();
    }

    public bool IsInGrid()
    {
        foreach (var piece in pieceIndices)
        {
            Vector2Int index = piece + positionIndex;
            if (index.x < 0 || index.y < 0 || index.x >= GridHandler.Width || index.y >= GridHandler.Height)
            {
                return false;
            }
        }
        return true;
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
