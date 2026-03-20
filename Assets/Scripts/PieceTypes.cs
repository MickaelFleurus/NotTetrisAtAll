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

        public static Sprite CreatePieceSpriteFile(List<Vector2Int> shape, int tileSize = 16, int border = 1)
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

            int texW = wCells * tileSize;
            int texH = hCells * tileSize;
            var tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Sprite baseTileSprite = GetSpriteForColor(PieceColor.Grey);
            Rect rect = baseTileSprite.rect;
            Texture2D baseTex = baseTileSprite.texture;
            int baseW = (int)rect.width;
            int baseH = (int)rect.height;
            Color[] basePixels = baseTex.GetPixels((int)rect.x, (int)rect.y, baseW, baseH);

            Color[] outPixels = new Color[texW * texH];
            Color clear = new Color(0, 0, 0, 0);
            for (int i = 0; i < outPixels.Length; i++) outPixels[i] = clear;

            Color borderBlend = Color.black;

            for (int pi = 0; pi < shape.Count; ++pi)
            {
                var p = shape[pi];
                int cellX = p.x - minX;
                int cellY = p.y - minY;
                int startX = cellX * tileSize;
                int startY = cellY * tileSize;

                for (int y = 0; y < tileSize; y++)
                {
                    for (int x = 0; x < tileSize; x++)
                    {
                        int px = startX + x;
                        int py = startY + y;
                        // nearest-neighbour sample from base tile
                        int sx = Mathf.Clamp(Mathf.FloorToInt(x * (baseW / (float)tileSize)), 0, baseW - 1);
                        int sy = Mathf.Clamp(Mathf.FloorToInt(y * (baseH / (float)tileSize)), 0, baseH - 1);
                        Color sample = basePixels[sy * baseW + sx];

                        bool isBorder = border > 0 && (x < border || y < border || x >= tileSize - border || y >= tileSize - border);
                        Color final = isBorder ? Color.Lerp(sample, borderBlend, 0.35f) : sample;

                        outPixels[py * texW + px] = final;
                    }
                }
            }

            tex.SetPixels(outPixels);
            tex.Apply();

            SaveSpriteToPng(tex, uniqueName);

            var sprite = Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), tileSize, 0, SpriteMeshType.FullRect);
            return sprite;
        }

        public static void SaveSpriteToPng(Texture2D texture, string name)
        {
            string folderPath = Path.Combine(Application.dataPath, "GeneratedSprites");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, $"{name}.png");
            Debug.Log($"Saving generated sprite to: {fullPath}");

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
