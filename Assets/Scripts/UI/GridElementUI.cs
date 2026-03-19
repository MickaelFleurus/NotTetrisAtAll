using UnityEngine;
using UnityEngine.UIElements;

public class GridElement : VisualElement
{
    public int columns = GridHandler.Width;
    public int rows = GridHandler.Height;
    public Color lineColor = new Color(1, 0, 1, 1.0f);

    public GridElement()
    {
        generateVisualContent += OnGenerateVisualContent;
    }

    void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;

        Rect r = contentRect;

        float cellWidth = r.width / columns;
        float cellHeight = r.height / rows;

        painter.strokeColor = lineColor;
        painter.lineWidth = 1;

        // Vertical lines
        for (int x = 0; x <= columns; x++)
        {
            float px = r.xMin + x * cellWidth;
            painter.BeginPath();
            painter.MoveTo(new Vector2(px, r.yMin));
            painter.LineTo(new Vector2(px, r.yMax));
            painter.Stroke();
        }

        // Horizontal lines
        for (int y = 0; y <= rows; y++)
        {
            float py = r.yMin + y * cellHeight;
            painter.BeginPath();
            painter.MoveTo(new Vector2(r.xMin, py));
            painter.LineTo(new Vector2(r.xMax, py));
            painter.Stroke();
        }
    }
}
