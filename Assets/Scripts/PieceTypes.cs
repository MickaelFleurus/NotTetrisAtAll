using System.Collections.Generic;
using System.IO;
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
        static Dictionary<PieceColor, Sprite> cache = new Dictionary<PieceColor, Sprite>();
        public static PieceColor GetRandomColor()
        {
            var values = System.Enum.GetValues(typeof(PieceColor));
            return (PieceColor)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        public static Sprite GetSpriteForColor(PieceColor color)
        {
            if (!cache.ContainsKey(color))
            {
                var sprite = Resources.Load<Sprite>($"Sprites/{color.ToString()}Cube");
                cache[color] = sprite;
            }
            return cache[color];
        }

        static List<Vector2Int> NormalizeToCentroid(List<Vector2Int> shape)
        {
            float avgX = shape.Aggregate(0, (sum, p) => sum + p.x) / (float)shape.Count;
            float avgY = shape.Aggregate(0, (sum, p) => sum + p.y) / (float)shape.Count;
            var offset = new Vector2Int(Mathf.RoundToInt(avgX), Mathf.RoundToInt(avgY));
            return shape.Select(p => p - offset).ToList();
        }

        static List<Vector2Int> DenormalizeToCentroid(List<Vector2Int> shape, Vector2Int targetCentroid)
        {
            float avgX = shape.Aggregate(0, (sum, p) => sum + p.x) / (float)shape.Count;
            float avgY = shape.Aggregate(0, (sum, p) => sum + p.y) / (float)shape.Count;
            var currentCentroid = new Vector2Int(Mathf.RoundToInt(avgX), Mathf.RoundToInt(avgY));
            var offset = targetCentroid - currentCentroid;
            return shape.Select(p => p + offset).ToList();
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

        static List<Vector2Int> DenormalizeByMin(List<Vector2Int> shape, Vector2Int targetMin)
        {
            int minX = shape.Min(p => p.x);
            int minY = shape.Min(p => p.y);
            var offset = targetMin - new Vector2Int(minX, minY);
            return shape.Select(p => p + offset).ToList();
        }

        public static Sprite CreatePieceSpriteFile(List<Vector2Int> shape, PieceColor color)
        {
            if (shape == null || shape.Count == 0) return null;
            string uniqueName = CreateUniqueNameForShape(shape);
            if (File.Exists(Application.dataPath + "/GeneratedSprites/" + uniqueName + ".png"))
            {
                var loaded = Resources.Load<Sprite>("GeneratedSprites/" + uniqueName);
                if (loaded != null)
                    return loaded;
            }

            int minX = shape.Min(p => p.x);
            int minY = shape.Min(p => p.y);
            int maxX = shape.Max(p => p.x);
            int maxY = shape.Max(p => p.y);

            int wCells = maxX - minX + 1;
            int hCells = maxY - minY + 1;

            int canvasCells = shape.Count;
            int texW = 64; //canvasCells* tileSize;
            int texH = 64; //canvasCells* tileSize;

            int tileSize = texW / (canvasCells + 1);
            int offsetX = (texW - wCells * tileSize) / 2;
            int offsetY = (texH - hCells * tileSize) / 2;

            var tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Sprite baseTileSprite = GetSpriteForColor(color);
            Rect rect = baseTileSprite.rect;
            Texture2D baseTex = baseTileSprite.texture;
            int baseW = (int)rect.width;
            int baseH = (int)rect.height;
            Color[] basePixels = baseTex.GetPixels(0, 0, baseW, baseH);

            Color[] outPixels = new Color[texW * texH];
            Color clear = new Color(0, 0, 0, 0);
            for (int i = 0; i < outPixels.Length; i++) outPixels[i] = clear;
            var zeroedShape = DenormalizeByMin(shape, Vector2Int.zero);

            foreach (Vector2Int piece in zeroedShape)
            {
                int startX = offsetX + piece.x * tileSize;
                int startY = offsetY + piece.y * tileSize;

                for (int y = 0; y < tileSize; y++)
                {
                    for (int x = 0; x < tileSize; x++)
                    {
                        int px = startX + x;
                        int py = startY + y;

                        int sx = Mathf.Clamp(Mathf.FloorToInt(x * (baseW / (float)tileSize)), 0, baseW - 1);
                        int sy = Mathf.Clamp(Mathf.FloorToInt(y * (baseH / (float)tileSize)), 0, baseH - 1);

                        outPixels[py * texW + px] = basePixels[sy * baseW + sx];
                    }
                }
            }

            tex.SetPixels(outPixels);
            tex.Apply();

            SaveSpriteToPng(tex, uniqueName);

            // Calculate center of the visual content
            float pivotX = 0.5f;
            float pivotY = 0.5f;

            var sprite = Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(pivotX, pivotY), tileSize, 0, SpriteMeshType.FullRect);
            return sprite;
        }

        public static void SaveSpriteToPng(Texture2D texture, string name)
        {
            string folderPath = Path.Combine(Application.dataPath, "GeneratedSprites");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, $"{name}.png");
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
        }

        public static string CreateUniqueNameForShape(List<Vector2Int> shape)
        {
            shape.Sort((a, b) =>
            {
                int cmp = a.x.CompareTo(b.x);
                if (cmp == 0) return a.y.CompareTo(b.y);
                return cmp;
            });
            int minX = shape.Min(p => p.x);
            int minY = shape.Min(p => p.y);
            shape = shape.Select(p => new Vector2Int(p.x - minX, p.y - minY)).ToList();


            string uniqueName = string.Join("_", shape.Select(p => $"{p.x}x{p.y}"));
            return uniqueName;
        }
    }
}
