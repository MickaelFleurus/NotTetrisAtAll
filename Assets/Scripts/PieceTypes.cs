
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Piece
{
    public enum PieceColor
    {
        Red,
        Green,
        Blue,
        Yellow,
        Grey,
        Pink,
        Orange,
        Purple,
        LightBlue
    }

    public static class PieceHelper
    {
        public static PieceColor GetRandomColor()
        {
            var values = System.Enum.GetValues(typeof(PieceColor));
            return (PieceColor)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        public static Sprite GetSpriteForColor(PieceColor color)
        {

            return Resources.Load<Sprite>($"Sprites/{color.ToString()}Cube");
        }
        static List<Vector2Int> NormalizeToCentroid(List<Vector2Int> shape)
        {
            float avgX = shape.Aggregate(0, (sum, p) => sum + p.x) / (float)shape.Count;
            float avgY = shape.Aggregate(0, (sum, p) => sum + p.y) / (float)shape.Count;
            var offset = new Vector2Int(Mathf.RoundToInt(avgX), Mathf.RoundToInt(avgY));
            return shape.Select(p => p - offset).ToList();
        }
        public static List<Vector2Int> GetRandomPieceShape(int size)
        {
            List<Vector2Int> shape = new List<Vector2Int>
            {
                Vector2Int.zero
            };
            for (int i = 0; i < size - 1; i++)
            {
                Vector2Int newPos = Vector2Int.zero;
                int attempts = 0;
                do
                {
                    var baseCell = shape[Random.Range(0, shape.Count)];
                    int x = Random.Range(-1, 2);
                    int y = (x == 0) ? Random.Range(-1, 2) : 0;
                    if (x == 0 && y == 0)
                        continue;
                    newPos = baseCell + new Vector2Int(x, y);
                    attempts++;
                    if (attempts > 100) break;
                } while (shape.Contains(newPos));
                shape.Add(newPos);
            }
            return NormalizeToCentroid(shape);

        }
    }
}
